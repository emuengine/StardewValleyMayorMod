using HarmonyLib;
using MayorMod.Constants;
using MayorMod.Data.Models;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley.Locations;
using System.Reflection;

namespace MayorMod.Data.Handlers;

public class HarmonyHandler
{
    public const string TILESHEET_MISC = "z_MayorMod_Misc_Tilesheet";
    public const string TILESHEET_WALLS_FLOORS = "walls_and_floors";
    public const string LAYER_BACK = "Back";
    public const string LAYER_BUILDINGS = "Buildings";
    public const string WALL_ID = "WallID";
    public const string FLOOR_ID = "FloorID";
    public const string INIT_ID = "512";
    public const string WALL_FLOOR_PREFIX = "MayorMod_";

    public static MayorModData MMData { get; set; }

    public static void Init(IManifest manifest)
    {
        var methodInfo = typeof(DecoratableLocation)
        .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
        .FirstOrDefault(x => x.Name.Contains("IsFloorableOrWallpaperableTile") && x.GetParameters().Length == 4);

        var harmony = new Harmony(manifest.UniqueID);
        harmony.Patch(
           original: methodInfo,
           prefix: new HarmonyMethod(typeof(HarmonyHandler), nameof(DecoratableLocation_IsFloorableOrWallpaperableTile_Prefix))
        );

        harmony.Patch(
           original: typeof(DecoratableLocation).GetMethod("ReadWallpaperAndFloorTileData"),
           postfix: new HarmonyMethod(typeof(HarmonyHandler), nameof(DecoratableLocation_ReadWallpaperAndFloorTileData_Postfix))
        );
    }

    /// <summary>
    /// Harmony post patch to add missing wallpaper vectors for mayormod tileset
    /// </summary>
    public static void DecoratableLocation_ReadWallpaperAndFloorTileData_Postfix(DecoratableLocation __instance)
    {
        if (!__instance.Name.StartsWith($"{ModKeys.MAYOR_MOD_CPID}_"))
        {
            return;
        }

        foreach (var key in __instance.wallpaperTiles.Keys.Where(k => k.StartsWith(WALL_FLOOR_PREFIX)))
        {
            var tilesToAdd = new List<Vector3>();
            foreach (var tile in __instance.wallpaperTiles[key])
            {
                tilesToAdd.Add(new Vector3(tile.X, tile.Y + 1, 1f));
                tilesToAdd.Add(new Vector3(tile.X, tile.Y + 2, 2f));
            }
            __instance.wallpaperTiles[key].AddRange(tilesToAdd);
        }
    }

    /// <summary>
    /// Harmony pre patch to set tileset to walls_and_floors on update
    /// </summary>
    public static bool DecoratableLocation_IsFloorableOrWallpaperableTile_Prefix(DecoratableLocation __instance, int x, int y, string layerName, ref bool __result)
    {
        if (!__instance.Name.StartsWith($"{ModKeys.MAYOR_MOD_CPID}_"))
        {
            return true;
        }

        var tile = __instance.map.GetLayer(layerName)?.Tiles[x, y];
        var tilesheet = tile?.TileSheet;

        if (tile is null || tile.Properties is null)
        {
            return true;
        }

        if (tilesheet is not null && tile.Properties.ContainsKey(FLOOR_ID))
        {
            string floorId = tile.Properties[FLOOR_ID];
            if (floorId.StartsWith(WALL_FLOOR_PREFIX) &&
                __instance.appliedFloor.ContainsKey(floorId) &&
                __instance.appliedFloor[floorId] != INIT_ID)
            {
                if (tilesheet.Id == TILESHEET_MISC)
                {
                    __instance.setMapTile(x, y, __instance.GetFlooringIndex(0, x, y), layerName, TILESHEET_WALLS_FLOORS);
                }
                __result = true;
                return false;
            }
        }

        if (tilesheet is not null && tile.Properties.ContainsKey(WALL_ID))
        {
            string WallId = tile.Properties[WALL_ID];
            if (WallId.StartsWith(WALL_FLOOR_PREFIX) &&
                __instance.appliedWallpaper.ContainsKey(WallId) &&
                __instance.appliedWallpaper[WallId] != INIT_ID)
            {
                if (tilesheet.Id == TILESHEET_MISC)
                {
                    __instance.setMapTile(x, y, 0, LAYER_BACK, TILESHEET_WALLS_FLOORS);
                    __instance.setMapTile(x, y + 1, 0, LAYER_BACK, TILESHEET_WALLS_FLOORS);
                    __instance.setMapTile(x, y + 2, 0, LAYER_BUILDINGS, TILESHEET_WALLS_FLOORS);
                }
                __result = true;
                return false;
            }
        }
        return true;
    }
}
