namespace MayorMod.Data.Menu;

public interface IClickableMenuItem : IMenuItem
{
    void OnLeftClick(int x, int y);
    void OnHover(int x, int y);
    int UpdateCursor(int index);
}
