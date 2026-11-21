using StardewValley;
using StardewValley.Delegates;
using StardewValley.Triggers;
using MayorMod.Constants;
using MayorMod.Data.Models;
using MayorMod.Data.Utilities;
using StardewModdingAPI;

namespace MayorMod.Data.Handlers;

public static class TaxHandler
{
    public const string PAY_TAX_ACTION = "emuEngineMayorMod_payTax";
    public const int TAX_AMOUNT = 200;
    public const int EMPORIUM_TAX_AMOUNT = TAX_AMOUNT * 2;
    public const int FUNDRAISER_AMOUNT = 500;
    private const string CC_CEREMONY_EVENT_ID = "191393";

    public static void Init()
    {
        TriggerActionManager.RegisterAction(PAY_TAX_ACTION, PayTaxAction);
        TriggerActionManager.RegisterAction(EventCommandHandler.CHARGE_TREASURY, ChargeTreasuryAction);
    }
    
    public static bool PayTaxAction(string[] args, TriggerActionContext context, out string error)
    {
        // get args
        if (!ArgUtility.TryGetInt(args, 1, out int amount, out error) ||
            !ArgUtility.TryGetOptional(args, 2, out string? incomeSource, out error))
        {
            return false;
        }

        // apply
        if (SaveHandler.SaveData is null)
        {
            error = "Save data is not initialized.";
            return false;
        }

        SaveHandler.SaveData.TownTreasury += amount;
        TreasuryDisplayHandler.ShowBriefly();

        //One off income
        if (!string.IsNullOrEmpty(incomeSource))
        {
            ModProgressHandler.AddProgressFlag($"{ModKeys.MAYOR_MOD_CPID}_{incomeSource}Income");
        }

        return true;
    }

    /// <summary>
    /// Gets budget rows for tax payers.
    /// </summary>
    public static IList<TableRow> GetTaxPayers(IModHelper helper)
    {
        var taxPayers = ModNPCKeys.TaxPayers.Select(id =>
        {
            var hasPaid = HasPaidTax(id);
            return new TableRow(ModUtils.GetTranslationForKey(helper, UIMenuKeys.Business(id)), hasPaid ? TAX_AMOUNT : null, hasPaid);
        }).ToList();

        var isJojaMartOpen = !Utility.HasAnyPlayerSeenEvent(CC_CEREMONY_EVENT_ID);
        if (isJojaMartOpen)
        {
            var hasEmporium = Utility.HasAnyPlayerSeenEvent(CompatibilityKeys.JojaEmporiumOpenEventID);
            var hasPaid = HasPaidTax(ModNPCKeys.MorrisId) || HasPaidTax(ModNPCKeys.MorrisTodId);
            var text = ModUtils.GetTranslationForKey(helper, UIMenuKeys.Business(hasEmporium ? CompatibilityKeys.JojaEmporium : ModNPCKeys.MorrisId));
            var amount = hasEmporium ? EMPORIUM_TAX_AMOUNT : TAX_AMOUNT;
            taxPayers.Add(new TableRow(text, hasPaid ? amount : null, hasPaid));
        }
        return taxPayers;
    }

    /// <summary>
    /// Check has NPC paid tax this month.
    /// </summary>
    private static bool HasPaidTax(string npcId)
    {
        return Game1.player.mailReceived.Contains($"{npcId}_{ConversationTopicKeys.PayingTax}");
    }

    /// <summary>
    /// Do we have fundraiser money this month.
    /// </summary>
    public static bool HasFundraiserIncome()
    {
        return ModProgressHandler.HasHostGotProgressFlag(ProgressFlags.FundraiserIncome);
    }

    /// <summary>
    /// Get active monthly costs.
    /// </summary>
    public static IList<string> GetActiveMonthlyCosts()
    {
        var costs = new List<string>();
        if (ModProgressHandler.HasProgressFlag(ProgressFlags.SecurityOnGuard))
        {
            costs.Add(CouncilMeetingKeys.MeetingTownSecurity);
        }
        if (ModProgressHandler.HasProgressFlag(ProgressFlags.HealthcareFunding))
        {
            costs.Add(CouncilMeetingKeys.MeetingHealthcareFunding);
        }
        return costs;
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

        SaveHandler.SaveData.TownTreasury += amount;
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
    /// Charges a monthly cost while active. The treasury can go into debt as far as MIN_TREASURY_FOR_MONTHLY_COSTS.
    /// There is a borrow limit and then the effects stop. These costs are added back to the outstanding menu until the budget is paid.
    /// </summary>
    /// <param name="costKey">cost key in TreasuryCostKeys.Costs</param>
    /// <param name="activeFlag">flag thats set while effect is active.</param>
    /// <param name="paidMessageKey">message shown when paid, {0} is the amount.</param>
    /// <param name="unpaidMessageKey">message shown when not paid.</param>
    /// <param name="onUnpaid">optional extra clean up when the effect stops.</param>
    private static void ChargeMonthlyCost(string costKey, string activeFlag, string paidMessageKey, string unpaidMessageKey, Action? onUnpaid = null)
    {
        if (!ModProgressHandler.HasProgressFlag(activeFlag))
            return;

        int cost = TreasuryCostKeys.Costs.GetValueOrDefault(costKey);
        if (TrySpendTreasury(cost, ModKeys.MIN_TREASURY_FOR_MONTHLY_COSTS))
        {
            SaveHandler.SaveData?.CostsPaidThisMonth.Add(costKey);
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
        SaveHandler.SaveData?.CostsPaidThisMonth.Add(costKey);

        var flag = (costKey == CouncilMeetingKeys.MeetingTownSecurity) ?
                   ProgressFlags.SecurityOnGuard :
                   $"{ModKeys.MAYOR_MOD_CPID}_{costKey}";
        ModProgressHandler.AddProgressFlag(flag);

        if (costKey == CouncilMeetingKeys.MeetingTownSecurity)
        {
            AssetInvalidationHandler.InvalidateLocationContexts();
        }

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
    /// Takes gold out of the town treasury if it won't go below the minimum balance.
    /// </summary>
    /// <param name="minBalance">lowest amount needed</param>
    /// <returns>true if the gold was taken, false if not</returns>
    public static bool TrySpendTreasury(int amount, int minBalance = 0)
    {
        if (SaveHandler.SaveData is null || (SaveHandler.SaveData.TownTreasury - amount) < minBalance)
        {
            return false;
        }

        SaveHandler.SaveData.TownTreasury -= amount;
        SaveHandler.UpdateFarmModData();
        return true;
    }

}
