using MayorMod.Constants;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

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
        
        if (arg2[1] == MayorDeskActionType)
        {
            ManorHouseTileActions.MayorDeskAction(_helper, farmer);
        }
        else if (!farmer.mailReceived.Contains(ModProgressManager.VotedForMayor))
        {
            switch (arg2[1])
            {
                //case MayorDeskActionType: ManorHouseTileActions.MayorDeskAction(_helper, farmer); break;
                case DeskActionType: VotingTileActions.DeskAction(farmer); break;
                case VotingBoothActionType: VotingTileActions.VotingBoothAction(_helper, farmer, arg2); break;
                case BallotBoxActionType: VotingTileActions.BallotBoxAction(farmer); break;
                default:
                {
                    _monitor?.Log($"Unknown tile action - {arg2[1]}", LogLevel.Error);
                    return false;
                }
            }
        }
        else
        {
            Game1.DrawDialogue(ModUtils.OfficerMikeNPC, DialogueKeys.OfficerMike.HaveVoted);
        }
        return true;
    }


}
