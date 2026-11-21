using MayorMod.Data.Models;

namespace MayorMod.Constants;

public static class CompatibilityKeys
{
    //Stardew Valley Expanded
    public const string SVE_MOD_ID = "FlashShifter.StardewValleyExpandedCP";
    public static readonly string DeedOfLand = "DeedOfLand";
    public static readonly string PlayerWillExpandFarm = "PlayerWillExpandFarm";
    public static readonly string LewisSpecialOrderEventID = "8033861";
    public static readonly string MorrisIsMayorEventID = "6665293";
    public static readonly string MorrisIsCampaigningForMayorEventID = "5553210";
    public static readonly string JojaEmporiumOpenEventID = "5553213";
    public static readonly string JojaEmporium = "JojaEmporium";

    //Law and Order SV Keys
    public const string LEWIS_UNDER_INVESTIGATION = "sdvhead.LawAndOrderSV_Mail_LewisNote";
    public const string LEWIS_IN_PRISON = "sdvhead.LawAndOrderSV_LewisInJail";

    //MS Offerings
    public const string MS_OFFERINGS_MOD_ID = "Nullnnow.MS-Offerings";
    public static readonly string LewisBasement = "LewisBasement";
    public static readonly string StaircaseItem = "(BC)71";

    //Sword & Sorcery
    public static readonly string SwordAndSorceryGuidebookId = "DN.SnS";
    public static readonly string SwordAndSorceryHandbookHint = "AssetUpdates.SubString.MayorLewisHouse";
    public static readonly string SwordAndSorceryHandbookHintReplacement = "AssetUpdates.SubString.Lewis";

    /// <summary>
    /// Events that are moved to a different location
    /// </summary>
    public static readonly List<EventTransfer> EventMoves = new()
    {
        //Lewis lives at Marnies
        new EventTransfer()
        {
            FromEventsAsset = XNBPathKeys.MANOR_HOUSE_EVENTS,
            ToEventsAsset = XNBPathKeys.ANIMAL_SHOP_EVENTS,
            EventIds = new() { "SnS.Ch1.Roslin", "SnS.Ch4.Roslin.2" }
        },
        //The ManorHouse is now the town hall
        new EventTransfer()
        {
            FromEventsAsset = XNBPathKeys.MANOR_HOUSE_EVENTS,
            ToEventsAsset = XNBPathKeys.MAYOR_MANOR_HOUSE_EVENTS,
            EventIds = new() { "2123243", "prizeTicketIntro" }
        },
        //Lewis is now locked out of Marnies instead
        new EventTransfer()
        {
            FromEventsAsset = XNBPathKeys.TOWN_EVENTS,
            ToEventsAsset = XNBPathKeys.FOREST_EVENTS,
            EventIds = new() { "SnS.Ch1.Mateo.7" }
        },
    };

}
