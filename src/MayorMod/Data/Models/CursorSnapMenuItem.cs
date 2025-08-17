namespace MayorMod.Data.Menu;

public partial class MayorModMenu
{
    public record CursorSnapMenuItem(IClickableMenuItem MenuItem, int Index);
}