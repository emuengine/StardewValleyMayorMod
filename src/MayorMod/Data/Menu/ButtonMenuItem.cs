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

    public ButtonMenuItem(MayorModMenu parent, Vector2 location, Action action)
    {
        _parent = parent;
        Name = $"Button{_id}";
        Location = location;
        ButtonAction = action;
        UpdateButtonComponent();

        _id++;
    }

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

    public void Draw(SpriteBatch spriteBatch)
    {
        ButtonComponent?.draw(spriteBatch);
    }

    public void OnLeftClick(int x, int y)
    {
        if (ButtonComponent is not null && ButtonComponent.containsPoint(x, y) && ButtonAction is not null)
        {
            ButtonAction.Invoke();
        }
    }

    public void OnHover(int x, int y)
    {
        ButtonComponent?.tryHover(x, y);
    }

    public void OnWindowResize(Rectangle oldBounds, Rectangle newBounds)
    {
        UpdateButtonComponent();
    }
}
