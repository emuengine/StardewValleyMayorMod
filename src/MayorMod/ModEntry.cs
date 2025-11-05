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
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.Objects;
using System.Xml.Linq;

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
    private bool _riverCleanUpRunOnce = true;
    

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
        HarmonyHandler.Init(ModManifest);

        Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
        Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
        Helper.Events.GameLoop.DayEnding += GameLoop_DayEnding;
        Helper.Events.Content.AssetRequested += OnAssetRequested;
        Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        if (helper.ModRegistry.IsLoaded(CompatibilityKeys.SVE_MOD_ID))
        {
            Helper.Events.Player.Warped += Player_Warped;
        }
        Helper.ConsoleCommands.Add(ModKeys.RESET_COMMAND, ModKeys.RESET_COMMAND_HELP_TEXT, (arg1, arg2) => ResignAndReset());
    }

    /// <summary>
    /// Runs on game launched. Adds GMCM config and registers tokens.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e">event args</param>
    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        _configHandler.InitGMCM();
        RegisterTokens();
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
                HarmonyHandler.MMData = _saveData;
            }
        }

        if (ModProgressHandler.HasProgressFlag(ProgressFlags.CleanUpRivers))
        {
            _riverCleanUpRunOnce = false;
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
            ModProgressHandler.RemoveProgressFlag(ProgressFlags.WonMayorElection);
            ModProgressHandler.RemoveProgressFlag(CouncilMeetingKeys.NotToday);
            
            if (_riverCleanUpRunOnce && ModProgressHandler.HasProgressFlag(ProgressFlags.CleanUpRivers))
            {
                _riverCleanUpRunOnce = false;
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

        if (e.NewLocation.NameOrUniqueName == "AnimalShop" && ModProgressHandler.HasProgressFlag(ProgressFlags.ElectedAsMayor))
        {
            if (e.NewLocation.farmers.Count == 0)
            {
                e.NewLocation.characters.Clear();
            }
            Vector2 shortsTile = new(11f, 7f);
            e.NewLocation.overlayObjects.Remove(shortsTile);
            var o = ItemRegistry.Create<StardewValley.Object>("(O)789");
            o.questItem.Value = true;
            o.TileLocation = shortsTile;
            o.IsSpawnedObject = true;
            e.NewLocation.overlayObjects.Add(shortsTile, o);
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
            if (e.NameWithoutLocale.StartsWith(XNBPathKeys.DIALOGUE))
            {
                _assetUpdateHandler.AssetUpdatesForLeafletDialogue(e);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(XNBPathKeys.PASSIVE_FESTIVALS))
            {
                _assetUpdateHandler.AssetUpdatesForPassiveFestivals(e, _saveData.VotingDate);
            }
        }
    }

    /// <summary>
    /// Reset the mod to factory settings.
    /// </summary>
    private void ResignAndReset()
    {
        if (!Context.IsWorldReady)
        {
            Monitor.Log("Can't reset in while not in game.", LogLevel.Info);
            return;
        }

        Monitor.Log("Reseting mod to fresh install.", LogLevel.Info);

        Monitor.Log("Removing mayor mod flags.", LogLevel.Info);
        Game1.MasterPlayer.mailReceived.RemoveWhere(m => m.Contains(ModKeys.MAYOR_MOD_CPID)); 
        Game1.MasterPlayer.mailbox.RemoveWhere(m => m.Contains(ModKeys.MAYOR_MOD_CPID));
        Game1.MasterPlayer.mailForTomorrow.RemoveWhere(m => m.Contains(ModKeys.MAYOR_MOD_CPID));

        Monitor.Log("Removing mayor mod from events seen.", LogLevel.Info);
        Game1.MasterPlayer.eventsSeen.RemoveWhere(m => m.Contains(ModKeys.MAYOR_MOD_CPID));
        Game1.MasterPlayer.activeDialogueEvents.RemoveWhere(m => m.Key.Contains(ModKeys.MAYOR_MOD_CPID));
        Game1.MasterPlayer.previousActiveDialogueEvents.RemoveWhere(m => m.Key.Contains(ModKeys.MAYOR_MOD_CPID));
        foreach (var item in Game1.MasterPlayer.giftedItems)
        {
            item.Value.RemoveWhere(g => g.Key.Contains(ModKeys.MAYOR_MOD_CPID));
        }

        if(_saveData is not null)
        {
            Monitor.Log("Clearing voting day data.", LogLevel.Info);
            _saveData.VotingDate = new SDate(1, Season.Spring);
            Helper.Data.WriteSaveData(ModKeys.SAVE_KEY, _saveData);
        }

        Monitor.Log("Invalidating cache for patched assets.", LogLevel.Info);
        Helper.GameContent.InvalidateCache(XNBPathKeys.MAIL);
        Helper.GameContent.InvalidateCache(XNBPathKeys.PASSIVE_FESTIVALS);
        Helper.GameContent.InvalidateCache(XNBPathKeys.LOCATIONS);
        Helper.GameContent.InvalidateCache(XNBPathKeys.CHARACTERS);

        Monitor.Log("Done.", LogLevel.Info);
        Monitor.Log("Please go to sleep and save and the mod should be reset.", LogLevel.Info);
    }

    /// <summary>
    /// Registers tokens for content patcher
    /// </summary>
    private void RegisterTokens()
    {
        var api = this.Helper.ModRegistry.GetApi<IContentPatcherAPI>("Pathoschild.ContentPatcher");
        api?.RegisterToken(this.ModManifest, ModKeys.MEETING_DAYS_TOKEN, () =>
        {
            return Context.IsWorldReady ? new List<string>() { ModUtils.GetFormattedMeetingDays(Helper, _configHandler.ModConfig) } : null;
        });
        api?.RegisterToken(this.ModManifest, ModKeys.VOTING_SEASON_TOKEN, () =>
        {
            return _saveData is not null ? new List<string>() { $"{_saveData.VotingDate.Season}" } : null;
        });
        api?.RegisterToken(this.ModManifest, ModKeys.VOTING_DAY_TOKEN, () =>
        {
            return _saveData is not null ? new List<string>() { $"{_saveData.VotingDate.Day}" } : null;
        });
    }
}