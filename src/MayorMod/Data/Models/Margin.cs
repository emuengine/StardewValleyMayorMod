namespace MayorMod.Data.Models;

public class Margin(int left, int top, int right, int bottom)
{
    public int Left { get; set; } = left;
    public int Right { get; set; } = right;
    public int Top { get; set; } = top;
    public int Bottom { get; set; } = bottom;
}
