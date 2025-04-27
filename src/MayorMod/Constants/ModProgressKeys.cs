namespace MayorMod.Constants;

public static class ModProgressKeys
{
    //public static readonly string RegisteringForBalot = $"{ModKeys.MayorModId}_EnableRegistrationOffice";
    //public static readonly string VotingMayor = $"{ModKeys.MayorModId}_EnableVotingBooth";
    //public static readonly string RunningForMayor = $"{ModKeys.MayorModId}_RunningForMayor";
    public static readonly string RegisteringForBalot = $"{ModKeys.MayorModCPId}_RegisteredForBallot";
    public static readonly string IsVotingDay = $"{ModKeys.MayorModCPId}_VotingDay";
    public static readonly string VotedForMayor = $"{ModKeys.MayorModCPId}_HasVoted";
    public static readonly string ElectedAsMayor = $"{ModKeys.MayorModCPId}_ElectedAsMayor";
    public static readonly string ManorHouseUnderConstruction = $"{ModKeys.MayorModCPId}_ManorHouseUnderConstruction";
}

