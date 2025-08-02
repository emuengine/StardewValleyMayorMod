using MayorMod.Data.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace MayorMod.Data.Menu;

public class BoolMenuItem : IMenuItem
{
    private readonly MayorModMenu _parent;
    private readonly Texture2D? _texture;
    public string Text { get; set; }
    public bool IsTicked { get; set; }
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

    public BoolMenuItem(MayorModMenu parent, string text, bool isTrue, Margin margin)
    {
        _parent = parent;
        _texture = _parent.Helper.ModContent.Load<Texture2D>("assets/tickCross.png");
        Text = text;
        IsTicked = isTrue;
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

        if (_texture is not null)
        {
            var size = (int) Font.MeasureString(Text).Y;
            int tickedSpriteIndex = Convert.ToInt32(!IsTicked);

            var buttonBounds = new Rectangle((int)(position.X + Font.MeasureString(Text).X) + 10, (int)position.Y, size, size);
            var textreSrcRect = new Rectangle(tickedSpriteIndex * 64, 0, 64, 64);
            spriteBatch.Draw(_texture, buttonBounds, textreSrcRect, Color.White);
        }
    }

    public void OnWindowResize(Rectangle oldBounds, Rectangle newBounds)
    {
    }
}
