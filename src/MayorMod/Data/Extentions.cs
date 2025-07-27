using MayorMod.Constants;
using MayorMod.Data.Models;
using StardewModdingAPI;
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


    public static string PatchString(this StringPatch stringPatch,IModHelper helper, string input)
    {
        var searchKey = stringPatch.IsTranslationKey ? ModUtils.GetTranslationForKey(helper, $"{ModKeys.MAYOR_MOD_CPID}_{stringPatch.SearchKey}") : stringPatch.SearchKey;
        var replaceKey = stringPatch.IsTranslationKey ? ModUtils.GetTranslationForKey(helper, $"{ModKeys.MAYOR_MOD_CPID}_{stringPatch.ReplaceKey}") : stringPatch.ReplaceKey;
        return input.Replace(searchKey, replaceKey);
    }
}
