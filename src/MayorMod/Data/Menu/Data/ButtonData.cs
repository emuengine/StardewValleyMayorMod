using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace MayorMod.Data.Menu.Data;

public class ButtonData
{
    public int Id { get; set; }
    public Rectangle BoundingBox { get; set; }
    public bool IsHighlighted { get; set; }
}