using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace MayorMod.Data.Menu;

public class MayorModMenu : IClickableMenu
{
    public IModHelper Helper{get; private set;}

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
    public IList<IMenuItem> MenuItems { get; set; } = [];
    public Rectangle MenuRect { get; set; }
    public Color BackgoundColour { get; set; } = Color.Transparent;

    public MayorModMenu(IModHelper helper, float marginWidthPercent = 1.0f, float marginHeightPercent = 1.0f)
    {
        Helper = helper;
        MarginHeightPercent = marginHeightPercent;
        MarginWidthPercent = marginWidthPercent;
        allClickableComponents = [];
    }

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

        foreach (IClickableMenuItem button in MenuItems.Where(mi => mi is IClickableMenuItem))
        {
            button.OnLeftClick(x, y);
        }
    }

    public override void performHoverAction(int x, int y)
    {
        base.performHoverAction(x, y);
        foreach (IClickableMenuItem button in MenuItems.Where(mi => mi is IClickableMenuItem))
        {
            button.OnHover(x, y);
        }
    }

    public override void receiveScrollWheelAction(int direction)
    {
        base.receiveScrollWheelAction(direction);
        foreach (IScrollableMenuItem button in MenuItems.Where(mi => mi is IScrollableMenuItem))
        {
            button.OnScroll(direction);
        }
    }

    //public override void snapToDefaultClickableComponent()
    //{
    //    var menuButton = MenuItems.FirstOrDefault(mi => mi is ButtonMenuItem);
    //    if (menuButton is not null)
    //    {
    //        currentlySnappedComponent = ((ButtonMenuItem)menuButton).ButtonComponent;
    //        snapCursorToCurrentSnappedComponent();
    //    }
    //}

    //protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
    //{
    //    base.customSnapBehavior(direction, oldRegion, oldID);
    //}

    //public override void automaticSnapBehavior(int direction, int oldRegion, int oldID)
    //{
    //    base.automaticSnapBehavior(direction, oldRegion, oldID);
    //}
}