using MayorMod.Data.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System.Diagnostics.CodeAnalysis;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace MayorMod.Data.Menu;

/// <summary>
/// A component for a menu which displays a scrollable menu of items you can select
/// </summary>
public partial class BigButtonListMenuItem : IClickableMenuItem, IScrollableMenuItem
{
    private readonly MayorModMenu _parent;
    private readonly Rectangle _buttonBackgroundSourceRect;
    private readonly float _fontHeight = 20;
    private Margin _margin;
    private Rectangle _boundingBox;
    private int _buttonIndexOffset;
    private int _scrollBarStart;
    private int _scrollBarEnd;
    private int _numberOfButtons = 4;
    private IList<string> _buttonText;
    private IList<ButtonData> _buttonData = [];
    private ClickableTextureComponent _upArrow;
    private ClickableTextureComponent _downArrow;
    private ClickableTextureComponent _scrollBar;
    public int NumberOfButtons
    {
        get
        {
            return _numberOfButtons;
        }
        set
        {
            _numberOfButtons = value;
            UpdateButtonData();
        }
    }
    private int _buttonPadding = 5;
    public int ButtonPadding
    {
        get
        {
            return _buttonPadding;
        }
        set
        {
            _buttonPadding = value;
            UpdateButtonData();
        }
    }
    public int TextPadding { get; set; } = 30;
    public Action<int> ButtonAction { get; set; }

    public BigButtonListMenuItem(MayorModMenu parent, Margin margin, IList<string> buttonText, Action<int> action)
    {
        _parent = parent;
        _margin = margin;
        _fontHeight = Game1.dialogueFont.MeasureString("TEXT").Y / 2;
        _buttonBackgroundSourceRect = new Rectangle(0, 256, 60, 60);
        _buttonText = buttonText;
        ButtonAction = action;
        UpdateButtonData();
    }

    /// <summary>
    /// Initialises the current bounding box, the buttons in the menu and the scroll bar
    /// </summary>
    [MemberNotNull(nameof(_upArrow))]
    [MemberNotNull(nameof(_downArrow))]
    [MemberNotNull(nameof(_scrollBar))]
    [MemberNotNull(nameof(_scrollBarStart))]
    [MemberNotNull(nameof(_scrollBarEnd))]
    private void UpdateButtonData()
    {
        _boundingBox = new Rectangle(_parent.MenuRect.X + _margin.Left,
                                     _parent.MenuRect.Y + _margin.Top,
                                     _parent.MenuRect.Width - _margin.Right,
                                     _parent.MenuRect.Height - _margin.Bottom);
        _buttonData = [];
        var scrollBarMargin = 40;
        var totalPadding = ButtonPadding * (NumberOfButtons - 1);
        var menuItemHeight = ((_boundingBox.Height - totalPadding) / NumberOfButtons);
        int totalButtons = Math.Min(_buttonText.Count, NumberOfButtons);

        for (int i = 0; i < totalButtons; i++)
        {
            _buttonData.Add(new ButtonData()
            {
                Id = i,
                BoundingBox = new Rectangle(_boundingBox.X, 
                                            _boundingBox.Y + (i * (menuItemHeight + ButtonPadding)), 
                                            _boundingBox.Width - scrollBarMargin, 
                                            menuItemHeight)
            });
        }

        //Scroll bar setup
        var arrowHeight = 48;
        var arrowWidth = 44;
        _upArrow = new ClickableTextureComponent(new Rectangle(_boundingBox.X + _boundingBox.Width - scrollBarMargin,
                                                               _boundingBox.Y, 
                                                               arrowWidth, 
                                                               arrowHeight), 
                                                 Game1.mouseCursors, 
                                                 new Rectangle(421, 459, 11, 12), 
                                                 4f);
        _downArrow = new ClickableTextureComponent(new Rectangle(_boundingBox.X + _boundingBox.Width - scrollBarMargin, 
                                                                 _boundingBox.Y + _boundingBox.Height - arrowHeight, 
                                                                 arrowWidth, 
                                                                 arrowHeight),
                                                   Game1.mouseCursors, 
                                                   new Rectangle(421, 472, 11, 12), 
                                                   4f);
        _scrollBarStart = _boundingBox.Y + _upArrow.bounds.Height;
        _scrollBarEnd = _boundingBox.Y + _boundingBox.Height - (_upArrow.bounds.Height * 2);
       _scrollBar = new ClickableTextureComponent(new Rectangle(_boundingBox.X + _boundingBox.Width - 28, 
                                                                 CalculateScrollBarPostion(), 
                                                                 24, 
                                                                 40), 
                                                   Game1.mouseCursors,
                                                   new Rectangle(435, 463, 6, 10),
                                                   4f);
    }

