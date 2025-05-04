using MayorMod.Constants;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.Menus;
using xTile.Dimensions;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace MayorMod.Data;

public class TileActions
{
    private static IMonitor? _monitor;

    public const string MayorModActionType = "MayorModAction";
    public const string MayorDeskActionType = "MayorDesk";
    public const string DeskActionType = "Desk";
    public const string DeskRegisterActionType = "DeskRegister";
    public const string VotingBoothActionType = "VotingBooth";
    public const string BallotBoxActionType = "BallotBox";

    public static void Init(IMonitor monitor)
    {
        _monitor = monitor;
        GameLocation.RegisterTileAction(MayorModActionType, GetVoteCard);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="location"></param>
    /// <param name="arg2"></param>
    /// <param name="farmer"></param>
    /// <param name="point"></param>
    /// <returns></returns>
    private static bool GetVoteCard(GameLocation location, string[] arg2, Farmer farmer, Point point)
    {
        //if (!farmer.mailReceived.Contains(ModProgressKeys.VotedForMayor))
        {
            if (arg2.Length >= 2)
            {
                switch (arg2[1])
                {
                    case MayorDeskActionType: MayorDeskAction(farmer); break;
                    case DeskActionType: DeskAction(farmer); break;
                    case DeskRegisterActionType: DeskRegisterAction(farmer); break;
                    case VotingBoothActionType: VotingBoothAction(location, farmer, arg2); break;
                    case BallotBoxActionType: BallotBoxAction(farmer); break;
                    default: _monitor?.Log($"Unknown tile action - {arg2[1]}", LogLevel.Error); break;
                }
            }
        }
        //else
        //{
        //    Game1.DrawDialogue(HelperMethods.OfficerMikeNPC, DialogueKeys.OfficerMike.HaveVoted);
        //}
        return true;
    }

    private static void MayorDeskAction(Farmer farmer)
    {
        //Game1.DrawDialogue(HelperMethods.OfficerMikeNPC, DialogueKeys.OfficerMike.RegisterForBallot);
        //Game1.activeClickableMenu = new LanguageSelectionMenu();
        //Game1.activeClickableMenu = new ChooseFromListMenu(Utility.GetJukeboxTracks(Game1.player, Game1.player.currentLocation), ChooseFromListMenu.playSongAction, isJukebox: true);
        //Game1.activeClickableMenu = new QuestLog();
        Game1.activeClickableMenu = new TestMenu();
        //Game1.activeClickableMenu = new ChooseFromListMenu([ "jukeboxTracks", "dfjbhsdjfhgs"], OnSongChosen);// isJukebox: false, location.miniJukeboxTrack.Value);

        //Game1.activeClickableMenu = new ItemListMenu(Game1.content.LoadString("Strings\\UI:ItemList_ItemsLost"), new List<Item>()
        //{
        //    new TestItem()
        //Game1.activeClickableMenu = new Billboard(true);
    }



    private static void DeskRegisterAction(Farmer farmer)
    {
        Game1.DrawDialogue(HelperMethods.OfficerMikeNPC, DialogueKeys.OfficerMike.RegisterForBallot);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="farmer"></param>
    public static void DeskAction(Farmer farmer)
    {
        if (farmer.Items.Any(i => i != null && i.Name == ModItemKeys.Ballot))
        {
            Game1.DrawDialogue(HelperMethods.OfficerMikeNPC, DialogueKeys.OfficerMike.NeedToFillBallot);
        }
        else if (farmer.Items.Any(i => i != null && i.Name == ModItemKeys.BallotUsed))
        {
            Game1.DrawDialogue(HelperMethods.OfficerMikeNPC, DialogueKeys.OfficerMike.NeedToVote);
        }
        else if (farmer.Items.HasEmptySlots())
        {
            //Game1.DrawDialogue(HelperMethods.OfficerMikeNPC, DialogueKeys.OfficerMike.CheckId);
            Game1.DrawDialogue(HelperMethods.OfficerMikeNPC, DialogueKeys.OfficerMike.GetBallot);
            DelayedAction.functionAfterDelay(() => { AddItemToMasterInventory(ModItemKeys.Ballot); }, 1000);
        }
        else
        {
            Game1.DrawDialogue(HelperMethods.OfficerMikeNPC, DialogueKeys.OfficerMike.CantCarryBallot);
        }
        //dont let leave if you have a voting card
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="location"></param>
    /// <param name="farmer"></param>
    /// <param name="args"></param>
    public static void VotingBoothAction(GameLocation location, Farmer farmer, string[] args)
    {
        var ballot = farmer.Items.FirstOrDefault(i => i != null && i.Name == ModItemKeys.Ballot);

        if (ballot is not null && args.Length == 3)
        {
            //Show filling in voting card
            var parsed = Int32.TryParse(args[2], out int boothNum);
            boothNum = parsed? boothNum: 0;
            var boothLocation = new Vector2(120, 62);
            if (boothNum > 2)
            {
                boothLocation = new Vector2(332, 126);
            }
            //Remove unused voting card
            farmer.removeItemFromInventory(ballot);

            float drawingTime = 500.0f;
            HelperMethods.DrawSpriteTemporarily(location, boothLocation + new Vector2((30 * boothNum), 0), ModItemKeys.BallotTexturePath, drawingTime);

            //Add used voting card
            DelayedAction.functionAfterDelay(() => { AddItemToMasterInventory(ModItemKeys.BallotUsed); }, (int)drawingTime);
        }
        else if (farmer.Items.Any(i => i != null && i.Name == ModItemKeys.Ballot))
        {
            Game1.DrawDialogue(HelperMethods.OfficerMikeNPC, DialogueKeys.OfficerMike.NeedToFillBallot);
        }
        else if (farmer.Items.Any(i => i != null && i.Name == ModItemKeys.BallotUsed))
        {
            Game1.DrawDialogue(HelperMethods.OfficerMikeNPC, DialogueKeys.OfficerMike.NeedToVote);
        }
        else
        {
            Game1.DrawDialogue(HelperMethods.OfficerMikeNPC, DialogueKeys.OfficerMike.NeedBallot);
        }
    }

    private static void AddItemToMasterInventory(string itemId)
    {
        var item = ItemRegistry.Create(itemId);
        Game1.player.addItemToInventory(item);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="farmer"></param>
    public static void BallotBoxAction(Farmer farmer)
    {
        var ballot = farmer.Items.FirstOrDefault(i => i != null && i.Name == ModItemKeys.BallotUsed);

        if (ballot is not null)
        {
            farmer.removeItemFromInventory(ballot);
            farmer.mailReceived.Add(ModProgressKeys.VotedForMayor);
            Game1.DrawDialogue(HelperMethods.OfficerMikeNPC, DialogueKeys.OfficerMike.HaveVoted);
        }
        else if (farmer.Items.Any(i => i != null && i.Name == ModItemKeys.Ballot))
        {
            Game1.DrawDialogue(HelperMethods.OfficerMikeNPC, DialogueKeys.OfficerMike.NeedToFillBallot);
        }
        else
        {
            Game1.DrawDialogue(HelperMethods.OfficerMikeNPC, DialogueKeys.OfficerMike.NeedBallot);
        }
    }
}

public class TestMenu : IClickableMenu
{
    public TestMenu()
    {
    }

    public override void draw(SpriteBatch b)
    {
        base.draw(b);

        if (!Game1.options.showClearBackgrounds)
        {
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
        }
        SpriteText.drawStringWithScrollCenteredAt(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:QuestLog.cs.11373"), xPositionOnScreen + width / 2, yPositionOnScreen - 64, "", 1f, null);
        //if (questPage == -1)
        {
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(384, 373, 18, 18), xPositionOnScreen, yPositionOnScreen, width, height, Color.White, 4f);
            //for (int i = 0; i < questLogButtons.Count; i++)
            //{
            //    //if (pages.Count > 0 && pages[currentPage].Count > i)
            //    {
            //        IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 396, 15, 15), questLogButtons[i].bounds.X, questLogButtons[i].bounds.Y, questLogButtons[i].bounds.Width, questLogButtons[i].bounds.Height, questLogButtons[i].containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()) ? Color.Wheat : Color.White, 4f, drawShadow: false);
            //        if (pages[currentPage][i].ShouldDisplayAsNew() || pages[currentPage][i].ShouldDisplayAsComplete())
            //        {
            //            Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(questLogButtons[i].bounds.X + 64 + 4, questLogButtons[i].bounds.Y + 44), new Rectangle(pages[currentPage][i].ShouldDisplayAsComplete() ? 341 : 317, 410, 23, 9), Color.White, 0f, new Vector2(11f, 4f), 4f + Game1.dialogueButtonScale * 10f / 250f, flipped: false, 0.99f);
            //        }
            //        else
            //        {
            //            Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(questLogButtons[i].bounds.X + 32, questLogButtons[i].bounds.Y + 28), pages[currentPage][i].IsTimedQuest() ? new Rectangle(410, 501, 9, 9) : new Rectangle(395 + (pages[currentPage][i].IsTimedQuest() ? 3 : 0), 497, 3, 8), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.99f);
            //        }
            //        pages[currentPage][i].IsTimedQuest();
            //        SpriteText.drawString(b, pages[currentPage][i].GetName(), questLogButtons[i].bounds.X + 128 + 4, questLogButtons[i].bounds.Y + 20, 999999, -1, 999999, 1f, 0.88f, junimoText: false, -1, "", null);
            //    }
            //}
        }
    }
}