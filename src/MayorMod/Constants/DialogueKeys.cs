namespace MayorMod.Constants;

public static class DialogueKeys
{
    public static readonly string DialogueLocation = "Characters/Dialogue/";
    public static readonly string StringsFromMaps = "Strings/StringsFromMaps";

    public static class VotingBooth
    {
        public static readonly string VotingBallotTitle = $"{StringsFromMaps}:{ModKeys.MayorModCPId}_VotingBallotTitle";
    }

    public static class CouncilMeeting
    {
        public static readonly string MeetingPlanned = $"{StringsFromMaps}:{ModKeys.MayorModCPId}_MeetingPlanned";
        public static readonly string HoldCouncilMeeting = $"{StringsFromMaps}:{ModKeys.MayorModCPId}_HoldCouncilMeeting";
        public static readonly string AgendaQuestion = $"{StringsFromMaps}:{ModKeys.MayorModCPId}_AgendaQuestion";
        public static readonly string AlreadyPlanned = $"{StringsFromMaps}:{ModKeys.MayorModCPId}_AlreadyPlanned";
        public static readonly string NoNewMeetings = $"{StringsFromMaps}:{ModKeys.MayorModCPId}_NoNewMeetings";
        public static readonly string MeetingIntro = $"{StringsFromMaps}:{ModKeys.MayorModCPId}_MeetingIntro";
        public static readonly string MeetingSaloonHours = $"{StringsFromMaps}:{ModKeys.MayorModCPId}_MeetingSaloonHours";
        public static readonly string MeetingTownSecurity = $"{StringsFromMaps}:{ModKeys.MayorModCPId}_MeetingTownSecurity";
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