    /// <summary>
    /// Performs the drawing for the menu buttons
    /// </summary>
    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (var button in _buttonData)
        {
            var texture = button.IsHighlighted ? Game1.menuTexture : Game1.uncoloredMenuTexture;
            var colour = button.IsHighlighted ? Color.White : new Color(245, 183, 93);

            IClickableMenu.drawTextureBox(spriteBatch,
                                          texture,
                                          _buttonBackgroundSourceRect,
                                          button.BoundingBox.X,
                                          button.BoundingBox.Y,
                                          button.BoundingBox.Width,
                                          button.BoundingBox.Height,
                                          colour);
            Utility.drawTextWithShadow(spriteBatch,
                                       _buttonText[button.Id + _buttonIndexOffset],
                                       Game1.dialogueFont,
                                       new Vector2(button.BoundingBox.X + TextPadding,
                                                   button.BoundingBox.Y + (button.BoundingBox.Height / 2) - _fontHeight),
                                       Game1.textColor);
        }
        DrawScrollBar(spriteBatch);
    }

    /// <summary>
    /// Does the drawing for the scroll bar
    /// </summary>
    /// <param name="spriteBatch"></param>
    private void DrawScrollBar(SpriteBatch spriteBatch)
    {
        IClickableMenu.drawTextureBox(spriteBatch,
                                        Game1.mouseCursors,
                                        new Rectangle(403, 383, 6, 6),
                                        _scrollBar.bounds.X,
                                        _upArrow.bounds.Y + _upArrow.bounds.Height,
                                        _scrollBar.bounds.Width,
                                        _downArrow.bounds.Y - (_upArrow.bounds.Y + _upArrow.bounds.Height) - 5,
                                        Color.White,
                                        4f);
        _upArrow.draw(spriteBatch);
        _downArrow.draw(spriteBatch);

        _scrollBar.draw(spriteBatch);
    }

    /// <summary>
    /// Handles the left click action for the control
    /// </summary>
    /// <param name="x">X pos of click</param>
    /// <param name="y">Y pos of click</param>
    public void OnLeftClick(int x, int y)
    {
        foreach (var button in _buttonData)
        {
            if (button.BoundingBox.Contains(x, y))
            {
                ButtonAction.Invoke(button.Id + _buttonIndexOffset);
            }
        }
        if (_upArrow.containsPoint(x, y))
        {
            OnScroll(1);
        }
        else if (_downArrow.containsPoint(x, y))
        {
            OnScroll(-1);
        }
    }

    /// <summary>
    /// Handles the hover action for the control
    /// </summary>
    /// <param name="x">X pos of mouse</param>
    /// <param name="y">Y pos of mouse</param>
    public void OnHover(int x, int y)
    {
        foreach (var button in _buttonData)
        {
            button.IsHighlighted = button.BoundingBox.Contains(x, y); 
            Game1.SetFreeCursorDrag();
        }
        _upArrow.tryHover(x, y);
        _downArrow.tryHover(x, y);
    }

    /// <summary>
    /// Handles the scroll action for the control
    /// </summary>
    /// <param name="direction">Direction of the scroll</param>
    public void OnScroll(int direction)
    {
        if (direction > 0 && _buttonIndexOffset - 1 >= 0)
        {
            _buttonIndexOffset -= 1;
        }
        else if (direction < 0 && _buttonIndexOffset + 1 <= _buttonText.Count - NumberOfButtons)
        {
            _buttonIndexOffset += 1;
        }
        _scrollBar.bounds.Y = CalculateScrollBarPostion();
    }

    /// <summary>
    /// Calculate the current location of the scroll bar
    /// </summary>
    /// <returns></returns>
    private int CalculateScrollBarPostion()
    {
        var currentIncrement = (float)_buttonIndexOffset / (_buttonText.Count - _numberOfButtons);
        var incrementSize = _scrollBarEnd - _scrollBarStart;
        var startingYPos = _boundingBox.Y + _upArrow.bounds.Height;
        var scrollBarY = startingYPos + (incrementSize * currentIncrement);
        return (int)scrollBarY;
    }

    /// <summary>
    /// Handles the ressizing of the window
    /// </summary>
    /// <param name="oldBounds">Old bounds of the window</param>
    /// <param name="newBounds">New bounds of the window</param>
    public void OnWindowResize(Rectangle oldBounds, Rectangle newBounds)
    {
        UpdateButtonData();
    }
}
