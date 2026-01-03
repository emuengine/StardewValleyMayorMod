using MayorMod.Constants;
using MayorMod.Data.Models;
using MayorMod.Data.Utilities;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace MayorMod.Data.Handlers;

public static class SaveHandler
{
    public static MayorModData? SaveData { get; set; }

    private static IMod _mod { get; set; } = null!;

    /// <summary>
    /// Initializes the SaveHandler with the required SMAPI helper, monitor, and mod manifest.
    /// </summary>
    /// <param name="helper">The mod helper instance.</param>
    /// <param name="monitor">The monitor for logging.</param>
    /// <param name="modManifest">The mod manifest containing version information.</param>
    public static void Init(IMod mod)
    {
        _mod = mod;
    }

    /// <summary>
    /// Updates the voting date in the save data and persists the changes.
    /// </summary>
    /// <param name="votingDate">The new voting date to set, or null to clear it.</param>
    public static void UpdateVotingDate(SDate? votingDate)
    {
        if (SaveData is not null)
        {
            SaveData.VotingDate = votingDate;
            _mod.Helper.Data.WriteSaveData(ModKeys.SAVE_KEY, SaveData);
        }
    }

    /// <summary>
    /// Updates the gold statue texture in the save data by converting it to a Base64 string.
    /// </summary>
    /// <param name="goldStatueTexture">The texture to save, or null to clear it.</param>
    public static void UpdateGoldStatueTexture(Texture2D? goldStatueTexture)
    {
        //this needs to be saved somewhere else
        if (SaveData is not null)
        {
            SaveData.GoldStaueBase64Image = TextureUtils.ConvertTextureToBase64String(_mod.Monitor, goldStatueTexture);
            _mod.Helper.Data.WriteSaveData(ModKeys.SAVE_KEY, SaveData);
        }
    }

    /// <summary>
    /// Resets the save data to a new instance with the current mod version and persists it.
    /// </summary>
    /// <returns>The newly created save data.</returns>
    public static MayorModData ResetSave()
    {
        SaveData = new MayorModData()
        {
            SaveVersion = new Version(
                _mod.ModManifest.Version.MajorVersion,
                _mod.ModManifest.Version.MinorVersion,
                _mod.ModManifest.Version.PatchVersion)
        };
        _mod.Helper.Data.WriteSaveData(ModKeys.SAVE_KEY, SaveData);
        return SaveData;
    }

    /// <summary>
    /// Loads the save data from disk and applies version updates if necessary.
    /// </summary>
    public static void LoadSaveData()
    {
        SaveData = _mod.Helper.Data.ReadSaveData<MayorModData>(ModKeys.SAVE_KEY) ?? ResetSave();

        var currentVersion = new Version(_mod.ModManifest.Version.MajorVersion,
                                         _mod.ModManifest.Version.MinorVersion,
                                         _mod.ModManifest.Version.PatchVersion);
        if (SaveData is not null && (SaveData.SaveVersion is null || SaveData.SaveVersion < currentVersion))
        {
            UpdateSaveDataTo1_1_16();
            UpdateSaveDataToLatest();
            _mod.Helper.Data.WriteSaveData(ModKeys.SAVE_KEY, SaveData);
        }
    }

    /// <summary>
    /// Updates the save data to the latest mod version.
    /// </summary>
    private static void UpdateSaveDataToLatest()
    {
        var currentVersion = new Version(_mod.ModManifest.Version.MajorVersion,
                                         _mod.ModManifest.Version.MinorVersion,
                                         _mod.ModManifest.Version.PatchVersion);
        if (SaveData is not null && 
            (SaveData.SaveVersion is null || 
            SaveData.SaveVersion < currentVersion))
        {
            _mod.Monitor.Log($"Updating MayorMod save data to version {currentVersion}", LogLevel.Info);
            SaveData.SaveVersion = currentVersion;
        }
    }

    /// <summary>
    /// Migrates save data to version 1.1.16.
    /// </summary>
    private static void UpdateSaveDataTo1_1_16()
    {
        var v1_1_16 = new Version(1, 1, 16);
        if (SaveData is not null &&
            (SaveData.SaveVersion is null || 
            SaveData.SaveVersion < v1_1_16))
        {
            _mod.Monitor.Log($"Updating MayorMod save data to version {v1_1_16}", LogLevel.Info);
            if (SaveData.VotingDate == new SDate(1, Season.Spring) || !ModProgressHandler.HasProgressFlag(ProgressFlags.RunningForMayor))
            {
                SaveData.VotingDate = null;
            }
            SaveData.SaveVersion = v1_1_16;
        }
    }
}
