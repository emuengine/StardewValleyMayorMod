using MayorMod.Constants;
using MayorMod.Data.Menu.Data;
using MayorMod.Data.Menu;
using StardewValley;
using Microsoft.Xna.Framework;

namespace MayorMod.Data.TileActions;

public static class ManorHouseTileActions
{
    public static void MayorDeskAction(Farmer farmer)
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

    private static void OnCoucilMeetingSelected(int obj)
    {
        //switch (obj)
        //{
        //    case 0: SetupCouncilMeeting_PublicSafety(monitor); break;
        //    default: monitor.Log("Cannot find council meeting to setup", LogLevel.Error); break;
        //}
        Game1.exitActiveMenu();
    }

    //private static void SetupCouncilMeeting_PublicSafety(IMonitor monitor)
    //{
    //    monitor.Log("I do things", LogLevel.Info);
    //}
}
