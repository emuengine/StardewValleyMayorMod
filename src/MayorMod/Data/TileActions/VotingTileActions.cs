using MayorMod.Constants;
using Microsoft.Xna.Framework;
using StardewValley;

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
    public static void VotingBoothAction(Farmer farmer, string[] votingBoothId)
    {
        var ballot = farmer.Items.FirstOrDefault(i => i != null && i.Name == ModItemKeys.Ballot);
        
        if (ballot is not null && votingBoothId.Length == 3)
        {
            //Show filling in voting card
            var parsed = int.TryParse(votingBoothId[2], out int boothNum);
            boothNum = parsed ? boothNum : 0;
            var boothLocation = new Vector2(120, 62);
            if (boothNum > 2)
            {
                boothLocation = new Vector2(332, 126);
            }
            //Remove unused voting card
            farmer.removeItemFromInventory(ballot);

            float drawingTime = 500.0f;
            ModUtils.DrawSpriteTemporarily(farmer.currentLocation, boothLocation + new Vector2(30 * boothNum, 0), ModItemKeys.BallotTexturePath, drawingTime);

            //Add used voting card
            DelayedAction.functionAfterDelay(() => { ModUtils.AddItemToInventory(ModItemKeys.BallotUsed); }, (int)drawingTime);
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
