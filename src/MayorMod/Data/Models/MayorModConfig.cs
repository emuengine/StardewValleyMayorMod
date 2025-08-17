namespace MayorMod.Data.Models;

public class MayorModConfig
{
    public int ThresholdForVote { get; set; } = 5;
    public int VoterPercentageNeeded { get; set; } = 50;
    public int NumberOfCampaignDays { get; set; } = 14;
    public bool[] MeetingDays { get; set; }  = new bool[] { false, false, false, false, true, false, false };
}
