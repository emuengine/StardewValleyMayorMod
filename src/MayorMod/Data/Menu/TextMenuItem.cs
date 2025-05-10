using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace MayorMod.Data.Menu;

public class TextMenuItem : IMenuItem
{
    private readonly MayorModMenu _parent;
    public string Text { get; set; }
    public Vector2 Location { get; set; }
    public SpriteFont Font { get; set; } = Game1.dialogueFont;
    public TextMenuItem(MayorModMenu parent, string text, Vector2 location)
    {
        _parent = parent;
        Text = text;
        Location = location;
    }

    public TextMenuItem(MayorModMenu parent, string text, Vector2 location, SpriteFont font) : this(parent, text, location)
    {
        Font = font;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        var position = new Vector2((int)(Location.X > 0 ?
                                    Location.X + _parent.MenuRect.X :
                                    Location.X + _parent.MenuRect.Width),
                           (int)(Location.Y > 0 ?
                                    Location.Y + _parent.MenuRect.Y :
                                    Location.Y + _parent.MenuRect.Height));
        Utility.drawTextWithShadow(spriteBatch, Text, Font, position, Game1.textColor);
    }

    public void OnWindowResize(Rectangle oldBounds, Rectangle newBounds)
    {
    }
}
