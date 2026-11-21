using MayorMod.Data.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace MayorMod.Data.Menu;

public class TableMenuItem : IMenuItem
{
    private readonly MayorModMenu _parent;
    private readonly Texture2D _tickTexture;
    public string Heading { get; set; }
    public IList<TableRow> Rows { get; set; }
    public TableRow? TotalRow { get; set; }
    public int Top { get; set; }
    public float StartPercent { get; set; }
    public float EndPercent { get; set; } = 1f;
    public int Padding { get; set; } = 20;
    public SpriteFont Font { get; set; } = Game1.smallFont;
    public Texture2D? Icon { get; set; }

    public int RowHeight => (int)Font.MeasureString("TEXT").Y + 8;
    public int Height => RowHeight * (Rows.Count + 1) + (TotalRow is null ? 0 : RowHeight + 8);

    /// <summary>
    /// Initializes a new instance of the TableMenuItem class.
    /// </summary>
    /// <param name="parent">The parent menu that this table belongs to.</param>
    /// <param name="heading">The heading shown above the rows.</param>
    /// <param name="rows">The rows to show.</param>
    /// <param name="top">The distance from the top of the menu.</param>
    public TableMenuItem(MayorModMenu parent, string heading, IList<TableRow> rows, int top)
    {
        _parent = parent;
        _tickTexture = _parent.Helper.ModContent.Load<Texture2D>("assets/tickCross.png");
        Heading = heading;
        Rows = rows;
        Top = top;
    }

    /// <summary>
    /// Draws the table.
    /// </summary>
    /// <param name="spriteBatch">The SpriteBatch to draw with.</param>
    public void Draw(SpriteBatch spriteBatch)
    {
        var left = _parent.MenuRect.X + (int)(_parent.MenuRect.Width * StartPercent) + Padding;
        var right = _parent.MenuRect.X + (int)(_parent.MenuRect.Width * EndPercent) - Padding;
        var hasTicks = Rows.Any(r => r.IsTicked is not null);
        var y = _parent.MenuRect.Y + Top;

        Utility.drawBoldText(spriteBatch, Heading, Font, new Vector2(left, y), Game1.textColor);
        y += RowHeight;
        foreach (var row in Rows)
        {
            DrawRow(spriteBatch, row, left, right, y, hasTicks);
            y += RowHeight;
        }

        if (TotalRow is not null)
        {
            spriteBatch.Draw(Game1.staminaRect, new Rectangle(left, y, right - left, 2), Game1.textColor * 0.5f);
            DrawRow(spriteBatch, TotalRow, left, right, y + 8, hasTicks);
        }
    }

    /// <summary>
    /// Draws the row data.
    /// </summary>
    private void DrawRow(SpriteBatch spriteBatch, TableRow row, int left, int right, int y, bool hasTicks)
    {
        var textHeight = (int)Font.MeasureString("TEXT").Y;

        if (hasTicks)
        {
            right -= textHeight;
            if (row.IsTicked is not null)
            {
                var tickSrcRect = new Rectangle(Convert.ToInt32(!row.IsTicked.Value) * 64, 0, 64, 64);
                spriteBatch.Draw(_tickTexture, new Rectangle(right, y, textHeight, textHeight), tickSrcRect, Color.White);
            }
            right -= 10;
        }

        if (row.Amount is not null)
        {
            var amount = row.Amount.Value.ToString();
            if (Icon is not null)
            {
                var iconSize = Icon.Height * Math.Max(1, textHeight / Icon.Height);
                right -= iconSize;
                spriteBatch.Draw(Icon, new Rectangle(right, y + (textHeight - iconSize) / 2, iconSize, iconSize), Color.White);
                right -= 8;
            }
            var amountWidth = (int)Font.MeasureString(amount).X;
            DrawRowText(spriteBatch, row, amount, new Vector2(right - amountWidth, y), row.AmountColor);
            right -= amountWidth + 16;
        }

        DrawRowText(spriteBatch, row, FitText(row.Text, right - left), new Vector2(left, y), Game1.textColor);
    }

    /// <summary>
    /// Shorten text with "..." so it fits in a specific width.
    /// </summary>
    private string FitText(string text, int maxWidth)
    {
        var dots = "...";
        if (Font.MeasureString(text).X <= maxWidth)
        {
            return text;
        }

        while (text.Length > 0 && Font.MeasureString(text + dots).X > maxWidth)
        {
            text = text[..^1];
        }
        return text.TrimEnd() + dots;
    }

    /// <summary>
    /// Draws the row's text.
    /// </summary>
    private void DrawRowText(SpriteBatch spriteBatch, TableRow row, string text, Vector2 position, Color color)
    {
        if (row.IsBold)
        {
            Utility.drawBoldText(spriteBatch, text, Font, position, color);
        }
        else
        {
            Utility.drawTextWithShadow(spriteBatch, text, Font, position, color);
        }
    }

    public string GetScreenReaderText()
    {
        var rows = TotalRow is null ? Rows : Rows.Append(TotalRow);
        var rowText = rows.Select(r => r.IsTicked is null ?
                                       $"{r.Text} {r.Amount}" :
                                       $"{r.Text} {r.Amount} {Game1.content.LoadString(r.IsTicked.Value ? "Strings\\Lexicon:QuestionDialogue_Yes" : "Strings\\Lexicon:QuestionDialogue_No")}");
        return string.Join(". ", rowText.Prepend(Heading).Where(t => !string.IsNullOrEmpty(t)));
    }

    public void OnWindowResize(Rectangle oldBounds, Rectangle newBounds)
    {
    }
}
