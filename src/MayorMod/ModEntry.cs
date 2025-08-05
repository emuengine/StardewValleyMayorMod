using MayorMod.Constants;
using MayorMod.Data;
using MayorMod.Data.Handlers;
using MayorMod.Data.Interfaces;
using MayorMod.Data.Models;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.Objects;

namespace MayorMod;

/// <summary>
/// The mod entry point.
/// </summary>
internal sealed class ModEntry : Mod
{
    private MayorModData _saveData = new();
    private ModConfigHandler _configHandler = null!;
    private AssetUpdateHandler _assetUpdateHandler = null!;
    private AssetInvalidationHandler _assetInvalidationHandler = null!;
    private bool riverCleanUpRunOnce = true;
    

    /// <summary>
    /// The mod entry point, called after the mod is first loaded.
    /// </summary>
    /// <param name="helper">Provides simplified APIs for writing mods.</param>
    public override void Entry(IModHelper helper)
    {
        _configHandler = new ModConfigHandler(Helper, ModManifest, Helper.ReadConfig<MayorModConfig>());
        _assetUpdateHandler = new AssetUpdateHandler(Helper, Monitor);
        _assetInvalidationHandler = new AssetInvalidationHandler(Helper);

        TileActionHandler.Init(Helper, Monitor, _configHandler.ModConfig);
        Phone.PhoneHandlers.Add(new PollingDataHandler(Helper, _configHandler.ModConfig));
        EventCommandHandler.AddExtraEventCommands(Monitor);

        Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
        Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
        Helper.Events.GameLoop.DayEnding += GameLoop_DayEnding;
        Helper.Events.Content.AssetRequested += OnAssetRequested;
        Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        if (helper.ModRegistry.IsLoaded(CompatibilityKeys.SVE_MOD_ID))
        {
            Helper.Events.Player.Warped += Player_Warped;
        }
    }

    /// <summary>
    /// Runs on game launched. Mainly used for adding the Generic Mod Menu
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e">event args</param>
    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        _configHandler.InitGMCM();

