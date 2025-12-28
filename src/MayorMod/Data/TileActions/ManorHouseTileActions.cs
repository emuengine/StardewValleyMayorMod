using MayorMod.Constants;
using MayorMod.Data.Menu;
using StardewValley;
using Microsoft.Xna.Framework;
using MayorMod.Data.Models;
using StardewModdingAPI;
using MayorMod.Data.Handlers;
using MayorMod.Data.Utilities;

namespace MayorMod.Data.TileActions;

public static partial class ManorHouseTileActions
{
    /// <summary>
    /// Handles actions for the the mayor desk
    /// </summary>
    /// <param name="farmer"></param>
    public static void MayorDeskAction(IModHelper helper, Farmer farmer, MayorModConfig modConfig)
    {
        //Sit at the desk
        var seat = MapSeat.FromData("2/1/down/custom 0.5 0.1 0/-1/-1/false", 19, 5);
        farmer.BeginSitting(seat);

        var menuItems = new string[]{
            ModUtils.GetTranslationForKey(helper, $"{ModKeys.MAYOR_MOD_CPID}_UIMenu.SelectCouncilMeeting"),
            ModUtils.GetTranslationForKey(helper, $"{ModKeys.MAYOR_MOD_CPID}_UIMenu.TownTreasury")
        };

        //Show menu
        //var menu = new MayorModMenu(helper, 0.8f, 0.8f);
        //menu.MenuItems = new List<IMenuItem>()
        //{
        //    new TextMenuItem(menu, Game1.content.LoadString(DialogueKeys.MayorDesk.Title), new Margin(0, 15, 0, 0)){ IsBold = true, Align = TextMenuItem.MenuItemAlign.Center },
        //    new TextMenuItem(menu, Game1.content.LoadString(DialogueKeys.MayorDesk.Description), new Margin(20, 60, 0, 0)),
        //    new BigButtonListMenuItem(menu, new Margin(30, 110, 60, 130), menuItems, (i)  => OnMayorMenuItemSelected(helper, modConfig, i)),
        //    new ButtonMenuItem(menu, new Vector2(-84, 20), () => { Game1.exitActiveMenu(); })
        //    {
        //        ButtonTypeSelected = ButtonMenuItem.ButtonType.Cancel
        //    }
        //};
        //Game1.activeClickableMenu = menu;
        ShowCouncilMeetingMenu(helper, modConfig);
    }

    /// <summary>
    /// Handles the selection of a mayor menu items and displays the selected menu.
    /// </summary>
    /// <param name="selectedIndex">The index of the selected menu item.</param>
    private static void OnMayorMenuItemSelected(IModHelper helper, MayorModConfig modConfig, int selectedIndex)
    {
        switch (selectedIndex)
        {
            case 0:
                ShowCouncilMeetingMenu(helper, modConfig);
                break;
        }
    }

    /// <summary>
    /// Show the council meeting planning menu
    /// </summary>
    public static void ShowCouncilMeetingMenu(IModHelper helper, MayorModConfig modConfig)
    {
        if (IsCouncilMeetingPlannded())
        {
            string dayPlanned = ModUtils.GetNextCouncilMeetingDay(helper, modConfig);
            Game1.drawObjectDialogue(Game1.content.LoadString(DialogueKeys.CouncilMeeting.AlreadyPlanned) + $" {dayPlanned}.");
            return;
        }

        //Check if there are any meetings available to plan
        var meetings = GetAvailableCouncilMeetings();
        if (meetings.Count == 0)
        {
            Game1.drawObjectDialogue(Game1.content.LoadString(DialogueKeys.CouncilMeeting.NoNewMeetings));
            return;
        }

        //Show council menu
        var councilMenu = new MayorModMenu(helper, 0.8f, 0.8f);
        councilMenu.MenuItems = new List<IMenuItem>()
        {
            new TextMenuItem(councilMenu, Game1.content.LoadString(DialogueKeys.CouncilMeeting.HoldCouncilMeeting), new Margin(0, 15, 0, 0)){ IsBold = true, Align = TextMenuItem.MenuItemAlign.Center },
            new TextMenuItem(councilMenu, Game1.content.LoadString(DialogueKeys.CouncilMeeting.AgendaQuestion), new Margin(20, 60, 0, 0)),
            new BigButtonListMenuItem(councilMenu, new Margin(30, 110, 60, 130), meetings.Select((cm, index) => $"{index + 1}. " + cm.Name).ToList(), (i)  => OnCoucilMeetingSelected(meetings[i])){ ScrollBarEnabled = true },
            new ButtonMenuItem(councilMenu, new Vector2(-84, 20), () => { Game1.exitActiveMenu(); })
            {
                ButtonTypeSelected = ButtonMenuItem.ButtonType.Cancel
            }
        };
        Game1.activeClickableMenu = councilMenu;
    }

