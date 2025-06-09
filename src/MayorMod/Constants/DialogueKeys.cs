namespace MayorMod.Constants;

public static class DialogueKeys
{
    public static readonly string DialogueLocation = "Characters/Dialogue/";
    public static readonly string UIStringsLocation = "Strings/UI";

    public static class VotingBooth
    {
        public static readonly string VotingBallotTitle = $"{UIStringsLocation}:{ModKeys.MayorModCPId}_VotingBallotTitle";
        public static readonly string VotingBallotDescription = $"{UIStringsLocation}:{ModKeys.MayorModCPId}_VotingBallotDescription";
    }

    public static class PollingData
    {
        public static readonly string PollingDataTitle = $"{UIStringsLocation}:{ModKeys.MayorModCPId}_PollingDataTitle";
        public static readonly string PollingDataIntro = $"{UIStringsLocation}:{ModKeys.MayorModCPId}_PollingDataIntro";
        public static readonly string HadDebate = $"{UIStringsLocation}:{ModKeys.MayorModCPId}_HadDebate";
        public static readonly string Leaflets = $"{UIStringsLocation}:{ModKeys.MayorModCPId}_Leaflets";
        public static readonly string VotersCanvassed = $"{UIStringsLocation}:{ModKeys.MayorModCPId}_VotersCanvassed";
        public static readonly string VotingForYou = $"{UIStringsLocation}:{ModKeys.MayorModCPId}_VotingForYou";
        public static readonly string PollingDataVotingDayWinning = $"{UIStringsLocation}:{ModKeys.MayorModCPId}_PollingDataVotingDayWinning";
        public static readonly string PollingDataVotingDayLosing = $"{UIStringsLocation}:{ModKeys.MayorModCPId}_PollingDataVotingDayLosing";
    }

    public static class CouncilMeeting
    {
        public static readonly string MeetingPlanned = $"{UIStringsLocation}:{ModKeys.MayorModCPId}_MeetingPlanned";
        public static readonly string HoldCouncilMeeting = $"{UIStringsLocation}:{ModKeys.MayorModCPId}_HoldCouncilMeeting";
        public static readonly string AgendaQuestion = $"{UIStringsLocation}:{ModKeys.MayorModCPId}_AgendaQuestion";
        public static readonly string AlreadyPlanned = $"{UIStringsLocation}:{ModKeys.MayorModCPId}_AlreadyPlanned";
        public static readonly string NoNewMeetings = $"{UIStringsLocation}:{ModKeys.MayorModCPId}_NoNewMeetings";
        public static readonly string MeetingIntro = $"{UIStringsLocation}:{ModKeys.MayorModCPId}_MeetingIntro";
        public static readonly string MeetingSaloonHours = $"{UIStringsLocation}:{ModKeys.MayorModCPId}_MeetingSaloonHours";
        public static readonly string MeetingTownSecurity = $"{UIStringsLocation}:{ModKeys.MayorModCPId}_MeetingTownSecurity";
        public static readonly string MeetingTownCleanup = $"{UIStringsLocation}:{ModKeys.MayorModCPId}_MeetingTownCleanup";
    }

    public static class OfficerMike
    {
        public static readonly string RegisterForBallot = $"{DialogueLocation}{ModKeys.MayorModCPId}_OfficerMike:RegisterForBallot";
        public static readonly string CheckId = $"{DialogueLocation}{ModKeys.MayorModCPId}_OfficerMike:CheckId";
        public static readonly string GetBallot = $"{DialogueLocation}{ModKeys.MayorModCPId}_OfficerMike:GetBallot";
        public static readonly string NeedToFillBallot = $"{DialogueLocation}{ModKeys.MayorModCPId}_OfficerMike:NeedToFillBallot";
        public static readonly string HaveVoted = $"{DialogueLocation}{ModKeys.MayorModCPId}_OfficerMike:HaveVoted";
        public static readonly string CantCarryBallot = $"{DialogueLocation}{ModKeys.MayorModCPId}_OfficerMike:CantCarryBallot";
        public static readonly string NeedBallot = $"{DialogueLocation}{ModKeys.MayorModCPId}_OfficerMike:NeedBallot";
        public static readonly string NeedToVote = $"{DialogueLocation}{ModKeys.MayorModCPId}_OfficerMike:NeedToVote";
    }
}

