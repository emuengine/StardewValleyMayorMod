using MayorMod.Constants;
using MayorMod.Data.Models;
using MayorMod.Data.Utilities;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Locations;

namespace MayorMod.Data.Handlers;

public static class ModProgressHandler
{
    private static IMod _mod = null!;

    public static void Init(IMod mod)
    {
        _mod = mod;
    }

    /// <summary>
    /// Check if player has a progress flag.
    /// </summary>
    /// <param name="flagId">Progress flag Id</param>
    /// <returns></returns>
    public static bool HasProgressFlag(string flagId)
    {
        if (Game1.player is null)
        {
            return false;
        }
        return Game1.player.mailReceived.Contains(flagId);
    }

    /// <summary>
    /// Check if host has a progress flag.
    /// </summary>
    /// <param name="flagId">Progress flag Id</param>
    /// <returns></returns>
    public static bool HasHostGotProgressFlag(string flagId)
    {
        if (Game1.MasterPlayer is null)
        {
            return false;
        }
        return Game1.MasterPlayer.mailReceived.Contains(flagId);
    }

    /// <summary>
    /// Add a progress flag to a player.
    /// </summary>
    /// <param name="flagId">Progress flag Id</param>
    public static void AddProgressFlag(string flagId)
    {
        if (!Game1.player.mailReceived.Contains(flagId))
        {
            Game1.player.mailReceived.Add(flagId);
        }
    }

    /// <summary>
    /// Remove a progress flag from a player.
    /// </summary>
    /// <param name="flagId">Progress flag Id</param>
    public static void RemoveProgressFlag(string flagId)
    {
        if (Game1.player.mailReceived.Contains(flagId))
        {
            Game1.player.mailReceived.Remove(flagId);
        }
    }

    /// <summary>
    /// Remove all MayorMod progress flags from a player.
    /// </summary>
    public static void RemoveAllModFlags()
    {
        Game1.player.mailReceived.RemoveWhere(m => m.Contains(ModKeys.MAYOR_MOD_CPID));
    }

    /// <summary>
    /// Check if player has a mail.
    /// </summary>
    /// <param name="flagId">Progress flag Id</param>
    /// <returns></returns>
    public static bool HasMail(Farmer farmer, string flagId)
    {
        if (farmer is null)
        {
            return false;
        }
        return farmer.mailReceived.Contains(flagId) ||
               farmer.mailForTomorrow.Contains(flagId)||
               farmer.mailbox.Contains(flagId);
    }


    public static void CampaignProgressUpdatesFarmhand()
    {
        if (Game1.IsMasterGame)
        {
            return;
        }

        if (HasHostGotProgressFlag(ProgressFlags.RunningForMayor))
        {
            //Farmhand updates for multiplayer
            if (HasMail(Game1.MasterPlayer, MailKeys.RegisteredForElectionMail) &&
                !HasMail(Game1.player, MailKeys.RegisteredForElectionFarmhandMail) &&
                !Game1.player.mailbox.Contains(MailKeys.RegisteredForElectionFarmhandMail))
            {
                Game1.player.mailbox.Add(MailKeys.RegisteredForElectionFarmhandMail);
                Game1.player.activeDialogueEvents.TryAdd(ConversationTopicKeys.MayorCampaignRunningTopic, 7);
            }

            var daysUntilVoting = ModUtils.GetVotingDate()?.DaysSinceStart - SDate.Now().DaysSinceStart;

            //Set mail for debate date
            if (daysUntilVoting < (ModKeys.DEBATE_DAY_OFFSET + 8))
            {
                if (!HasMail(Game1.player, MailKeys.DebateDateFarmhandMail))
                {
                    Game1.player.mailbox.Add(MailKeys.DebateDateFarmhandMail);
                }
            }
        }
    }

    /// <summary>
    /// Does updates related to voting day. 
    /// </summary>
    public static void CampaignProgressUpdates()
    {
        if (!Game1.IsMasterGame)
        {
            return;
        }

        if (HasProgressFlag(ProgressFlags.RegisterVotingDate) && SaveHandler.SaveData is not null)
        {
            //Set voting date
            VotingHandler.ResetElection();
            SaveHandler.UpdateVotingDate(ModUtils.GetDateWithoutFestival(ModConfigHandler.ModConfig.NumberOfCampaignDays));
            RemoveProgressFlag(ProgressFlags.RegisterVotingDate);
            AssetInvalidationHandler.QueueElectionInvalidations();
        }

        if (HasHostGotProgressFlag(ProgressFlags.RunningForMayor) && SaveHandler.SaveData?.VotingDate is not null)
        {
            var daysUntilVoting = SaveHandler.SaveData.VotingDate.DaysSinceStart - SDate.Now().DaysSinceStart;

            //Set mail for debate date
            if (daysUntilVoting < (ModKeys.DEBATE_DAY_OFFSET + 8))
            {
                if (!HasMail(Game1.MasterPlayer, MailKeys.DebateDate))
                {
                    Game1.MasterPlayer.mailbox.Add(MailKeys.DebateDate);
                }
            }

            //Debate is today
            if (daysUntilVoting == (ModKeys.DEBATE_DAY_OFFSET + 1))
            {
                Game1.MasterPlayer.mailReceived.Add(MailKeys.MayorDebate);
            }

            switch (daysUntilVoting)
            {
                //Set mail for day before voting
                case 2: { Game1.player.mailbox.Add(MailKeys.VoteTomorrow); } break;
                //Set as voting day
                case 1: { AddProgressFlag(ProgressFlags.IsVotingDay); } break;
                //Complete voting day
                case 0: { EndOfVotingDay(); } break;
            }
        }
    }

