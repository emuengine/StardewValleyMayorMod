using MayorMod.Constants;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;

namespace MayorMod.Data.TileActions;

/// <summary>
/// Handles various map Tile Actions 
/// </summary>
internal static class TileActionManager
{
    private static IModHelper _helper = null!;
    private static IMonitor _monitor = null!;

    public const string MayorModActionType = "MayorModAction";
    public const string MayorDeskActionType = "MayorDesk";
    public const string DeskActionType = "Desk";
    public const string DeskRegisterActionType = "DeskRegister";
    public const string VotingBoothActionType = "VotingBooth";
    public const string BallotBoxActionType = "BallotBox";
    public const string LostAndFoundActionType = "LostAndFound";
    public const string DivorceBookActionType = "DivorceBook";
    public const string LedgerBookActionType = "LedgerBook";
    public const string MayorFridgeActionType = "MayorFridge";


    public static void Init(IModHelper helper, IMonitor monitor)
    {
        _helper = helper;
        _monitor = monitor;
        GameLocation.RegisterTileAction(MayorModActionType, HandleMayorModTileAction);
    }

    /// <summary>
    /// Handle mayor tile action
    /// </summary>
    /// <param name="location"></param>
    /// <param name="arg2"></param>
    /// <param name="farmer"></param>
    /// <param name="point"></param>
    /// <returns></returns>
    private static bool HandleMayorModTileAction(GameLocation location, string[] arg2, Farmer farmer, Point point)
    {
        if (arg2.Length < 2)
        {
            _monitor.Log("MayorModAction is missing parameters", LogLevel.Error);
            return false;
        }
        
        switch (arg2[1])
        {
            case LostAndFoundActionType: Game1.getLocationFromName(nameof(ManorHouse)).performAction(LostAndFoundActionType, Game1.player, new xTile.Dimensions.Location(point.X,point.Y)); break;
            case DivorceBookActionType: Game1.getLocationFromName(nameof(ManorHouse)).performAction(DivorceBookActionType, Game1.player, new xTile.Dimensions.Location(point.X, point.Y)); break;
            case LedgerBookActionType: Game1.getLocationFromName(nameof(ManorHouse)).performAction(LedgerBookActionType, Game1.player, new xTile.Dimensions.Location(point.X, point.Y)); break;
            case MayorFridgeActionType: Game1.getLocationFromName(nameof(ManorHouse)).performAction(MayorFridgeActionType, Game1.player, new xTile.Dimensions.Location(point.X, point.Y)); break;
            case MayorDeskActionType: ManorHouseTileActions.MayorDeskAction(_helper, farmer); break;
            case DeskActionType: VotingTileActions.VotingDeskAction(farmer); break;
            case VotingBoothActionType: VotingTileActions.VotingBoothAction(_helper, farmer, arg2); break;
            case BallotBoxActionType: VotingTileActions.BallotBoxAction(farmer); break;
            default:
            {
                _monitor?.Log($"Unknown tile action - {arg2[1]}", LogLevel.Error);
                return false;
            }
        }
        return true;
    }
}
