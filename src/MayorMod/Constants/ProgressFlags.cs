namespace MayorMod.Constants;

public static class ProgressFlags
{
    public static readonly string RunningForMayor = $"{ModKeys.MAYOR_MOD_CPID}_RunningForMayor";
    public static readonly string RegisterVotingDate = $"{ModKeys.MAYOR_MOD_CPID}_RegisterVotingDate";
    public static readonly string IsVotingDay = $"{ModKeys.MAYOR_MOD_CPID}_VotingDay";
    public static readonly string VotedForMayor = $"{ModKeys.MAYOR_MOD_CPID}_HasVoted";
    public static readonly string HasVotedForHostFarmer = $"{ModKeys.MAYOR_MOD_CPID}_HasVotedForHostFarmer";
    public static readonly string WonMayorElection = $"{ModKeys.MAYOR_MOD_CPID}_WonMayorElection";
    public static readonly string LostMayorElection = $"{ModKeys.MAYOR_MOD_CPID}_LostMayorElection";
    public static readonly string ElectedAsMayor = $"{ModKeys.MAYOR_MOD_CPID}_ElectedAsMayor";
    public static readonly string TownCleanup = $"{ModKeys.MAYOR_MOD_CPID}_TownCleanup";
    public static readonly string GusVotingForYou = $"{ModKeys.MAYOR_MOD_CPID}_CampaignMail";
    public static readonly string CompleteTrashBearWorldState = "trashBearDone";
    public static readonly string NeedMayorRetryEvent = $"{ModKeys.MAYOR_MOD_CPID}_NeedMayorRetryEvent";
}