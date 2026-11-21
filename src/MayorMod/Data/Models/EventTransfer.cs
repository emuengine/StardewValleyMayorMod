namespace MayorMod.Data.Models;

public class EventTransfer
{
    public string FromEventsAsset { get; set; } = string.Empty;
    public string ToEventsAsset { get; set; } = string.Empty;
    public List<string> EventIds { get; set; } = new();
}
