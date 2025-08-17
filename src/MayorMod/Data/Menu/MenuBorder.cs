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

    /// <summary>
    /// Initializes a new instance of the MenuBorder class.
    /// </summary>
    /// <param name="parent">The parent menu that this text menu item belongs to.</param>
    public MenuBorder(MayorModMenu parent)
    {
        _parent = parent;
    }

    /// <summary>
    /// Draws the menu border on the screen.
    /// </summary>
    /// <param name="spriteBatch">The SpriteBatch to draw with.</param>
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

    /// <summary>
    /// Handles window resize events for this text menu item. No update needed.
    /// </summary>
    public void OnWindowResize(Rectangle oldBounds, Rectangle newBounds)
    {
    }
}
