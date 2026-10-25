namespace MayorMod.Constants;

public class CouncilMeetingKeys
{
    public static readonly string PlannedPrefix = $"{ModKeys.MAYOR_MOD_CPID}_CouncilMeetingPlanned";
    public static readonly string NotToday = $"{ModKeys.MAYOR_MOD_CPID}_CouncilMeetingNotToday";
    public static readonly string HeldPrefix = $"{ModKeys.MAYOR_MOD_CPID}_CouncilMeetingHeld";

    //Meetings
    public static readonly string MeetingIntro = "Intro";
    public static readonly string MeetingSaloonHours = "SaloonHours";
    public static readonly string MeetingTownSecurity = "TownSecurity";
    public static readonly string MeetingTownCleanup = "TownCleanup";
    public static readonly string MeetingRiverCleanup = "RiverCleanup";
    public static readonly string MeetingTownRoads = "TownRoads";
    public static readonly string MeetingStrategicReserve = "StrategicReserve";
    public static readonly string MeetingBusFare = "BusFare";
    public static readonly string MeetingHealthcareFunding = "HealthcareFunding";

    //Special Order Keys
    public static readonly string SpecialOrderStrategicReserve = $"{ModKeys.MAYOR_MOD_CPID}_StrategicReserveSpecialOrder";

    public static readonly string MailStrategicReserveRoomReady = $"{ModKeys.MAYOR_MOD_CPID}_StrategicReserveRoomReadyMail";
}
