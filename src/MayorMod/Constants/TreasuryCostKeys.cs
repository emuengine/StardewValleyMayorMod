namespace MayorMod.Constants;

public static class TreasuryCostKeys
{
    public static readonly string FloodRepair = "FloodRepair";
    public static readonly string BusFareHalf = "BusFareHalf";
    public static readonly string BusFareFree = "BusFareFree";
    public static readonly string ConstructionGrant = "ConstructionGrant";

    /// <summary>
    /// Cost of everything that can be charged to the town treasury, by cost key
    /// </summary>
    public static readonly Dictionary<string, int> Costs = new()
    {
        { CouncilMeetingKeys.MeetingTownSecurity, 300 },
        { CouncilMeetingKeys.MeetingTownCleanup, 300 },
        { CouncilMeetingKeys.MeetingRiverCleanup, 300 },
        { CouncilMeetingKeys.MeetingTownRoads, 3000 },
        { CouncilMeetingKeys.MeetingStrategicReserve, 1000 },
        { CouncilMeetingKeys.MeetingHealthcareFunding, 500 },
        { BusFareHalf, 1000 },
        { BusFareFree, 2500 },
        { ConstructionGrant, 2800 },
        { FloodRepair, 3000 },
    };
}
