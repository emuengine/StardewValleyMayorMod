namespace MayorMod.Constants;

public static class ModNPCKeys
{
    public static readonly string OfficerMikeId = $"{ModKeys.MAYOR_MOD_CPID}_OfficerMike";

    // Vanilla Core NPCs
    public static readonly string MarlonId = "Marlon";
    public static readonly string LewisId = "Lewis";
    public static readonly string MorrisId = "Morris";
    public static readonly string GusId = "Gus";
    public static readonly string MaruId = "Maru";
    public static readonly string PennyId = "Penny";
    public static readonly string AlexId = "Alex";
    public static readonly string ElliottId = "Elliott";
    public static readonly string HarveyId = "Harvey";
    public static readonly string SamId = "Sam";
    public static readonly string SebastianId = "Sebastian";
    public static readonly string ShaneId = "Shane";
    public static readonly string AbigailId = "Abigail";
    public static readonly string EmilyId = "Emily";
    public static readonly string HaleyId = "Haley";
    public static readonly string LeahId = "Leah";
    public static readonly string CarolineId = "Caroline";
    public static readonly string ClintId = "Clint";
    public static readonly string DemetriusId = "Demetrius";
    public static readonly string EvelynId = "Evelyn";
    public static readonly string GeorgeId = "George";
    public static readonly string JodiId = "Jodi";
    public static readonly string KentId = "Kent";
    public static readonly string LinusId = "Linus";
    public static readonly string MarnieId = "Marnie";
    public static readonly string PamId = "Pam";
    public static readonly string PierreId = "Pierre";
    public static readonly string RobinId = "Robin";
    public static readonly string WillyId = "Willy";
    public static readonly string WizardId = "Wizard";

    // SVE NPCs
    public static readonly string ClaireId = "Claire";
    public static readonly string LanceId = "Lance";
    public static readonly string OliviaId = "Olivia";
    public static readonly string SophiaId = "Sophia";
    public static readonly string VictorId = "Victor";
    public static readonly string AndyId = "Andy";
    public static readonly string SusanId = "Susan";
    public static readonly string GuntherId = "Gunther";

    // Voter Lists
    public static readonly IList<string> VanillaVoters = new List<string>
    {
        AlexId, ElliottId, HarveyId, SamId, SebastianId, ShaneId,
        AbigailId, EmilyId, HaleyId, LeahId, MaruId, PennyId, CarolineId,
        ClintId, DemetriusId, EvelynId, GeorgeId, GusId, JodiId, KentId,
        LewisId, LinusId, MarnieId, PamId, PierreId, RobinId, WillyId, WizardId
    };

    public static readonly IList<string> SVEVoters = new List<string>
    {
        ClaireId, LanceId, OliviaId, SophiaId, VictorId, AndyId,
        GuntherId, MarlonId, MorrisId, SusanId
    };
}
