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
    private bool _modDataCacheInvalidationNeeded;

    /// <summary>
    /// The mod entry point, called after the mod is first loaded.
    /// </summary>
    /// <param name="helper">Provides simplified APIs for writing mods.</param>
    public override void Entry(IModHelper helper)
    {
        TileActionManager.Init(Helper, Monitor);
        Phone.PhoneHandlers.Add(new PollingDataHandler(Helper));
        EventCommands.AddExtraEventCommands(Monitor);

        Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
        Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
        Helper.Events.GameLoop.DayEnding += GameLoop_DayEnding;
        Helper.Events.Content.AssetRequested += OnAssetRequested;
        if (helper.ModRegistry.IsLoaded(ModKeys.SVE_MOD_ID))
        {
            Helper.Events.Player.Warped += Player_Warped;
        }
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
                VotingDate = ModUtils.GetDateWithoutFestival(ModKeys.CAMPAIGN_DURATION)
            };
            Helper.Data.WriteSaveData(ModKeys.SAVE_KEY, _saveData);
            ModProgressManager.RemoveProgressFlag(ProgressFlags.RegisterVotingDate);
            _modDataCacheInvalidationNeeded = true;
        }

        //Complete voting day
        if (_saveData is not null && _saveData.VotingDate == SDate.Now() && ModProgressManager.HasProgressFlag(ProgressFlags.RunningForMayor))
        {
            var pd = new VotingManager(Game1.MasterPlayer);
            ModProgressManager.RemoveAllModFlags();
            if (pd.HasWonElection(Helper))
            {
                ModProgressManager.AddProgressFlag(ProgressFlags.WonMayorElection);
                Game1.MasterPlayer.mailbox.Add($"{ModKeys.MAYOR_MOD_CPID}_WonElectionMail");
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
    }

    /// <summary>
    /// Does updates on player map change
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e">event args</param>
    private void Player_Warped(object? sender, WarpedEventArgs e)
    {
        if (e.NewLocation.NameOrUniqueName == nameof(Town))
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
        if (ModProgressManager.HasProgressFlag(ProgressFlags.ElectedAsMayor) && e.NameWithoutLocale.StartsWith(XNBPathKeys.EVENTS))
        {
            e.Edit(AssetUpdatesForEvents);
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
    /// Updates event assets when farmer is mayor
    /// </summary>
    /// <param name="events">event data</param>
    private void AssetUpdatesForEvents(IAssetData events)
    {
        if (events.NameWithoutLocale.StartsWith(XNBPathKeys.TOWN_EVENTS))
        {
            var data = events.AsDictionary<string, string>().Data;
            var key = data.Keys.FirstOrDefault(k => k.Contains("191393"));
            if (key is not null)
            {
                data[key] = data[key].Replace("Lewis", "Governor_NewMayorEvent")
                                     .Replace("broadcastEvent/", "broadcastEvent/addTemporaryActor Governor_NewMayorEvent 16 32 52 33 0 true Character/")
                                     .Replace("Governor_NewMayorEvent 52 33 0", "Governor 1000 1000 0")
                                     .Replace("speak Governor_NewMayorEvent", "speak Governor");
            }
        }
        if (events.NameWithoutLocale.StartsWith(XNBPathKeys.CC_EVENTS))
        {
            var data = events.AsDictionary<string, string>().Data;
            data["Punch"] = data["Punch"].Replace("Lewis", "Governor");
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
        if (!dialogues.AsDictionary<string, string>().Data.ContainsKey($"AcceptGift_(O){ModItemKeys.Leaflet}") &&
            !dialogues.AsDictionary<string, string>().Data.ContainsKey($"RejectItem_(O){ModItemKeys.Leaflet}"))
        {
            var data = dialogues.AsDictionary<string, string>().Data;
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
}
