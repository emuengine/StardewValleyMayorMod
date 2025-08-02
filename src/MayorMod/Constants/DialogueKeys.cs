namespace MayorMod.Constants;

public static class DialogueKeys
{
    public static class VotingBooth
    {
        public static readonly string VotingBallotTitle = $"{XNBPathKeys.UI}:{ModKeys.MAYOR_MOD_CPID}_VotingBallotTitle";
        public static readonly string VotingBallotDescription = $"{XNBPathKeys.UI}:{ModKeys.MAYOR_MOD_CPID}_VotingBallotDescription";
    }

    public static class PollingData
    {
        public static readonly string PollingDataTitle = $"{XNBPathKeys.UI}:{ModKeys.MAYOR_MOD_CPID}_PollingDataTitle";
        public static readonly string PollingDataIntro = $"{XNBPathKeys.UI}:{ModKeys.MAYOR_MOD_CPID}_PollingDataIntro";
        public static readonly string HadDebate = $"{XNBPathKeys.UI}:{ModKeys.MAYOR_MOD_CPID}_HadDebate";
        public static readonly string Leaflets = $"{XNBPathKeys.UI}:{ModKeys.MAYOR_MOD_CPID}_Leaflets";
        public static readonly string VotersCanvassed = $"{XNBPathKeys.UI}:{ModKeys.MAYOR_MOD_CPID}_VotersCanvassed";
        public static readonly string VotingForYou = $"{XNBPathKeys.UI}:{ModKeys.MAYOR_MOD_CPID}_VotingForYou";
        public static readonly string PollingDataVotingDayWinning = $"{XNBPathKeys.UI}:{ModKeys.MAYOR_MOD_CPID}_PollingDataVotingDayWinning";
        public static readonly string PollingDataVotingDayLosing = $"{XNBPathKeys.UI}:{ModKeys.MAYOR_MOD_CPID}_PollingDataVotingDayLosing";
    }

    public static class CouncilMeeting
    {
        public static readonly string MeetingPlanned = $"{XNBPathKeys.UI}:{ModKeys.MAYOR_MOD_CPID}_MeetingPlanned";
        public static readonly string HoldCouncilMeeting = $"{XNBPathKeys.UI}:{ModKeys.MAYOR_MOD_CPID}_HoldCouncilMeeting";
        public static readonly string AgendaQuestion = $"{XNBPathKeys.UI}:{ModKeys.MAYOR_MOD_CPID}_AgendaQuestion";
        public static readonly string AlreadyPlanned = $"{XNBPathKeys.UI}:{ModKeys.MAYOR_MOD_CPID}_AlreadyPlanned";
        public static readonly string NoNewMeetings = $"{XNBPathKeys.UI}:{ModKeys.MAYOR_MOD_CPID}_NoNewMeetings";
        public static readonly string MeetingIntro = $"{XNBPathKeys.UI}:{ModKeys.MAYOR_MOD_CPID}_MeetingIntro";
        public static readonly string MeetingSaloonHours = $"{XNBPathKeys.UI}:{ModKeys.MAYOR_MOD_CPID}_MeetingSaloonHours";
        public static readonly string MeetingTownSecurity = $"{XNBPathKeys.UI}:{ModKeys.MAYOR_MOD_CPID}_MeetingTownSecurity";
        public static readonly string MeetingTownCleanup = $"{XNBPathKeys.UI}:{ModKeys.MAYOR_MOD_CPID}_MeetingTownCleanup";
        public static readonly string MeetingRiverCleanup = $"{XNBPathKeys.UI}:{ModKeys.MAYOR_MOD_CPID}_MeetingRiverCleanup";
        public static readonly string MeetingTownRoads = $"{XNBPathKeys.UI}:{ModKeys.MAYOR_MOD_CPID}_MeetingTownRoads";
    }

    public static class OfficerMike
    {
        public static readonly string RegisterForBallot = $"{XNBPathKeys.DIALOGUE}/{ModKeys.MAYOR_MOD_CPID}_OfficerMike:RegisterForBallot";
        public static readonly string CheckId = $"{XNBPathKeys.DIALOGUE}/{ModKeys.MAYOR_MOD_CPID}_OfficerMike:CheckId";
        public static readonly string GetBallot = $"{XNBPathKeys.DIALOGUE}/{ModKeys.MAYOR_MOD_CPID}_OfficerMike:GetBallot";
        public static readonly string NeedToFillBallot = $"{XNBPathKeys.DIALOGUE}/{ModKeys.MAYOR_MOD_CPID}_OfficerMike:NeedToFillBallot";
        public static readonly string HaveVoted = $"{XNBPathKeys.DIALOGUE}/{ModKeys.MAYOR_MOD_CPID}_OfficerMike:HaveVoted";
        public static readonly string CantCarryBallot = $"{XNBPathKeys.DIALOGUE}/{ModKeys.MAYOR_MOD_CPID}_OfficerMike:CantCarryBallot";
        public static readonly string NeedBallot = $"{XNBPathKeys.DIALOGUE}/{ModKeys.MAYOR_MOD_CPID}_OfficerMike:NeedBallot";
        public static readonly string NeedToVote = $"{XNBPathKeys.DIALOGUE}/{ModKeys.MAYOR_MOD_CPID}_OfficerMike:NeedToVote";
    }

    public static class Resignation
    {
        public static readonly string Question = $"{ModKeys.MAYOR_MOD_CPID}_UIMenu.Resign_Question";
        public static readonly string DoubleCheck = $"{ModKeys.MAYOR_MOD_CPID}_UIMenu.Resign_DoubleCheck";
        public static readonly string ResignText = $"{ModKeys.MAYOR_MOD_CPID}_UIMenu.Resign_ResignText";
    }
}

