namespace MayorMod;
public static class Constants
{
    public static readonly string  MayorModPrefix = "MayorMod";
    public static readonly string  MayorModCPPrefix = "EmuEngine.MayorModCP";
    public static readonly string  MayorModSaveKey = $"{MayorModPrefix}_SaveData";

    public static class ProgressKey
    {
        public static readonly string  RegisteringForBalot = $"{MayorModPrefix}_EnableRegistrationOffice";
        public static readonly string  VotingMayor = $"{MayorModPrefix}_EnableVotingBooth";
        public static readonly string  RunningForMayor = $"{MayorModPrefix}_RunningForMayor";
        public static readonly string  VotedForMayor = $"{MayorModPrefix}_HasVoted";
        public static readonly string  ElectedAsMayor = $"{MayorModPrefix}_ElectedAsMayor";
    }
    public static class ActionKey
    {
        public const string Action = "MayorModAction";
        public const string DeskAction = "Desk";
        public const string DeskRegisterAction = "DeskRegister";
        public const string VotingBoothAction = "VotingBooth";
        public const string BallotBoxAction = "BallotBox";
    }
    public static class ItemKey
    {

        public static readonly string Ballot = $"{MayorModCPPrefix}_Ballot";
        public static readonly string  BallotUsed = $"{MayorModCPPrefix}_BallotUsed";
        public static readonly string  ElectionSign = $"{MayorModCPPrefix}_ElectionSign";
    }
    public static class DialogueKey
    {
        public static readonly string  RegisterForBallot = "Characters/Dialogue/MayorMod_OfficerMike:RegisterForBallot";
        public static readonly string  GetBallot = "Characters/Dialogue/MayorMod_OfficerMike:GetBallot";
        public static readonly string  NeedToFillBallot = "Characters/Dialogue/MayorMod_OfficerMike:NeedToFillBallot";
        public static readonly string  HaveVoted = "Characters/Dialogue/MayorMod_OfficerMike:HaveVoted";
        public static readonly string  CantCarryBallot = "Characters/Dialogue/MayorMod_OfficerMike:CantCarryBallot";
        public static readonly string  NeedBallot = "Characters/Dialogue/MayorMod_OfficerMike:NeedBallot";
        public static readonly string  NeedToVote = "Characters/Dialogue/MayorMod_OfficerMike:NeedToVote";
    }
    public static class AssetPath
    {
        public static readonly string  BallotTexturePath = $"Mods/{MayorModCPPrefix}/Ballot";
    }
}
