using MayorMod.Constants;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace MayorMod.Data.Handlers;

public class AssetInvalidationHandler
{
    private static IMod _mod = null!;
    private static bool MailCacheInvalidate {get; set;}
    private static bool PassiveFestivalCacheInvalidate { get; set; }
    private static bool? RiverCleanupInvalidate { get; set; }
    private static bool? ConstructionGrantInvalidate { get; set; }
    private static bool SubstringPatchesAppliedForMayor { get; set; }

    public static void Init(IMod mod)
    {
        _mod = mod;
    }

    /// <summary>
    /// This invalidates the cache so that the dates for voting are correct in mail and passive festivals
    /// PassiveFestivals seem to be loaded before the save data so we need to reload them to make the
    /// variable date passive festivals show. They also don't seem to reload between loading saves
    /// so you can have the voting day appear in other saves even though you're not running for mayor.
    /// </summary>
    public static void InvalidateModDataIfNeeded()
    {
        if (MailCacheInvalidate)
        {
            _mod.Helper.GameContent.InvalidateCache(XNBPathKeys.MAIL);
            MailCacheInvalidate = false;
        }

        if (PassiveFestivalCacheInvalidate)
        {
            _mod.Helper.GameContent.InvalidateCache(XNBPathKeys.PASSIVE_FESTIVALS);
            Game1.PerformPassiveFestivalSetup();
            Game1.UpdatePassiveFestivalStates();
            PassiveFestivalCacheInvalidate = false;
        }

        var riverCleanupNeedsInvalidation = ModProgressHandler.HasProgressFlag(ProgressFlags.ElectedAsMayor) &&
                           ModProgressHandler.HasProgressFlag(ProgressFlags.CleanUpRivers);
        if (RiverCleanupInvalidate != riverCleanupNeedsInvalidation)
        {
            _mod.Helper.GameContent.InvalidateCache(XNBPathKeys.LOCATIONS);
            RiverCleanupInvalidate = riverCleanupNeedsInvalidation;
        }

        var constructionGrantNeedsInvalidation = ModProgressHandler.HasHostGotProgressFlag(ProgressFlags.ConstructionGrant);
        if (ConstructionGrantInvalidate != constructionGrantNeedsInvalidation)
        {
            _mod.Helper.GameContent.InvalidateCache(XNBPathKeys.BUILDINGS);
            ConstructionGrantInvalidate = constructionGrantNeedsInvalidation;
        }

        var isMayor = ModProgressHandler.HasProgressFlag(ProgressFlags.ElectedAsMayor);
        if (SubstringPatchesAppliedForMayor != isMayor)
        {
            AssetUpdateHandler.InvalidateSubstringPatchedAssets();
            SubstringPatchesAppliedForMayor = isMayor;
        }
    }


    /// <summary>
    /// Queues assets for invalidation when save files are loaded.
    /// </summary>
    public static void QueueSaveInvalidations(bool resetCharacters = false)
    {
        QueueElectionInvalidations();
        RiverCleanupInvalidate = null;
        ConstructionGrantInvalidate = null;
        InvalidateLocationContexts();
        if (resetCharacters)
        {
            _mod.Helper.GameContent.InvalidateCache(XNBPathKeys.CHARACTERS);
        }
    }

    /// <summary>
    /// Queues assets for invalidation when election dates have changed.
    /// </summary>
    public static void QueueElectionInvalidations()
    {
        MailCacheInvalidate = true;
        PassiveFestivalCacheInvalidate = true;
    }

    public static void InvalidationNPCSchedules()
    {
        foreach (var npc in Utility.getAllVillagers())
        {
            npc.resetForNewDay(SDate.Now().Day);
        }
    }

    /// <summary>
    /// Reloads the location contexts so the pass out cost goes back to normal when the guard is off duty.
    /// </summary>
    public static void InvalidateLocationContexts()
    {
        _mod.Helper.GameContent.InvalidateCache(XNBPathKeys.LOCATION_CONTEXTS);
        if (ModProgressHandler.HasHostGotProgressFlag(ProgressFlags.SecurityOnGuard))
        {
            LocationContexts.Default.MaxPassOutCost = 0;
        }
    }
}
