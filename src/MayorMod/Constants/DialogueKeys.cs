namespace MayorMod.Constants;

public static class DialogueKeys
{
    public static readonly string DialogueLocation = "Characters/Dialogue/";

    public static class OfficerMike
    {
        public static readonly string RegisterForBallot = $"{DialogueLocation}{ModKeys.MayorModCPId}_OfficerMike:RegisterForBallot";
        public static readonly string GetBallot = $"{DialogueLocation}{ModKeys.MayorModCPId}_OfficerMike:GetBallot";
        public static readonly string NeedToFillBallot = $"{DialogueLocation}{ModKeys.MayorModCPId}_OfficerMike:NeedToFillBallot";
        public static readonly string HaveVoted = $"{DialogueLocation}{ModKeys.MayorModCPId}_OfficerMike:HaveVoted";
        public static readonly string CantCarryBallot = $"{DialogueLocation}{ModKeys.MayorModCPId}_OfficerMike:CantCarryBallot";
        public static readonly string NeedBallot = $"{DialogueLocation}{ModKeys.MayorModCPId}_OfficerMike:NeedBallot";
        public static readonly string NeedToVote = $"{DialogueLocation}{ModKeys.MayorModCPId}_OfficerMike:NeedToVote";
    }
}

