using MayorMod.Constants;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Extensions;
using System.Text.RegularExpressions;

namespace MayorMod.Data;
public static class HelperMethods
{
    private static NPC? _officerMikeNPC;
    /// <summary>
    /// The NPC instance for Officer Mike, or a fuzzy search result if the instance hasn't been initialized.
    /// </summary>
    public static NPC OfficerMikeNPC => _officerMikeNPC is null ? Utility.fuzzyCharacterSearch(ModNPCKeys.OfficerMike) : _officerMikeNPC;

    /// <summary>
    /// Draws a sprite temporarily at the specified location and position.
    /// </summary>
    /// <param name="location">The game location where the sprite will be drawn.</param>
    /// <param name="position">The 2D position where the sprite will be drawn.</param>
    /// <param name="textureName">The name of the texture to draw.</param>
    /// <param name="timeInMiliseconds">The duration in milliseconds for which the sprite will be visible (default: 1000ms).</param>
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

    /// <summary>
    /// Retrieves the translation for a given key from Content Patcher.
    /// </summary>
    /// <param name="helper">The IModHelper instance.</param>
    /// <param name="translationKey">The translation key to look up.</param>
    /// <returns>The translated string if found, otherwise an empty string.</returns>
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

    /// <summary>
    /// Returns a date without any festival days after the specified offset.
    /// </summary>
    /// <param name="dayOffset">The number of days to look ahead in the future.</param>
    /// <returns>A date that is not a festival day, calculated based on the current date and the provided offset.</returns>
    public static SDate GetDateWithoutFestival(int dayOffset)
    {
        //TODO check for community day
        //TODO just pick a damn day if its more than a month
        var returnDate = SDate.Now().AddDays(dayOffset);
        while (Utility.isFestivalDay(returnDate.Day, returnDate.Season))
        {
            returnDate.AddDays(1);
        }
        return returnDate;
    }

    /// <summary>
    /// Checks if the base name of an asset name starts with a specified string (case-insensitive).
    /// </summary>
    /// <param name="assetName">The asset name to check.</param>
    /// <param name="startsWith">The string that should start the asset name (case-insensitive).</param>
    /// <returns>True if the base name of the asset starts with the specified string, false otherwise.</returns>
    public static bool AssetNameStartsWith(IAssetName assetName, string startsWith)
    {
        var assetNameClean = Regex.Replace(assetName.BaseName, "[^a-zA-Z0-9]", "");
        var startsWithClean = Regex.Replace(startsWith, "[^a-zA-Z0-9]", "");
        return assetNameClean.StartsWithIgnoreCase(startsWithClean);
    }

    /// <summary>
    /// Retrieves the NPC that is currently interacting with the player.
    /// </summary>
    /// <returns>The interacting NPC, or null if no interaction is occurring.</returns>
    public static NPC GetNPCForPlayerInteraction()
    {
        if (Utility.checkForCharacterInteractionAtTile(Game1.player.GetGrabTile(), Game1.GetPlayer(Game1.player.UniqueMultiplayerID)))
        {
            return Game1.currentLocation.isCharacterAtTile(Game1.player.GetGrabTile());
        }
        return null;
    }
}
