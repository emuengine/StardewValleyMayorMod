using StardewValley;
using StardewValley.Delegates;
using System.Reflection;
using xTile.Layers;
using xTile.Tiles;

namespace MayorMod.Data;

public static class EventCommands
{
    public static void AddExtraEventCommands()
    {
        var storageMethodInfo = typeof(EventCommands).GetMethod(nameof(StoreMapTile));
        if (storageMethodInfo is null)
        {
            //TODO Log error
            return;
        }
        var storageCommand = (EventCommandDelegate)Delegate.CreateDelegate(typeof(EventCommandDelegate), storageMethodInfo);
        Event.RegisterCommand("storeMapTile", storageCommand);

        var retrievalMethodInfo = typeof(EventCommands).GetMethod(nameof(RetrieveMapTile));
        if (retrievalMethodInfo is null)
        {
            //TODO Log error
            return;
        }
        var retrievalCommand = (EventCommandDelegate)Delegate.CreateDelegate(typeof(EventCommandDelegate), retrievalMethodInfo);
        Event.RegisterCommand("retrieveMapTile", retrievalCommand);
    }

    public static Dictionary<string, Tile> TileStorage { get; set; } = [];

    public static void RetrieveMapTile (Event @event, string[] args, EventContext context)
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

        if (string.IsNullOrEmpty(storedTileId) || !TileStorage.ContainsKey(storedTileId))
        {
            context.LogErrorAndSkip($"the storedTileId is not found");
            return;
        }

        layer.Tiles[@event.OffsetTileX(tilePos.X), @event.OffsetTileY(tilePos.Y)] = TileStorage[storedTileId];
        TileStorage.Remove(storedTileId);
        @event.CurrentCommand++;
    }

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
}