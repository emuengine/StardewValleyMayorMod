﻿using MayorMod.Constants;
using MayorMod.Data.Models;
using StardewModdingAPI;
using StardewValley;

namespace MayorMod.Data.Handlers;

public class VotingHandler
{
    public const int PointsPerHeart = 250;
    public static readonly string TalkingToVotersTopic = $"{ModKeys.MAYOR_MOD_CPID}_TalkingToVotersTopic";
    public static readonly string MayorDebateEvent = $"{ModKeys.MAYOR_MOD_CPID}_MayorDebateEvent";
    public static readonly string LeafletItem = $"{ModKeys.MAYOR_MOD_CPID}_Leaflet";
    private static readonly IList<string> Voters = ["Alex","Elliott","Harvey","Sam","Sebastian","Shane",
                                                   "Abigail","Emily","Haley","Leah","Maru","Penny","Caroline",
                                                   "Clint","Demetrius","Evelyn","George","Gus","Jodi","Kent",
                                                   "Lewis","Linus","Marnie","Pam","Pierre","Robin","Willy","Wizard"];
    private static readonly IList<string> SVEVoters = ["Claire", "Lance", "Olivia", "Sophia", "Victor", "Andy", 
                                                      "Gunther", "Marlon", "Morris", "Susan"];
    private readonly Farmer _farmer;
    private readonly MayorModConfig _mayorModConfig;

    public bool IsVotingRNG { get; set; } = true;

    public VotingHandler(Farmer farmer, MayorModConfig mayorModConfig)
    {
        _farmer = farmer;
        _mayorModConfig = mayorModConfig;
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
        if (ModProgressHandler.HasProgressFlag(ProgressFlags.VotedForMayor)) 
        {
            //If you dont for for yourself you're voting for Lewis
            votes = ModProgressHandler.HasProgressFlag(ProgressFlags.HasVotedForHostFarmer) ? 1 : -1;
        }
        return votes;
    }

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
        IList<string> easyVotes = [ModNPCKeys.GusId, ModNPCKeys.PennyId, ModNPCKeys.MaruId];

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

    public int CalculateTotalLeaflets(IModHelper helper)
    {
        var voters = GetVotingVillagers(helper);
        return voters.Sum(v => HasNPCGotLeaflet(v) ? 1 : 0);
    }

    public static List<string> GetVotingVillagers(IModHelper helper)
    {
        var villagers = Voters.ToList();
        if (helper.ModRegistry.IsLoaded(CompatibilityKeys.SVE_MOD_ID))
        {
            villagers.AddRange(SVEVoters);
        }
        return villagers;
    }

    public int CalculateTotalVotes(IModHelper helper)
    {
        var voters = GetVotingVillagers(helper);
        var votes = voters.Sum(v => VotingForFarmer(v) ? 1 : 0);
        votes += CalculatePlayerVotes();
        return votes;
    }

    public bool HasWonElection(IModHelper helper)
    {
        var voters = GetVotingVillagers(helper);
        var threshold = voters.Count * (_mayorModConfig.VoterPercentageNeeded/100.0);
        return CalculateTotalVotes(helper) > threshold;
    }
}
