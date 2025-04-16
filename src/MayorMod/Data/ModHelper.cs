using MayorMod.Constants;
using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
using System.Text.RegularExpressions;

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

    public static NPC GetNPCForPlayerInteraction()
    {
        if (Utility.checkForCharacterInteractionAtTile(Game1.player.GetGrabTile(), Game1.GetPlayer(Game1.player.UniqueMultiplayerID)))
        {
            return Game1.currentLocation.isCharacterAtTile(Game1.player.GetGrabTile());
        }
        return null;
    }

    public static bool AssetNameStartsWith(IAssetName assetName, string startsWith)
    {
        var assetNameClean = Regex.Replace(assetName.BaseName, "[^a-zA-Z0-9]", "");
        var startsWithClean = Regex.Replace(startsWith, "[^a-zA-Z0-9]", "");
        return assetNameClean.StartsWithIgnoreCase(startsWithClean);
    }

    public static (string, string)? GetAdditionalDialogueForLeaflets(IAssetName assetName)
    {
          if (assetName.IsEquivalentTo("Characters/Dialogue/Abigail") ||
            assetName.IsEquivalentTo("Characters/Dialogue/Alex") ||
            assetName.IsEquivalentTo("Characters/Dialogue/Caroline") ||
            assetName.IsEquivalentTo("Characters/Dialogue/Clint") ||
            assetName.IsEquivalentTo("Characters/Dialogue/Demetrius") ||
            assetName.IsEquivalentTo("Characters/Dialogue/Elliott") ||
            assetName.IsEquivalentTo("Characters/Dialogue/Emily") ||
            assetName.IsEquivalentTo("Characters/Dialogue/Evelyn") ||
            assetName.IsEquivalentTo("Characters/Dialogue/George") ||
            assetName.IsEquivalentTo("Characters/Dialogue/Gus") ||
            assetName.IsEquivalentTo("Characters/Dialogue/Haley") ||
            assetName.IsEquivalentTo("Characters/Dialogue/Harvey") ||
            assetName.IsEquivalentTo("Characters/Dialogue/Jodi") ||
            assetName.IsEquivalentTo("Characters/Dialogue/Kent") ||
            assetName.IsEquivalentTo("Characters/Dialogue/Leah") ||
            assetName.IsEquivalentTo("Characters/Dialogue/Lewis") ||
            assetName.IsEquivalentTo("Characters/Dialogue/Marnie") ||
            assetName.IsEquivalentTo("Characters/Dialogue/Maru") ||
            assetName.IsEquivalentTo("Characters/Dialogue/Pam") ||
            assetName.IsEquivalentTo("Characters/Dialogue/Penny") ||
            assetName.IsEquivalentTo("Characters/Dialogue/Pierre") ||
            assetName.IsEquivalentTo("Characters/Dialogue/Robin") ||
            assetName.IsEquivalentTo("Characters/Dialogue/Sam") ||
            assetName.IsEquivalentTo("Characters/Dialogue/Sandy") ||
            assetName.IsEquivalentTo("Characters/Dialogue/Sebastian") ||
            assetName.IsEquivalentTo("Characters/Dialogue/Shane") ||
            assetName.IsEquivalentTo("Characters/Dialogue/Willy") ||
            assetName.IsEquivalentTo("Characters/Dialogue/Wizard"))
        {
            //Adult
            return ("AcceptGift_(O)EmuEngine.MayorModCP_Leaflet", "Sure I'll take a look at your ideas.");
        }
        if (assetName.IsEquivalentTo("Characters/Dialogue/Leo") ||
            assetName.IsEquivalentTo("Characters/Dialogue/Vincent") ||
            assetName.IsEquivalentTo("Characters/Dialogue/Jas"))
        {
            //Kid
            return ("RejectItem_(O)EmuEngine.MayorModCP_Leaflet", "I'm just a kid. I can't vote.");
        }
        if (assetName.IsEquivalentTo("Characters/Dialogue/Krobus") ||
            assetName.IsEquivalentTo("Characters/Dialogue/Mister Qi") ||
            assetName.IsEquivalentTo("Characters/Dialogue/Dwarf") ||
            assetName.IsEquivalentTo("Characters/Dialogue/Gil"))
        {
            //Other
        }

        return null;
    }
}
