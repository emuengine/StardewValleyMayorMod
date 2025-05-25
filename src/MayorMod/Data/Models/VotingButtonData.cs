using Microsoft.Xna.Framework;

namespace MayorMod.Data.Models;

public record VotingButtonData
{
    public string Name { get; set; } = string.Empty;
    public Rectangle ButtonRect { get; set; }
    public int TextureIndex { get; set; }
    public Color Colour { get; set; }
}
