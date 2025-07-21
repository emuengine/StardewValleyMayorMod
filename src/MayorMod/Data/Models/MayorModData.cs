
using StardewModdingAPI.Utilities;
using StardewValley;

namespace MayorMod.Data.Models;

/// <summary>
/// This is the data model for saving MayorMod progess for a player
/// </summary>
public sealed class MayorModData
{
    public SDate VotingDate { get; set; } = new SDate(1, Season.Spring);
}
