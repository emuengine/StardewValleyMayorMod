using MayorMod.Constants;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;

namespace MayorMod.Data;
public static class ModHelper
{
    private static NPC? _officerMikeNPC;
    public static NPC OfficerMikeNPC => _officerMikeNPC is null? Utility.fuzzyCharacterSearch("MayorMod_OfficerMike"): _officerMikeNPC;

    public static NetStringHashSet MasterPlayerMail => Game1.MasterPlayer.mailReceived;

    public static void RemoveProgressMails()
    {
        MasterPlayerMail.Remove(ModProgressKeys.RegisteringForBalot);
        MasterPlayerMail.Remove(ModProgressKeys.VotingMayor);
    }


    public static void DrawSpriteTemporarily(GameLocation location, Vector2 position, string textureName, float timeInMiliseconds = 1000.0f)
    {
        location.temporarySprites.Add(new TemporaryAnimatedSprite(textureName, 
                                      sourceRect: new Rectangle(0, 0, 16, 16),
                                      animationInterval: timeInMiliseconds, 
                                      animationLength: 100, 
                                      numberOfLoops: 10,
                                      position: position * Game1.pixelZoom, 
                                      flicker: false, 
                                      flipped: false, 
                                      layerDepth: 1.0f, 
                                      alphaFade: 0.0f, 
                                      color: Color.White, 
                                      scale: Game1.pixelZoom, 
                                      scaleChange: 0.0f, 
                                      rotation: 0.0f,
                                      rotationChange: 0.0f));
    }
}
