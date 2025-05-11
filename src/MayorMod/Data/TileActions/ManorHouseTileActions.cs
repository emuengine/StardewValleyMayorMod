using MayorMod.Constants;
using MayorMod.Data.Menu;
using StardewValley;
using Microsoft.Xna.Framework;
using MayorMod.Data.Models;

namespace MayorMod.Data.TileActions;

public static partial class ManorHouseTileActions
{

    public static void MayorDeskAction(Farmer farmer)
    {
        var isMeetingPlanned = Game1.MasterPlayer.mailReceived.Any(p => p.StartsWith(CouncilMeetingKeys.PlannedPrefix)) ||
                               Game1.MasterPlayer.mailForTomorrow.Any(p => p.StartsWith(CouncilMeetingKeys.PlannedPrefix));
        if (isMeetingPlanned)
        {
            Game1.drawObjectDialogue(Game1.content.LoadString(DialogueKeys.CouncilMeeting.AlreadyPlanned));
            return;
        }

        var seat = MapSeat.FromData("2/1/down/custom 0.5 0.1 0/-1/-1/false", 19, 5);
        farmer.BeginSitting(seat);

        var meetings = GetAvailableCouncilMeetings();
        if (meetings.Count == 0)
        {
            Game1.drawObjectDialogue(Game1.content.LoadString(DialogueKeys.CouncilMeeting.NoNewMeetings));
            return;
        }

        var menu = new MayorModMenu(0.8f, 0.8f);
        menu.MenuItems = 
        [
            new TextMenuItem(menu, Game1.content.LoadString(DialogueKeys.CouncilMeeting.HoldCouncilMeeting), new Vector2(15, 20)),
            new BigButtonMenuItem(menu, new Margin(30, 90, 60, 110), [.. meetings.Select(cm => cm.Name)], (i)  => OnCoucilMeetingSelected(meetings[i])),
            new ButtonMenuItem(menu, new Vector2(-84, 20), () => { Game1.exitActiveMenu(); })
            {
                ButtonTypeSelected = ButtonMenuItem.ButtonType.Cancel
            },
        ];
        Game1.activeClickableMenu = menu;
    }

    public static IList<CouncilMeetingData> GetAvailableCouncilMeetings()
    {
        //IList<CouncilMeeting> meetings = [
        //    new CouncilMeeting(){ Name = Game1.content.LoadString(DialogueKeys.CouncilMeeting.MeetingOption0), EventMailId = "{CouncilMeetingKeys.MeetingIntro}"},
        //    new CouncilMeeting(){ Name = Game1.content.LoadString(DialogueKeys.CouncilMeeting.MeetingOption1) },
        //    new CouncilMeeting(){ Name = Game1.content.LoadString(DialogueKeys.CouncilMeeting.MeetingOption2) },
        //    new CouncilMeeting(){ Name = Game1.content.LoadString(DialogueKeys.CouncilMeeting.MeetingOption3) },
        //    new CouncilMeeting(){ Name = Game1.content.LoadString(DialogueKeys.CouncilMeeting.MeetingOption4) },
        //    new CouncilMeeting(){ Name = Game1.content.LoadString(DialogueKeys.CouncilMeeting.MeetingOption5) }
        // ];
        IList<CouncilMeetingData> meetings = [
            new CouncilMeetingData(Game1.content.LoadString(DialogueKeys.CouncilMeeting.MeetingOption0), CouncilMeetingKeys.MeetingIntro),
        ];


        return [.. meetings.Where(m=>!m.EventHasHappened)];
    }

    private static void OnCoucilMeetingSelected(CouncilMeetingData selectedMeeting )
    {
        var meetingMailId = CouncilMeetingKeys.PlannedPrefix + selectedMeeting.EventMailId;
        Game1.MasterPlayer.mailForTomorrow.Add(meetingMailId);
        Game1.exitActiveMenu();
        Game1.drawObjectDialogue(Game1.content.LoadString(DialogueKeys.CouncilMeeting.MeetingPlanned));
    }
}
