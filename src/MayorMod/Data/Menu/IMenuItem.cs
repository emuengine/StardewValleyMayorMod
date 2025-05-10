using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MayorMod.Data.Menu;

public interface IMenuItem
{
    void Draw(SpriteBatch spriteBatch);
    void OnWindowResize(Rectangle oldBounds, Rectangle newBounds);
}