namespace MayorMod.Constants;

public static class DialogueKeys
{
    public static readonly string DialogueLocation = "Characters/Dialogue/";
    public static readonly string StringsFromMaps = "Strings/StringsFromMaps";
    public static class CouncilMeeting
    {
        public static readonly string HoldCouncilMeeting = $"{StringsFromMaps}:{ModKeys.MayorModCPId}_HoldCouncilMeeting";
        public static readonly string AlreadyPlanned = $"{StringsFromMaps}:{ModKeys.MayorModCPId}_AlreadyPlanned";
        public static readonly string MeetingOption0 = $"{StringsFromMaps}:{ModKeys.MayorModCPId}_MeetingOption0";
        public static readonly string MeetingOption1 = $"{StringsFromMaps}:{ModKeys.MayorModCPId}_MeetingOption1";
        public static readonly string MeetingOption2 = $"{StringsFromMaps}:{ModKeys.MayorModCPId}_MeetingOption2";
        public static readonly string MeetingOption3 = $"{StringsFromMaps}:{ModKeys.MayorModCPId}_MeetingOption3";
        public static readonly string MeetingOption4 = $"{StringsFromMaps}:{ModKeys.MayorModCPId}_MeetingOption4";
        public static readonly string MeetingOption5 = $"{StringsFromMaps}:{ModKeys.MayorModCPId}_MeetingOption5";
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

