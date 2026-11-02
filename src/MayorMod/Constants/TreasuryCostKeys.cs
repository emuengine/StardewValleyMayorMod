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
        { CouncilMeetingKeys.MeetingTownRoads, 2400 },
        { CouncilMeetingKeys.MeetingStrategicReserve, 700 },
        { CouncilMeetingKeys.MeetingHealthcareFunding, 400 },
        { BusFareHalf, 700 },
        { BusFareFree, 1500 },
        { ConstructionGrant, 2000 },
        { FloodRepair, 3000 },
    };
}
