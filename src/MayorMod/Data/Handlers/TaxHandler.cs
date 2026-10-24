using StardewValley;
using StardewValley.Delegates;
using StardewValley.Triggers;
using MayorMod.Constants;
using StardewModdingAPI;

namespace MayorMod.Data.Handlers;

public static class TaxHandler
{
    public const string PAY_TAX_ACTION = "emuEngineMayorMod_payTax";

    public static void Init()
    {
        TriggerActionManager.RegisterAction(PAY_TAX_ACTION, PayTaxAction);
        TriggerActionManager.RegisterAction(EventCommandHandler.CHARGE_TREASURY, ChargeTreasuryAction);
    }
    
    public static bool PayTaxAction(string[] args, TriggerActionContext context, out string error)
    {
        // get args
        if (!ArgUtility.TryGetInt(args, 1, out int amount, out error))
            return false;

        // apply
        if (SaveHandler.SaveData is null)
        {
            error = "Save data is not initialized.";
            return false;
        }

        SaveHandler.SaveData.TownTreasury += amount;
        Game1.showGlobalMessage($"Town Treasury : {SaveHandler.SaveData.TownTreasury}G");

        return true;
    }

    /// <summary>
    /// Adds gold to the town treasury or negative numbers take it away.
    /// </summary>
    public static void AdjustTreasuryCommand(IMonitor monitor, string[] args)
    {
        if (!Context.IsWorldReady || !Context.IsMainPlayer || SaveHandler.SaveData is null)
        {
            monitor.Log("Load a save as the host first.", LogLevel.Error);
            return;
        }

        if (!ArgUtility.TryGetInt(args, 0, out int amount, out var error))
        {
            monitor.Log(error, LogLevel.Error);
            return;
        }

        SaveHandler.SaveData.TownTreasury = Math.Max(0, SaveHandler.SaveData.TownTreasury + amount);
        SaveHandler.UpdateFarmModData();
        monitor.Log($"Town Treasury : {SaveHandler.SaveData.TownTreasury}G", LogLevel.Info);
    }

    /// <summary>
    /// Trigger action for ChargeTreasury
    /// </summary>
    public static bool ChargeTreasuryAction(string[] args, TriggerActionContext context, out string error)
    {
        if (!ArgUtility.TryGet(args, 1, out var costKey, out error, allowBlank: false, "string costKey"))
            return false;

        ChargeTreasury(costKey);
        return true;
    }

    /// <summary>
    /// Charges a cost to the treasury. If the treasury can't afford it then its added to the outstanding menu
    /// and the effects start once it is paid from the budget.
    /// </summary>
    public static void ChargeTreasury(string costKey)
    {
        int cost = TreasuryCostKeys.Costs.GetValueOrDefault(costKey);
        if (TrySpendTreasury(cost))
        {
            ApplyPaidCostEffects(costKey);
            Game1.showGlobalMessage($"Town Treasury : {SaveHandler.SaveData?.TownTreasury}G");
            return;
        }

        SaveHandler.SaveData?.OutstandingCouncilCosts.Add(costKey);
        Game1.showGlobalMessage(Game1.content.LoadString(DialogueKeys.CouncilMeeting.CostOwed));
    }

    /// <summary>
    /// Switches on the council event effect after it has been paid for.
    /// </summary>
    public static void ApplyPaidCostEffects(string costKey)
    {
        var flag = (costKey == CouncilMeetingKeys.MeetingTownSecurity) ?
                   ProgressFlags.SecurityOnGuard :
                   $"{ModKeys.MAYOR_MOD_CPID}_{costKey}";
        ModProgressHandler.AddProgressFlag(flag);

        if (costKey == CouncilMeetingKeys.MeetingStrategicReserve)
        {
            Game1.player.team.AddSpecialOrder(CouncilMeetingKeys.SpecialOrderStrategicReserve, null);
            if (!Game1.player.hasOrWillReceiveMail(CouncilMeetingKeys.MailStrategicReserveRoomReady))
            {
                Game1.addMailForTomorrow(CouncilMeetingKeys.MailStrategicReserveRoomReady);
            }
        }
    }

    /// <summary>
    /// Takes gold out of the town treasury if there is enough to cover the amount.
    /// </summary>
    /// <returns>true if the gold was taken, false if not</returns>
    public static bool TrySpendTreasury(int amount)
    {
        if (SaveHandler.SaveData is null || SaveHandler.SaveData.TownTreasury < amount)
            return false;

        SaveHandler.SaveData.TownTreasury -= amount;
        SaveHandler.UpdateFarmModData();
        return true;
    }

}
