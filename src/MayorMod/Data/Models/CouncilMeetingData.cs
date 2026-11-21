using MayorMod.Constants;
using StardewValley;
using MayorMod.Data.Handlers;

namespace MayorMod.Data.Models;

public class CouncilMeetingData
{
    private static readonly Dictionary<string, string> AcceptanceFlags = new()
    {
        [CouncilMeetingKeys.MeetingSaloonHours] = ProgressFlags.SaloonHoursEarlyLate,
        [CouncilMeetingKeys.MeetingFundraiser] = ProgressFlags.FundraiserAgreed,
        [CouncilMeetingKeys.MeetingTownSecurity] = ProgressFlags.SecurityOnGuard
    };

    public string Name { get; set; }
    public string EventMailId { get; set; }
    public bool IsPaid { get; set; }

    public bool EventHasHappened => HasMeetingHappened(EventMailId);

    public string[] CostKeys
    {
        get
        {
            if (EventMailId == CouncilMeetingKeys.MeetingBusFare)
            {
                return new[] { TreasuryCostKeys.BusFareHalf, TreasuryCostKeys.BusFareFree };
            }

            if (TreasuryCostKeys.Costs.ContainsKey(EventMailId))
            {
                return new[] { EventMailId };
            }

            return Array.Empty<string>();
        }
    }

    /// <summary>
    /// Has meetings that can be reconsidered
    /// </summary>
    public bool CanReconsiderMeeting
    {
        get
        {
            if (!EventHasHappened || EventMailId == CouncilMeetingKeys.MeetingIntro)
            {
                return false;
            }

            var acceptedFlags = EventMailId == CouncilMeetingKeys.MeetingBusFare ? 
                                    CostKeys.Select(key => $"{ModKeys.MAYOR_MOD_CPID}_{key}"):
                                    new[] { AcceptanceFlags.GetValueOrDefault(EventMailId, $"{ModKeys.MAYOR_MOD_CPID}_{EventMailId}") };

            // Approved but unpaid meetings should be in the budget menu.
            return (!acceptedFlags.Any(flag =>
                       (Game1.MasterPlayer.mailReceived.Contains(flag) ||
                        Game1.MasterPlayer.mailReceived.Contains(ProgressFlags.DisabledCouncilEffectPrefix + flag)))) &&
                   !CostKeys.Any(cost => (SaveHandler.SaveData?.OutstandingCouncilCosts.Contains(cost) == true));
        }
    }

    public CouncilMeetingData(string name, string eventMailId)
    {
        Name = name;
        EventMailId = eventMailId;
    }

    public static bool HasMeetingHappened(string eventMailId)
    {
        return Game1.MasterPlayer.eventsSeen.Any(p => p == GetMeetingEventName(eventMailId));
    }

    public static string GetMeetingEventName(string eventMailId)
    {
        return $"{ModKeys.MAYOR_MOD_CPID}_CouncilMeeting{eventMailId}Event";
    }
}
