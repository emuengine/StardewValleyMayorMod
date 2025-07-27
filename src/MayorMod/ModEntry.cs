using GenericModConfigMenu;
using MayorMod.Constants;
using MayorMod.Data;
using MayorMod.Data.Models;
using MayorMod.Data.TileActions;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.GameData;
using StardewValley.GameData.Locations;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.Objects;
using System.Text.Json;

namespace MayorMod;

/// <summary>
/// The mod entry point.
/// </summary>
internal sealed class ModEntry : Mod
{
    private MayorModData _saveData = new();
    private MayorModConfig _mayorModConfig = new();
    private bool _modDataCacheInvalidationNeeded;
    private Dictionary<string, Dictionary<string, List<StringPatch>>> _mayorStringReplacements = [];

    /// <summary>
    /// The mod entry point, called after the mod is first loaded.
    /// </summary>
    /// <param name="helper">Provides simplified APIs for writing mods.</param>
    public override void Entry(IModHelper helper)
    {
        var stringPatchLocation = Path.Join(helper.DirectoryPath, ModKeys.REPLACEMENT_STRING_CONFIG);
        if (!File.Exists(stringPatchLocation))
        {
            Monitor.Log($"Error: File not found {stringPatchLocation}", LogLevel.Error);
        }
        var stringPatchFile =  File.ReadAllText(stringPatchLocation);
        _mayorStringReplacements = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, List<StringPatch>>>>(stringPatchFile) ?? [];
        _mayorModConfig = Helper.ReadConfig<MayorModConfig>();

        TileActionManager.Init(Helper, Monitor);
        Phone.PhoneHandlers.Add(new PollingDataHandler(Helper, _mayorModConfig));
        EventCommands.AddExtraEventCommands(Monitor);

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
        InitGMCM();
    }

    /// <summary>
    /// Loads the save game data
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e">event args</param>
    private void GameLoop_SaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        _modDataCacheInvalidationNeeded = true;

