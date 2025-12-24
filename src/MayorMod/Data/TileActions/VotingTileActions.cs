using MayorMod.Constants;
using MayorMod.Data.Handlers;
using MayorMod.Data.Menu;
using MayorMod.Data.Models;
using MayorMod.Data.Utilities;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace MayorMod.Data.TileActions;

public static class VotingTileActions
{
    public static IList<string> Candidates { get; set; } = new List<string>() { Game1.MasterPlayer.eventsSeen.Contains(CompatibilityKeys.MorrisIsMayorEventID)? 
                                                                                        ModNPCKeys.MorrisId: 
                                                                                        ModNPCKeys.LewisId, 
                                                                                Game1.MasterPlayer.Name };

    /// <summary>
    /// Tile action for the security desk in the voting area. Officer mike should give the farmer a voting ballot.
    /// </summary>
    /// <param name="farmer">Current player</param>
    public static void VotingDeskAction(Farmer farmer)
    {
        if (farmer.mailReceived.Contains(ProgressFlags.VotedForMayor))
        {
            Game1.DrawDialogue(ModUtils.OfficerMikeNPC, DialogueKeys.OfficerMike.HaveVoted);
            return;
        }

        if (farmer.HasItemInInventory(ModItemKeys.Ballot))
        {
            Game1.DrawDialogue(ModUtils.OfficerMikeNPC, DialogueKeys.OfficerMike.NeedToFillBallot);
        }
        else if (farmer.HasItemInInventory(ModItemKeys.BallotUsed))
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
        if (farmer.mailReceived.Contains(ProgressFlags.VotedForMayor))
        {
            Game1.DrawDialogue(ModUtils.OfficerMikeNPC, DialogueKeys.OfficerMike.HaveVoted);
            return;
        }

        var ballot = farmer.ItemFromInventory(ModItemKeys.Ballot);

        if (ballot is not null && votingBoothId.Length == 3)
        {
            Game1.activeClickableMenu = GetVotingMenu(helper, (i) => { SelectVote(ballot, votingBoothId, i); });
        }
        else if (farmer.HasItemInInventory(ModItemKeys.Ballot))
        {
            Game1.DrawDialogue(ModUtils.OfficerMikeNPC, DialogueKeys.OfficerMike.NeedToFillBallot);
        }
        else if (farmer.HasItemInInventory(ModItemKeys.BallotUsed))
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
        var menu = new MayorModMenu(helper, 0.4f, 0.9f);

        menu.BackgoundColour = Color.White;
        menu.MenuItems = new List<IMenuItem>()
        {
            new MenuBorder(menu),
            new TextMenuItem(menu, Game1.content.LoadString(DialogueKeys.VotingBooth.VotingBallotTitle), new Margin(0, 30, 0, 0)){ IsBold = true, Align = TextMenuItem.MenuItemAlign.Center },
            new TextMenuItem(menu, Game1.content.LoadString(DialogueKeys.VotingBooth.VotingBallotDescription), new Margin(15, 100, 0, 0)),
            new VotingListMenuItem(menu, new Margin(30, 160, 30, 40), Candidates, votingAction),
            new ButtonMenuItem(menu, new Vector2(-84, 20), () => { Game1.exitActiveMenu(); })
            {
                ButtonTypeSelected = ButtonMenuItem.ButtonType.Cancel
            },
        };
        return menu;
    }

    private static void SelectVote(Item ballot,string[] votingBoothId, int candidateIndex)
    {
        Game1.exitActiveMenu();

        if (candidateIndex< Candidates.Count && Candidates[candidateIndex] == Game1.MasterPlayer.Name)
        {
            ModProgressHandler.AddProgressFlag(ProgressFlags.HasVotedForHostFarmer);
        }

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
        if (farmer.mailReceived.Contains(ProgressFlags.VotedForMayor))
        {
            Game1.DrawDialogue(ModUtils.OfficerMikeNPC, DialogueKeys.OfficerMike.HaveVoted);
            return;
        }

        var ballot = farmer.ItemFromInventory(ModItemKeys.BallotUsed);

        if (ballot is not null)
        {
            farmer.removeItemFromInventory(ballot);
            ModProgressHandler.AddProgressFlag(ProgressFlags.VotedForMayor);
            Game1.DrawDialogue(ModUtils.OfficerMikeNPC, DialogueKeys.OfficerMike.HaveVoted);
        }
        else if (farmer.HasItemInInventory(ModItemKeys.Ballot))
        {
            Game1.DrawDialogue(ModUtils.OfficerMikeNPC, DialogueKeys.OfficerMike.NeedToFillBallot);
        }
        else
        {
            Game1.DrawDialogue(ModUtils.OfficerMikeNPC, DialogueKeys.OfficerMike.NeedBallot);
        }
    }
}
