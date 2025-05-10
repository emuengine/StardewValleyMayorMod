namespace MayorMod.Data.Menu;

public interface IScrollableMenuItem : IMenuItem
{
    void OnScroll(int direction);
}
