namespace MayorMod.Constants;

public static partial class ModKeys
{
    public const string MAYOR_MOD_ID = "EmuEngine.MayorMod";
    public const string MAYOR_MOD_CPID = "EmuEngine.MayorModCP";
    public const string SAVE_KEY = "EmuEngine.MayorMod_SaveData";
    public const string CONFIG_MENU_ID = "spacechase0.GenericModConfigMenu";
    public const string REPLACEMENT_STRING_CONFIG = "MayorAssetPatches.json";
    public const string RUBBISH_ID = "(O)167|(O)168|(O)169|(O)170|(O)171|(O)172";
    public const string MEETING_DAYS_TOKEN = "CouncilMeetingDays";
    public const string TREASURY_COST_TOKEN = "TreasuryCost";
    public const string VOTING_DAY_TOKEN = "VotingDay";
    public const string VOTING_RESULT_TOKEN = "VotingResultText";
    public const string DEBATE_DAY_TOKEN = "DebateDayText";
    public const int DEED_COST = 250000;
    public const int SUBSIDISED_BUS_FARE = 250;
    public const string RESET_COMMAND = "MayorModReset";
    public const string RESET_COMMAND_HELP_TEXT = "Resets the mod by removing all mail, events, and progress flags, restoring it to a fresh install state.\n\nUsage: MayorModReset [mayor]\n- mayor: reset to just after being elected.";
    public const string RESET_COMMAND_MAYOR_ARG = "mayor";
    public const string TREASURY_COMMAND = "MayorModTreasury";
    public const string TREASURY_COMMAND_HELP_TEXT = "Add/Remove gold to/from the town treasury\n\nUsage: MayorModTreasury <amount>";
    public const int DEBATE_DAY_OFFSET = 3;
}