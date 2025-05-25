using MayorMod.Data.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace MayorMod.Data.Menu;

public class TextMenuItem : IMenuItem
{
    private readonly MayorModMenu _parent;
    public string Text { get; set; }
    public Margin TextMargin { get; set; }
    public SpriteFont Font { get; set; } = Game1.dialogueFont;
    public bool IsBold { get; set; }
    public MenuItemAlign Align { get; set; } = MenuItemAlign.Left;

    public enum MenuItemAlign
    {
        Left,
        Right,
        Center
    }

    public TextMenuItem(MayorModMenu parent, string text, Margin margin)
    {
        _parent = parent;
        Text = text;
        TextMargin = margin;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        int xVal;
        if (Align == MenuItemAlign.Left)
        {
            xVal = TextMargin.Left + _parent.MenuRect.X;
        }
        else if (Align == MenuItemAlign.Right)
        {
            xVal = (_parent.MenuRect.X + _parent.MenuRect.Width) - TextMargin.Right;
        }
        else
        {
            var textHalf = (int)(Font.MeasureString(Text).X / 2.0);
            var windowHalf = (int)(_parent.MenuRect.Width / 2.0);
            xVal = _parent.MenuRect.X + (windowHalf - textHalf);
        }
        var position = new Vector2(xVal, TextMargin.Top + _parent.MenuRect.Y);

        if (IsBold)
        {
            Utility.drawBoldText(spriteBatch, Text, Font, position, Game1.textColor);
        }
        else
        {
            Utility.drawTextWithColoredShadow(spriteBatch, Text, Font, position, Game1.textColor, Color.Transparent);
        }
    }

    public void OnWindowResize(Rectangle oldBounds, Rectangle newBounds)
    {
    }
}
