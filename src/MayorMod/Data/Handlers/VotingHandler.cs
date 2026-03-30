using MayorMod.Constants;
using MayorMod.Data.Models;
using MayorMod.Data.Utilities;
using StardewModdingAPI;
using StardewValley;
using System.Text.Json;

namespace MayorMod.Data.Handlers;

/// <summary>
/// Handles voting logic for the mayor mod.
/// </summary>
public static class VotingHandler
{
    private static IMod _mod = null!;
    private static Farmer _candidateFarmer = Game1.MasterPlayer;
    public const int POINTS_PER_HEART = 250;
    public static bool Loaded => _mod != null && _candidateFarmer != null;

    public static void Init(IMod mod)
    {
        _mod = mod;
    }

    /// <summary>
    /// Gets the number of hearts an NPC has.
    /// </summary>
    /// <param name="name">The name of the NPC.</param>
    /// <returns>The number of hearts the NPC has.</returns>
    public static int GetNPCHearts(string name)
    {
        int hearts = 0;
        if (_candidateFarmer.friendshipData.FieldDict.TryGetValue(name, out var popularity))
        {
            hearts = popularity.TargetValue.Points / POINTS_PER_HEART;
        }
        return hearts;
    }

    /// <summary>
    /// Checks whether an NPC has been canvassed.
    /// </summary>
    /// <param name="name">The name of the NPC.</param>
    /// <returns>True if the NPC has been canvassed, false otherwise.</returns>
    public static bool HasNPCBeenCanvassed(string name)
    {
        return _candidateFarmer.mailReceived.Any(m => m.Trim().Equals($"{name}_{ConversationTopicKeys.TalkingToVotersTopic}", StringComparison.InvariantCultureIgnoreCase));
    }

    /// <summary>
    /// Checks whether an NPC has received a leaflet.
    /// </summary>
    /// <param name="name">The name of the NPC.</param>
    /// <returns>True if the NPC has received a leaflet, false otherwise.</returns>
    public static bool HasNPCGotLeaflet(string name)
    {
        bool hasLeaflet = false;
        if (_candidateFarmer.giftedItems.TryGetValue(name, out var gifts))
        {
            hasLeaflet = gifts.ContainsKey(ModItemKeys.Leaflet);
        }
        return hasLeaflet;
    }

    /// <summary>
    /// Checks whether the player has won the debate event.
    /// </summary>
    /// <returns>True if the player has won the debate event, false otherwise.</returns>
    public static bool HasWonDebate()
    {
        return _candidateFarmer.eventsSeen.Any(e => e.Equals(ProgressFlags.MayorDebateEvent));
    }

    /// <summary>
    /// Calculates the number of votes by players (Host & farmhands).
    /// </summary>
    /// <returns>The number of votes for the player.</returns>
    public static int CalculatePlayerVotes(IModHelper helper)
    {
        if (!Context.IsWorldReady)
        {
            return 0;
        }

        var existingVotesJson = ModUtils.GetFarmModData(MultiplayerKeys.PLAYER_VOTES);
        var allVotes = !string.IsNullOrEmpty(existingVotesJson) ?
            JsonSerializer.Deserialize<List<PlayerVote>>(existingVotesJson) ?? new List<PlayerVote>() : 
            new List<PlayerVote>();

        var votesFor = allVotes.Count(v => v.VotedFor.Equals(Game1.MasterPlayer.Name, StringComparison.InvariantCultureIgnoreCase));
        var votesAgainst = allVotes.Count(v => !v.VotedFor.Equals(Game1.MasterPlayer.Name, StringComparison.InvariantCultureIgnoreCase));
        var voteNumber = votesFor - votesAgainst;
        return voteNumber;
    }

    /// <summary>
    /// Adds a vote for a specified player from the given farmer in the multiplayer farm data.
    /// </summary>
    /// <param name="voter">The farmer submitting the vote.</param>
    /// <param name="votedFor">The unique identifier of the player being voted for.</param>
    /// <returns>true if the vote was successfully recorded; otherwise, false.</returns>
    public static void AddPlayerVote(Farmer voter, string votedFor)
    {
        if (!Context.IsWorldReady)
        {
            return;
        }

        var playerVote = new PlayerVote()
        {
            PlayerID = voter.UniqueMultiplayerID,
            VotedFor = votedFor
        };

        var existingVotesJson = ModUtils.GetFarmModData(MultiplayerKeys.PLAYER_VOTES);
        existingVotesJson = string.IsNullOrEmpty(existingVotesJson) ? "[]" : existingVotesJson;
        var existingVotes = JsonSerializer.Deserialize<List<PlayerVote>>(existingVotesJson);
        existingVotes!.Add(playerVote);

        ModUtils.UpsertFarmModData(MultiplayerKeys.PLAYER_VOTES, JsonSerializer.Serialize(existingVotes));
    }

