using Microsoft.Xna.Framework;
using StardewValley;

namespace MayorMod.Data.Models;

public class TableRow
{
    public string Text { get; set; }
    public int? Amount { get; set; }
    public bool? IsTicked { get; set; }
    public Color AmountColor { get; set; } = Game1.textColor;
    public bool IsBold { get; set; }

    public TableRow(string text, int? amount = null, bool? isTicked = null)
    {
        Text = text;
        Amount = amount;
        IsTicked = isTicked;
    }
}
