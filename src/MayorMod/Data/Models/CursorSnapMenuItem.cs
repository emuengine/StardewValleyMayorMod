namespace MayorMod.Data.Menu;

public partial class MayorModMenu
{
    public record CursorSnapMenuItem(IClickableMenuItem menuItem, int index);
}