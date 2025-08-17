using MayorMod.Constants;
using StardewValley;

namespace MayorMod.Data.Models;

public class CouncilMeetingData(string name, string eventMailId)
{
    public string Name { get; set; } = name;
    public string EventMailId { get; set; } = eventMailId;

    public bool EventHasHappened => HasMeetingHappened(eventMailId);

    public static bool HasMeetingHappened(string eventMailId)
    {
        return Game1.MasterPlayer.eventsSeen.Any(p => p == GetMeetingEventName(eventMailId));
    }

    public static string GetMeetingEventName(string eventMailId)
    {
        return $"{ModKeys.MAYOR_MOD_CPID}_CouncilMeeting{eventMailId}Event";
    }
}
