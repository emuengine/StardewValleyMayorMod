using MayorMod.Constants;
using MayorMod.Data.Menu;
using MayorMod.Data.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using static StardewValley.Minigames.Intro;

namespace MayorMod.Data.TileActions;

public static class VotingTileActions
{
    /// <summary>
    /// Tile action for the security desk in the voting area. Officer mike should give the farmer a voting ballot.
    /// </summary>
    /// <param name="farmer">Current player</param>
    public static void DeskAction(Farmer farmer)
    {
        if (farmer.Items.Any(i => i != null && i.Name == ModItemKeys.Ballot))
        {
            Game1.DrawDialogue(ModUtils.OfficerMikeNPC, DialogueKeys.OfficerMike.NeedToFillBallot);
        }
        else if (farmer.Items.Any(i => i != null && i.Name == ModItemKeys.BallotUsed))
        {
            Game1.DrawDialogue(ModUtils.OfficerMikeNPC, DialogueKeys.OfficerMike.NeedToVote);
        }
        else if (farmer.Items.HasEmptySlots())
        {
            Game1.DrawDialogue(ModUtils.OfficerMikeNPC, DialogueKeys.OfficerMike.GetBallot);
            DelayedAction.functionAfterDelay(() => { ModUtils.AddItemToInventory(ModItemKeys.Ballot); }, 1000);
        }
        else
        {
            Game1.DrawDialogue(ModUtils.OfficerMikeNPC, DialogueKeys.OfficerMike.CantCarryBallot);
        }
        //TODO: Dont let leave if you have a voting card
    }

    /// <summary>
    /// Tile action for the voting booth in the voting area. Changes ballot into ballot(voted)
    /// </summary>
    /// <param name="farmer">Current player</param>
    /// <param name="votingBoothId">Voting booth id</param>
    public static void VotingBoothAction(IModHelper helper, Farmer farmer, string[] votingBoothId)
    {

        var ballot = farmer.Items.FirstOrDefault(i => i != null && i.Name == ModItemKeys.Ballot);

        if (ballot is not null && votingBoothId.Length == 3)
        {
            Game1.activeClickableMenu = GetVotingMenu(helper, (i) => { SelectVote(ballot, votingBoothId, i); });
        }
        else if (farmer.Items.Any(i => i != null && i.Name == ModItemKeys.Ballot))
        {
            Game1.DrawDialogue(ModUtils.OfficerMikeNPC, DialogueKeys.OfficerMike.NeedToFillBallot);
        }
        else if (farmer.Items.Any(i => i != null && i.Name == ModItemKeys.BallotUsed))
        {
            Game1.DrawDialogue(ModUtils.OfficerMikeNPC, DialogueKeys.OfficerMike.NeedToVote);
        }
        else
        {
            Game1.DrawDialogue(ModUtils.OfficerMikeNPC, DialogueKeys.OfficerMike.NeedBallot);
        }
    }

    private static MayorModMenu GetVotingMenu(IModHelper helper, Action<int> votingAction)
    {
        //Show voting ballot menu
        var _texture = helper.ModContent.Load<Texture2D>("assets/ballotBackground.png");
        var menu = new MayorModMenu(helper, 0.4f, 0.7f);
        menu.BackgoundColour = Color.White;
        menu.MenuItems =
        [
            new MenuBorder(menu),
            new TextMenuItem(menu, Game1.content.LoadString(DialogueKeys.VotingBooth.VotingBallotTitle), new Margin(0, 25, 0, 0)){ IsBold = true, Align = TextMenuItem.MenuItemAlign.Center },
            new VotingListMenuItem(menu, new Margin(30, 120, 30, 50), votingAction),
            new ButtonMenuItem(menu, new Vector2(-84, 20), () => { Game1.exitActiveMenu(); })
            {
                ButtonTypeSelected = ButtonMenuItem.ButtonType.Cancel
            },
        ];
        return menu;
    }

    private static void SelectVote(Item ballot,string[] votingBoothId, int candidateIndex)
    {
        Game1.exitActiveMenu();
        var v = candidateIndex;

        //Show filling in voting card
        var parsed = int.TryParse(votingBoothId[2], out int boothNum);
        boothNum = parsed ? boothNum : 0;
        var boothLocation = new Vector2(120, 62);
        if (boothNum > 2)
        {
            boothLocation = new Vector2(332, 126);
        }
        //Remove unused voting card
        Game1.player.removeItemFromInventory(ballot);

        float drawingTime = 500.0f;
        ModUtils.DrawSpriteTemporarily(Game1.player.currentLocation, boothLocation + new Vector2(30 * boothNum, 0), ModItemKeys.BallotTexturePath, drawingTime);

        //Add used voting card
        DelayedAction.functionAfterDelay(() => { ModUtils.AddItemToInventory(ModItemKeys.BallotUsed); }, (int)drawingTime);
    }

    /// <summary>
    /// Tile action for the bollot box in the voting area. Allows the player to cast their vote.
    /// </summary>
    /// <param name="farmer">Current player</param>
    public static void BallotBoxAction(Farmer farmer)
    {
        var ballot = farmer.Items.FirstOrDefault(i => i != null && i.Name == ModItemKeys.BallotUsed);

        if (ballot is not null)
        {
            farmer.removeItemFromInventory(ballot);
            ModProgressManager.AddProgressFlag(ModProgressManager.VotedForMayor);
            Game1.DrawDialogue(ModUtils.OfficerMikeNPC, DialogueKeys.OfficerMike.HaveVoted);
        }
        else if (farmer.Items.Any(i => i != null && i.Name == ModItemKeys.Ballot))
        {
            Game1.DrawDialogue(ModUtils.OfficerMikeNPC, DialogueKeys.OfficerMike.NeedToFillBallot);
        }
        else
        {
            Game1.DrawDialogue(ModUtils.OfficerMikeNPC, DialogueKeys.OfficerMike.NeedBallot);
        }
    }
}
