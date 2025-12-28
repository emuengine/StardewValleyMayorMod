using StardewValley;
using StardewValley.Delegates;
using StardewValley.Triggers;

namespace MayorMod.Data.Handlers;

public class TaxHandler
{
    public const string PAY_TAX_ACTION = "emuEngineMayorMod_payTax";

    public TaxHandler()
    {
        TriggerActionManager.RegisterAction(PAY_TAX_ACTION, PayTaxAction);
    }
    
    public bool PayTaxAction(string[] args, TriggerActionContext context, out string error)
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

}
