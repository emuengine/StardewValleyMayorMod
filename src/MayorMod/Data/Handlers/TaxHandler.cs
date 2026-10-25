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
    /// Charges all the monthly costs on the 1st of the month.
    /// </summary>
    public static void ChargeMonthlyCosts()
    {
        if (!Game1.IsMasterGame || Game1.dayOfMonth != 1)
            return;

        ChargeSecurityWages();
        ChargeHealthcareFunding();
    }

    /// <summary>
    /// Pays the monthly security guard wages.
    /// </summary>
    private static void ChargeSecurityWages()
    {
        ChargeMonthlyCost(CouncilMeetingKeys.MeetingTownSecurity,
                          ProgressFlags.SecurityOnGuard,
                          DialogueKeys.CouncilMeeting.SecurityWagesPaid,
                          DialogueKeys.CouncilMeeting.SecurityWagesUnpaid,
                          AssetInvalidationHandler.InvalidateLocationContexts);
    }

    /// <summary>
    /// Pays the monthly healthcare funding.
    /// </summary>
    private static void ChargeHealthcareFunding()
    {
        ChargeMonthlyCost(CouncilMeetingKeys.MeetingHealthcareFunding,
                          ProgressFlags.HealthcareFunding,
                          DialogueKeys.CouncilMeeting.HealthcareFundingPaid,
                          DialogueKeys.CouncilMeeting.HealthcareFundingUnpaid);
    }

    /// <summary>
    /// Charge a monthly cost while active. If the treasury can't pay then the effect stops and
    /// the cost is added to the outstanding menu until it is paid from the budget.
    /// </summary>
    /// <param name="costKey">cost key in TreasuryCostKeys.Costs</param>
    /// <param name="activeFlag">progress flag that is set while the effect is active</param>
    /// <param name="paidMessageKey">message shown when paid, {0} is the amount</param>
    /// <param name="unpaidMessageKey">message shown when it couldn't be paid</param>
    /// <param name="onUnpaid">optional extra clean up when the effect stops</param>
    private static void ChargeMonthlyCost(string costKey, string activeFlag, string paidMessageKey, string unpaidMessageKey, Action? onUnpaid = null)
    {
        if (!ModProgressHandler.HasProgressFlag(activeFlag))
            return;

        int cost = TreasuryCostKeys.Costs.GetValueOrDefault(costKey);
        if (TrySpendTreasury(cost))
        {
            Game1.showGlobalMessage(Game1.content.LoadString(paidMessageKey, cost));
            return;
        }

        ModProgressHandler.RemoveProgressFlag(activeFlag);
        onUnpaid?.Invoke();
        if (SaveHandler.SaveData is not null && !SaveHandler.SaveData.OutstandingCouncilCosts.Contains(costKey))
        {
            SaveHandler.SaveData.OutstandingCouncilCosts.Add(costKey);
        }
        Game1.showGlobalMessage(Game1.content.LoadString(unpaidMessageKey));
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