    /// <summary>
    /// Checks whether an NPC is voting for the player.
    /// </summary>
    /// <param name="name">The name of the NPC.</param>
    /// <returns>True if the NPC is voting for the player, false otherwise.</returns>
    public static bool VotingForFarmer(string name)
    {
        var thresholdToBeat = ModConfigHandler.ModConfig.ThresholdForVote;

        //Marlon is your manager so will always vote for you.
        if (name.Equals(ModNPCKeys.MarlonId, StringComparison.InvariantCultureIgnoreCase))
        {
            return true;
        }

        //Gus will vote for you if you opened that mail.
        if (name.Equals(ModNPCKeys.GusId, StringComparison.InvariantCultureIgnoreCase) &&
            ModProgressHandler.HasProgressFlag(ProgressFlags.GusVotingForYou))
        {
            return true;
        }

        //The mayor so he is harder get votes from.
        if (name.Equals(ModUtils.GetCurrentMayor(_mod.Helper), StringComparison.InvariantCultureIgnoreCase))
        {
            thresholdToBeat += 3;
        }

        //Town people who were on the council are easier to get votes from.
        var easyVotes = new List<string> { ModNPCKeys.GusId, ModNPCKeys.PennyId, ModNPCKeys.MaruId };
        if (easyVotes.Contains(name))
        {
            thresholdToBeat -= 2;
        }

        var votingPoints = GetNPCHearts(name);
        votingPoints += HasNPCBeenCanvassed(name) ? 1 : 0;
        votingPoints += HasNPCGotLeaflet(name) ? 1 : 0;
        votingPoints += HasWonDebate() ? 1 : 0;

        return votingPoints > thresholdToBeat;
    }

    /// <summary>
    /// Calculates the total number of leaflets received by voters.
    /// </summary>
    /// <param name="helper">The mod helper instance.</param>
    /// <returns>The total number of leaflets received by voters.</returns>
    public static int CalculateTotalLeaflets()
    {
        var voters = GetVotingVillagers();
        return voters.Sum(v => HasNPCGotLeaflet(v) ? 1 : 0);
    }

    /// <summary>
    /// Gets all eligible voters.
    /// </summary>
    /// <param name="helper">The mod helper instance.</param>
    /// <returns>A list of all eligible voters.</returns>
    public static List<string> GetVotingVillagers()
    {
        var villagers = ModNPCKeys.VanillaVoters.ToList();
        if (_mod.Helper.ModRegistry.IsLoaded(CompatibilityKeys.SVE_MOD_ID))
        {
            villagers.AddRange(ModNPCKeys.SVEVoters);
        }
        return villagers;
    }

    /// <summary>
    /// Calculates the total number of votes cast by voters.
    /// </summary>
    /// <returns>The total number of votes cast by voters.</returns>
    public static int CalculateTotalVotes()
    {
        var voters = GetVotingVillagers();
        var votes = voters.Sum(v => VotingForFarmer(v) ? 1 : 0);
        votes += CalculatePlayerVotes(_mod.Helper);
        return votes;
    }

    /// <summary>
    /// Checks whether the player has won the election.
    /// </summary>
    /// <returns>True if the player has won the election, false otherwise.</returns>
    public static bool HasWonElection()
    {
        var voters = GetVotingVillagers();
        var threshold = voters.Count * (ModConfigHandler.ModConfig.VoterPercentageNeeded / 100.0);
        var electionResult =  CalculateTotalVotes() > threshold;
        return electionResult;
    }

    /// <summary>
    /// Generates a formatted text summary of the voting results for the current mayoral election.
    /// </summary>
    /// <returns>The formatted voting results text.</returns>
    public static string GetVotingResultText()
    {
        var playerVotesJson = ModUtils.GetFarmModData(MultiplayerKeys.PLAYER_VOTES) ?? "[]";
        var playerVotes = JsonSerializer.Deserialize<List<PlayerVote>>(playerVotesJson);

        var totalVoters = GetVotingVillagers().Count + Game1.getAllFarmers().Count();
        var votesFor = CalculateTotalVotes();
        var votesAgainst = totalVoters - votesFor;
        var currentMayorName = ModUtils.GetCurrentMayor(_mod.Helper);
        return string.Format(ModUtils.GetTranslationForKey(_mod.Helper, $"{ModKeys.MAYOR_MOD_CPID}_Mail.OfficialElectionMail.ResultText"), votesFor, currentMayorName,votesAgainst);
    }
}
