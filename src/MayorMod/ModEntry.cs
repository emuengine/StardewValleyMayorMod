using MayorMod.Constants;
using MayorMod.Data;
using MayorMod.Data.Models;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.GameData;

namespace MayorMod;

/// <summary>The mod entry point.</summary>
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
        Helper.Events.Display.MenuChanged += Display_MenuChanged;
    }

    private void GameLoop_SaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        _saveData = base.Helper.Data.ReadSaveData<MayorModData>(ModKeys.MayorModSaveKey);
        _saveData ??= new MayorModData();
    }

    private void GameLoop_DayStarted(object? sender, DayStartedEventArgs e)
    {
        if (Game1.player.mailReceived.Contains(ModProgressKeys.RegisteringForBalot))
        {
            Game1.player.mailReceived.Remove(ModProgressKeys.RegisteringForBalot);
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
        if (Game1.player.mailReceived.Contains(ModProgressKeys.RegisteringForBalot))
        {
            _saveData.RunningForMayor = true;
            _saveData.VotingDate = HelperMethods.GetDateWithoutFestival(10);
            Helper.Data.WriteSaveData(ModKeys.MayorModSaveKey, _saveData);
        }

        //End of voting day
        if (_saveData is not null && _saveData.RunningForMayor && _saveData.VotingDate == SDate.Now())
        {
            Game1.MasterPlayer.mailReceived.Remove(ModProgressKeys.IsVotingDay);
            _saveData.RunningForMayor = false;
            //TODO Make it so you can lose election but for now just assume you win
            Game1.MasterPlayer.mailReceived.Add(ModProgressKeys.ElectedAsMayor);
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
            StartTime = 600,
            ShowOnCalendar = true,
            OnlyShowMessageOnFirstDay = true,
        };
        data[$"{ModKeys.MayorModCPId}_VotingDayPassiveFestival"] = votingDay;
    }

    private void AssetUpdatesForDialogue(IAssetData dialogues)
    {
        if (!dialogues.AsDictionary<string, string>().Data.ContainsKey($"AcceptGift_(O){ModItemKeys.LeafletSign}") &&
            !dialogues.AsDictionary<string, string>().Data.ContainsKey($"RejectItem_(O){ModItemKeys.LeafletSign}"))
        {
            var data = dialogues.AsDictionary<string, string>().Data;
            data[$"RejectItem_(O){ModItemKeys.LeafletSign}"] = HelperMethods.GetTranslationForKey(Helper, $"{ModKeys.MayorModCPId}_Gifting.Default.Leaflet");
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

    private void Display_MenuChanged(object? sender, MenuChangedEventArgs e)
    {
        //if (e.NewMenu is DialogueBox db && 
        //    db.characterDialogue.TranslationKey != null && 
        //    db.characterDialogue.TranslationKey.Contains("Introduction"))
        //{
        //    var g = e;
        //}
    }

    //private void GameLoop_DayStarted(object? sender, DayStartedEventArgs e)
    //{
    //    //ModHelper.RemoveProgressMails();
    //    //ModHelper.MasterPlayerMail.Add(ModProgressKeys.VotingMayor);

    //   // Game1.MasterPlayer.addQuest("EmuEngine.MayorModCP_CampaignWithLeafletsQuest");
    //}

    //private void GameLoop_DayEnding(object? sender, DayEndingEventArgs e)
    //{
    //    //ModHelper.RemoveProgressMails();

    //    //if (Game1.dayOfMonth % 3 == 2)
    //    //{
    //    //    ModHelper.MasterPlayerMail.Add(Constants.ProgressKey.RegisteringForBalot);
    //    //}
    //    //else if (Game1.dayOfMonth % 3 == 0)
    //    //{
    //    //    ModHelper.MasterPlayerMail.Add(Constants.ProgressKey.VotingMayor);
    //    //}
    //}


    ///*********
    //** Private methods
    //*********/
    ///// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
    ///// <param name="sender">The event sender.</param>
    ///// <param name="e">The event data.</param>
    //private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    //{
    //    // ignore if player hasn't loaded a save yet
    //    if (!Context.IsWorldReady)
    //        return;

    //    // print button presses to the console window
    //    this.Monitor.Log($"{Game1.player.Name} pressed {e.Button}.", LogLevel.Debug);

    //    if (e.Button == SButton.X)
    //    {
    //        //ICursorPosition cursorPos = this.Helper.Input.GetCursorPosition();
    //        //Game1.spriteBatch.DrawString(Game1.smallFont, "some text", cursorPos.ScreenPixels, Color.Black);
    //        if (MasterPlayerMail.Contains("runningForMayor"))
    //        {
    //            MasterPlayerMail.Remove("runningForMayor");
    //        }
    //        else
    //        {
    //            MasterPlayerMail.Add("runningForMayor");
    //        }

    //        //Game save 
    //        //// read data
    //        //var model = this.Helper.Data.ReadSaveData<MayorModData>(Constants.MayorModSaveKey);
    //        //// save data (if needed)
    //        //this.Helper.Data.WriteSaveData("example-key", model);


    //        //var f = Game1.player.mailbox;
    //        //Game1.player.obsolete_canUnderstandDwarves

    //        //Add mail 
    //        //Game1.player.mailbox.Add("MyModMail1");
    //        //Game1.player.mailbox.Add("MyModMail2");
    //        //Game1.player.mailbox.Add("MyModMail3");
    //        //Game1.player.mailbox.Add("MyWizardMail");

    //        //Teleport to location
    //        //GameLocation Location = Utility.fuzzyLocationSearch("FarmHouse");
    //        //Action TeleportFunction = delegate {
    //        //    //Insert here the coordinates you want to teleport to
    //        //    int X = 1;
    //        //    int Y = 1;

    //        //    //The direction you want the Farmer to face after the teleport
    //        //    // 0 = up, 1 = right, 2 = down, 3 = left
    //        //    int Direction = 1;

    //        //    //The teleport command itself
    //        //    Game1.warpFarmer(new LocationRequest(Location.NameOrUniqueName, Location.uniqueName.Value != null, Location), X, Y, Direction);
    //        //};
    //        //DelayedAction.functionAfterDelay(TeleportFunction, 100);

    //        //Noise
    //        //Location.playSound("axe");
    //    }
    //}
}
