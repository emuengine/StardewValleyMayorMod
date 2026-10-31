﻿using MayorMod.Constants;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.BellsAndWhistles;

namespace MayorMod.Data.Handlers;

/// <summary>
/// Shows the current town treasury in the top left.
/// </summary>
public static class TreasuryDisplayHandler
{
    private const int DIAL_DIGITS = 6;
    private const int BOX_DIGITS = 4;
    private const int DIGIT_WIDTH = 6;
    private const float POPUP_SECONDS = 5f;
    private const float SLIDE_SECONDS = 0.5f;
    private static Texture2D _coinIcon = null!;
    private static float _popupTimeLeft;
    private static float _slidePosition;
    private readonly static MoneyDial TreasuryDial = new MoneyDial(DIAL_DIGITS, playSound: false);


    public static void Init(IMod mod)
    {
        _coinIcon = mod.Helper.ModContent.Load<Texture2D>("assets/treasuryMoney.png");
        mod.Helper.Events.Display.RenderedHud += DrawTreasuryHUD;
    }

    /// <summary>
    /// Slides the treasury display down for a few seconds.
    /// </summary>
    public static void ShowBriefly()
    {
        _popupTimeLeft = POPUP_SECONDS;
    }

    /// <summary>
    /// Draws the treasury total.
    /// </summary>
    private static void DrawTreasuryHUD(object? sender, RenderedHudEventArgs e)
    {
        if (SaveHandler.SaveData is null ||
            !Game1.MasterPlayer.eventsSeen.Contains(ProgressFlags.TaxIntroEvent))
        {
            return;
        }

        UpdateSlidePosition(Game1.currentLocation?.NameOrUniqueName == ModKeys.MANOR_HOUSE_LOCATION);
        if (_slidePosition <= 0f)
        {
            return;
        }

        var treasury = SaveHandler.SaveData.TownTreasury;
        var MaxDisplayedValue = (int)Math.Pow(10, DIAL_DIGITS) - 1;
        var drawPosition = new Vector2(16f, (int)Utility.Lerp(-26f, 0f, _slidePosition) * 4);
        var digitsPosition = drawPosition + new Vector2(108, 40);
        DrawBox(e.SpriteBatch, drawPosition);

        if (treasury >= 0)
        {
            TreasuryDial.draw(e.SpriteBatch, digitsPosition, Math.Min(treasury, MaxDisplayedValue));
        }
        else
        {
            var displayedValue = Math.Min(-treasury, MaxDisplayedValue / 10);
            var minusX = DIGIT_WIDTH * 4 * (DIAL_DIGITS - displayedValue.ToString().Length - 1);

            e.SpriteBatch.Draw(Game1.staminaRect, 
                                new Rectangle((int)digitsPosition.X + minusX + 2, 
                                              (int)digitsPosition.Y + 14, 
                                              16, 
                                              4), 
                                Color.Red);

            DrawDigits(e.SpriteBatch, digitsPosition, displayedValue, Color.Red);
        }
        e.SpriteBatch.Draw(_coinIcon, 
                           drawPosition + new Vector2(4f, 6f) * 4f, 
                           _coinIcon.Bounds,
                           Color.White, 
                           0f, 
                           Vector2.Zero, 
                           4f, 
                           SpriteEffects.None, 
                           0f);
    }

    /// <summary>
    /// Slides the display in and out if the popup time runs out.
    /// </summary>
    /// <param name="keepOpen">true to keep the display showing</param>
    private static void UpdateSlidePosition(bool keepOpen)
    {
        var elapsed = (float)Game1.currentGameTime.ElapsedGameTime.TotalSeconds;
        if (Game1.activeClickableMenu is null)
        {
            _popupTimeLeft = Math.Max(0f, _popupTimeLeft - elapsed);
        }

        var isShowing = (keepOpen || _popupTimeLeft > 0f)? 1 : -1;
        _slidePosition += isShowing * (elapsed / SLIDE_SECONDS);
        _slidePosition = Utility.Clamp(_slidePosition, 0f, 1f);
    }

    /// <summary>
    /// Draws the game's currency box widened to fit all the dial digits
    /// </summary>
    private static void DrawBox(SpriteBatch b, Vector2 position)
    {
        var BoxLeftSource = new Rectangle(42, 0, 39, 26);
        var BoxRightSource = new Rectangle(81, 0, 18, 26);
        var BoxDigitSource = new Rectangle(75, 0, DIGIT_WIDTH, 26);

        b.Draw(Game1.mouseCursors_1_6, position, BoxLeftSource, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
        position.X += BoxLeftSource.Width * 4;
        for (int i = BOX_DIGITS; i < DIAL_DIGITS; i++)
        {
            b.Draw(Game1.mouseCursors_1_6, position, BoxDigitSource, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
            position.X += DIGIT_WIDTH * 4;
        }
        b.Draw(Game1.mouseCursors_1_6, position, BoxRightSource, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
    }

    /// <summary>
    /// Draws the money dial digits in a given colour
    /// </summary>
    private static void DrawDigits(SpriteBatch b, Vector2 position, int value, Color color)
    {
        var xPos = DIGIT_WIDTH * 4 * (DIAL_DIGITS - value.ToString().Length);
        foreach (var digit in value.ToString())
        {
            b.Draw(Game1.mouseCursors, 
                    position + new Vector2(xPos, 0), 
                    new Rectangle(286, 502 - (digit - '0') * 8, 5, 8), 
                    color, 
                    0f, 
                    Vector2.Zero, 
                    4f, 
                    SpriteEffects.None, 
                    1f);
            xPos += DIGIT_WIDTH * 4;
        }
    }
}
