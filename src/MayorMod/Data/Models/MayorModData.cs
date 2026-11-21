using StardewModdingAPI.Utilities;

namespace MayorMod.Data.Models;

/// <summary>
/// This is the data model for saving MayorMod progess for a player
/// </summary>
public sealed class MayorModData
{
    public Version? SaveVersion { get; set; }
    public SDate? VotingDate { get; set; }
    public string? GoldStaueBase64Image { get; set; }
    public int TownTreasury { get; set; } = 0;
    public List<string> OutstandingCouncilCosts { get; set; } = new List<string>();
    public List<string> CostsPaidThisMonth { get; set; } = new List<string>();
}
