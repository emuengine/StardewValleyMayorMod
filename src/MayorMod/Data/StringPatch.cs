using MayorMod.Constants;
using StardewModdingAPI;

namespace MayorMod.Data;

public class StringPatch
{
    public string SearchKey { get; set; } = string.Empty;
    public string ReplaceKey { get; set; } = string.Empty;
    public bool IsTranslationKey { get; set; }

    public string PatchString(IModHelper helper, string input)
    {
        var searchKey = IsTranslationKey ? ModUtils.GetTranslationForKey(helper, $"{ModKeys.MAYOR_MOD_CPID}_{SearchKey}") : SearchKey;
        var replaceKey = IsTranslationKey ? ModUtils.GetTranslationForKey(helper, $"{ModKeys.MAYOR_MOD_CPID}_{ReplaceKey}") : ReplaceKey;
        return input.Replace(searchKey, replaceKey);
    }
}