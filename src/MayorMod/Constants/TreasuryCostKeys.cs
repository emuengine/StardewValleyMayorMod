namespace MayorMod.Constants;

public static class TreasuryCostKeys
{
    public static readonly string FloodRepair = "FloodRepair";

    /// <summary>
    /// Cost of everything that can be charged to the town treasury, by cost key
    /// </summary>
    public static readonly Dictionary<string, int> Costs = new()
    {
        { CouncilMeetingKeys.MeetingTownSecurity, 500 },
        { CouncilMeetingKeys.MeetingTownCleanup, 500 },
        { CouncilMeetingKeys.MeetingRiverCleanup, 300 },
        { CouncilMeetingKeys.MeetingTownRoads, 1500 },
        { CouncilMeetingKeys.MeetingStrategicReserve, 1000 },
        { FloodRepair, 2000 },
    };
}
