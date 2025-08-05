using MayorMod.Constants;
using MayorMod.Data.Handlers;
using MayorMod.Data.Models;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.Locations;
using StardewValley.Menus;
using System.Text.RegularExpressions;
using static StardewValley.GameLocation;

namespace MayorMod.Data;
public static class ModUtils
{
    public static Random RNG { get; } = new();

    public static string[] DayNames { get; } = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];

    private static NPC? _marlonNPC;
    /// <summary>
    /// The NPC instance for Marlon, or a fuzzy search result if the instance hasn't been initialized.
    /// </summary>
    public static NPC MarlonNPC
    {
        get
        {
            _marlonNPC ??= Utility.fuzzyCharacterSearch(ModNPCKeys.MarlonId);
            return _marlonNPC;
        }
    }

    private static NPC? _officerMikeNPC;
    /// <summary>
    /// The NPC instance for Officer Mike, or a fuzzy search result if the instance hasn't been initialized.
    /// </summary>
    public static NPC OfficerMikeNPC
    {
        get
        {
            _officerMikeNPC ??= Utility.fuzzyCharacterSearch(ModNPCKeys.OfficerMikeId);
            return _officerMikeNPC;
        }
    }

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
            var modInfo = helper.ModRegistry.Get(ModKeys.MAYOR_MOD_CPID);
            var cpPack = modInfo?.GetType().GetProperty("ContentPack")?.GetValue(modInfo) as IContentPack;
            var key = cpPack?.Translation.Get(translationKey).ToString();
            return key ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Returns a date without any festival days after the specified offset. 
    /// It will just pick a damn day if its more than 30 days
    /// </summary>
    /// <param name="dayOffset">The number of days to look ahead in the future.</param>
    /// <returns>A date that is not a festival day, calculated based on the current date and the provided offset.</returns>
    public static SDate GetDateWithoutFestival(int dayOffset)
    {
        var returnDate = SDate.Now().AddDays(dayOffset);
        int count = 0;
        while (count < 30 && (Utility.isFestivalDay(returnDate.Day, returnDate.Season) || IsBooksellerVisiting(returnDate) || IsPassiveFestivalDay(returnDate)))
        {
            count++;
            returnDate = returnDate.AddDays(1);
        }
        return returnDate;
    }

    /// <summary>
    /// Check if date is on a passive festival day
    /// </summary>
    /// <param name="date">date to check</param>
    /// <param name="isActive">Check the conditions for passive festival or just dates</param>
    public static bool IsPassiveFestivalDay(SDate date, bool isActive = false)
    {
        if (isActive)
        {
            return Utility.TryGetPassiveFestivalDataForDay(date.Day, date.Season, null, out _,out _);
        }
        else
        {
            foreach (var id in DataLoader.PassiveFestivals(Game1.content).Keys)
            {
                Utility.TryGetPassiveFestivalData(id, out PassiveFestivalData data);
                if (data is not null && date.Day >= data.StartDay && date.Day <= data.EndDay && date.Season == data.Season)
                {
                    return true;
                }
            }
            return false;
        }
    }

    /// <summary>
    /// Check if the bookseller is visiting for a date.
    /// </summary>
    /// <param name="date">Date to chec</param>
    /// <returns>True if bookseller is visiting on date</returns>
    public static bool IsBooksellerVisiting(SDate date)
    {
        var rngSeeded = Utility.CreateRandom(date.Year * 11, Game1.uniqueIDForThisGame, date.SeasonIndex);
        int[] possible_days = Game1.season switch
        {
            Season.Spring => [11, 12, 21, 22, 25],
            Season.Summer => [9, 12, 18, 25, 27],
            Season.Fall => [4, 7, 8, 9, 12, 19, 22, 25],
            Season.Winter => [5, 11, 12, 19, 22, 24],
            _ => [],
        };
        var randomDate = rngSeeded.Next(possible_days.Length);
        IList<int> days = 
        [
            possible_days[randomDate],
            possible_days[(randomDate + possible_days.Length / 2) % possible_days.Length]
        ];
        return days.Contains(date.Day);
    }

    /// <summary>
    /// Adds an item to the current players inventory
    /// </summary>
    /// <param name="itemId">Item to add</param>
    public static void AddItemToInventory(string itemId)
    {
        var item = ItemRegistry.Create(itemId);
        Game1.player.addItemToInventory(item);
    }

    /// <summary>
    /// Force planned council meeting mails to be added to the mailReceived list
    /// </summary>
    public static void ForceCouncilMailDelivery()
    {
        //TODO: look into why I seem to have to do this
        var meetingTomorrow = Game1.MasterPlayer.mailForTomorrow.FirstOrDefault(p => p.StartsWith(CouncilMeetingKeys.PlannedPrefix));
        if (meetingTomorrow is not null)
        {
            Game1.MasterPlayer.mailForTomorrow.Remove(meetingTomorrow);
            Game1.MasterPlayer.mailReceived.Add(meetingTomorrow);
        }
    }

    public static void DrawDialogueCharacterString(string location, params string[] stringFormatParam)
    {
        var haveVotingCardDialogue = Game1.content.LoadString($"Strings\\Characters:{location}");
        haveVotingCardDialogue = string.Format(haveVotingCardDialogue, stringFormatParam);
        Game1.drawObjectDialogue(haveVotingCardDialogue);
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
    public static NPC? GetNPCForPlayerInteraction()
    {
        if (Utility.checkForCharacterInteractionAtTile(Game1.player.GetGrabTile(), Game1.GetPlayer(Game1.player.UniqueMultiplayerID)))
        {
            return Game1.currentLocation.isCharacterAtTile(Game1.player.GetGrabTile());
        }
        return null;
    }

    /// <summary>
    /// Code to try Remove Travelling Cart. Doesn't work but might neeed it in the future.
    /// </summary>
    private static void RemoveTravellingCart()
    {
        //need to edit the passive festivals too
        var f = Game1.locationData["Forest"];
        Forest forest = (Forest)Game1.getLocationFromName("Forest");
        forest.travelingMerchantBounds.Clear();
        forest.travelingMerchantDay = false;
        ((Forest)Game1.getLocationFromName(nameof(Forest))).ShouldTravelingMerchantVisitToday();
    }

    public static void OpenResignationDialogue(IModHelper helper, Farmer farmer)
    {
        if (farmer.userID != Game1.MasterPlayer.userID)
        {
            return;
        }

        afterQuestionBehavior resign = (Farmer who, string whichAnswer) =>
        {
            if (whichAnswer == "Yes")
            {
                ModProgressHandler.AddProgressFlag(ProgressFlags.ModNeedsReset);
                Game1.drawObjectDialogue(GetTranslationForKey(helper, DialogueKeys.Resignation.ResignText));
            }
        };

        afterQuestionBehavior doubleCheckResign = (Farmer who, string whichAnswer) =>
        {
            if (whichAnswer == "Yes")
            {
                DelayedAction.functionAfterDelay(()=> // Hack to chain question dialogues
                {
                    farmer.currentLocation.createQuestionDialogue(GetTranslationForKey(helper, DialogueKeys.Resignation.DoubleCheck),
                                                                  farmer.currentLocation.createYesNoResponses(),
                                                                  resign);
                }, 1);
            }
        };

        if (!ModProgressHandler.HasProgressFlag(ProgressFlags.ModNeedsReset))
        {
            Game1.player.currentLocation.createQuestionDialogue(GetTranslationForKey(helper, DialogueKeys.Resignation.Question), 
                                                                farmer.currentLocation.createYesNoResponses(), 
                                                                doubleCheckResign);
        }
    }
    
    public static string GetNextCouncilMeetingDay(IModHelper helper, MayorModConfig modConfig)
    {
        var today = (int)WorldDate.GetDayOfWeekFor(Game1.dayOfMonth);

        int nextMeetingDay = Enumerable.Range(0, 7)
            .Select(offset => (today + offset) % 7)
            .FirstOrDefault(i => modConfig.MeetingDays[i]);

        return ModUtils.GetTranslationForKey(helper, $"{ModKeys.MAYOR_MOD_CPID}_UIMenu.{DayNames[nextMeetingDay]}");
    }


    public static string GetFormattedMeetingDays(IModHelper helper, MayorModConfig modConfig)
    {
        var activeDays = DayNames.Select( dt => ModUtils.GetTranslationForKey(helper, $"{ModKeys.MAYOR_MOD_CPID}_UIMenu.{dt}"))
            .Where((day, index) => modConfig.MeetingDays[index])
            .ToList();
        var and = ModUtils.GetTranslationForKey(helper, $"{ModKeys.MAYOR_MOD_CPID}_UIMenu.And");

        return activeDays.Count switch
        {
            0 => string.Empty,
            1 => activeDays[0],
            2 => $"{activeDays[0]} {and} {activeDays[1]}",
            _ => string.Join(", ", activeDays.Take(activeDays.Count - 1)) + $", {and} " + activeDays.Last(),
        };
    }
}
