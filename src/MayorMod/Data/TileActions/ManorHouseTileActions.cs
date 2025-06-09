using MayorMod.Constants;
using MayorMod.Data.Menu;
using StardewValley;
using Microsoft.Xna.Framework;
using MayorMod.Data.Models;
using StardewModdingAPI;

namespace MayorMod.Data.TileActions;

public static partial class ManorHouseTileActions
{
    /// <summary>
    /// Handles actions for the the mayor desk
    /// </summary>
    /// <param name="farmer"></param>
    public static void MayorDeskAction(IModHelper helper, Farmer farmer)
    {
        //Sit at the desk
        var seat = MapSeat.FromData("2/1/down/custom 0.5 0.1 0/-1/-1/false", 19, 5);
        farmer.BeginSitting(seat);

        if (IsCouncilMeetingPlannded())
        {
            Game1.drawObjectDialogue(Game1.content.LoadString(DialogueKeys.CouncilMeeting.AlreadyPlanned));
            return;
        }

        //Check if there are any meetings available to plan
        var meetings = GetAvailableCouncilMeetings();
        if (meetings.Count == 0)
        {
            Game1.drawObjectDialogue(Game1.content.LoadString(DialogueKeys.CouncilMeeting.NoNewMeetings));
            return;
        }

        //Show council meeting menu
        var menu = new MayorModMenu(helper, 0.8f, 0.8f);
        menu.MenuItems =
        [
            new TextMenuItem(menu, Game1.content.LoadString(DialogueKeys.CouncilMeeting.HoldCouncilMeeting), new Margin(0, 15, 0, 0)){ IsBold = true, Align = TextMenuItem.MenuItemAlign.Center },
            new TextMenuItem(menu, Game1.content.LoadString(DialogueKeys.CouncilMeeting.AgendaQuestion), new Margin(20, 60, 0, 0)),
            new BigButtonListMenuItem(menu, new Margin(30, 110, 60, 130), [.. meetings.Select((cm, index) => $"{index + 1}. " + cm.Name)], (i)  => OnCoucilMeetingSelected(meetings[i])),
            new ButtonMenuItem(menu, new Vector2(-84, 20), () => { Game1.exitActiveMenu(); })
            {
                ButtonTypeSelected = ButtonMenuItem.ButtonType.Cancel
            }
        ];
        Game1.activeClickableMenu = menu;
    }

    /// <summary>
    /// Get the list of council meeting the player can pick from
    /// </summary>
    /// <returns>list of council meetings</returns>
    private static IList<CouncilMeetingData> GetAvailableCouncilMeetings()
    {
        IList<CouncilMeetingData> meetings = [
            new CouncilMeetingData(Game1.content.LoadString(DialogueKeys.CouncilMeeting.MeetingIntro), CouncilMeetingKeys.MeetingIntro),
        ];
        if (ModProgressManager.HasProgressFlag(CouncilMeetingKeys.HeldPrefix + CouncilMeetingKeys.MeetingIntro))
        {
            meetings = [
                new CouncilMeetingData(Game1.content.LoadString(DialogueKeys.CouncilMeeting.MeetingTownSecurity), CouncilMeetingKeys.MeetingTownSecurity),
                new CouncilMeetingData(Game1.content.LoadString(DialogueKeys.CouncilMeeting.MeetingSaloonHours), CouncilMeetingKeys.MeetingSaloonHours),
                new CouncilMeetingData(Game1.content.LoadString(DialogueKeys.CouncilMeeting.MeetingTownCleanup), CouncilMeetingKeys.MeetingTownCleanup),
             ];
        }

        return [.. meetings.Where(m=>!m.EventHasHappened)];
    }

    /// <summary>
    /// Selects the next council meeting to run
    /// </summary>
    /// <param name="selectedMeeting">council meeting to plan</param>
    private static void OnCoucilMeetingSelected(CouncilMeetingData selectedMeeting )
    {
        var meetingMailId = CouncilMeetingKeys.PlannedPrefix + selectedMeeting.EventMailId;
        Game1.MasterPlayer.mailForTomorrow.Add(meetingMailId);
        Game1.exitActiveMenu();
        Game1.drawObjectDialogue(Game1.content.LoadString(DialogueKeys.CouncilMeeting.MeetingPlanned));
    }

    private static bool IsCouncilMeetingPlannded()
    {
        return Game1.MasterPlayer.mailReceived.Any(p => p.StartsWith(CouncilMeetingKeys.PlannedPrefix)) ||
               Game1.MasterPlayer.mailForTomorrow.Any(p => p.StartsWith(CouncilMeetingKeys.PlannedPrefix));
    }
}
