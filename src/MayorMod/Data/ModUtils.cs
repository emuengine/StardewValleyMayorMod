using MayorMod.Constants;
using MayorMod.Data.Handlers;
using MayorMod.Data.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.GameData;
using static StardewValley.FarmerRenderer;
using static StardewValley.GameLocation;

namespace MayorMod.Data;
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
    /// Creates a new texture by applying a gold color palette to the specified source texture.
    /// </summary>
    /// texture.</remarks>
    /// <param name="sourceTexture">The original texture to be transformed. Cannot be null.</param>
    /// <returns>A new Texture2D instance with the gold color effect applied, or null if the source texture is null.</returns>
    public static Texture2D GoldifyTexture(Texture2D sourceTexture)
    {
        var device = Game1.graphics.GraphicsDevice;
        var goldTexture = new Texture2D(device, sourceTexture.Width, sourceTexture.Height);

        var pixels = new Color[sourceTexture.Width * sourceTexture.Height];
        sourceTexture.GetData(pixels);

        var goldPalette = new[]
        {
            (threshold: 0.15f, color: new Color(90, 36, 13)),      // Darkest gold
            (threshold: 0.35f, color: new Color(156, 85, 8)),      // Dark gold
            (threshold: 0.55f, color: new Color(238, 156, 44)),    // Medium gold
            (threshold: 0.75f, color: new Color(255, 233, 79)),    // Bright gold
            (threshold: 0.90f, color: new Color(255, 255, 168)),   // Light gold
            (threshold: 1.00f, color: new Color(255, 255, 255))    // White
        };

        for (int i = 0; i < pixels.Length; i++)
        {
            var pixel = pixels[i];
            if (pixel.A == 0)
            {
                continue;
            }

            float brightness = CalculateBrightness(pixel);
            brightness = ApplyContrast(brightness, 1.3f);

            var goldColor = MapBrightnessToGold(brightness, goldPalette);
            pixels[i] = new Color(goldColor.R, goldColor.G, goldColor.B, pixel.A);
        }

        goldTexture.SetData(pixels);
        return goldTexture;
    }

    /// <summary>
    /// Calculates the perceived brightness of a color using a weighted average of its red, green, and blue components.
    /// </summary>
    /// <param name="pixel">The color whose brightness is to be calculated.</param>
    /// <returns>A floating-point value between 0 and 1 representing the perceived brightness of the color, where 0 is black and
    /// 1 is white.</returns>
    public static float CalculateBrightness(Color pixel)
    {
        return (pixel.R * 0.299f + pixel.G * 0.587f + pixel.B * 0.114f) / 255f;
    }

    /// <summary>
    /// Adjusts the specified brightness value by applying the given contrast factor.
    /// </summary>
    /// <param name="brightness">The original brightness value to adjust. Must be between 0.0 and 1.0, where 0.0 represents black and 1.0
    /// represents white.</param>
    /// <param name="contrast">The contrast factor to apply. Values greater than 1.0 increase contrast; values between 0.0 and 1.0 decrease
    /// contrast.</param>
    /// <returns>The adjusted brightness value after applying the contrast factor, clamped to the range 0.0 to 1.0.</returns>
    public static float ApplyContrast(float brightness, float contrast)
    {
        brightness = (brightness - 0.5f) * contrast + 0.5f;
        return Math.Clamp(brightness, 0f, 1f);
    }

    /// <summary>
    /// Maps a brightness value to a corresponding color by interpolating between colors in a gold-themed palette.
    /// <param name="brightness">The brightness value to map.</param>
    /// <param name="palette">An array of threshold and color pairs that define the palette. Each tuple specifies a threshold value and its
    /// associated color, ordered by increasing threshold.</param>
    /// <returns>A color interpolated from the palette that corresponds to the specified brightness value.</returns>
    private static Color MapBrightnessToGold(float brightness, (float threshold, Color color)[] palette)
    {
        for (int i = 0; i < palette.Length - 1; i++)
        {
            if (brightness < palette[i + 1].threshold)
            {
                float rangeSize = palette[i + 1].threshold - palette[i].threshold;
                float normalizedPosition = (brightness - palette[i].threshold) / rangeSize;
                return Color.Lerp(palette[i].color, palette[i + 1].color, normalizedPosition);
            }
        }

        return palette[^1].color;
    }

    /// <summary>
    /// The method uses alpha blending to combine the textures, and the original textures are not modified.
    /// The returned texture has the same dimensions as the base texture.
    /// </summary>
    /// <param name="baseTexture">The base texture onto which the gold texture will be drawn. Must not be null.</param>
    /// <param name="goldTexture">The texture to overlay on top of the base texture. If null, the method returns null.</param>
    /// <returns>A new Texture2D containing the result of drawing the gold texture over the base texture. Returns null if
    /// goldTexture is null.</returns>
    public static Texture2D MergeTextures(Texture2D baseTexture, Texture2D goldTexture)
    {
        var renderTarget = new RenderTarget2D(
            Game1.graphics.GraphicsDevice,
            baseTexture.Width,
            baseTexture.Height,
            false,
            SurfaceFormat.Color,
            DepthFormat.None,
            0,
            RenderTargetUsage.DiscardContents);

        var previousTargets = Game1.graphics.GraphicsDevice.GetRenderTargets();
        Game1.graphics.GraphicsDevice.SetRenderTarget(renderTarget);
        Game1.graphics.GraphicsDevice.Clear(Color.Transparent);

        using (var batch = new SpriteBatch(Game1.graphics.GraphicsDevice))
        {
            batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            batch.Draw(baseTexture, Vector2.Zero, Color.White);
            batch.Draw(goldTexture, Vector2.Zero, Color.White);
            batch.End();
        }

        Game1.graphics.GraphicsDevice.SetRenderTargets(previousTargets);

        return renderTarget;
    }

    /// <summary>
    /// Renders and returns a texture representing the current player's standing pose, suitable for use in user
    /// interface elements or previews.
    /// </summary>
    /// <returns>A <see cref="Texture2D"/> containing the rendered image of the player's standing sprite, including hair and
    /// accessories.</returns>
    public static Texture2D GetFarmerStandingTexture()
    {
        var farmerWidth = 16;
        var farmerHeight = 48; //makes it 3 tiles high instead of 2 because hats are big
        var scale = 0.25f; // Scale down to 1x size (4x is default not sure why)
        var facingDirection = 2;
        var layerDepth = 0.8f;
        var animationFrame = new FarmerSprite.AnimationFrame(0, 0, secondaryArm: false, flip: false);
        var srcRect = new Rectangle(0, 0, farmerWidth, 32);
        var farmerPosition = new Vector2(0, 10);

        var retFarmerTexture = new RenderTarget2D(Game1.graphics.GraphicsDevice, farmerWidth, farmerHeight, false, SurfaceFormat.Color,
            DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

        var previousTargets = Game1.graphics.GraphicsDevice.GetRenderTargets();
        Game1.graphics.GraphicsDevice.SetRenderTarget(retFarmerTexture);
        Game1.graphics.GraphicsDevice.Clear(Color.Transparent);

        using (var batch = new SpriteBatch(Game1.graphics.GraphicsDevice))
        {
            batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            FarmerRenderer.isDrawingForUI = true;

            // Hide HairAndAccesories to draw them separatly
            var originalFeatureOffset = FarmerRenderer.featureXOffsetPerFrame[0];
            FarmerRenderer.featureXOffsetPerFrame[0] = -500;

            Game1.MasterPlayer.FarmerRenderer.draw(batch, animationFrame, currentFrame: 0, srcRect, farmerPosition,
                Vector2.Zero, layerDepth, facingDirection, Color.White, rotation: 0f, scale, Game1.MasterPlayer);
            
            FarmerRenderer.featureXOffsetPerFrame[0] = originalFeatureOffset;

            //This is done separatly to fix texture offsets for hair, shirt and accessories
            ModUtils.DrawHairAndAccesories(batch, facingDirection, Game1.MasterPlayer, farmerPosition, Vector2.Zero,
                scale, currentFrame: 0, rotation: 0f, Color.White, layerDepth);

            FarmerRenderer.isDrawingForUI = false;
            batch.End();
        }
        Game1.graphics.GraphicsDevice.SetRenderTargets(previousTargets);
        return retFarmerTexture;
    }

    /// <summary>
    /// Draws the hair, hat, and accessories for a specified farmer character at the given position and orientation.
    /// </summary>
    /// <param name="b">The sprite batch used to draw the hair and accessories.</param>
    /// <param name="facingDirection">The direction the character is facing.</param>
    /// <param name="who">The farmer to be drawn.</param>
    /// <param name="position">The on-screen position at which to draw the character's features, in pixels.</param>
    /// <param name="origin">The origin point for rotation and scaling, relative to the drawn features.</param>
    /// <param name="scale">The scale factor.</param>
    /// <param name="currentFrame">The current animation frame index, used to determine feature offsets and special cases.</param>
    /// <param name="rotation">The rotation, in radians, to apply to the drawn features.</param>
    /// <param name="overrideColor">The color to use when drawing the features.</param>
    /// <param name="layerDepth">The draw layer depth.</param>
    public static void DrawHairAndAccesories(SpriteBatch b, int facingDirection, Farmer who, Vector2 position, Vector2 origin, float scale, int currentFrame, float rotation, Color overrideColor, float layerDepth)
    {
        var renderer = Game1.MasterPlayer.FarmerRenderer;
        var drawScale = 4f * scale;
        var featureOffsetX = featureXOffsetPerFrame[currentFrame];
        var featureOffsetY = featureYOffsetPerFrame[currentFrame];

        // Determine hair style (accounting for hat coverage)
        var hairId = who.getHair();
        var hairMetadata = Farmer.GetHairStyleMetadata(hairId);
        var hat = who.hat.Value;

        if (hat != null && hat.hairDrawType.Value == 1 && hairMetadata != null && hairMetadata.coveredIndex != -1)
        {
            hairId = hairMetadata.coveredIndex;
            hairMetadata = Farmer.GetHairStyleMetadata(hairId);
        }

        // Setup colors
        var hairColor = who.prismaticHair.Value ? Utility.GetPrismaticColor() : who.hairstyleColor.Value;
        var effectiveColor = overrideColor.Equals(Color.White) ? Color.White : overrideColor;

        // Setup shirt
        who.GetDisplayShirt(out var shirtTexture, out var spriteIndex);
        renderer.shirtSourceRect = new Rectangle(spriteIndex * 8 % 128, spriteIndex * 8 / 128 * 32, 8, 8);

        // Setup hair
        var hairTexture = hairMetadata?.texture ?? hairStylesTexture;
        renderer.hairstyleSourceRect = hairMetadata != null
            ? new Rectangle(hairMetadata.tileX * 16, hairMetadata.tileY * 16, 16, 32)
            : new Rectangle(hairId * 16 % hairTexture.Width, hairId * 16 / hairTexture.Width * 96, 16, 32);

        // Setup accessory
        if (who.accessory.Value >= 0)
        {
            renderer.accessorySourceRect = new Rectangle(
                who.accessory.Value * 16 % accessoriesTexture.Width,
                who.accessory.Value * 16 / accessoriesTexture.Width * 32,
                16, 16);
        }

        // Setup hat
        var hatTexture = hatsTexture;
        var isHatErrorItem = false;

        if (hat != null)
        {
            var hatData = ItemRegistry.GetDataOrErrorItem(hat.QualifiedItemId);
            var hatSpriteIndex = hatData.SpriteIndex;
            hatTexture = hatData.GetTexture();
            renderer.hatSourceRect = new Rectangle(
                20 * hatSpriteIndex % hatTexture.Width,
                20 * hatSpriteIndex / hatTexture.Width * 20 * 4,
                20, 20);

            if (hatData.IsErrorItem)
            {
                renderer.hatSourceRect = hatData.GetSourceRect();
                isHatErrorItem = true;
            }
        }

        // Determine accessory layer
        var accessoryLayer = who.accessory.Value >= 0 && renderer.drawAccessoryBelowHair(who.accessory.Value)
            ? FarmerSpriteLayers.AccessoryUnderHair
            : FarmerSpriteLayers.Accessory;

        // Draw shirt
        if (!who.bathingClothes.Value && (renderer.skin.Value != -12345 || who.shirtItem.Value != null))
        {
            var shirtPosition = position + new Vector2(4, 15);
            b.Draw(shirtTexture, shirtPosition, renderer.shirtSourceRect, effectiveColor, rotation, origin, drawScale, SpriteEffects.None, GetLayerDepth(layerDepth, FarmerSpriteLayers.Shirt));

            var dyeRect = renderer.shirtSourceRect;
            dyeRect.Offset(128, 0);
            var dyeColor = overrideColor.Equals(Color.White) ? Utility.MakeCompletelyOpaque(who.GetShirtColor()) : overrideColor;
            b.Draw(shirtTexture, shirtPosition, dyeRect, dyeColor, rotation, origin, drawScale, SpriteEffects.None, GetLayerDepth(layerDepth, FarmerSpriteLayers.Shirt, dyeLayer: true));
        }

        // Draw accessory
        if (who.accessory.Value >= 0)
        {
            // Special handling for accessory 26
            if (who.accessory.Value == 26 && ((uint)(currentFrame - 24) <= 2u || currentFrame == 70))
            {
                renderer.positionOffset.Y += 4f;
            }

            var accessoryPosition = position + new Vector2(0, 2);
            var accessoryColor = overrideColor.Equals(Color.White) && renderer.isAccessoryFacialHair(who.accessory.Value)
                ? hairColor
                : overrideColor;

            b.Draw(accessoriesTexture, accessoryPosition, renderer.accessorySourceRect, accessoryColor, rotation, origin, drawScale, SpriteEffects.None, GetLayerDepth(layerDepth, accessoryLayer));
        }

        // Draw hair
        var genderOffset = (who.IsMale && who.hair.Value >= 16) ? -1 : ((!who.IsMale && who.hair.Value < 16) ? 1 : 0);
        var hairPosition = position + origin + renderer.positionOffset + new Vector2(featureOffsetX, featureOffsetY + genderOffset);
        var hairDrawColor = overrideColor.Equals(Color.White) ? hairColor : overrideColor;
        b.Draw(hairTexture, hairPosition, renderer.hairstyleSourceRect, hairDrawColor, rotation, origin, drawScale, SpriteEffects.None, GetLayerDepth(layerDepth, FarmerSpriteLayers.Hair));

        // Draw hat
        if (hat != null && !who.bathingClothes.Value)
        {
            var flip = who.FarmerSprite.CurrentAnimationFrame.flip;
            var hairstyleOffset = hat.ignoreHairstyleOffset.Value ? 0 : hairstyleHatOffset[who.hair.Value % 16];
            var flipMultiplier = flip ? -1 : 1;

            var hatPosition = position + new Vector2(
                (0f - drawScale) * 2f + flipMultiplier * featureOffsetX * drawScale,
                (0f - drawScale) * 1f + featureOffsetY + hairstyleOffset + 1f + renderer.heightOffset.Value - 3);

            var hatColor = hat.isPrismatic.Value ? Utility.GetPrismaticColor() : overrideColor;

            // Special handling for masks facing backward
            if (!isHatErrorItem && hat.isMask && facingDirection == 0)
            {
                // Draw bottom part of mask
                var bottomMaskRect = renderer.hatSourceRect;
                bottomMaskRect.Height -= 11;
                bottomMaskRect.Y += 11;

                var bottomPosition = position + origin + renderer.positionOffset + new Vector2(0f, 11f * drawScale) + new Vector2(
                    (0f - drawScale) * 2f + flipMultiplier * featureOffsetX * 4,
                    -16 + featureOffsetY * 4 + hairstyleOffset + 4 + renderer.heightOffset.Value);

                b.Draw(hatTexture, bottomPosition, bottomMaskRect, overrideColor, rotation, origin, drawScale, SpriteEffects.None, GetLayerDepth(layerDepth, FarmerSpriteLayers.Hat));

                // Draw top part of mask
                var topMaskRect = renderer.hatSourceRect;
                topMaskRect.Height = 11;
                b.Draw(hatTexture, hatPosition, topMaskRect, hatColor, rotation, origin, drawScale, SpriteEffects.None, GetLayerDepth(layerDepth, FarmerSpriteLayers.HatMaskUp));
            }
            else
            {
                b.Draw(hatTexture, hatPosition, renderer.hatSourceRect, hatColor, rotation, origin, drawScale, SpriteEffects.None, GetLayerDepth(layerDepth, FarmerSpriteLayers.Hat));
            }
        }
    }

    /// <summary>
    /// Decodes a Base64-encoded string into a Texture2D object.
    /// </summary>
    /// <param name="monitor">The monitor for logging errors.</param>
    /// <param name="goldStaueBase64Image">The Base64-encoded string representation of the texture, or null.</param>
    /// <returns>A Texture2D object if decoding is successful, otherwise null.</returns>
    public static Texture2D? DecodeTextureFromBase64String(IMonitor monitor, string? goldStaueBase64Image)
    {
        if (string.IsNullOrEmpty(goldStaueBase64Image))
        {
            return null;
        }

        try
        {
            var imageBytes = Convert.FromBase64String(goldStaueBase64Image);
            
            using var memoryStream = new MemoryStream(imageBytes);
            var texture = Texture2D.FromStream(Game1.graphics.GraphicsDevice, memoryStream);
            
            return texture;
        }
        catch (Exception ex)
        {
            monitor.Log($"Failed to decode gold statue texture from Base64 string: {ex.Message}", LogLevel.Error);
            return null;
        }
    }

    /// <summary>
    /// Converts a Texture2D object to a Base64-encoded PNG string for storage.
    /// </summary>
    /// <param name="monitor">The monitor for logging errors.</param>
    /// <param name="goldStatueTexture">The texture to convert, or null.</param>
    /// <returns>A Base64-encoded string representation of the texture if conversion is successful, otherwise null.</returns>
    public static string? ConvertTextureToBase64String(IMonitor monitor, Texture2D? goldStatueTexture)
    {
        if (goldStatueTexture is null)
        {
            return null;
        }

        try
        {
            using var memoryStream = new MemoryStream();
            goldStatueTexture.SaveAsPng(memoryStream, goldStatueTexture.Width, goldStatueTexture.Height);
            
            var imageBytes = memoryStream.ToArray();
            return Convert.ToBase64String(imageBytes);
        }
        catch (Exception ex)
        {
            monitor.Log($"Failed to convert gold statue texture to Base64 string: {ex.Message}", LogLevel.Error);
            return null;
        }
    }


    /// <summary>
    /// Initializes and returns the gold statue texture by loading the base statue texture, rendering the player's character,
    /// applying a gold color effect, and merging them together. The resulting texture is saved to the save data.
    /// </summary>
    /// <returns>A Texture2D containing the merged gold statue with the player's character.</returns>
    public static Texture2D InitGoldStatueTexture()
    {
        var statueBaseTexture = Game1.content.Load<Texture2D>(ModItemKeys.StatueBaseTexturePath);
        var farmerTexture = GetFarmerStandingTexture();
        var goldFarmerTexture = GoldifyTexture(farmerTexture);
        var mergedTexture = MergeTextures(statueBaseTexture, goldFarmerTexture);
        SaveHandler.UpdateGoldStatueTexture(mergedTexture);
        return mergedTexture;
    }
}
