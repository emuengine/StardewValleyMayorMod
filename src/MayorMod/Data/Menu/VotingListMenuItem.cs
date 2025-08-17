using MayorMod.Data.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace MayorMod.Data.Menu;

internal partial class VotingListMenuItem : IClickableMenuItem
{
    private readonly MayorModMenu _parent;
    private readonly Texture2D? _texture;
    private readonly Margin _margin;
    private readonly float _fontHeight;
    private readonly IList<string> _candidates;
    private bool _closing;
    private Rectangle _boundingBox;
    //public int Padding { get; set; } = 5;
    private IList<VotingButtonData> _buttons = new List<VotingButtonData>();
    public Action<int> ButtonAction { get; set; }

    internal VotingListMenuItem(MayorModMenu parent, Margin margin, IList<string> candidates, Action<int> action)
    {
        _parent = parent;
        _margin = margin;
        _candidates = candidates;
        _fontHeight = Game1.dialogueFont.MeasureString("TEXT").Y / 2;
        _texture = _parent.Helper.ModContent.Load<Texture2D>("assets/voteTickbox.png");
        ButtonAction = action;
        Init();
    }

    /// <summary>
    /// Initializes the voting list menu item's properties.
    /// </summary>
    private void Init()
    {
        _boundingBox = new Rectangle(_parent.MenuRect.X + _margin.Left,
                                     _parent.MenuRect.Y + _margin.Top,
                                     _parent.MenuRect.Width - _margin.Right - _margin.Left,
                                     _parent.MenuRect.Height - _margin.Bottom - _margin.Top);

        _buttons.Clear();
        var height = (_boundingBox.Height / _candidates.Count);
        for (int i = 0; i < _candidates.Count; i++)
        {
            _buttons.Add(new VotingButtonData
            {
                Name = _candidates[i],
                Colour =  i % 2 == 0? Color.Blue : Color.Green,
                ButtonRect = new Rectangle(_boundingBox.X,
                                           _boundingBox.Y + (height * i),
                                           _boundingBox.Width,
                                           height)
            });
        }
    }

    /// <summary>
    /// Draws the voting list menu item on the screen.
    /// </summary>
    /// <param name="spriteBatch">The SpriteBatch to draw with.</param>
    public void Draw(SpriteBatch spriteBatch)
    {
        for (int i = 0; i < _buttons.Count; i++)
        {
            var squareBounds = _buttons[i].ButtonRect;
            squareBounds.Height = (int)(squareBounds.Height * .6);
            squareBounds.Y += (int)(squareBounds.Height * .2);

            var colourBounds = squareBounds;
            colourBounds.Width /= 10;
            colourBounds.X += colourBounds.Width * 9;

            Utility.DrawSquare(spriteBatch, colourBounds, 5, Color.Transparent, _buttons[i].Colour);
            Utility.DrawSquare(spriteBatch, squareBounds, 5, Color.Black);


            if (_texture is not null)
            {
                var buttonBounds = squareBounds;
                buttonBounds.X += 15;
                buttonBounds.Y += ((buttonBounds.Height / 2) - 32);
                buttonBounds.Width = buttonBounds.Height = 64;

                var textreSrcRect = new Rectangle(_buttons[i].TextureIndex * 64, 0, 64, 64);
                spriteBatch.Draw(_texture, buttonBounds, textreSrcRect, Color.White);
            }

            Utility.drawBoldText(spriteBatch,
                                 _buttons[i].Name,
                                 Game1.dialogueFont,
                                 new Vector2(squareBounds.X + 100,
                                             squareBounds.Y + (squareBounds.Height/2) - _fontHeight),
                                 Game1.textColor);
        }
    }

    /// <summary>
    /// Handles hover events for the voting list menu item.
    /// </summary>
    /// <param name="x">The x-coordinate of the mouse position.</param>
    /// <param name="y">The y-coordinate of the mouse position.</param>
    public void OnHover(int x, int y)
    {
        for (int i = 0; i < _buttons.Count; i++)
        {
            if (_buttons[i].TextureIndex != 2)
            {
                _buttons[i].TextureIndex = _buttons[i].ButtonRect.Contains(new Point(x, y)) ? 1 : 0;
            }
        }
    }

    /// <summary>
    /// Handles left-click events for the voting list menu item.
    /// </summary>
    /// <param name="x">The x-coordinate of the mouse position.</param>
    /// <param name="y">The y-coordinate of the mouse position.</param>
    public void OnLeftClick(int x, int y)
    {
        if(_closing)
        {
            return; 
        }

        for (int i = 0; i < _buttons.Count; i++)
        {
            if (_buttons[i].ButtonRect.Contains(new Point(x, y)))
            {
                _closing = true;
                _buttons[i].TextureIndex = 2;
                DelayedAction.functionAfterDelay(() =>
                {
                    ButtonAction.Invoke(i);
                }, 200);
                return;
            }
        }
    }

    /// <summary>
    /// Handles window resize events for the voting list menu item.
    /// </summary>
    /// <param name="oldBounds">The old bounds of the window.</param>
    /// <param name="newBounds">The new bounds of the window.</param>
    public void OnWindowResize(Rectangle oldBounds, Rectangle newBounds)
    {
        Init();
    }

    /// <summary>
    /// Updates the cursor position for the voting list menu item.
    /// </summary>
    /// <param name="index">The index of the button to update the cursor position for.</returns>
    public int UpdateCursor(int index)
    {
        var cursorData = _buttons.Select(b => new Point(b.ButtonRect.X + (b.ButtonRect.Width / 2), b.ButtonRect.Y + (b.ButtonRect.Height / 2))).ToList();
        if (index >= 0 && index < cursorData.Count)
        {
            Game1.setMousePosition(cursorData[index]);
            return index;
        }
        return -1;
    }
}
