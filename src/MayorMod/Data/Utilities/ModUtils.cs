using MayorMod.Constants;
using MayorMod.Data.Handlers;
using MayorMod.Data.Models;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.GameData;
using static StardewValley.GameLocation;

namespace MayorMod.Data.Utilities;
public static class ModUtils
{
    public static Random RNG { get; } = new();

    public static string[] DayNames { get; } = new string[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

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
            Season.Spring => new int[] { 11, 12, 21, 22, 25 },
            Season.Summer => new int[] { 9, 12, 18, 25, 27 },
            Season.Fall => new int[] { 4, 7, 8, 9, 12, 19, 22, 25 },
            Season.Winter => new int[] { 5, 11, 12, 19, 22, 24 },
            _ => Array.Empty<int>(),
        };
        var randomDate = rngSeeded.Next(possible_days.Length);
        var days = new List<int>()
        {
            possible_days[randomDate],
            possible_days[(randomDate + possible_days.Length / 2) % possible_days.Length]
        };
        return days.Contains(date.Day);
    }

    /// <summary>
    /// Adds an item to the current players inventory.
    /// </summary>
    /// <param name="itemId">Item to add</param>
    public static void AddItemToInventory(string itemId)
    {
        var item = ItemRegistry.Create(itemId);
        Game1.player.addItemToInventory(item);
    }

    /// <summary>
    /// Creates a dialogue which allows you to resign as mayor and reset the mod.
    /// </summary>
    /// <param name="helper"></param>
    /// <param name="farmer"></param>
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

    /// <summary>
    /// Determines and returns the next upcoming day (from today) that a council meeting is scheduled,
    /// formatted as a localized day name.
    /// </summary>
    /// <param name="helper">The mod helper used for translation.</param>
    /// <param name="modConfig">The config object containing the active meeting days.</param>
    /// <returns>A localized string representing the next scheduled meeting day.</returns>
    public static string GetNextCouncilMeetingDay(IModHelper helper, MayorModConfig modConfig)
    {
        var today = (int)WorldDate.GetDayOfWeekFor(Game1.dayOfMonth);

        int nextMeetingDay = Enumerable.Range(1, 7)
            .Select(offset => (today + offset) % 7)
            .FirstOrDefault(i => modConfig.MeetingDays[i]);

        return ModUtils.GetTranslationForKey(helper, $"{ModKeys.MAYOR_MOD_CPID}_UIMenu.{DayNames[nextMeetingDay]}");
    }

    /// <summary>
    /// Returns a human-readable, localized string representing the days of the week
    /// when the mayor has scheduled meetings, formatted with commas and an "and" before the last item.
    /// </summary>
    /// <param name="helper">The mod helper used for translations.</param>
    /// <param name="modConfig">The config object that contains which days meetings are held.</param>
    /// <returns>A formatted, localized string of meeting days (e.g., "Monday and Wednesday").</returns>
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

    /// <summary>
    /// Gets the name of the current mayor based on the player's progress and game events.
    /// </summary>
    /// <returns>A string containing the name of the current mayor.</returns>
    public static string GetCurrentMayor(IModHelper helper)
    {
        var morrisIsMayor = Game1.MasterPlayer.eventsSeen.Contains(CompatibilityKeys.MorrisIsMayorEventID);
        var playerIsMayor = ModProgressHandler.HasProgressFlag(ProgressFlags.ElectedAsMayor);
        if (playerIsMayor)
        {
            return Game1.MasterPlayer.Name;
        }
        else if (morrisIsMayor)
        {
            return GetTranslationForKey(helper, $"{ModKeys.MAYOR_MOD_CPID}_AssetUpdates.SubString.Morris");
        }
        else
        {
            return GetTranslationForKey(helper, $"{ModKeys.MAYOR_MOD_CPID}_AssetUpdates.SubString.Lewis");
        }
    }

    public static void AddConversationTopic(string topicId, int daysDuration)
    {
        Game1.player.activeDialogueEvents[topicId] = daysDuration;
    }

    public static void RemoveConversationTopic(string topicId)
    {
        Game1.MasterPlayer.previousActiveDialogueEvents.RemoveWhere(m => m.Key == topicId);
        Game1.MasterPlayer.activeDialogueEvents.RemoveWhere(m => m.Key == topicId);
        //look at HasNPCBeenCanvassed
    }
    public static bool HasConversationTopic(string topicId)
    {
        return Game1.player.activeDialogueEvents.ContainsKey(topicId) && Game1.player.activeDialogueEvents[topicId] > 0;
    }
}
