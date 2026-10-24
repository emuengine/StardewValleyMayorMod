namespace MayorMod.Constants;

public static class TreasuryCostKeys
{
    public static readonly string FloodRepair = "FloodRepair";
    public static readonly string BusFareHalf = "BusFareHalf";
    public static readonly string BusFareFree = "BusFareFree";

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
        { BusFareHalf, 1000 },
        { BusFareFree, 2500 },
        { FloodRepair, 2000 },
    };
}
