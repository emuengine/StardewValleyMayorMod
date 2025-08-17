using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace MayorMod.Data.Menu;

public class ButtonMenuItem : IClickableMenuItem
{
    private static int _id = 0;
    private readonly MayorModMenu _parent;
    private Vector2 _location;
    public Vector2 Location 
    { 
        get => _location;
        set
        {
            _location = value;
            UpdateButtonComponent();
        }
    }
    private ButtonType _buttonTypeSelected = ButtonType.Ok;
    public ButtonType ButtonTypeSelected
    {
        get => _buttonTypeSelected;
        set
        {
            _buttonTypeSelected = value;
            UpdateButtonComponent();
        }
    }
    public ClickableTextureComponent? ButtonComponent { get; private set; }
    public string Name { get; set; }
    public Action ButtonAction { get; set; }
    public string Text { get; set; } = string.Empty;
    public string HoverText { get; set; } = string.Empty;

    public enum ButtonType
    {
        ArrowDown = 11,
        ArrowUp = 12,
        ArrowRight = 33,
        ArrowLeft = 44,
        Star = 25,
        Target = 32,
        Ok = 46,
        Cancel = 47
    }

    /// <summary>
    /// Creates a new button item with given properties.
    /// </summary>
    /// <param name="parent">The parent menu.</param>
    /// <param name="location">The location of the button on the screen.</param>
    /// <param name="action">The action to be invoked when the button is clicked.</param>
    public ButtonMenuItem(MayorModMenu parent, Vector2 location, Action action)
    {
        _parent = parent;
        Name = $"Button{_id}";
        Location = location;
        ButtonAction = action;
        UpdateButtonComponent();

        _id++;
    }

    /// <summary>
    /// Updates and refreshes the button's component with new bounds and properties.
    /// </summary>
    private void UpdateButtonComponent()
    {
        var bounds = new Rectangle((int)(Location.X>0?
                                            Location.X + _parent.MenuRect.X: 
                                            Location.X + _parent.MenuRect.Width + _parent.MenuRect.X),
                                   (int)(Location.Y > 0 ? 
                                            Location.Y + _parent.MenuRect.Y : 
                                            Location.Y + _parent.MenuRect.Height + _parent.MenuRect.Y),
                                    64, 64);

        ButtonComponent = new ClickableTextureComponent(Name,
                                bounds,
                                Text,
                                HoverText,
                                Game1.mouseCursors,
                                Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, (int)ButtonTypeSelected),
                                1f);

        var old = _parent.allClickableComponents.FirstOrDefault(b => b.name == this.Name);
        if (old is not null)
        {
            _parent.allClickableComponents.Remove(old);
        }
        _parent.allClickableComponents.Add(ButtonComponent);
    }

    /// <summary>
    /// Draws the button's visuals using a SpriteBatch.
    /// </summary>
    /// <param name="spriteBatch">The SpriteBatch to use for drawing.</param>
    public void Draw(SpriteBatch spriteBatch)
    {
        ButtonComponent?.draw(spriteBatch);
    }

    /// <summary>
    /// Invokes the button's action and handles left-click on the button.
    /// </summary>
    /// <param name="x">The x-coordinate of the click point.</param>
    /// <param name="y">The y-coordinate of the click point.</param>
    public void OnLeftClick(int x, int y)
    {
        if (ButtonComponent is not null && ButtonComponent.containsPoint(x, y) && ButtonAction is not null)
        {
            ButtonAction.Invoke();
        }
    }

    /// <summary>
    /// Attempts to hover over the button if the given point is within its bounds.
    /// </summary>
    /// <param name="x">The x-coordinate of the point to check.</param>
    /// <param name="y">The y-coordinate of the point to check.</param>
    public void OnHover(int x, int y)
    {
        ButtonComponent?.tryHover(x, y);
    }

    /// <summary>
    /// Handles the ressizing of the window
    /// </summary>
    /// <param name="oldBounds">Old bounds of the window</param>
    /// <param name="newBounds">New bounds of the window</param>
    public void OnWindowResize(Rectangle oldBounds, Rectangle newBounds)
    {
        UpdateButtonComponent();
    }

    /// <summary>
    /// Updates the cursor position to be on top of the button if it is selected.
    /// </summary>
    /// <param name="index">The current index (not used in this method).</param>
    /// <returns>The updated index, which is usually 0 or -1.</returns>
    public int UpdateCursor(int index)
    {
        if (index == 0 && ButtonComponent is not null)
        {
            Game1.setMousePosition(ButtonComponent.bounds.X + (ButtonComponent.bounds.Width / 2), ButtonComponent.bounds.Y + (ButtonComponent.bounds.Height / 2));
            return index;
        }
        else
        {
            return -1;
        }
    }
}
