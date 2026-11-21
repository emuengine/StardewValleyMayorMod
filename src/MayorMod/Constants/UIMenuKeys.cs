namespace MayorMod.Constants;

public static class UIMenuKeys
{
    private static readonly string UIMenuPrefix = $"{ModKeys.MAYOR_MOD_CPID}_UIMenu.";

    public static readonly string And = UIMenuPrefix + "And";
    public static readonly string Budget = UIMenuPrefix + "Budget";
    public static readonly string CouncilEffects = UIMenuPrefix + "CouncilEffects";
    public static readonly string CouncilEffectsDescription = UIMenuPrefix + "CouncilEffectsDescription";
    public static readonly string EffectOff = UIMenuPrefix + "EffectOff";
    public static readonly string EffectOn = UIMenuPrefix + "EffectOn";
    public static readonly string IncomeFundraiser = UIMenuPrefix + "IncomeFundraiser";
    public static readonly string MonthlyCosts = UIMenuPrefix + "MonthlyCosts";
    public static readonly string MonthlyTotal = UIMenuPrefix + "MonthlyTotal";
    public static readonly string NetTotal = UIMenuPrefix + "NetTotal";
    public static readonly string NoMonthlyCosts = UIMenuPrefix + "NoMonthlyCosts";
    public static readonly string OutstandingSummary = UIMenuPrefix + "OutstandingSummary";
    public static readonly string SelectCouncilMeeting = UIMenuPrefix + "SelectCouncilMeeting";
    public static readonly string TaxIncome = UIMenuPrefix + "TaxIncome";

    // ModConfig
    public static readonly string VotingOptions = UIMenuPrefix + "VotingOptions";
    public static readonly string VotingOptionsTooltip = UIMenuPrefix + "VotingOptions.Tooltip";
    public static readonly string VoteThreshold = UIMenuPrefix + "VoteThreshold";
    public static readonly string VoteThresholdTooltip = UIMenuPrefix + "VoteThreshold.Tooltip";
    public static readonly string VoterPercentageNeeded = UIMenuPrefix + "VoterPercentageNeeded";
    public static readonly string VoterPercentageNeededTooltip = UIMenuPrefix + "VoterPercentageNeeded.Tooltip";
    public static readonly string NumberOfCampaignDays = UIMenuPrefix + "NumberOfCampaignDays";
    public static readonly string NumberOfCampaignDaysTooltip = UIMenuPrefix + "NumberOfCampaignDays.Tooltip";
    public static readonly string CouncilMeetingDays = UIMenuPrefix + "CouncilMeetingDays";
    public static readonly string CouncilMeetingDaysTooltip = UIMenuPrefix + "CouncilMeetingDays.Tooltip";

    // Buttons
    public static readonly string Back = UIMenuPrefix + "Back";
    public static readonly string Options = UIMenuPrefix + "Options";
    public static readonly string Up = UIMenuPrefix + "Up";
    public static readonly string Down = UIMenuPrefix + "Down";
    public static readonly string Left = UIMenuPrefix + "Left";
    public static readonly string Right = UIMenuPrefix + "Right";
    public static readonly string Star = UIMenuPrefix + "Star";
    public static readonly string Target = UIMenuPrefix + "Target";

    // Town Hall
    public static readonly string ManorHouseBoughtDeed = UIMenuPrefix + "ManorHouse_BoughtDeed";
    public static readonly string ManorHouseDeedBook = UIMenuPrefix + "ManorHouse_DeedBook";
    public static readonly string ManorHouseDeedBookQuestion = UIMenuPrefix + "ManorHouse_DeedBookQuestion";
    public static readonly string ManorHouseNotEnoughMoney = UIMenuPrefix + "ManorHouse_NotEnoughMoney";

    // Resignation
    public static readonly string ResignDoubleCheck = UIMenuPrefix + "Resign_DoubleCheck";
    public static readonly string ResignQuestion = UIMenuPrefix + "Resign_Question";
    public static readonly string ResignResignText = UIMenuPrefix + "Resign_ResignText";

    // Days
    public static readonly string Monday = UIMenuPrefix + "Monday";
    public static readonly string Tuesday = UIMenuPrefix + "Tuesday";
    public static readonly string Wednesday = UIMenuPrefix + "Wednesday";
    public static readonly string Thursday = UIMenuPrefix + "Thursday";
    public static readonly string Friday = UIMenuPrefix + "Friday";
    public static readonly string Saturday = UIMenuPrefix + "Saturday";
    public static readonly string Sunday = UIMenuPrefix + "Sunday";

    public static string Day(string dayName) => UIMenuPrefix + dayName;

    // Businesses
    public static string Business(string name) => UIMenuPrefix + "Business" + name;
}