    /// <summary>
    /// Processes the end-of-day logic for the voting system, updating election results, player progress, and mailbox
    /// notifications as appropriate.
    /// </summary>
    private static void EndOfVotingDay()
    {
        ModUtils.UpsertFarmModData(MultiplayerKeys.ELECTION_RESULT, VotingHandler.GetVotingResultText());
        SaveHandler.UpdateVotingDate(null);
        RemoveAllModFlags();
        AssetInvalidationHandler.QueueElectionInvalidations();

        if (Game1.IsMasterGame)
        {
            if (VotingHandler.HasWonElection())
            {
                AddProgressFlag(ProgressFlags.WonMayorElection);
                Game1.MasterPlayer.mailbox.Add(MailKeys.WonOfficialElectioMail);
                Game1.MasterPlayer.mailbox.Add(MailKeys.WonElectionMail);
            }
            else
            {
                Game1.MasterPlayer.mailbox.Add(MailKeys.LoseOfficialElectionMail);
                AddProgressFlag(ProgressFlags.LostMayorElection);
            }
        }
    }

    /// <summary>
    /// Performs daily update actions related to the player's role as mayor, including progress flag management and
    /// triggering special events.
    /// </summary>
    public static void DayAsMayorUpdates()
    {
        // Complete day as mayor
        if (HasProgressFlag(ProgressFlags.ElectedAsMayor))
        {
            RemoveProgressFlag(ProgressFlags.WonMayorElection);
            RemoveProgressFlag(CouncilMeetingKeys.NotToday);

            // Finish Robin building
            if (HasProgressFlag(ProgressFlags.ConstructionGrant))
            {
                FinishRobinsConstruction();
            }

            // Tax stuff
            if (SDate.Now().Day == WorldDate.DaysPerMonth)
            {
                ModUtils.RemoveConversationTopic(ConversationTopicKeys.PayingTax);
                RemoveProgressFlag(ProgressFlags.FundraiserIncome);
                SaveHandler.SaveData?.CostsPaidThisMonth.Clear();
            }
            if (!ModUtils.HasConversationTopic(ConversationTopicKeys.PayingTax))
            {
                ModUtils.AddConversationTopic(ConversationTopicKeys.PayingTax, WorldDate.DaysPerMonth + 1);
            }
        }
    }

    /// <summary>
    /// Adds Lewis's shorts as a quest item to the Animal Shop when the player has been elected as mayor and enters the
    /// location.
    /// </summary>
    public static void AddLewisShortsToAnimalShop(GameLocation location)
    {
        //Add shorts to animal shop if mayor
        if (location.NameOrUniqueName == "AnimalShop" && HasProgressFlag(ProgressFlags.ElectedAsMayor))
        {
            if (location.farmers.Count == 0)
            {
                location.characters.Clear();
            }
            Vector2 shortsTile = new(11f, 7f);
            location.overlayObjects.Remove(shortsTile);
            var o = ItemRegistry.Create<StardewValley.Object>("(O)789");
            o.questItem.Value = true;
            o.TileLocation = shortsTile;
            o.IsSpawnedObject = true;
            location.overlayObjects.Add(shortsTile, o);
        }
    }

    /// <summary>
    /// Remove bushes on SVE town map if is voting day and need voting office
    /// </summary>
    /// <param name="e"></param>
    public static void RemoveTownBushesOnVotingDay(GameLocation location)
    {
        if (location.NameOrUniqueName == nameof(Town) && HasProgressFlag(ProgressFlags.IsVotingDay))
        {
            location.terrainFeatures.RemoveWhere(tf => tf.Key.Equals(new Vector2(30, 34)));
            location.terrainFeatures.RemoveWhere(tf => tf.Key.Equals(new Vector2(31, 35)));

            //TODO Look into if this is better to use
            //Utility.clearObjectsInArea(bounds, gameLocation);
        }
    }

    /// <summary>
    /// Reset mod if flagged
    /// </summary>
    public static void HandleModResetIfNeeded()
    {
        if (HasProgressFlag(ProgressFlags.ModNeedsReset))
        {
            ResignAndReset();
        }
    }

    /// <summary>
    /// Allow NeedMayorRetryEvent to repeat
    /// </summary>
    public static void UpdateIfMayorRetryNeeded()
    {
        if (Game1.player.eventsSeen.Contains(ProgressFlags.NeedMayorRetryEvent))
        {
            Game1.player.eventsSeen.Remove(ProgressFlags.NeedMayorRetryEvent);
        }
    }


