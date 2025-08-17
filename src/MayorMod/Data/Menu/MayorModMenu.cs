using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace MayorMod.Data.Menu;

public partial class MayorModMenu : IClickableMenu
{
    private CursorSnapMenuItem _selectedCursorItem;

    private float _marginWidthPercent;
    public float MarginWidthPercent
    {
        get => _marginWidthPercent;
        set 
        { 
            _marginWidthPercent = value;
            CalculateMenuRect();
        }
    }
    private float _marginHeightPercent;
    public float MarginHeightPercent
    {
        get => _marginHeightPercent;
        set
        {
            _marginHeightPercent = value;
            CalculateMenuRect();
        }
    }
    public IModHelper Helper { get; private set; }
    public IList<IMenuItem> MenuItems { get; set; } = new List<IMenuItem>();
    public Rectangle MenuRect { get; set; }
    public Color BackgoundColour { get; set; } = Color.Transparent;

#pragma warning disable CS8618
    public MayorModMenu(IModHelper helper, float marginWidthPercent = 1.0f, float marginHeightPercent = 1.0f)
    {
        Helper = helper;
        MarginHeightPercent = marginHeightPercent;
        MarginWidthPercent = marginWidthPercent;
        allClickableComponents = new List<ClickableComponent>();
    }
#pragma warning restore CS8618

    private void CalculateMenuRect()
    {
        var menuSize = new Vector2(Game1.uiViewport.Width * MarginWidthPercent, Game1.uiViewport.Height * MarginHeightPercent);
        int bufferWidthSpace = (int)(Game1.uiViewport.Width - menuSize.X);
        int bufferHeightSpace = (int)(Game1.uiViewport.Height - menuSize.Y);
        MenuRect = new(bufferWidthSpace / 2, bufferHeightSpace / 2, (int)menuSize.X, (int)menuSize.Y);
    }

    public override void draw(SpriteBatch spriteBatch)
    {
        base.draw(spriteBatch);

        if (BackgoundColour == Color.Transparent)
        {
            drawTextureBox(spriteBatch, MenuRect.X, MenuRect.Y, MenuRect.Width, MenuRect.Height, Color.White);
        }
        else
        {
            Utility.DrawSquare(spriteBatch, MenuRect, 0, Color.White, Color.White);
        }

        foreach (var component in MenuItems)
        {
            component.Draw(spriteBatch);
        }

        drawMouse(spriteBatch);
    }

    public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
    {
        base.gameWindowSizeChanged(oldBounds, newBounds);
        CalculateMenuRect();
        foreach (var component in MenuItems)
        {
            component.OnWindowResize(oldBounds, newBounds);
        }
    }

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        base.receiveLeftClick(x, y, playSound);
        foreach (var button in MenuItems.Where(mi => mi is IClickableMenuItem).Cast<IClickableMenuItem>())
        {
            button.OnLeftClick(x, y);
        }
    }

    public override void performHoverAction(int x, int y)
    {
        base.performHoverAction(x, y);
        foreach (var button in MenuItems.Where(mi => mi is IClickableMenuItem).Cast<IClickableMenuItem>())
        {
            button.OnHover(x, y);
        }
    }

    public override void receiveScrollWheelAction(int direction)
    {
        base.receiveScrollWheelAction(direction);
        foreach (var button in MenuItems.Where(mi => mi is IScrollableMenuItem).Cast<IScrollableMenuItem>())
        {
            button.OnScroll(direction);
        }
    }

    public override void applyMovementKey(int direction)
    {
        base.applyMovementKey(direction);
        CustomSnapBehavior(direction == 2 || direction == 1 ? 1 : -1);
    }

    /// <summary>
    /// Snap behavior for gamepad.
    /// </summary>
    /// <param name="direction"></param>
    private void CustomSnapBehavior(int direction)
    {
        var cursorButtons = MenuItems.Where(mi => mi is IClickableMenuItem)
                                     .Cast<IClickableMenuItem>()
                                     .ToList();
        if (!cursorButtons.Any())
        {
            return;
        }

        if (_selectedCursorItem is null)
        {
            _selectedCursorItem = new CursorSnapMenuItem(cursorButtons[0], 0);
            _selectedCursorItem.MenuItem.UpdateCursor(_selectedCursorItem.Index);
        }
        else 
        {
            var updated = _selectedCursorItem.MenuItem.UpdateCursor(_selectedCursorItem.Index + direction);
            if (updated != -1)
            {
                _selectedCursorItem = new CursorSnapMenuItem(_selectedCursorItem.MenuItem, updated);
            }
            else
            {
                var index = cursorButtons.FindIndex(c => c == _selectedCursorItem.MenuItem);
                var update = Math.Clamp(index + direction, 0, cursorButtons.Count - 1); 
                _selectedCursorItem = new CursorSnapMenuItem(cursorButtons[update], 0);
                _selectedCursorItem.MenuItem.UpdateCursor(_selectedCursorItem.Index);
            }
        }
    }
}