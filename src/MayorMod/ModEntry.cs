using MayorMod.Data;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace MayorMod
{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {
        private static NetStringHashSet MasterPlayerMail => Game1.MasterPlayer.mailReceived;

        private bool _loadNewMayorGovernorSprite = false;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            //helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.Content.AssetRequested += this.OnAssetRequested;

            TileActions.Init(this.Monitor);
        }

        private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Events/Custom_MayorMod_NewMayor"))
            {
                //Loading the "NewMayor" event so swap the sprite for the Governor
                _loadNewMayorGovernorSprite = true;
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Characters/Governor") && _loadNewMayorGovernorSprite)
            {
                e.Edit(asset => { asset.ReplaceWith(Helper.GameContent.Load<Texture2D>("Characters/Governor_NewMayorEvent")); });
                _loadNewMayorGovernorSprite = false;
            }
        }


        //#region Mail
        //private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        //{
        //    if (e.NameWithoutLocale.IsEquivalentTo("Data/mail"))
        //    {
        //        e.Edit(this.EditImpl);
        //    }
        //    if(e.NameWithoutLocale.IsEquivalentTo("Characters/Dialogue/Lewis"))
        //    {
        //        e.Edit(this.LewisDialogueEdits);
        //    }
        //    //if (e.NameWithoutLocale.IsEquivalentTo("Data/Festivals/FestivalDates"))
        //    //{
        //    //    e.Edit(this.FestivalDateEdits);
        //    //}
        //}

        //public void FestivalDateEdits(IAssetData asset)
        //{
        //    var data = asset.AsDictionary<string, string>().Data;
        //    data["spring9"] = "Voting Day!";
        //}

        //public void LewisDialogueEdits(IAssetData asset)
        //{
        //    var data = asset.AsDictionary<string, string>().Data;
        //    data["Introduction"] = "Im about to be fired!";
        //}

        //public void EditImpl(IAssetData asset)
        //{
        //    var data = asset.AsDictionary<string, string>().Data;

        //    // "MyModMail1" is referred to as the mail Id.  It is how you will uniquely identify and reference your mail.
        //    // The @ will be replaced with the player's name.  Other items do not seem to work (''i.e.,'' %pet or %farm)
        //    // %item object 388 50 %%   - this adds 50 pieces of wood when added to the end of a letter.
        //    // %item tools Axe Hoe %%   - this adds tools; may list any of Axe, Hoe, Can, Scythe, and Pickaxe
        //    // %item money 250 601  %%  - this sends a random amount of gold from 250 to 601 inclusive.
        //    // For more details, see: https://stardewvalleywiki.com/Modding:Mail_data 
        //    data["MyModMail1"] = "Hello @... ^A single carat is a new line ^^Two carats will double space.";
        //    data["MyModMail2"] = "This is how you send an existing item via email! %item object 388 50 %%";
        //    data["MyModMail3"] = "Coin $   Star =   Heart <   Dude +  Right Arrow >   Up Arrow `";
        //    data["MyWizardMail"] = "Include Wizard in the mail Id to use the special background on a letter";
        //}
        //#endregion

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
}
