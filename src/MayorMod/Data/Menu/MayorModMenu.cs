using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace MayorMod.Data.Menu;

public class MayorModMenu : IClickableMenu
{
    private float _marginWidthPercent = 1.0f;
    public float MarginWidthPercent
    {
        get 
        { 
            return _marginWidthPercent;
        }
        set 
        { 
            _marginWidthPercent = value;
            CalculateMenuRect();
        }
    }
    private float _marginHeightPercent = 1.0f;
    public float MarginHeightPercent
    {
        get
        {
            return _marginHeightPercent;
        }
        set
        {
            _marginHeightPercent = value;
            CalculateMenuRect();
        }
    }
    public IList<IMenuItem> MenuItems { get; set; } = [];
    public Rectangle MenuRect { get; set; }

    public MayorModMenu(float marginWidthPercent, float marginHeightPercent)
    {
        MarginHeightPercent = marginHeightPercent;
        MarginWidthPercent = marginWidthPercent;
        allClickableComponents = [];
        CalculateMenuRect();
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

        drawTextureBox(spriteBatch, MenuRect.X, MenuRect.Y, MenuRect.Width, MenuRect.Height, Color.White);

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