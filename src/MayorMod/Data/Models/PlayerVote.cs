namespace MayorMod.Data.Models;

public class PlayerVote
{
    public long PlayerID { get; set; } = -1;
    public string VotedFor { get; set; } = string.Empty;
}
