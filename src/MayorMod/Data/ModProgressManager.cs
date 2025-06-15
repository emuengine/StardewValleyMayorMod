using MayorMod.Constants;
using StardewValley;

namespace MayorMod.Data;

public static class ModProgressManager
{
    /// <summary>
    /// Check if player has a progress flag.
    /// </summary>
    /// <param name="flagId">Progress flag Id</param>
    /// <returns></returns>
    public static bool HasProgressFlag(string flagId)
    {
        if (Game1.player is null)
        {
            return false;
        }
        return Game1.player.mailReceived.Contains(flagId);
    }

    /// <summary>
    /// Add a progress flag to a player.
    /// </summary>
    /// <param name="flagId">Progress flag Id</param>
    public static void AddProgressFlag(string flagId)
    {
        if (!Game1.player.mailReceived.Contains(flagId))
        {
            Game1.player.mailReceived.Add(flagId);
        }
    }

    /// <summary>
    /// Remove a progress flag from a player.
    /// </summary>
    /// <param name="flagId">Progress flag Id</param>
    public static void RemoveProgressFlag(string flagId)
    {
        if (Game1.player.mailReceived.Contains(flagId))
        {
            Game1.player.mailReceived.Remove(flagId);
        }
    }

    /// <summary>
    /// Remove all MayorMod progress flags from a player.
    /// </summary>
    public static void RemoveAllModFlags()
    {
        Game1.player.mailReceived.RemoveWhere(m => m.Contains(ModKeys.MAYOR_MOD_CPID));
    }
}