    /// <summary>
    /// Reset the mod to factory settings.
    /// </summary>
    /// <param name="resetType">progress reset point</param>
    public static void ResignAndReset(ResetType resetType = ResetType.FreshInstall)
    {
        if (!Context.IsWorldReady)
        {
            _mod.Monitor.Log("Can't reset in while not in game.", LogLevel.Info);
            return;
        }

        _mod.Monitor.Log("Reseting mod to fresh install.", LogLevel.Info);

        _mod.Monitor.Log("Removing mayor mod flags.", LogLevel.Info);
        Game1.player.mailReceived.RemoveWhere(m => m.Contains(ModKeys.MAYOR_MOD_CPID));
        Game1.player.mailbox.RemoveWhere(m => m.Contains(ModKeys.MAYOR_MOD_CPID));
        Game1.player.mailForTomorrow.RemoveWhere(m => m.Contains(ModKeys.MAYOR_MOD_CPID));

        _mod.Monitor.Log("Removing mayor mod from events seen.", LogLevel.Info);
        Game1.player.eventsSeen.RemoveWhere(m => m.Contains(ModKeys.MAYOR_MOD_CPID));
        Game1.player.activeDialogueEvents.RemoveWhere(m => m.Key.Contains(ModKeys.MAYOR_MOD_CPID));
        Game1.player.previousActiveDialogueEvents.RemoveWhere(m => m.Key.Contains(ModKeys.MAYOR_MOD_CPID));
        Game1.player.triggerActionsRun.RemoveWhere(m => m.Contains(ModKeys.MAYOR_MOD_CPID));
        foreach (var item in Game1.player.giftedItems)
        {
            item.Value.RemoveWhere(g => g.Key.Contains(ModKeys.MAYOR_MOD_CPID));
        }

        _mod.Monitor.Log("Clearing save data.", LogLevel.Info);
        SaveHandler.ResetSave();
        VotingHandler.ResetElection();

        if (resetType == ResetType.NewMayor)
        {
            _mod.Monitor.Log("Resetting to be a newly elected mayor.", LogLevel.Info);
            Game1.player.eventsSeen.Add(ProgressFlags.MayorWinSpeechEvent);
            Game1.player.mailReceived.Add(MailKeys.WonOfficialElectioMail);
            Game1.player.mailReceived.Add(MailKeys.WonElectionMail);
            AddProgressFlag(ProgressFlags.ElectedAsMayor);
        }

        if (resetType == ResetType.VotingDay)
        {
            Game1.player.eventsSeen.Add(ProgressFlags.NeedMayorEvent);
            Game1.player.eventsSeen.Add(ProgressFlags.NeedMayorMorrisEvent);
            Game1.player.mailReceived.Add(MailKeys.RegisteredForElectionMail);
            Game1.player.mailReceived.Add(MailKeys.DebateDate);
            var votingDate = ModUtils.GetDateWithoutFestival(1);
            SaveHandler.UpdateVotingDate(votingDate);
            AddProgressFlag(ProgressFlags.RunningForMayor);
            _mod.Monitor.Log($"Voting is scheduled for {votingDate}.", LogLevel.Info);
        }

        AssetInvalidationHandler.QueueSaveInvalidations(true);

        _mod.Monitor.Log("Done.", LogLevel.Info);
        _mod.Monitor.Log("Please go to sleep and save and the mod should be reset.", LogLevel.Info);
    }

    /// <summary>
    /// Update the bus stop fare if changed by council.
    /// </summary>
    /// <param name="location"></param>
    public static void UpdateBusFare(GameLocation location)
    {
        if (location is BusStop busStop)
        {
            if (HasHostGotProgressFlag(ProgressFlags.BusFareFree))
            {
                busStop.TicketPrice = 0;
            }
            else if (HasHostGotProgressFlag(ProgressFlags.BusFareHalf))
            {
                busStop.TicketPrice = ModKeys.SUBSIDISED_BUS_FARE;
            }
            else
            {
                busStop.TicketPrice = ModKeys.DEFAULT_BUS_FARE;
            }
        }
    }

    /// <summary>
    /// If any building or upgrading finish immediatly.
    /// </summary>
    public static void FinishRobinsConstruction()
    {
        if (!Game1.netWorldState.Value.Builders.TryGetValue(ModNPCKeys.RobinId, out var builderData))
        {
            return;
        }

        var location = Game1.getLocationFromName(builderData.buildingLocation.Value);
        var building = location?.getBuildingAt(Utility.PointToVector2(builderData.buildingTile.Value));
        building?.FinishConstruction();
    }

    /// <summary>
    /// Finish farmers house upgrade overnight.
    /// </summary>
    public static void FinishHouseUpgradeOvernight()
    {
        if (HasHostGotProgressFlag(ProgressFlags.ConstructionGrant) && Game1.player.daysUntilHouseUpgrade.Value > 1)
        {
            Game1.player.daysUntilHouseUpgrade.Value = 1;
        }
    }
}
