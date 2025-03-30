namespace MayorMod;
public static class Constants
{
    public const string MayorModPrefix = "MayorMod";
    public readonly static string MayorModSaveKey = $"{MayorModPrefix}_SaveData";

    public static class ProgressKey
    {
        public readonly static string RunningForMayor = $"{MayorModPrefix}_RunningForMayor";
        public readonly static string ElectedAsMayor = $"{MayorModPrefix}_ElectedAsMayor";
        public readonly static string VotedForMayor = $"{MayorModPrefix}_Voted";
    }
    public static class ActionKey
    {
        public const string Action = "MayorModAction";
        public const string DeskAction = "Desk";
        public const string VotingBoothAction = "VotingBooth";
        public const string BallotBoxAction = "BallotBox";
    }
    public static class ItemKey
    {
        public const string Ballot = "MayorMod_Ballot";
        public const string BallotUsed = "MayorMod_BallotUsed";
        public const string ElectionSign = "MayorMod_ElectionSign";
    }
    public static class DialogueKey
    {
        public const string GetBallot = "Strings\\Characters:GetBallot";
        public const string NeedToFillBallot = "Strings\\Characters:NeedToFillBallot";
        public const string HaveVoted = "Strings\\Characters:HaveVoted";
        public const string CantCarryBallot = "Strings\\Characters:CantCarryBallot";
        public const string NeedBallot = "Strings\\Characters:NeedBallot";
        public const string NeedToVote = "Strings\\Characters:NeedToVote";
    }
    public static class AssetPath
    {
        public const string BallotTexturePath = "LooseSprites/Ballot";
    }
}
