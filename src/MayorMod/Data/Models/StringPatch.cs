namespace MayorMod.Data.Models;

public class StringPatch
{
    public string SearchKey { get; set; } = string.Empty;
    public string ReplaceKey { get; set; } = string.Empty;
    public bool IsTranslationKey { get; set; }
    public bool IsRegEx { get; set; } = false;
}