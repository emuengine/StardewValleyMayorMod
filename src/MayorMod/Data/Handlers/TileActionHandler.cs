using MayorMod.Data.Models;
using MayorMod.Data.TileActions;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;

namespace MayorMod.Data.Handlers;

/// <summary>
/// Handles various map Tile Actions 
/// </summary>
internal static class TileActionHandler
{
    private static IModHelper _helper = null!;
    private static IMonitor _monitor = null!;
    private static MayorModConfig _modConfig = null!;

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
    public const string Resign = "Resign";


    public static void Init(IModHelper helper, IMonitor monitor, MayorModConfig modConfig)
    {
        _helper = helper;
        _monitor = monitor;
        _modConfig = modConfig;
        GameLocation.RegisterTileAction(MayorModActionType, HandleMayorModTileAction);
    }

    public class TestManorHouse : ManorHouse
    {
        public override bool answerDialogue(Response answer)
        {
            return base.answerDialogue(answer);
        }
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
            case LostAndFoundActionType: { MayorMachines.CheckLostAndFound(); } break;
            case DivorceBookActionType: { MayorMachines.DivorceBook(); } break;
            case LedgerBookActionType: { MayorMachines.ReadLedgerBook(); } break;
            case MayorFridgeActionType: { MayorMachines.MayorFridge(point); } break;
            case MayorDeskActionType: ManorHouseTileActions.MayorDeskAction(_helper, farmer, _modConfig); break;
            case DeskActionType: VotingTileActions.VotingDeskAction(farmer); break;
            case VotingBoothActionType: VotingTileActions.VotingBoothAction(_helper, farmer, arg2); break;
            case BallotBoxActionType: VotingTileActions.BallotBoxAction(farmer); break;
            case Resign: ModUtils.OpenResignationDialogue(_helper, farmer); break;
            default:
            {
                _monitor?.Log($"Unknown tile action - {arg2[1]}", LogLevel.Error);
                return false;
            }
        }
        return true;
    }

}