        if (Helper is not null && Helper.Data is not null)
        {
            var saveData = Helper.Data.ReadSaveData<MayorModData>(ModKeys.SAVE_KEY);
            if (saveData is not null)
            {
                _saveData = saveData;
            }
        }
    }

    /// <summary>
    /// Sets the flag for VotingDay and invalidates the cache if needed
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e">event args</param>
    private void GameLoop_DayStarted(object? sender, DayStartedEventArgs e)
    {
        if (_saveData is not null && ModProgressManager.HasProgressFlag(ProgressFlags.RunningForMayor))
        {
            if (_saveData.VotingDate == SDate.Now())
            {
                ModProgressManager.AddProgressFlag(ProgressFlags.IsVotingDay);
            }
            else if (_saveData.VotingDate.AddDays(-1) == SDate.Now())
            {
                Game1.MasterPlayer.mailbox.Add($"{ModKeys.MAYOR_MOD_CPID}_VoteTomorrowMail");
            }
        }

        if (ModProgressManager.HasProgressFlag(ProgressFlags.ElectedAsMayor) && 
            ModProgressManager.HasProgressFlag(ProgressFlags.CleanUpRivers))
        {
            Helper.GameContent.InvalidateCache(XNBPathKeys.LOCATIONS);
        }

        if (_modDataCacheInvalidationNeeded)
        {
            InvalidateModData();
        }

        //Town cleanup
        if (ModProgressManager.HasProgressFlag(ProgressFlags.TownCleanup) && 
            !NetWorldState.checkAnywhereForWorldStateID(ProgressFlags.CompleteTrashBearWorldState))
        {
            NetWorldState.addWorldStateIDEverywhere(ProgressFlags.CompleteTrashBearWorldState);
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
        if (ModProgressManager.HasProgressFlag(ProgressFlags.RegisterVotingDate))
        {
            _saveData = new MayorModData()
            {
                VotingDate = ModUtils.GetDateWithoutFestival(_mayorModConfig.NumberOfCampaignDays)
            };
            Helper.Data.WriteSaveData(ModKeys.SAVE_KEY, _saveData);
            ModProgressManager.RemoveProgressFlag(ProgressFlags.RegisterVotingDate);
            _modDataCacheInvalidationNeeded = true;
        }

        //Complete voting day
        if (_saveData is not null && _saveData.VotingDate == SDate.Now() && ModProgressManager.HasProgressFlag(ProgressFlags.RunningForMayor))
        {
            var pd = new VotingManager(Game1.MasterPlayer, _mayorModConfig);
            ModProgressManager.RemoveAllModFlags();
            if (pd.HasWonElection(Helper))
            {
                ModProgressManager.AddProgressFlag(ProgressFlags.WonMayorElection);
                Game1.MasterPlayer.mailbox.Add($"{ModKeys.MAYOR_MOD_CPID}_WonElectionMail"); 
                _modDataCacheInvalidationNeeded = true;
            }
            else
            {
                ModProgressManager.AddProgressFlag(ProgressFlags.LostMayorElection);
            }
        }

        // Complete day as mayor
        if (ModProgressManager.HasProgressFlag(ProgressFlags.ElectedAsMayor))
        {
            if (ModProgressManager.HasProgressFlag(ProgressFlags.WonMayorElection))
            {
                ModProgressManager.RemoveProgressFlag(ProgressFlags.WonMayorElection);
            }

            ModUtils.ForceCouncilMailDelivery();
        }

        //Allow NeedMayorRetryEvent to repeat
        if (Game1.player.eventsSeen.Contains(ProgressFlags.NeedMayorRetryEvent))
        {
            Game1.player.eventsSeen.Remove(ProgressFlags.NeedMayorRetryEvent);
        }
    }

    /// <summary>
    /// Does updates on player map change
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e">event args</param>
    private void Player_Warped(object? sender, WarpedEventArgs e)
    {
        if (e.NewLocation.NameOrUniqueName == nameof(Town) && ModProgressManager.HasProgressFlag(ProgressFlags.IsVotingDay))
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
        if (ModProgressManager.HasProgressFlag(ProgressFlags.ElectedAsMayor))
        {
            AssetSubstringPatch(e);
            if (e.NameWithoutLocale.IsEquivalentTo(XNBPathKeys.LOCATIONS) && ModProgressManager.HasProgressFlag(ProgressFlags.CleanUpRivers))
            {
                e.Edit(RemoveTrashFromRivers);
                return;
            }
        }

        if (_saveData is null || !ModProgressManager.HasProgressFlag(ProgressFlags.RunningForMayor))
        {
            return;
        }

        if (e.NameWithoutLocale.IsEquivalentTo(XNBPathKeys.MAIL))
        {
            e.Edit(AssetUpdatesForMail);
        }
        else if (e.NameWithoutLocale.IsEquivalentTo(XNBPathKeys.PASSIVE_FESTIVALS))
        {
            e.Edit(AssetUpdatesForPassiveFestivals);
        }
        else if (e.NameWithoutLocale.StartsWith(XNBPathKeys.DIALOGUE))
        {
            e.Edit(AssetUpdatesForDialogue);
        }
    }

    /// <summary>
    /// Update the Locations default fish catch rate
    /// </summary>
    /// <param name="asset"></param>
    private void RemoveTrashFromRivers(IAssetData asset)
    {
        var data = asset.AsDictionary<string, LocationData>().Data;
        var rubbish = data["Default"].Fish.FirstOrDefault(f => f.Id == ModKeys.RUBBISH_ID);
        if (rubbish is not null)
        {
            rubbish.Chance = 0.01f;
        }
    }


    /// <summary>
    /// Replaces substrings in assets from patches loaded from json 
    /// NOTE: Can possibly be done by content patcher but I couldn't work out how.
    /// </summary>
    /// <param name="e"></param>
    private void AssetSubstringPatch(AssetRequestedEventArgs e)
    {
        try
        {
            if (e.NameWithoutLocale is not null &&
                e.NameWithoutLocale.BaseName is not null &&
                _mayorStringReplacements.ContainsKey(e.NameWithoutLocale.BaseName))
            {
                e.Edit((asset) =>
                {
                    var updates = _mayorStringReplacements[e.NameWithoutLocale.BaseName];
                    if (e.NameWithoutLocale.BaseName.Equals(XNBPathKeys.SECRET_NOTES, StringComparison.InvariantCultureIgnoreCase))
                    {
                        var data = asset.AsDictionary<int, string>().Data;
                        foreach (var key in updates.Keys)
                        {
                            if (Int32.TryParse(key, out int KeyInt) && data.ContainsKey(KeyInt))
                            {
                                data[KeyInt] = updates[key].Aggregate(data[KeyInt], (current, patch) => patch.PatchString(Helper, current));
                            }
                        }
                    }
                    else
                    {
                        var data = asset.AsDictionary<string, string>().Data;
                        foreach (var key in updates.Keys)
                        {
                            if (data.Keys.FirstOrDefault(k => k.StartsWith(key)) is { } keyMatch)
                            {
                                data[keyMatch] = updates[key].Aggregate(data[keyMatch], (current, patch) => patch.PatchString(Helper, current));
                            }
                        }
                    }
                }, AssetEditPriority.Late);
            }
        }
        catch (Exception ex)
        {
            Monitor.Log($"Failed to patch asset - {ex.Message}", LogLevel.Error);
        }
    }

    /// <summary>
    /// Updates mail assets that depend on voting day
    /// </summary>
    /// <param name="mails">mail data</param>
    private void AssetUpdatesForMail(IAssetData mails)
    {
        var data = mails.AsDictionary<string, string>().Data;
        var title = ModUtils.GetTranslationForKey(Helper, $"{ModKeys.MAYOR_MOD_CPID}_Mail.RegistrationMail.Title");
        var body = ModUtils.GetTranslationForKey(Helper, $"{ModKeys.MAYOR_MOD_CPID}_Mail.RegistrationMail.Body");
        body = string.Format(body, $"{_saveData.VotingDate.Season} {_saveData.VotingDate.Day}");
        data[$"{ModKeys.MAYOR_MOD_CPID}_RegisteredForElectionMail"] = $"{body}[#]{title}";

        //TODO Add mail the day before voting day
    }

    /// <summary>
    /// Updates Passive Festivals assets that depend on voting day
    /// </summary>
    /// <param name="festivals">festivals data</param>
    private void AssetUpdatesForPassiveFestivals(IAssetData festivals)
    {
        var data = festivals.AsDictionary<string, PassiveFestivalData>().Data;
        var votingDay = new PassiveFestivalData()
        {
            DisplayName = ModUtils.GetTranslationForKey(Helper, $"{ModKeys.MAYOR_MOD_CPID}_Festival.VotingDay.Name"),
            StartMessage = ModUtils.GetTranslationForKey(Helper, $"{ModKeys.MAYOR_MOD_CPID}_Festival.VotingDay.Message"),
            Season = _saveData.VotingDate.Season,
            StartDay = _saveData.VotingDate.Day,
            EndDay = _saveData.VotingDate.Day,
            StartTime = 610,
            ShowOnCalendar = true,
        };
        data[$"{ModKeys.MAYOR_MOD_CPID}_VotingDayPassiveFestival"] = votingDay;
    }

    /// <summary>
    /// Updates dialogue so that all characters not specifically designated will reject election leaflets
    /// </summary>
    /// <param name="dialogues">dialogues data</param>
    private void AssetUpdatesForDialogue(IAssetData dialogues)
    {
        var data = dialogues.AsDictionary<string, string>().Data;
        if (!data.ContainsKey($"AcceptGift_(O){ModItemKeys.Leaflet}") &&
            !data.ContainsKey($"RejectItem_(O){ModItemKeys.Leaflet}"))
        {
            data[$"RejectItem_(O){ModItemKeys.Leaflet}"] = ModUtils.GetTranslationForKey(Helper, $"{ModKeys.MAYOR_MOD_CPID}_Gifting.Default.Leaflet");
        }
    }

    /// <summary>
    /// This invalidates the cache so that the dates for voting are correct in mail and passive festivals
    /// PassiveFestivals are loaded before the damn save data so we need to reload them to make the
    /// variable date passive festivals show. They also don't seem to reload between loading saves
    /// so you can have the voting day appear in other saves even though you're not running for mayor.
    /// </summary>
    public void InvalidateModData()
    {
        Helper.GameContent.InvalidateCache(XNBPathKeys.MAIL);
        Helper.GameContent.InvalidateCache(XNBPathKeys.PASSIVE_FESTIVALS);
        Game1.PerformPassiveFestivalSetup();
        Game1.UpdatePassiveFestivalStates();
        _modDataCacheInvalidationNeeded = false;
    }


    /// <summary>
    /// Setup Generic Mod Config Menu
    /// </summary>
    private void InitGMCM()
    {
        var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>(ModKeys.CONFIG_MENU_ID);
        if (configMenu is null)
        {
            return;
        }

        configMenu.Register(
            mod: this.ModManifest,
            reset: () => this._mayorModConfig = new MayorModConfig(),
            save: () => this.Helper.WriteConfig(this._mayorModConfig)
        );


        configMenu.AddSectionTitle(
            mod: this.ModManifest,
            text: () => ModUtils.GetTranslationForKey(Helper, $"{ModKeys.MAYOR_MOD_CPID}_UIMenu.VotingOptions")
            //tooltip: () => ModUtils.GetTranslationForKey(Helper, $"{ModKeys.MAYOR_MOD_CPID}_UIMenu.VotingOptions.Tooltip")
        );

        configMenu.AddNumberOption(
            mod: this.ModManifest,
            name: () => ModUtils.GetTranslationForKey(Helper, $"{ModKeys.MAYOR_MOD_CPID}_UIMenu.VoteThreshold"),
            tooltip: () => ModUtils.GetTranslationForKey(Helper, $"{ModKeys.MAYOR_MOD_CPID}_UIMenu.VoteThreshold.Tooltip"),
            getValue: () => this._mayorModConfig.ThresholdForVote,
            setValue: value => this._mayorModConfig.ThresholdForVote = value,
            min: 0,
            max: 15,
            interval: 1
        );

        configMenu.AddNumberOption(
            mod: this.ModManifest,
            name: () => ModUtils.GetTranslationForKey(Helper, $"{ModKeys.MAYOR_MOD_CPID}_UIMenu.VoterPercentageNeeded"),
            tooltip: () => ModUtils.GetTranslationForKey(Helper, $"{ModKeys.MAYOR_MOD_CPID}_UIMenu.VoterPercentageNeeded.Tooltip"),
            getValue: () => this._mayorModConfig.VoterPercentageNeeded,
            setValue: value => this._mayorModConfig.VoterPercentageNeeded = value,
            min: 0,
            max: 100,
            interval: 1
        );

        configMenu.AddNumberOption(
            mod: this.ModManifest,
            name: () => ModUtils.GetTranslationForKey(Helper, $"{ModKeys.MAYOR_MOD_CPID}_UIMenu.NumberOfCampaignDays"),
            tooltip: () => ModUtils.GetTranslationForKey(Helper, $"{ModKeys.MAYOR_MOD_CPID}_UIMenu.NumberOfCampaignDays.Tooltip"),
            getValue: () => this._mayorModConfig.NumberOfCampaignDays,
            setValue: value => this._mayorModConfig.NumberOfCampaignDays = value,
            min: 0,
            max: 100,
            interval: 1
        );
    }
}