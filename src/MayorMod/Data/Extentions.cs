using MayorMod.Constants;
using MayorMod.Data.Models;
using MayorMod.Data.Utilities;
using StardewModdingAPI;
using StardewValley;
using System.Text.RegularExpressions;

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
        var searchKeys = new List<string>();
        if (stringPatch.IsRegEx)
        {
            var regex = new Regex(stringPatch.SearchKey);
            searchKeys = regex.Matches(input).Select(m =>m.Value).ToList();
        }
        else
        {
            searchKeys.Add(stringPatch.IsTranslationKey ? ModUtils.GetTranslationForKey(helper, $"{ModKeys.MAYOR_MOD_CPID}_{stringPatch.SearchKey}") : stringPatch.SearchKey);
        }

        var replaceKey = string.Empty;
        if (stringPatch.ReplaceKey != "EMPTY")
        {
            replaceKey = stringPatch.IsTranslationKey ? ModUtils.GetTranslationForKey(helper, $"{ModKeys.MAYOR_MOD_CPID}_{stringPatch.ReplaceKey}") : stringPatch.ReplaceKey;
        }

        return searchKeys.Aggregate(input, (current, key) => current.Replace(key, replaceKey));
    }
}
