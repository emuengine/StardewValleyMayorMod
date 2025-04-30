using MayorMod.Constants;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace MayorMod.Data;

public class TileActions
{
    private static IMonitor? _monitor;

    public const string MayorModActionType = "MayorModAction";
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
        if (!farmer.mailReceived.Contains(ModProgressKeys.VotedForMayor))
        {
            if (arg2.Length >= 2)
            {
                switch (arg2[1])
                {
                    case DeskActionType: DeskAction(farmer); break;
                    case DeskRegisterActionType: DeskRegisterAction(farmer); break;
                    case VotingBoothActionType: VotingBoothAction(location, farmer, arg2); break;
                    case BallotBoxActionType: BallotBoxAction(farmer); break;
                    default: _monitor?.Log($"Unknown tile action - {arg2[1]}", LogLevel.Error); break;
                }
            }
        }
        else
        {
            Game1.DrawDialogue(HelperMethods.OfficerMikeNPC, DialogueKeys.OfficerMike.HaveVoted);
        }
        return true;
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
            Game1.DrawDialogue(HelperMethods.OfficerMikeNPC, DialogueKeys.OfficerMike.CheckId);
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
