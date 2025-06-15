using MayorMod.Constants;
using StardewValley;

namespace MayorMod.Data;

public class VotingManager
{
    public const int PointsPerHeart = 250;
    public static readonly string TalkingToVotersTopic = $"{ModKeys.MAYOR_MOD_CPID}_TalkingToVotersTopic";
    public static readonly string MayorDebateEvent = $"{ModKeys.MAYOR_MOD_CPID}_MayorDebateEvent";
    public static readonly string LeafletItem = $"{ModKeys.MAYOR_MOD_CPID}_Leaflet";
    public static readonly IList<string> Voters = ["Alex","Elliott","Harvey","Sam","Sebastian","Shane",
                                                   "Abigail","Emily","Haley","Leah","Maru","Penny","Caroline",
                                                   "Clint","Demetrius","Evelyn","George","Gus","Jodi","Kent",
                                                   "Lewis","Linus","Marnie","Pam","Pierre","Robin","Willy","Wizard"];
    private readonly Farmer _farmer;

    internal int HeartThreshold { get; set; } = 5;
    public bool IsVotingRNG { get; set; } = true;

    public VotingManager(Farmer farmer)
    {
        _farmer = farmer;
    }

    public int GetNPCHearts(string name)
    {
        int hearts = 0;
        if (_farmer.friendshipData.FieldDict.TryGetValue(name, out var popularity))
        {
            hearts = popularity.TargetValue.Points / PointsPerHeart;
        }
        return hearts;
    }

    public bool HasNPCBeenCanvassed(string name)
    {
        return _farmer.mailReceived.Any(m => m.Trim().Equals($"{name}_{TalkingToVotersTopic}" ,StringComparison.InvariantCultureIgnoreCase));
    }

    public bool HasNPCGotLeaflet(string name)
    {
        bool hasLeaflet = false;
        if (_farmer.giftedItems.TryGetValue(name, out var gifts))
        {
            hasLeaflet = gifts.ContainsKey(LeafletItem);
        }
        return hasLeaflet;
    }

    public bool HasWonDebate()
    {
        return _farmer.eventsSeen.Any(e => e.Equals(MayorDebateEvent));
    }

    public int CalculatePlayerVotes()
    {
        var votes = 0;
        if (Game1.IsMultiplayer)
        {
            //TODO: Count votes for multiplayer
        }
        if (votes > 0)
        {
            votes = ModProgressManager.HasProgressFlag(ModProgressManager.HasVotedForHostFarmer) ? 1 : -1;
        }
        return votes;
    }

    public bool VotingForFarmer(string name)
    {
        var hearts = GetNPCHearts(name);
        hearts += HasNPCBeenCanvassed(name) ? 1 : 0;
        hearts += HasNPCGotLeaflet(name) ? 1 : 0;
        hearts += HasWonDebate() ? 1 : 0;
        var threshold = HeartThreshold;
        threshold += name.Equals("Lewis", StringComparison.InvariantCultureIgnoreCase) ? 3 : 0;
        if (IsVotingRNG)
        {
            return (hearts * (1.0 / threshold)) > ModUtils.RNG.NextDouble();
        }
        else
        {
            return hearts > threshold;
        }
    }

    public int CalculateTotalLeaflets()
    {
        return Voters.Sum(v => HasNPCGotLeaflet(v) ? 1 : 0);
    }

    public int CalculateTotalVotes()
    {
        var votes =  Voters.Sum(v => VotingForFarmer(v) ? 1 : 0);
        votes += CalculatePlayerVotes();
        return votes;
    }

    public bool HasWonElection()
    {
        var threshold = Voters.Count / 2;
        return CalculateTotalVotes() > threshold;
    }
}
