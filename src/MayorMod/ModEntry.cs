using MayorMod.Constants;
using MayorMod.Data;
using MayorMod.Data.Models;
using MayorMod.Data.TileActions;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.GameData;

namespace MayorMod;

/// <summary>
/// The mod entry point.
/// </summary>
internal sealed class ModEntry : Mod
{
#pragma warning disable CS8618
    private MayorModData _saveData = new();
    private TileActionManager _tileActions;
    private bool _modDataCacheInvalidationNeeded;
#pragma warning restore CS8618

    /// <summary>
    /// The mod entry point, called after the mod is first loaded.
    /// </summary>
    /// <param name="helper">Provides simplified APIs for writing mods.</param>
    public override void Entry(IModHelper helper)
    {
        _tileActions = new TileActionManager(Monitor);

        Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
        Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
        Helper.Events.GameLoop.DayEnding += GameLoop_DayEnding;
        Helper.Events.Content.AssetRequested += OnAssetRequested;
        Helper.Events.Input.ButtonPressed += OnButtonPressed;
    }

    private void GameLoop_SaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        _modDataCacheInvalidationNeeded = true;

        if (Helper is not null && Helper.Data is not null)
        {
            var saveData = Helper.Data.ReadSaveData<MayorModData>(ModKeys.MayorModSaveKey);
            if (saveData is not null)
            {
                _saveData = saveData;
            }
        }
    }

    private void GameLoop_DayStarted(object? sender, DayStartedEventArgs e)
    {
        if (_saveData is not null && ModProgressManager.HasProgressFlag(ModProgressManager.RunningForMayor))
        {
            if (_saveData.VotingDate == SDate.Now())
            {
                ModProgressManager.AddProgressFlag(ModProgressManager.IsVotingDay);
            }
        }

        if (_modDataCacheInvalidationNeeded)
        {
            InvalidateModData();
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
        Helper.GameContent.InvalidateCache("Data/Mail");
        Helper.GameContent.InvalidateCache("Data/PassiveFestivals");
        Game1.PerformPassiveFestivalSetup();
        Game1.UpdatePassiveFestivalStates();
        _modDataCacheInvalidationNeeded = false;
    }

    private void GameLoop_DayEnding(object? sender, DayEndingEventArgs e)
    {
        //Set voting date
        if (ModProgressManager.HasProgressFlag(ModProgressManager.RegisterVotingDate))
        {
            _saveData = new MayorModData()
            {
                VotingDate = ModUtils.GetDateWithoutFestival(10)
            };
            Helper.Data.WriteSaveData(ModKeys.MayorModSaveKey, _saveData);
            ModProgressManager.RemoveProgressFlag(ModProgressManager.RegisterVotingDate);
            _modDataCacheInvalidationNeeded = true;
        }

        //Complete voting day
        if (_saveData is not null && _saveData.VotingDate == SDate.Now() && ModProgressManager.HasProgressFlag(ModProgressManager.RunningForMayor))
        {
            //TODO: Make it so you can lose election but for now just assume you win
            ModProgressManager.RemoveAllModFlags();
            ModProgressManager.AddProgressFlag(ModProgressManager.WonMayorElection);
        }

        // Complete day as mayor
        if (ModProgressManager.HasProgressFlag(ModProgressManager.ElectedAsMayor))
        {
            if (ModProgressManager.HasProgressFlag(ModProgressManager.WonMayorElection))
            {
                ModProgressManager.RemoveProgressFlag(ModProgressManager.WonMayorElection);
            }

            ModUtils.ForceCouncilMailDelivery();
        }
    }

    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (_saveData is null || !ModProgressManager.HasProgressFlag(ModProgressManager.RunningForMayor))
        {
            return;
        }

        if (e.NameWithoutLocale.IsEquivalentTo("Data/Mail"))
        {
            e.Edit(AssetUpdatesForMail);
        }
        else if (e.NameWithoutLocale.IsEquivalentTo("Data/PassiveFestivals"))
        {
            e.Edit(AssetUpdatesForPassiveFestivals);
        }
        else if (e.NameWithoutLocale.StartsWith("Characters/Dialogue"))
        {
            e.Edit(AssetUpdatesForDialogue);
        }
    }

    private void AssetUpdatesForMail(IAssetData mails)
    {
        var data = mails.AsDictionary<string, string>().Data;
        var title = ModUtils.GetTranslationForKey(Helper, $"{ModKeys.MayorModCPId}_Mail.RegistrationMail.Title");
        var body = ModUtils.GetTranslationForKey(Helper, $"{ModKeys.MayorModCPId}_Mail.RegistrationMail.Body");
        body = string.Format(body, $"{_saveData.VotingDate.Season} {_saveData.VotingDate.Day}");
        data[$"{ModKeys.MayorModCPId}_RegisteredForElectionMail"] = $"{body}[#]{title}";

        //TODO Add mail the day before voting day
    }

    private void AssetUpdatesForPassiveFestivals(IAssetData festivals)
    {
        var data = festivals.AsDictionary<string, PassiveFestivalData>().Data;
        var votingDay = new PassiveFestivalData()
        {
            DisplayName = ModUtils.GetTranslationForKey(Helper, $"{ModKeys.MayorModCPId}_Festival.VotingDay.Name"),
            StartMessage = ModUtils.GetTranslationForKey(Helper, $"{ModKeys.MayorModCPId}_Festival.VotingDay.Message"),
            Season = _saveData.VotingDate.Season,
            StartDay = _saveData.VotingDate.Day,
            EndDay = _saveData.VotingDate.Day,
            StartTime = 610,
            ShowOnCalendar = true,
        };
        data[$"{ModKeys.MayorModCPId}_VotingDayPassiveFestival"] = votingDay;
    }

    private void AssetUpdatesForDialogue(IAssetData dialogues)
    {
        if (!dialogues.AsDictionary<string, string>().Data.ContainsKey($"AcceptGift_(O){ModItemKeys.Leaflet}") &&
            !dialogues.AsDictionary<string, string>().Data.ContainsKey($"RejectItem_(O){ModItemKeys.Leaflet}"))
        {
            var data = dialogues.AsDictionary<string, string>().Data;
            data[$"RejectItem_(O){ModItemKeys.Leaflet}"] = ModUtils.GetTranslationForKey(Helper, $"{ModKeys.MayorModCPId}_Gifting.Default.Leaflet");
        }
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        //if (e.Button.IsActionButton())
        //{
        //    if (Game1.player.ActiveItem is not null && Game1.player.ActiveItem.ItemId == ModItemKeys.ElectionSign)
        //    {
        //        //item placed add percentage for vote
        //    }

        //    var n = HelperMethods.GetNPCForPlayerInteraction();
        //}
    }
}
