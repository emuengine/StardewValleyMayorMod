using MayorMod.Constants;
using StardewModdingAPI;
using StardewValley;

namespace MayorMod.Data.Handlers;

public class AssetInvalidationHandler
{
    private readonly IModHelper _helper;

    public bool MailCacheInvalidate {get; set;}
    public bool LocationCacheInvalidate { get; set; }
    public bool PassiveFestivalCacheInvalidate { get; set; }

    public AssetInvalidationHandler(IModHelper helper)
    {
        _helper = helper;
    }

    /// <summary>
    /// This invalidates the cache so that the dates for voting are correct in mail and passive festivals
    /// PassiveFestivals seem to be loaded before the save data so we need to reload them to make the
    /// variable date passive festivals show. They also don't seem to reload between loading saves
    /// so you can have the voting day appear in other saves even though you're not running for mayor.
    /// </summary>
    public void InvalidateModDataIfNeeded()
    {
        if (MailCacheInvalidate)
        {
            _helper.GameContent.InvalidateCache(XNBPathKeys.MAIL);
            MailCacheInvalidate = false;
        }

        if (PassiveFestivalCacheInvalidate)
        {
            _helper.GameContent.InvalidateCache(XNBPathKeys.PASSIVE_FESTIVALS);
            Game1.PerformPassiveFestivalSetup();
            Game1.UpdatePassiveFestivalStates();
            PassiveFestivalCacheInvalidate = false;
        }

        if (LocationCacheInvalidate)
        {
            _helper.GameContent.InvalidateCache(XNBPathKeys.LOCATIONS);
            LocationCacheInvalidate = false;
        }
    }


    public void UpdateAssetInvalidations()
    {
        MailCacheInvalidate = true;
        PassiveFestivalCacheInvalidate = true;
        if (ModProgressHandler.HasProgressFlag(ProgressFlags.ElectedAsMayor) &&
            ModProgressHandler.HasProgressFlag(ProgressFlags.CleanUpRivers))
        {
            LocationCacheInvalidate = true;
        }
    }
}
