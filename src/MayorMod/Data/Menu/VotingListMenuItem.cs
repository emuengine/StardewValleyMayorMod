using MayorMod.Data.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Extensions;

namespace MayorMod.Data.Menu
{
    internal class VotingListMenuItem : IClickableMenuItem
    {
        private bool _closing = false;
        private Margin _margin;
        private MayorModMenu _parent;
        private float _fontHeight;
        private Rectangle _boundingBox;
        private Texture2D? _texture;
        //public int Padding { get; set; } = 5;
        private IList<VotingButton> _buttons = [];
        public Action<int> ButtonAction { get; set; }
        //TODO: Fix this
        public IList<string> Candidates = ["Lewis", Game1.player.Name];

        public record VotingButton
        {
            public string Name { get; set; } = string.Empty;
            public Rectangle ButtonRect { get; set; }
            public int TextureIndex { get; set; }
            public Color Colour { get; set; }
        }

        internal VotingListMenuItem(MayorModMenu parent, Margin margin, Action<int> action)
        {
            _parent = parent;
            _margin = margin;
            _fontHeight = Game1.dialogueFont.MeasureString("TEXT").Y / 2;
            ButtonAction = action;
            Init();
        }

        private void Init()
        {
            _boundingBox = new Rectangle(_parent.MenuRect.X + _margin.Left,
                                         _parent.MenuRect.Y + _margin.Top,
                                         _parent.MenuRect.Width - _margin.Right - _margin.Left,
                                         _parent.MenuRect.Height - _margin.Bottom - _margin.Top);

            _texture = _parent.Helper.ModContent.Load<Texture2D>("assets/voteTickbox.png");

            _buttons.Clear();
            var height = (_boundingBox.Height / Candidates.Count);
            for (int i = 0; i < Candidates.Count; i++)
            {
                _buttons.Add(new VotingButton
                {
                    Name = Candidates[i],
                    Colour =  i % 2 == 0? Color.Blue : Color.Green,
                    ButtonRect = new Rectangle(_boundingBox.X,
                                               _boundingBox.Y + (height * i),
                                               _boundingBox.Width,
                                               height)
                });
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            var height = (_boundingBox.Height / _buttons.Count);

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

        public void OnWindowResize(Rectangle oldBounds, Rectangle newBounds)
        {
            Init();
        }
    }
}
