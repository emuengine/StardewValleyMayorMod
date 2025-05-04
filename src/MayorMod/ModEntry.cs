using MayorMod.Constants;
using MayorMod.Data;
using MayorMod.Data.Models;
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
    private MayorModData _saveData;

    /// <summary>
    /// The mod entry point, called after the mod is first loaded.
    /// </summary>
    /// <param name="helper">Provides simplified APIs for writing mods.</param>
    public override void Entry(IModHelper helper)
    {
        TileActions.Init(Monitor);

        Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
        Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
        Helper.Events.GameLoop.DayEnding += GameLoop_DayEnding;
        Helper.Events.Content.AssetRequested += OnAssetRequested;
        Helper.Events.Input.ButtonPressed += OnButtonPressed;
    }

    private void GameLoop_SaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        _saveData = base.Helper.Data.ReadSaveData<MayorModData>(ModKeys.MayorModSaveKey);
        _saveData ??= new MayorModData();
    }

    private void GameLoop_DayStarted(object? sender, DayStartedEventArgs e)
    {
        if (Game1.MasterPlayer.mailReceived.Contains(ModProgressKeys.RegisteringForBalot))
        {
            Game1.MasterPlayer.mailReceived.Remove(ModProgressKeys.RegisteringForBalot);
        }

        if (_saveData is not null && _saveData.RunningForMayor)
        {
            if (_saveData.VotingDate == SDate.Now())
            {
                Game1.MasterPlayer.mailReceived.Add(ModProgressKeys.IsVotingDay);
            }

            //PassiveFestivals are loaded before the damn save data so we need to
            //reload them to make the variable date passive festivals show.
            Helper.GameContent.InvalidateCache("Data/Mail"); 
            Helper.GameContent.InvalidateCache("Data/PassiveFestivals");
            Game1.PerformPassiveFestivalSetup();
            Game1.UpdatePassiveFestivalStates();
        }
    }

    private void GameLoop_DayEnding(object? sender, DayEndingEventArgs e)
    {
        //Set voting date
        if (Game1.MasterPlayer.mailReceived.Contains(ModProgressKeys.RegisteringForBalot))
        {
            _saveData.RunningForMayor = true;
            _saveData.VotingDate = HelperMethods.GetDateWithoutFestival(10);
            Helper.Data.WriteSaveData(ModKeys.MayorModSaveKey, _saveData);
        }

        //End of first day as mayor
        if (Game1.MasterPlayer.mailReceived.Contains(ModProgressKeys.ManorHouseUnderConstruction))
        {
            Game1.MasterPlayer.mailReceived.Remove(ModProgressKeys.ManorHouseUnderConstruction);
            Game1.MasterPlayer.mailReceived.Add(ModProgressKeys.ElectedAsMayor);
        }

        //End of voting day
        if (_saveData is not null && _saveData.RunningForMayor && _saveData.VotingDate == SDate.Now())
        {
            Game1.MasterPlayer.mailReceived.Remove(ModProgressKeys.IsVotingDay);
            _saveData.RunningForMayor = false;
            //TODO Make it so you can lose election but for now just assume you win
            Game1.MasterPlayer.mailReceived.Add(ModProgressKeys.ManorHouseUnderConstruction);
            _saveData.ElectedMayor = true;
            Helper.Data.WriteSaveData(ModKeys.MayorModSaveKey, _saveData);
        }
    }

    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (_saveData is null || !_saveData.RunningForMayor)
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
        var title = HelperMethods.GetTranslationForKey(Helper, $"{ModKeys.MayorModCPId}_Mail.RegistrationMail.Title");
        var body = HelperMethods.GetTranslationForKey(Helper, $"{ModKeys.MayorModCPId}_Mail.RegistrationMail.Body");
        body = string.Format(body, $"{_saveData.VotingDate.Season} {_saveData.VotingDate.Day}");
        data[$"{ModKeys.MayorModCPId}_RegisteredForElectionMail"] = $"{body}[#]{title}";

        //TODO Add mail the day before voting day
    }

    private void AssetUpdatesForPassiveFestivals(IAssetData festivals)
    {
        var data = festivals.AsDictionary<string, PassiveFestivalData>().Data;
        var votingDay = new PassiveFestivalData()
        {
            DisplayName = HelperMethods.GetTranslationForKey(Helper, $"{ModKeys.MayorModCPId}_Festival.VotingDay.Name"),
            StartMessage = HelperMethods.GetTranslationForKey(Helper, $"{ModKeys.MayorModCPId}_Festival.VotingDay.Message"),
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
            data[$"RejectItem_(O){ModItemKeys.Leaflet}"] = HelperMethods.GetTranslationForKey(Helper, $"{ModKeys.MayorModCPId}_Gifting.Default.Leaflet");
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
