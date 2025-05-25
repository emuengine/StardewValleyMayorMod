using MayorMod.Constants;
using StardewValley;

namespace MayorMod.Data.Models;

public class CouncilMeetingData(string name, string eventMailId)
{
    public string Name { get; set; } = name;
    public string EventMailId { get; set; } = eventMailId;

    public bool EventHasHappened
    {
        get
        {
            return Game1.MasterPlayer.mailReceived.Any(p => p == CouncilMeetingKeys.HeldPrefix + EventMailId);
        }
    }
}
