using MayorMod.Constants;
using StardewValley;

namespace MayorMod.Data.Models;

public class CouncilMeetingData
{
    public string Name { get; set; }
    public string EventMailId { get; set; }

    public bool EventHasHappened => HasMeetingHappened(EventMailId);

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
