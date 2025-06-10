using StardewValley;

namespace MayorMod.Data;

public static class Extentions
{
    public static bool HasItemInInventory(this Farmer farmer, string itemId)
    {
        return farmer.Items.Any(i => i != null && i.Name == itemId);
    }

    public static Item? ItemFromInventory(this Farmer farmer, string itemId)
    {
        return farmer.Items.FirstOrDefault(i => i != null && i.Name == itemId);
    }
}
