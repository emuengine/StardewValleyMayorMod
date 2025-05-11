using MayorMod.Constants;
using MayorMod.Data.Menu;
using MayorMod.Data.Menu.Data;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace MayorMod.Data;

public class TileActions
{
    private readonly IMonitor _monitor;

    public const string MayorModActionType = "MayorModAction";
    public const string MayorDeskActionType = "MayorDesk";
    public const string DeskActionType = "Desk";
    public const string DeskRegisterActionType = "DeskRegister";
    public const string VotingBoothActionType = "VotingBooth";
    public const string BallotBoxActionType = "BallotBox";

    public TileActions(IMonitor monitor)
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
    private bool GetVoteCard(GameLocation location, string[] arg2, Farmer farmer, Point point)
    {
        if (arg2.Length < 2)
        {
            _monitor.Log("MayorModAction is missing parameters", LogLevel.Error);
            return false;
        }
        
        if (arg2[1] == MayorDeskActionType)
        {
            MayorDeskAction(farmer);
        }
        else if (!farmer.mailReceived.Contains(ModProgressKeys.VotedForMayor))
        {
            switch (arg2[1])
            {
                case MayorDeskActionType: MayorDeskAction(farmer); break;
                case DeskActionType: DeskAction(farmer); break;
                case DeskRegisterActionType: DeskRegisterAction(farmer); break;
                case VotingBoothActionType: VotingBoothAction(location, farmer, arg2); break;
                case BallotBoxActionType: BallotBoxAction(farmer); break;
                default:
                {
                    _monitor?.Log($"Unknown tile action - {arg2[1]}", LogLevel.Error);
                    return false;
                }
            }
        }
        else
        {
            Game1.DrawDialogue(HelperMethods.OfficerMikeNPC, DialogueKeys.OfficerMike.HaveVoted);
        }
        return true;
    }

    private void MayorDeskAction(Farmer farmer)
    {
        if (Game1.MasterPlayer.mailReceived.Contains(ModProgressKeys.CouncilMeetingPlanned))
        {
            Game1.drawObjectDialogue(Game1.content.LoadString(DialogueKeys.CouncilMeeting.AlreadyPlanned));
            return;
        }

        var seat = MapSeat.FromData("2/1/down/custom 0.5 0.1 0/-1/-1/false", 19, 5);
        farmer.BeginSitting(seat);

        IList<string> buttonNames = [Game1.content.LoadString(DialogueKeys.CouncilMeeting.MeetingOption0),
                                     Game1.content.LoadString(DialogueKeys.CouncilMeeting.MeetingOption1),
                                     Game1.content.LoadString(DialogueKeys.CouncilMeeting.MeetingOption2),
                                     Game1.content.LoadString(DialogueKeys.CouncilMeeting.MeetingOption3),
                                     Game1.content.LoadString(DialogueKeys.CouncilMeeting.MeetingOption4),
                                     Game1.content.LoadString(DialogueKeys.CouncilMeeting.MeetingOption5)];

        var menu = new MayorModMenu(0.8f, 0.8f);
        menu.MenuItems = [
                         new TextMenuItem(menu, Game1.content.LoadString(DialogueKeys.CouncilMeeting.HoldCouncilMeeting), new Vector2(15, 20)),
                         new BigButtonMenuItem(menu, new Margin(30, 90, 60, 110), buttonNames, OnCoucilMeetingSelected),
                         new ButtonMenuItem(menu, new Vector2(-84, 20), () => { Game1.exitActiveMenu(); })
                         {
                             ButtonTypeSelected = ButtonMenuItem.ButtonType.Cancel
                         },
                         ];
        Game1.activeClickableMenu = menu;
    }

    private void OnCoucilMeetingSelected(int obj)
    {
        _monitor.Log("Not implemented yet.", LogLevel.Error);
        return;

        switch (obj)
        {
            case 0: SetupCouncilMeeting_PublicSafety(); break;
            default: _monitor.Log("Cannot find council meeting to setup", LogLevel.Error); break;
        }
        Game1.exitActiveMenu();
    }

    private void SetupCouncilMeeting_PublicSafety()
    {
        _monitor.Log("I do things", LogLevel.Info);
    }

    private void DeskRegisterAction(Farmer farmer)
    {
        Game1.DrawDialogue(HelperMethods.OfficerMikeNPC, DialogueKeys.OfficerMike.RegisterForBallot);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="farmer"></param>
    public void DeskAction(Farmer farmer)
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
    public void VotingBoothAction(GameLocation location, Farmer farmer, string[] args)
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

    private void AddItemToMasterInventory(string itemId)
    {
        var item = ItemRegistry.Create(itemId);
        Game1.player.addItemToInventory(item);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="farmer"></param>
    public void BallotBoxAction(Farmer farmer)
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
