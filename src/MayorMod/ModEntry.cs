using MayorMod.Constants;
using MayorMod.Data;
using MayorMod.Data.Handlers;
using MayorMod.Data.Interfaces;
using MayorMod.Data.Utilities;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Network;
using StardewValley.Objects;

namespace MayorMod;

/// <summary>
/// The mod entry point.
/// </summary>
internal sealed class ModEntry : Mod
{    
    /// <summary>
    /// The mod entry point, called after the mod is first loaded.
    /// </summary>
    /// <param name="helper">Provides simplified APIs for writing mods.</param>
    public override void Entry(IModHelper helper)
    {
        AssetUpdateHandler.Init(this);
        TaxHandler.Init();
        SaveHandler.Init(this);
        ModProgressHandler.Init(this);
        AssetInvalidationHandler.Init(this);
        TileActionHandler.Init(this);
        Phone.PhoneHandlers.Add(new PollingDataHandler(this));
        EventCommandHandler.Init(this);
        HarmonyHandler.Init(this);

        Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
        Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
        Helper.Events.GameLoop.DayEnding += GameLoop_DayEnding;
        Helper.Events.Content.AssetRequested += OnAssetRequested;
        Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        if (helper.ModRegistry.IsLoaded(CompatibilityKeys.SVE_MOD_ID))
        {
            Helper.Events.Player.Warped += Player_Warped;
        }
        Helper.ConsoleCommands.Add(ModKeys.RESET_COMMAND, ModKeys.RESET_COMMAND_HELP_TEXT, (arg1, arg2) => ModProgressHandler.ResignAndReset());
    }

    /// <summary>
    /// Runs on game launched. Adds GMCM config and registers tokens.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e">event args</param>
    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        ModConfigHandler.Init(this);
        RegisterTokens();
    }

    /// <summary>
    /// Loads the save game data
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e">event args</param>
    private void GameLoop_SaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        if (Game1.IsMasterGame)
        {
            SaveHandler.LoadSaveData();
        }
        AssetInvalidationHandler.InvalidationNPCSchedules();

        if (ModProgressHandler.HasProgressFlag(ProgressFlags.CleanUpRivers))
        {
            ModProgressHandler.RiverCleanUpRunOnce = false;
        }

        AssetInvalidationHandler.UpdateAssetInvalidations();
    }

    /// <summary>
    /// Sets the flag for VotingDay and invalidates the cache if needed
    /// </summary>
    /// <param name="sender"></param> 
    /// <param name="e">event args</param>
    private void GameLoop_DayStarted(object? sender, DayStartedEventArgs e)
    {
        AssetInvalidationHandler.InvalidateModDataIfNeeded();

        if (ModProgressHandler.HasProgressFlag(ProgressFlags.ElectedAsMayor))
        {
            //Set if council day
            var day = (int)WorldDate.GetDayOfWeekFor(Game1.dayOfMonth);
            ModProgressHandler.RemoveProgressFlag(ProgressFlags.IsCouncilDay);
            if (ModConfigHandler.ModConfig.MeetingDays[day])
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
        ModProgressHandler.CampaignProgressUpdates();
        ModProgressHandler.CampaignProgressUpdatesFarmhand();
        ModProgressHandler.DayAsMayorUpdates();
        ModProgressHandler.UpdateIfMayorRetryNeeded();
        ModProgressHandler.HandleModResetIfNeeded();
    }

    /// <summary>
    /// Does updates on player map change
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e">event args</param>
    private void Player_Warped(object? sender, WarpedEventArgs e)
    {
        ModProgressHandler.RemoveTownBushesOnVotingDay(e.NewLocation);
        ModProgressHandler.AddLewisShortsToAnimalShop(e.NewLocation);
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
            AssetUpdateHandler.AssetSubstringPatch(e);
            if (e.NameWithoutLocale.IsEquivalentTo(XNBPathKeys.LOCATIONS) && ModProgressHandler.HasProgressFlag(ProgressFlags.CleanUpRivers))
            {
                AssetUpdateHandler.RemoveTrashFromRivers(e);
                return;
            }
        }

        if (ModProgressHandler.HasHostGotProgressFlag(ProgressFlags.RunningForMayor))
        {
            if (e.NameWithoutLocale.StartsWith(XNBPathKeys.DIALOGUE))
            {
                AssetUpdateHandler.AssetUpdatesForLeafletDialogue(e);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(XNBPathKeys.PASSIVE_FESTIVALS))
            {
                AssetUpdateHandler.AssetUpdatesForPassiveFestivals(e);
            }
        }
    }

    /// <summary>
    /// Registers tokens for content patcher
    /// </summary>
    private void RegisterTokens()
    {
        var api = this.Helper.ModRegistry.GetApi<IContentPatcherAPI>("Pathoschild.ContentPatcher");
        api?.RegisterToken(this.ModManifest, ModKeys.MEETING_DAYS_TOKEN, () =>
        {
            return Context.IsWorldReady ? new List<string>() { ModUtils.GetFormattedMeetingDays(Helper, ModConfigHandler.ModConfig) } : null;
        });
        api?.RegisterToken(this.ModManifest, ModKeys.VOTING_DAY_TOKEN, () =>
        {
            var voteDate = ModUtils.GetVotingDate();
            return voteDate is not null ? new List<string>() { $"{voteDate.Day} {voteDate.Season}" } : new List<string>() { "NOT LOADED" };
        });
        api?.RegisterToken(this.ModManifest, ModKeys.VOTING_RESULT_TOKEN, () =>
        {            
            var result = new VotingHandler(Game1.MasterPlayer, ModConfigHandler.ModConfig).GetVotingResultText(Helper);
            return !string.IsNullOrEmpty(result) ? new List<string>() { result } : null;
        });
        api?.RegisterToken(this.ModManifest, ModKeys.DEBATE_DAY_TOKEN, () =>
        {
            var voteDate = ModUtils.GetVotingDate();
            var debateDate = voteDate?.AddDays(-1 * ModKeys.DEBATE_DAY_OFFSET);
            return debateDate is not null ? new List<string>() { $"{debateDate.Day} {debateDate.Season}" } : new List<string>() { "NOT LOADED" };
        });        
    }
}