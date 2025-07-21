namespace MayorMod.Data.Models;

public class MayorModConfig
{
    public int ThresholdForVote { get; internal set; } = 5;
    public int VoterPercentageNeeded { get; set; } = 50;
    public int NumberOfCampaignDays { get; set; } = 14;
}
