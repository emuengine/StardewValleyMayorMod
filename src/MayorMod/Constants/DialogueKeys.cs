namespace MayorMod.Constants;

public static class DialogueKeys
{
    public static class VotingBooth
    {
        public static readonly string VotingBallotTitle = $"{ModKeys.XNBPathKeys.UI}:{ModKeys.MAYOR_MOD_CPID}_VotingBallotTitle";
        public static readonly string VotingBallotDescription = $"{ModKeys.XNBPathKeys.UI}:{ModKeys.MAYOR_MOD_CPID}_VotingBallotDescription";
    }

    public static class PollingData
    {
        public static readonly string PollingDataTitle = $"{ModKeys.XNBPathKeys.UI}:{ModKeys.MAYOR_MOD_CPID}_PollingDataTitle";
        public static readonly string PollingDataIntro = $"{ModKeys.XNBPathKeys.UI}:{ModKeys.MAYOR_MOD_CPID}_PollingDataIntro";
        public static readonly string HadDebate = $"{ModKeys.XNBPathKeys.UI}:{ModKeys.MAYOR_MOD_CPID}_HadDebate";
        public static readonly string Leaflets = $"{ModKeys.XNBPathKeys.UI}:{ModKeys.MAYOR_MOD_CPID}_Leaflets";
        public static readonly string VotersCanvassed = $"{ModKeys.XNBPathKeys.UI}:{ModKeys.MAYOR_MOD_CPID}_VotersCanvassed";
        public static readonly string VotingForYou = $"{ModKeys.XNBPathKeys.UI}:{ModKeys.MAYOR_MOD_CPID}_VotingForYou";
        public static readonly string PollingDataVotingDayWinning = $"{ModKeys.XNBPathKeys.UI}:{ModKeys.MAYOR_MOD_CPID}_PollingDataVotingDayWinning";
        public static readonly string PollingDataVotingDayLosing = $"{ModKeys.XNBPathKeys.UI}:{ModKeys.MAYOR_MOD_CPID}_PollingDataVotingDayLosing";
    }

    public static class CouncilMeeting
    {
        public static readonly string MeetingPlanned = $"{ModKeys.XNBPathKeys.UI}:{ModKeys.MAYOR_MOD_CPID}_MeetingPlanned";
        public static readonly string HoldCouncilMeeting = $"{ModKeys.XNBPathKeys.UI}:{ModKeys.MAYOR_MOD_CPID}_HoldCouncilMeeting";
        public static readonly string AgendaQuestion = $"{ModKeys.XNBPathKeys.UI}:{ModKeys.MAYOR_MOD_CPID}_AgendaQuestion";
        public static readonly string AlreadyPlanned = $"{ModKeys.XNBPathKeys.UI}:{ModKeys.MAYOR_MOD_CPID}_AlreadyPlanned";
        public static readonly string NoNewMeetings = $"{ModKeys.XNBPathKeys.UI}:{ModKeys.MAYOR_MOD_CPID}_NoNewMeetings";
        public static readonly string MeetingIntro = $"{ModKeys.XNBPathKeys.UI}:{ModKeys.MAYOR_MOD_CPID}_MeetingIntro";
        public static readonly string MeetingSaloonHours = $"{ModKeys.XNBPathKeys.UI}:{ModKeys.MAYOR_MOD_CPID}_MeetingSaloonHours";
        public static readonly string MeetingTownSecurity = $"{ModKeys.XNBPathKeys.UI}:{ModKeys.MAYOR_MOD_CPID}_MeetingTownSecurity";
        public static readonly string MeetingTownCleanup = $"{ModKeys.XNBPathKeys.UI}:{ModKeys.MAYOR_MOD_CPID}_MeetingTownCleanup";
    }

    public static class OfficerMike
    {
        public static readonly string RegisterForBallot = $"{ModKeys.XNBPathKeys.DIALOGUE}/{ModKeys.MAYOR_MOD_CPID}_OfficerMike:RegisterForBallot";
        public static readonly string CheckId = $"{ModKeys.XNBPathKeys.DIALOGUE}/{ModKeys.MAYOR_MOD_CPID}_OfficerMike:CheckId";
        public static readonly string GetBallot = $"{ModKeys.XNBPathKeys.DIALOGUE}/{ModKeys.MAYOR_MOD_CPID}_OfficerMike:GetBallot";
        public static readonly string NeedToFillBallot = $"{ModKeys.XNBPathKeys.DIALOGUE}/{ModKeys.MAYOR_MOD_CPID}_OfficerMike:NeedToFillBallot";
        public static readonly string HaveVoted = $"{ModKeys.XNBPathKeys.DIALOGUE}/{ModKeys.MAYOR_MOD_CPID}_OfficerMike:HaveVoted";
        public static readonly string CantCarryBallot = $"{ModKeys.XNBPathKeys.DIALOGUE}/{ModKeys.MAYOR_MOD_CPID}_OfficerMike:CantCarryBallot";
        public static readonly string NeedBallot = $"{ModKeys.XNBPathKeys.DIALOGUE}/{ModKeys.MAYOR_MOD_CPID}_OfficerMike:NeedBallot";
        public static readonly string NeedToVote = $"{ModKeys.XNBPathKeys.DIALOGUE}/{ModKeys.MAYOR_MOD_CPID}_OfficerMike:NeedToVote";
    }
}

