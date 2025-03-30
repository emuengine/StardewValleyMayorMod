using Microsoft.Xna.Framework;
using StardewValley;

namespace MayorMod.Data;
public static class Utils
{
    public static void DrawDialogueCharacterString(string location, params string[] stringFormatParam)
    {
        var haveVotingCardDialogue = Game1.content.LoadString($"Strings\\Characters:{location}");
        haveVotingCardDialogue = string.Format(haveVotingCardDialogue, stringFormatParam);
        Game1.drawObjectDialogue(haveVotingCardDialogue);
    }

    public static void DrawSpriteTemporarily(GameLocation location, Vector2 position, string textureName, float timeInMiliseconds = 1000.0f)
    {
        location.temporarySprites.Add(new TemporaryAnimatedSprite(textureName, 
                                      new Microsoft.Xna.Framework.Rectangle(0, 0, 16, 16),
                                      timeInMiliseconds, 
                                      100, 
                                      10, 
                                      position * Game1.pixelZoom, 
                                      false, 
                                      false, 
                                      1.0f, 
                                      0.0f, 
                                      Color.White, 
                                      Game1.pixelZoom, 
                                      0.0f, 
                                      0.0f, 
                                      0.0f));
    }
}
