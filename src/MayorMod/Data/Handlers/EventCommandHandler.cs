using StardewModdingAPI;
using StardewValley;
using StardewValley.Delegates;
using xTile.Tiles;

namespace MayorMod.Data.Handlers;

public static class EventCommandHandler
{
    public const string STORE_MAP_TILE = "emuEngineMayorMod_storeMapTile";
    public const string RETRIEVE_MAP_TILE = "emuEngineMayorMod_retrieveMapTile";
    public const string CLEAR_DIALOGUE_EVENT = "emuEngineMayorMod_clearDialogueEvent";
    public const string CLEAR_SEEN_EVENT = "emuEngineMayorMod_clearSeenEvent";

    /// <summary>
    /// Stores full tile object. This allows restoring of animated tiles.
    /// </summary>
    public static Dictionary<string, Tile> TileStorage { get; set; } = new Dictionary<string, Tile>();


    /// <summary>
    /// Add custom event commands
    /// </summary>
    public static void AddExtraEventCommands(IMonitor monitor)
    {
        //storeMapTile
        var storageMethodInfo = typeof(EventCommandHandler).GetMethod(nameof(StoreMapTile));
        if (storageMethodInfo is null)
        {
            monitor.Log($"Error: MethodInfo for {StoreMapTile} not found");
            return;
        }
        var storageCommand = (EventCommandDelegate)Delegate.CreateDelegate(typeof(EventCommandDelegate), storageMethodInfo);
        Event.RegisterCommand(STORE_MAP_TILE, storageCommand);

        //retrieveMapTile
        var retrievalMethodInfo = typeof(EventCommandHandler).GetMethod(nameof(RetrieveMapTile));
        if (retrievalMethodInfo is null)
        {
            monitor.Log($"Error: MethodInfo for {RetrieveMapTile} not found");
            return;
        }
        var retrievalCommand = (EventCommandDelegate)Delegate.CreateDelegate(typeof(EventCommandDelegate), retrievalMethodInfo);
        Event.RegisterCommand(RETRIEVE_MAP_TILE, retrievalCommand);

        //clearActiveDialogueEvent
        var clearDialogueEventMethodInfo = typeof(EventCommandHandler).GetMethod(nameof(ClearDialogueEvent));
        if (clearDialogueEventMethodInfo is null)
        {
            monitor.Log($"Error: MethodInfo for {ClearDialogueEvent} not found");
            return;
        }
        var clearDialogueEventCommand = (EventCommandDelegate)Delegate.CreateDelegate(typeof(EventCommandDelegate), clearDialogueEventMethodInfo);
        Event.RegisterCommand(CLEAR_DIALOGUE_EVENT, clearDialogueEventCommand);


        //clearSeenEvent
        var clearSeenEventMethodInfo = typeof(EventCommandHandler).GetMethod(nameof(ClearSeenEvent));
        if (clearSeenEventMethodInfo is null)
        {
            monitor.Log($"Error: MethodInfo for {ClearSeenEvent} not found");
            return;
        }
        var clearSeenEventCommand = (EventCommandDelegate)Delegate.CreateDelegate(typeof(EventCommandDelegate), clearSeenEventMethodInfo);
        Event.RegisterCommand(CLEAR_SEEN_EVENT, clearSeenEventCommand);
    }

    /// <summary>
    /// Retrieve map tile object stored by key
    /// </summary>
    public static void RetrieveMapTile (Event @event, string[] args, EventContext context)
    {
        if (!ArgUtility.TryGet(args, 1, out var layerId, out var error, allowBlank: true, "string layerId") ||
            !ArgUtility.TryGetPoint(args, 2, out var tilePos, out error, "Point tilePos") ||
            !ArgUtility.TryGet(args, 4, out var storedTileId, out error, allowBlank: true, "string storedTileId") ||
            !ArgUtility.TryGetOptionalBool(args, 5, out var deleteOnRetrieve, out error, true, "string deleteOnRetrieve"))
        {
            context.LogErrorAndSkip(error);
            return;
        }

        var layer = context.Location.map.GetLayer(layerId);
        if (layer == null)
        {
            context.LogErrorAndSkip("the '" + context.Location.NameOrUniqueName + "' location doesn't have required map layer " + layerId);
            return;
        }

        if (string.IsNullOrEmpty(storedTileId) || !TileStorage.ContainsKey(storedTileId))
        {
            context.LogErrorAndSkip($"the storedTileId is not found");
            return;
        }

        layer.Tiles[@event.OffsetTileX(tilePos.X), @event.OffsetTileY(tilePos.Y)] = TileStorage[storedTileId];
        if (deleteOnRetrieve)
        {
            TileStorage.Remove(storedTileId);
        }
        @event.CurrentCommand++;
    }

    /// <summary>
    /// Store map tile object by key
    /// </summary>
    public static void StoreMapTile(Event @event, string[] args, EventContext context)
    {
        if (!ArgUtility.TryGet(args, 1, out var layerId, out var error, allowBlank: true, "string layerId") || 
            !ArgUtility.TryGetPoint(args, 2, out var tilePos, out error, "Point tilePos") ||
            !ArgUtility.TryGet(args, 4, out var storedTileId, out error, allowBlank: true, "string storedTileId"))
        {
            context.LogErrorAndSkip(error);
            return;
        }

        var layer = context.Location.map.GetLayer(layerId);
        if (layer == null)
        {
            context.LogErrorAndSkip("the '" + context.Location.NameOrUniqueName + "' location doesn't have required map layer " + layerId);
            return;
        }

        if (string.IsNullOrEmpty(storedTileId))
        {
            context.LogErrorAndSkip($"the storedTileId is null or empty");
            return;
        }

        var tile = layer.Tiles[@event.OffsetTileX(tilePos.X), @event.OffsetTileY(tilePos.Y)];
        if (tile == null)
        {
            context.LogErrorAndSkip($"the '{context.Location.NameOrUniqueName}' location has a null tile at ({tilePos.X}, {tilePos.Y}) for layer {layerId}");
            return;
        }
        TileStorage[storedTileId] = tile;
        @event.CurrentCommand++;
    }

    /// <summary>
    /// Remove dialogueEventId from activeDialogueEvents and previousActiveDialogueEvents 
    /// </summary>
    /// <param name="event"></param>
    /// <param name="args"></param>
    /// <param name="context"></param>
    public static void ClearDialogueEvent(Event @event, string[] args, EventContext context)
    {
        if (!ArgUtility.TryGet(args, 1, out var dialogueEventId, out var error, allowBlank: true, "string dialogueEventId"))
        {
            context.LogErrorAndSkip(error);
            return;
        }

        Game1.player.activeDialogueEvents.RemoveWhere((m) => m.Key.Contains(dialogueEventId));
        Game1.player.previousActiveDialogueEvents.RemoveWhere((m) => m.Key.Contains(dialogueEventId));
        @event.CurrentCommand++;
    }

    /// <summary>
    /// Remove event from eventsSeen
    /// </summary>
    /// <param name="event"></param>
    /// <param name="args"></param>
    /// <param name="context"></param>
    public static void ClearSeenEvent(Event @event, string[] args, EventContext context)
    {
        if (!ArgUtility.TryGet(args, 1, out var seenEventId, out var error, allowBlank: true, "string seenEventId"))
        {
            context.LogErrorAndSkip(error);
            return;
        }

        Game1.player.eventsSeen.RemoveWhere(m => m.Contains(seenEventId));
        @event.CurrentCommand++;
    }
}