namespace MayorMod.Data.Menu;

public interface IScrollableMenuItem : IMenuItem
{
    bool OnScroll(int direction);
}