using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace MayorMod.Data.Menu;

internal class MenuBorder : IMenuItem
{
    private readonly MayorModMenu _parent;

    public int BorderOffset { get; set; } = 10;
    public int BorderWidth { get; set; } = 5;
    public Color BorderColour { get; set; } = Color.Black;

    public MenuBorder(MayorModMenu parent)
    {
        _parent = parent;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        var borderOffsetTimes2 = BorderOffset * 2;
        Utility.DrawSquare(spriteBatch, 
                           new Rectangle(_parent.MenuRect.X + BorderOffset, 
                                         _parent.MenuRect.Y + BorderOffset, 
                                         _parent.MenuRect.Width - borderOffsetTimes2, 
                                         _parent.MenuRect.Height - borderOffsetTimes2),
                           BorderWidth,
                           BorderColour);
    }

    public void OnWindowResize(Rectangle oldBounds, Rectangle newBounds)
    {
    }
}
