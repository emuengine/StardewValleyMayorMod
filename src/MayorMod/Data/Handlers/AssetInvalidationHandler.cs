using MayorMod.Constants;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace MayorMod.Data.Handlers;

public class AssetInvalidationHandler
{
    private static IMod _mod = null!;
    private static bool MailCacheInvalidate {get; set;}
    private static bool LocationCacheInvalidate { get; set; }
    private static bool PassiveFestivalCacheInvalidate { get; set; }

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

        if (LocationCacheInvalidate)
        {
            _mod.Helper.GameContent.InvalidateCache(XNBPathKeys.LOCATIONS);
            LocationCacheInvalidate = false;
        }
    }


    public static void UpdateAssetInvalidations()
    {
        MailCacheInvalidate = true;
        PassiveFestivalCacheInvalidate = true;
        if (ModProgressHandler.HasProgressFlag(ProgressFlags.ElectedAsMayor) &&
            ModProgressHandler.HasProgressFlag(ProgressFlags.CleanUpRivers))
        {
            LocationCacheInvalidate = true;
        }
    }

    public static void InvalidationNPCSchedules()
    {
        foreach (var npc in Utility.getAllVillagers())
        {
            npc.resetForNewDay(SDate.Now().Day);
        }
    }

    public static void InvalidationLocations()
    {
        LocationCacheInvalidate = true;
    }
}
