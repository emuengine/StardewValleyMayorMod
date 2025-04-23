using MayorMod.Constants;
using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
using System.Text.RegularExpressions;

namespace MayorMod.Data;
public static class HelperMethods
{
    private static NPC? _officerMikeNPC;
    public static NPC OfficerMikeNPC => _officerMikeNPC is null ? Utility.fuzzyCharacterSearch(ModNPCKeys.OfficerMike) : _officerMikeNPC;
    public static NetStringHashSet MasterPlayerMail => Game1.MasterPlayer.mailReceived;


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

    public static NPC GetNPCForPlayerInteraction()
    {
        if (Utility.checkForCharacterInteractionAtTile(Game1.player.GetGrabTile(), Game1.GetPlayer(Game1.player.UniqueMultiplayerID)))
        {
            return Game1.currentLocation.isCharacterAtTile(Game1.player.GetGrabTile());
        }
        return null;
    }

    public static string GetTranslationForKey(IModHelper helper, string translationKey)
    {
        try
        {
            var modInfo = helper.ModRegistry.Get(ModKeys.MayorModCPId);
            var cpPack = modInfo.GetType().GetProperty("ContentPack")?.GetValue(modInfo) as IContentPack;
            return cpPack.Translation.Get(translationKey).ToString();
        }
        catch
        {
            return string.Empty;
        }
    }

    public static bool AssetNameStartsWith(IAssetName assetName, string startsWith)
    {
        var assetNameClean = Regex.Replace(assetName.BaseName, "[^a-zA-Z0-9]", "");
        var startsWithClean = Regex.Replace(startsWith, "[^a-zA-Z0-9]", "");
        return assetNameClean.StartsWithIgnoreCase(startsWithClean);
    }
}
