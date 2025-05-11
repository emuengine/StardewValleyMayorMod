using MayorMod.Constants;
using StardewValley;

namespace MayorMod.Data.TileActions;

public static partial class ManorHouseTileActions
{
    public class CouncilMeetingData
    {
        public string Name { get; set; } 
        public string EventMailId { get; set; }

        public CouncilMeetingData(string name, string eventMailId)
        {
            Name = name;
            EventMailId = eventMailId;
        }

        public bool EventHasHappened
        {
            get
            {
                return Game1.MasterPlayer.mailReceived.Any(p => p == CouncilMeetingKeys.HeldPrefix + EventMailId);
            }
        }
    }
}
