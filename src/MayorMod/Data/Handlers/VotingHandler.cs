using MayorMod.Constants;
using MayorMod.Data.Models;
using StardewModdingAPI;
using StardewValley;

namespace MayorMod.Data.Handlers;

/// <summary>
/// Handles voting logic for the mayor mod.
/// </summary>
public class VotingHandler
{
    public const int PointsPerHeart = 250;
    public static readonly string TalkingToVotersTopic = $"{ModKeys.MAYOR_MOD_CPID}_TalkingToVotersTopic";
    public static readonly string MayorDebateEvent = $"{ModKeys.MAYOR_MOD_CPID}_MayorDebateEvent";
    public static readonly string LeafletItem = $"{ModKeys.MAYOR_MOD_CPID}_Leaflet";
    public static readonly IList<string> Voters = 
                                new List<string>(){"Alex","Elliott","Harvey","Sam","Sebastian","Shane",
                                                   "Abigail","Emily","Haley","Leah","Maru","Penny","Caroline",
                                                   "Clint","Demetrius","Evelyn","George","Gus","Jodi","Kent",
                                                   "Lewis","Linus","Marnie","Pam","Pierre","Robin","Willy","Wizard" };
    public static readonly IList<string> SVEVoters =
                                new List<string>(){"Claire", "Lance", "Olivia", "Sophia", "Victor", "Andy",
                                                   "Gunther", "Marlon", "Morris", "Susan"};
    private readonly Farmer _farmer;
    private readonly MayorModConfig _mayorModConfig;

    public bool IsVotingRNG { get; set; } = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="VotingHandler"/> class.
    /// </summary>
    /// <param name="farmer">The current farmer instance.</param>
    /// <param name="mayorModConfig">The current mayor mod config instance.</param>
    public VotingHandler(Farmer farmer, MayorModConfig mayorModConfig)
    {
        _farmer = farmer;
        _mayorModConfig = mayorModConfig;
    }

    /// <summary>
    /// Gets the number of hearts an NPC has.
    /// </summary>
    /// <param name="name">The name of the NPC.</param>
    /// <returns>The number of hearts the NPC has.</returns>
    public int GetNPCHearts(string name)
    {
        int hearts = 0;
        if (_farmer.friendshipData.FieldDict.TryGetValue(name, out var popularity))
        {
            hearts = popularity.TargetValue.Points / PointsPerHeart;
        }
        return hearts;
    }

    /// <summary>
    /// Checks whether an NPC has been canvassed.
    /// </summary>
    /// <param name="name">The name of the NPC.</param>
    /// <returns>True if the NPC has been canvassed, false otherwise.</returns>
    public bool HasNPCBeenCanvassed(string name)
    {
        return _farmer.mailReceived.Any(m => m.Trim().Equals($"{name}_{TalkingToVotersTopic}" ,StringComparison.InvariantCultureIgnoreCase));
    }

    /// <summary>
    /// Checks whether an NPC has received a leaflet.
    /// </summary>
    /// <param name="name">The name of the NPC.</param>
    /// <returns>True if the NPC has received a leaflet, false otherwise.</returns>
    public bool HasNPCGotLeaflet(string name)
    {
        bool hasLeaflet = false;
        if (_farmer.giftedItems.TryGetValue(name, out var gifts))
        {
            hasLeaflet = gifts.ContainsKey(LeafletItem);
        }
        return hasLeaflet;
    }

    /// <summary>
    /// Checks whether the player has won the debate event.
    /// </summary>
    /// <returns>True if the player has won the debate event, false otherwise.</returns>
    public bool HasWonDebate()
    {
        return _farmer.eventsSeen.Any(e => e.Equals(MayorDebateEvent));
    }

    /// <summary>
    /// Calculates the number of votes the player has cast.
    /// </summary>
    /// <returns>The number of votes the player has cast.</returns>
    public int CalculatePlayerVotes()
    {
        var votes = 0;
        if (Game1.IsMultiplayer)
        {
            //TODO: Count votes for multiplayer
        }
        if (ModProgressHandler.HasProgressFlag(ProgressFlags.VotedForMayor)) 
        {
            //If you dont for for yourself you're voting for Lewis
            votes = ModProgressHandler.HasProgressFlag(ProgressFlags.HasVotedForHostFarmer) ? 1 : -1;
        }
        return votes;
    }

    /// <summary>
    /// Checks whether an NPC is voting for the player.
    /// </summary>
    /// <param name="name">The name of the NPC.</param>
    /// <returns>True if the NPC is voting for the player, false otherwise.</returns>
    public bool VotingForFarmer(string name)
    {
        if (name.Equals(ModNPCKeys.MarlonId, StringComparison.InvariantCultureIgnoreCase))
        {
            return true;
        }

        if (name.Equals(ModNPCKeys.GusId, StringComparison.InvariantCultureIgnoreCase) &&
            ModProgressHandler.HasProgressFlag(ProgressFlags.GusVotingForYou))
        {
            return true;
        }
        var easyVotes = new List<string> { ModNPCKeys.GusId, ModNPCKeys.PennyId, ModNPCKeys.MaruId };

        var hearts = GetNPCHearts(name);
        hearts += HasNPCBeenCanvassed(name) ? 1 : 0;
        hearts += HasNPCGotLeaflet(name) ? 1 : 0;
        hearts += HasWonDebate() ? 1 : 0;
        var threshold = _mayorModConfig.ThresholdForVote;
        threshold += name.Equals(ModNPCKeys.LewisId, StringComparison.InvariantCultureIgnoreCase) ? 3 : 0;
        threshold -= easyVotes.Contains(name) ? 2 : 0;
        if (IsVotingRNG)
        {
            return hearts * (1.0 / threshold) > ModUtils.RNG.NextDouble();
        }
        else
        {
            return hearts > threshold;
        }
    }

    /// <summary>
    /// Calculates the total number of leaflets received by voters.
    /// </summary>
    /// <param name="helper">The mod helper instance.</param>
    /// <returns>The total number of leaflets received by voters.</returns>
    public int CalculateTotalLeaflets(IModHelper helper)
    {
        var voters = GetVotingVillagers(helper);
        return voters.Sum(v => HasNPCGotLeaflet(v) ? 1 : 0);
    }

    /// <summary>
    /// Gets all eligible voters.
    /// </summary>
    /// <param name="helper">The mod helper instance.</param>
    /// <returns>A list of all eligible voters.</returns>
    public static List<string> GetVotingVillagers(IModHelper helper)
    {
        var villagers = Voters.ToList();
        if (helper.ModRegistry.IsLoaded(CompatibilityKeys.SVE_MOD_ID))
        {
            villagers.AddRange(SVEVoters);
        }
        return villagers;
    }

    /// <summary>
    /// Calculates the total number of votes cast by voters.
    /// </summary>
    /// <param name="helper">The mod helper instance.</param>
    /// <returns>The total number of votes cast by voters.</returns>
    public int CalculateTotalVotes(IModHelper helper)
    {
        var voters = GetVotingVillagers(helper);
        var votes = voters.Sum(v => VotingForFarmer(v) ? 1 : 0);
        votes += CalculatePlayerVotes();
        return votes;
    }

    /// <summary>
    /// Checks whether the player has won the election.
    /// </summary>
    /// <param name="helper">The mod helper instance.</param>
    /// <returns>True if the player has won the election, false otherwise.</returns>
    public bool HasWonElection(IModHelper helper)
    {
        var voters = GetVotingVillagers(helper);
        var threshold = voters.Count * (_mayorModConfig.VoterPercentageNeeded/100.0);
        return CalculateTotalVotes(helper) > threshold;
    }
}