    /// <summary>
    /// Get the list of council meeting the player can pick from
    /// </summary>
    /// <returns>list of council meetings</returns>
    private static IList<CouncilMeetingData> GetAvailableCouncilMeetings()
    {
        var meetings = new List<CouncilMeetingData>()
        {
            new(Game1.content.LoadString(DialogueKeys.CouncilMeeting.MeetingIntro), CouncilMeetingKeys.MeetingIntro),
        };
        if (CouncilMeetingData.HasMeetingHappened(CouncilMeetingKeys.MeetingIntro))
        {
            meetings = new List<CouncilMeetingData>()
            {
                new(Game1.content.LoadString(DialogueKeys.CouncilMeeting.MeetingTownSecurity), CouncilMeetingKeys.MeetingTownSecurity),
                new(Game1.content.LoadString(DialogueKeys.CouncilMeeting.MeetingSaloonHours), CouncilMeetingKeys.MeetingSaloonHours),
                new(Game1.content.LoadString(DialogueKeys.CouncilMeeting.MeetingTownCleanup), CouncilMeetingKeys.MeetingTownCleanup),
                new(Game1.content.LoadString(DialogueKeys.CouncilMeeting.MeetingStrategicReserve), CouncilMeetingKeys.MeetingStrategicReserve),
            };
            if (CouncilMeetingData.HasMeetingHappened(CouncilMeetingKeys.MeetingTownCleanup))
            {
                meetings.Add(new CouncilMeetingData(Game1.content.LoadString(DialogueKeys.CouncilMeeting.MeetingRiverCleanup), CouncilMeetingKeys.MeetingRiverCleanup));
            }
            if (!Game1.MasterPlayer.eventsSeen.Contains(CompatibilityKeys.MorrisIsCampaigningForMayorEventID)) //SVE event for Mayor Morris
            {
                meetings.Add(new CouncilMeetingData(Game1.content.LoadString(DialogueKeys.CouncilMeeting.MeetingTownRoads), CouncilMeetingKeys.MeetingTownRoads));
            }
        }

        return meetings.Where(m=>!m.EventHasHappened).ToList();
    }

    /// <summary>
    /// Selects the next council meeting to run
    /// </summary>
    /// <param name="selectedMeeting">council meeting to plan</param>
    private static void OnCoucilMeetingSelected(CouncilMeetingData selectedMeeting )
    {
        ModProgressHandler.AddProgressFlag(CouncilMeetingKeys.PlannedPrefix + selectedMeeting.EventMailId);
        ModProgressHandler.AddProgressFlag(CouncilMeetingKeys.NotToday);
        Game1.exitActiveMenu();
        Game1.drawObjectDialogue(Game1.content.LoadString(DialogueKeys.CouncilMeeting.MeetingPlanned));
    }

    /// <summary>
    /// Check if council meeting is planned
    /// </summary>
    private static bool IsCouncilMeetingPlannded()
    {
        CheckForCouncilPlanningIssues();
        return Game1.MasterPlayer.mailReceived.Any(p => p.StartsWith(CouncilMeetingKeys.PlannedPrefix));
    }

    /// <summary>
    /// Remove events seen where planned event key exists
    /// </summary>
    private static void CheckForCouncilPlanningIssues()
    {
        var planned = Game1.MasterPlayer.mailReceived
                                        .Where(plannedMail => plannedMail.StartsWith(CouncilMeetingKeys.PlannedPrefix))
                                        .Select(mail => $"{ModKeys.MAYOR_MOD_CPID}_CouncilMeeting{mail[CouncilMeetingKeys.PlannedPrefix.Length..]}Event")
                                        .ToHashSet();
        Game1.MasterPlayer.eventsSeen.RemoveWhere(e => planned.Contains(e));
    }
}