        var api = this.Helper.ModRegistry.GetApi<IContentPatcherAPI>("Pathoschild.ContentPatcher");
        api?.RegisterToken(this.ModManifest, "CoucilMeetingDays", () =>
        {
            // save is loaded
            if (Context.IsWorldReady)
            {
                return [ModUtils.GetFormattedMeetingDays(Helper, _configHandler.ModConfig)];
            }

            // no save loaded (e.g. on the title screen)
            return null;
        });
    }

    /// <summary>
    /// Loads the save game data
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e">event args</param>
    private void GameLoop_SaveLoaded(object? sender, SaveLoadedEventArgs e)
    {

        if (Helper is not null && Helper.Data is not null)
        {
            var saveData = Helper.Data.ReadSaveData<MayorModData>(ModKeys.SAVE_KEY);
            if (saveData is not null)
            {
                _saveData = saveData;
            }
        }

        if (ModProgressHandler.HasProgressFlag(ProgressFlags.CleanUpRivers))
        {
            riverCleanUpRunOnce = false;
        }

        _assetInvalidationHandler.UpdateAssetInvalidations();
    }

    /// <summary>
    /// Sets the flag for VotingDay and invalidates the cache if needed
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e">event args</param>
    private void GameLoop_DayStarted(object? sender, DayStartedEventArgs e)
    {
        if (_saveData is not null && ModProgressHandler.HasProgressFlag(ProgressFlags.RunningForMayor))
        {
            if (_saveData.VotingDate == SDate.Now())
            {
                ModProgressHandler.AddProgressFlag(ProgressFlags.IsVotingDay);
            }
            else if (_saveData.VotingDate.AddDays(-1) == SDate.Now())
            {
                Game1.MasterPlayer.mailbox.Add($"{ModKeys.MAYOR_MOD_CPID}_VoteTomorrowMail");
            }
        }

        _assetInvalidationHandler.InvalidateModDataIfNeeded();

        if (ModProgressHandler.HasProgressFlag(ProgressFlags.ElectedAsMayor))
        {
            //Set if council day
            var day = (int)WorldDate.GetDayOfWeekFor(Game1.dayOfMonth);
            ModProgressHandler.RemoveProgressFlag(ProgressFlags.IsCouncilDay);
            if (_configHandler.ModConfig.MeetingDays[day])
            {
                ModProgressHandler.AddProgressFlag(ProgressFlags.IsCouncilDay);
            }

            //Town cleanup
            if (ModProgressHandler.HasProgressFlag(ProgressFlags.TownCleanup) &&
                !NetWorldState.checkAnywhereForWorldStateID(ProgressFlags.CompleteTrashBearWorldState))
            {
                NetWorldState.addWorldStateIDEverywhere(ProgressFlags.CompleteTrashBearWorldState);
            }

            //Security guard so no money loss on passout
            if (ModProgressHandler.HasProgressFlag(ProgressFlags.SecurityOnGuard))
            {
                LocationContexts.Default.MaxPassOutCost = 0;
            }
        }
    }

    /// <summary>
    /// Updates flags for mayor at end of day
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e">event args</param>
    private void GameLoop_DayEnding(object? sender, DayEndingEventArgs e)
    {
        //Set voting date
        if (ModProgressHandler.HasProgressFlag(ProgressFlags.RegisterVotingDate))
        {
            _saveData = new MayorModData()
            {
                VotingDate = ModUtils.GetDateWithoutFestival(_configHandler.ModConfig.NumberOfCampaignDays)
            };
            Helper.Data.WriteSaveData(ModKeys.SAVE_KEY, _saveData);
            ModProgressHandler.RemoveProgressFlag(ProgressFlags.RegisterVotingDate);
            _assetInvalidationHandler.UpdateAssetInvalidations();
        }

        //Complete voting day
        if (_saveData is not null && _saveData.VotingDate == SDate.Now() && ModProgressHandler.HasProgressFlag(ProgressFlags.RunningForMayor))
        {
            var voteManager = new VotingHandler(Game1.MasterPlayer, _configHandler.ModConfig);
            ModProgressHandler.RemoveAllModFlags();
            if (voteManager.HasWonElection(Helper))
            {
                ModProgressHandler.AddProgressFlag(ProgressFlags.WonMayorElection);
                Game1.MasterPlayer.mailbox.Add($"{ModKeys.MAYOR_MOD_CPID}_WonElectionMail");
                _assetInvalidationHandler.UpdateAssetInvalidations();
            }
            else
            {
                ModProgressHandler.AddProgressFlag(ProgressFlags.LostMayorElection);
            }
        }

        // Complete day as mayor
        if (ModProgressHandler.HasProgressFlag(ProgressFlags.ElectedAsMayor))
        {
            if (ModProgressHandler.HasProgressFlag(ProgressFlags.WonMayorElection))
            {
                ModProgressHandler.RemoveProgressFlag(ProgressFlags.WonMayorElection);
            }

            ModUtils.ForceCouncilMailDelivery();
            
            if (riverCleanUpRunOnce && ModProgressHandler.HasProgressFlag(ProgressFlags.CleanUpRivers))
            {
                riverCleanUpRunOnce = false;
                _assetInvalidationHandler.LocationCacheInvalidate = true;
            }
        }

        //Allow NeedMayorRetryEvent to repeat
        if (Game1.player.eventsSeen.Contains(ProgressFlags.NeedMayorRetryEvent))
        {
            Game1.player.eventsSeen.Remove(ProgressFlags.NeedMayorRetryEvent);
        }

        //Allow NeedMayorRetryEvent to repeat
        if (ModProgressHandler.HasProgressFlag(ProgressFlags.ModNeedsReset))
        {
            ResignAndReset();
        }
    }

    /// <summary>
    /// Does updates on player map change
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e">event args</param>
    private void Player_Warped(object? sender, WarpedEventArgs e)
    {
        if (e.NewLocation.NameOrUniqueName == nameof(Town) && ModProgressHandler.HasProgressFlag(ProgressFlags.IsVotingDay))
        {
            //Remove bushes on SVE town map
            e.NewLocation.terrainFeatures.RemoveWhere(tf => tf.Key.Equals(new Vector2(30, 34)));
            e.NewLocation.terrainFeatures.RemoveWhere(tf => tf.Key.Equals(new Vector2(31, 35)));
            
            //TODO Look into if this is better to use
            //Utility.clearObjectsInArea(bounds, gameLocation);
        }
    }

    /// <summary>
    /// Updates assets for dynamic assets that change depending on mod data
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e">event args</param>
    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (ModProgressHandler.HasProgressFlag(ProgressFlags.ElectedAsMayor))
        {
            _assetUpdateHandler.AssetSubstringPatch(e);
            if (e.NameWithoutLocale.IsEquivalentTo(XNBPathKeys.LOCATIONS) && ModProgressHandler.HasProgressFlag(ProgressFlags.CleanUpRivers))
            {
                _assetUpdateHandler.RemoveTrashFromRivers(e);
                return;
            }
        }

        if (_saveData is not null && ModProgressHandler.HasProgressFlag(ProgressFlags.RunningForMayor))
        {
            _assetUpdateHandler.RunningForMayorAssetUpdates(e, _saveData);
        }
    }


    /// <summary>
    /// Reset the mod to factory settings.
    /// </summary>
    void ResignAndReset()
    {
        Game1.MasterPlayer.mailReceived.RemoveWhere(m => m.Contains(ModKeys.MAYOR_MOD_CPID)); 
        Game1.MasterPlayer.mailbox.RemoveWhere(m => m.Contains(ModKeys.MAYOR_MOD_CPID));
        Game1.MasterPlayer.mailForTomorrow.RemoveWhere(m => m.Contains(ModKeys.MAYOR_MOD_CPID));
        Game1.MasterPlayer.eventsSeen.RemoveWhere(m => m.Contains(ModKeys.MAYOR_MOD_CPID));
        Game1.MasterPlayer.activeDialogueEvents.RemoveWhere((m) => m.Key.Contains(ModKeys.MAYOR_MOD_CPID));
        Game1.MasterPlayer.previousActiveDialogueEvents.RemoveWhere((m) => m.Key.Contains(ModKeys.MAYOR_MOD_CPID));

        Helper.GameContent.InvalidateCache(XNBPathKeys.MAIL);
        Helper.GameContent.InvalidateCache(XNBPathKeys.PASSIVE_FESTIVALS);
        Helper.GameContent.InvalidateCache(XNBPathKeys.LOCATIONS);
        Helper.GameContent.InvalidateCache(XNBPathKeys.CHARACTERS);
    }
}