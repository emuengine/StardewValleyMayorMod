using MayorMod.Data.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

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
        public int Padding { get; set; } = 5;
        private IList<VotingButton> _buttons = [];
        public Action<int> ButtonAction { get; set; }
        //TODO: Fix this
        public IList<string> Candidates = ["Lewis", Game1.player.Name];

        public record VotingButton
        {
            public string Name { get; set; } = string.Empty;
            public Rectangle ButtonRect { get; set; }
            public int TextureIndex { get; set; }
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
                    ButtonRect = new Rectangle(_boundingBox.X + 25,
                                               _boundingBox.Y + Padding + (height * i) + (height / 2) - 32,
                                               64,
                                               64),
                });
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            var height = (_boundingBox.Height / _buttons.Count);
            for (int i = 0; i < _buttons.Count; i++)
            {
                Utility.DrawSquare(spriteBatch,
                   new Rectangle(_boundingBox.X + Padding + 2,
                                 _boundingBox.Y + Padding + (height * i),
                                 _boundingBox.Width - Padding,
                                 (int)((_boundingBox.Height - Padding) / (_buttons.Count+.2)) - 4),
                   5,
                   Color.Black);

                int textPadding = Padding;
                if (_texture is not null)
                {
                    var textreSrcRect = new Rectangle(_buttons[i].TextureIndex*64, 0, 64, 64);
                    spriteBatch.Draw(_texture, _buttons[i].ButtonRect, textreSrcRect, Color.White);
                    textPadding = _texture.Width + 25 + Padding;
                }

                Utility.drawBoldText(spriteBatch,
                                     _buttons[i].Name,
                                     Game1.dialogueFont,
                                     new Vector2(_boundingBox.X + textPadding,
                                                 _boundingBox.Y + (height*i) + (height / 2) - _fontHeight),
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
