using MayorMod.Constants;
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
    public static bool RiverCleanUpRunOnce { get; set; } = true;

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
    /// Does updates related to voting day. 
    /// </summary>
    public static void CampaignProgressUpdates()
    {
        if (HasProgressFlag(ProgressFlags.RegisterVotingDate) && SaveHandler.SaveData is not null)
        {
            //Set voting date
            SaveHandler.UpdateVotingDate(ModUtils.GetDateWithoutFestival(ModConfigHandler.ModConfig.NumberOfCampaignDays));
            RemoveProgressFlag(ProgressFlags.RegisterVotingDate);
            AssetInvalidationHandler.UpdateAssetInvalidations();
        }

        if (SaveHandler.SaveData?.VotingDate is not null && HasProgressFlag(ProgressFlags.RunningForMayor))
        {
            var daysUntilVoting = SaveHandler.SaveData.VotingDate.DaysSinceStart - SDate.Now().DaysSinceStart;

            //Set mail for debate date
            if (daysUntilVoting < (ModKeys.DEBATE_DAY_OFFSET + 8) &&
                !Game1.MasterPlayer.mailReceived.Contains($"{ModKeys.MAYOR_MOD_CPID}_DebateDateMail"))
            {
                Game1.MasterPlayer.mailbox.Add($"{ModKeys.MAYOR_MOD_CPID}_DebateDateMail");
            }

            //Debate is today
            if (daysUntilVoting == (ModKeys.DEBATE_DAY_OFFSET + 1))
            {
                Game1.MasterPlayer.mailReceived.Add($"{ModKeys.MAYOR_MOD_CPID}_MayorDebateMail");
            }

            switch (daysUntilVoting)
            {
                //Set mail for day before voting
                case 2: { Game1.MasterPlayer.mailbox.Add($"{ModKeys.MAYOR_MOD_CPID}_VoteTomorrowMail"); } break;
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
        SaveHandler.UpdateVotingDate(null);
        var voteManager = new VotingHandler(Game1.MasterPlayer, ModConfigHandler.ModConfig);
        RemoveAllModFlags();

        if (voteManager.HasWonElection(_mod.Helper))
        {
            AddProgressFlag(ProgressFlags.WonMayorElection);
            Game1.MasterPlayer.mailbox.Add($"{ModKeys.MAYOR_MOD_CPID}_WonOfficialElectionMail");
            Game1.MasterPlayer.mailbox.Add($"{ModKeys.MAYOR_MOD_CPID}_WonElectionMail");
            AssetInvalidationHandler.UpdateAssetInvalidations();
        }
        else
        {
            Game1.MasterPlayer.mailbox.Add($"{ModKeys.MAYOR_MOD_CPID}_LoseOfficialElectionMail");
            AddProgressFlag(ProgressFlags.LostMayorElection);
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

            if (RiverCleanUpRunOnce && HasProgressFlag(ProgressFlags.CleanUpRivers))
            {
                RiverCleanUpRunOnce = false;
                AssetInvalidationHandler.InvalidationLocations();
            }

            // Tax stuff
            //if (SDate.Now().Day >= 21 && !ModUtils.HasConversationTopic(DialogueKeys.ConversationTopics.PayingTax))
            //{
            //    ModUtils.AddConversationTopic(DialogueKeys.ConversationTopics.PayingTax, 7);
            //}
            //else if(SDate.Now().Day < 21 && ModUtils.HasConversationTopic(DialogueKeys.ConversationTopics.PayingTax))
            //{
            //    ModUtils.RemoveConversationTopic(DialogueKeys.ConversationTopics.PayingTax);
            //}
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
    public static void ResignAndReset()
    {
        if (!Context.IsWorldReady)
        {
            _mod.Monitor.Log("Can't reset in while not in game.", LogLevel.Info);
            return;
        }

        _mod.Monitor.Log("Reseting mod to fresh install.", LogLevel.Info);

        _mod.Monitor.Log("Removing mayor mod flags.", LogLevel.Info);
        Game1.MasterPlayer.mailReceived.RemoveWhere(m => m.Contains(ModKeys.MAYOR_MOD_CPID));
        Game1.MasterPlayer.mailbox.RemoveWhere(m => m.Contains(ModKeys.MAYOR_MOD_CPID));
        Game1.MasterPlayer.mailForTomorrow.RemoveWhere(m => m.Contains(ModKeys.MAYOR_MOD_CPID));

        _mod.Monitor.Log("Removing mayor mod from events seen.", LogLevel.Info);
        Game1.MasterPlayer.eventsSeen.RemoveWhere(m => m.Contains(ModKeys.MAYOR_MOD_CPID));
        Game1.MasterPlayer.activeDialogueEvents.RemoveWhere(m => m.Key.Contains(ModKeys.MAYOR_MOD_CPID));
        Game1.MasterPlayer.previousActiveDialogueEvents.RemoveWhere(m => m.Key.Contains(ModKeys.MAYOR_MOD_CPID));
        Game1.MasterPlayer.triggerActionsRun.RemoveWhere(m => m.Contains(ModKeys.MAYOR_MOD_CPID));
        foreach (var item in Game1.MasterPlayer.giftedItems)
        {
            item.Value.RemoveWhere(g => g.Key.Contains(ModKeys.MAYOR_MOD_CPID));
        }

        _mod.Monitor.Log("Clearing save data.", LogLevel.Info);
        SaveHandler.ResetSave();

        _mod.Monitor.Log("Invalidating cache for patched assets.", LogLevel.Info);
        _mod.Helper.GameContent.InvalidateCache(XNBPathKeys.MAIL);
        _mod.Helper.GameContent.InvalidateCache(XNBPathKeys.PASSIVE_FESTIVALS);
        _mod.Helper.GameContent.InvalidateCache(XNBPathKeys.LOCATIONS);
        _mod.Helper.GameContent.InvalidateCache(XNBPathKeys.CHARACTERS);

        _mod.Monitor.Log("Done.", LogLevel.Info);
        _mod.Monitor.Log("Please go to sleep and save and the mod should be reset.", LogLevel.Info);
    }
}