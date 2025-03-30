using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace MayorMod.Data;

public class TileActions
{
    private static IMonitor? _monitor;

    public static void Init(IMonitor monitor)
    {
        _monitor = monitor;
        GameLocation.RegisterTileAction(Constants.ActionKey.Action, GetVoteCard);
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
        if (!farmer.mailReceived.Contains(Constants.ProgressKey.VotedForMayor))
        {
            if (arg2.Length >= 2)
            {
                switch (arg2[1])
                {
                    case Constants.ActionKey.DeskAction: DeskAction(farmer); break;
                    case Constants.ActionKey.VotingBoothAction: VotingBoothAction(location, farmer, arg2); break;
                    case Constants.ActionKey.BallotBoxAction: BallotBoxAction(farmer); break;
                    default: _monitor?.Log($"Unknown tile action - {arg2[1]}", LogLevel.Error); break;
                }
            }
        }
        else
        {
            Utils.DrawDialogueCharacterString(Constants.DialogueKey.HaveVoted);
        }
        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="farmer"></param>
    public static void DeskAction(Farmer farmer)
    {
        if (farmer.Items.Any(i => i != null && i.Name == Constants.ItemKey.Ballot))
        {
            Utils.DrawDialogueCharacterString(Constants.DialogueKey.NeedToFillBallot);

        }
        else if (farmer.Items.HasEmptySlots())
        {
            Utils.DrawDialogueCharacterString(Constants.DialogueKey.GetBallot, farmer.displayName);
            var ballot = ItemRegistry.Create(Constants.ItemKey.Ballot);
            farmer.addItemToInventory(ballot);
        }
        else
        {
            Utils.DrawDialogueCharacterString(Constants.DialogueKey.CantCarryBallot);
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
        var ballot = farmer.Items.FirstOrDefault(i => i != null && i.Name == Constants.ItemKey.Ballot);

        if (ballot is not null && args.Length == 3)
        {
            //Show filling in voting card
            var parsed = Int32.TryParse(args[2], out int boothNum);
            boothNum = parsed? boothNum: 0;
            Utils.DrawSpriteTemporarily(location, new Vector2(120 + (30 * boothNum), 62), Constants.AssetPath.BallotTexturePath, 500.0f);

            //Remove unused voting card
            farmer.removeItemFromInventory(ballot);

            //Add used voting card
            var ballotUsed = ItemRegistry.Create(Constants.ItemKey.BallotUsed);
            farmer.addItemToInventory(ballotUsed);
        }
        else if (farmer.Items.Any(i => i != null && i.Name == Constants.ItemKey.Ballot))
        {
            Utils.DrawDialogueCharacterString(Constants.DialogueKey.NeedToFillBallot);
        }
        else
        {
            Utils.DrawDialogueCharacterString(Constants.DialogueKey.NeedBallot);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="farmer"></param>
    public static void BallotBoxAction(Farmer farmer)
    {
        var ballot = farmer.Items.FirstOrDefault(i => i != null && i.Name == Constants.ItemKey.BallotUsed);

        if (ballot is not null)
        {
            farmer.removeItemFromInventory(ballot);
            farmer.mailReceived.Add(Constants.ProgressKey.VotedForMayor);
            Utils.DrawDialogueCharacterString(Constants.DialogueKey.HaveVoted);
        }
        else if (farmer.Items.Any(i => i != null && i.Name == Constants.ItemKey.Ballot))
        {
            Utils.DrawDialogueCharacterString(Constants.DialogueKey.NeedToFillBallot); 
        }
        else
        {
            Utils.DrawDialogueCharacterString(Constants.DialogueKey.NeedBallot);
        }
    }
}
