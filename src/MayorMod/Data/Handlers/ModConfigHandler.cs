using MayorMod.Constants;
using MayorMod.Data.Configuration;
using MayorMod.Data.Models;
using StardewModdingAPI;

namespace MayorMod.Data.Handlers;

public class ModConfigHandler
{
    private readonly IModHelper _helper;
    private readonly IManifest _modManifest;
    public MayorModConfig ModConfig { get; set; } = new();


    public ModConfigHandler(IModHelper helper, IManifest modManifest, MayorModConfig modConfig) 
    {
        _helper = helper;
        _modManifest= modManifest;
        ModConfig = modConfig;
    }

    /// <summary>
    /// Setup Generic Mod Config Menu
    /// </summary>
    public void InitGMCM()
    {
        var configMenu = _helper.ModRegistry.GetApi<IGenericModConfigMenuApi>(ModKeys.CONFIG_MENU_ID);
        if (configMenu is null)
        {
            return;
        }

        configMenu.Register(
            mod: _modManifest,
            reset: () => ModConfig = new MayorModConfig(),
            save: () => _helper.WriteConfig(ModConfig)
        );


        configMenu.AddSectionTitle(
            mod: _modManifest,
            text: () => ModUtils.GetTranslationForKey(_helper, $"{ModKeys.MAYOR_MOD_CPID}_UIMenu.VotingOptions")
        //tooltip: () => ModUtils.GetTranslationForKey(Helper, $"{ModKeys.MAYOR_MOD_CPID}_UIMenu.VotingOptions.Tooltip")
        );

        configMenu.AddNumberOption(
            mod: _modManifest,
            name: () => ModUtils.GetTranslationForKey(_helper, $"{ModKeys.MAYOR_MOD_CPID}_UIMenu.VoteThreshold"),
            tooltip: () => ModUtils.GetTranslationForKey(_helper, $"{ModKeys.MAYOR_MOD_CPID}_UIMenu.VoteThreshold.Tooltip"),
            getValue: () => ModConfig.ThresholdForVote,
            setValue: value => ModConfig.ThresholdForVote = value,
            min: 0,
            max: 15,
            interval: 1
        );

        configMenu.AddNumberOption(
            mod: _modManifest,
            name: () => ModUtils.GetTranslationForKey(_helper, $"{ModKeys.MAYOR_MOD_CPID}_UIMenu.VoterPercentageNeeded"),
            tooltip: () => ModUtils.GetTranslationForKey(_helper, $"{ModKeys.MAYOR_MOD_CPID}_UIMenu.VoterPercentageNeeded.Tooltip"),
            getValue: () => ModConfig.VoterPercentageNeeded,
            setValue: value => ModConfig.VoterPercentageNeeded = value,
            min: 0,
            max: 100,
            interval: 1
        );

        configMenu.AddNumberOption(
            mod: _modManifest,
            name: () => ModUtils.GetTranslationForKey(_helper, $"{ModKeys.MAYOR_MOD_CPID}_UIMenu.NumberOfCampaignDays"),
            tooltip: () => ModUtils.GetTranslationForKey(_helper, $"{ModKeys.MAYOR_MOD_CPID}_UIMenu.NumberOfCampaignDays.Tooltip"),
            getValue: () => ModConfig.NumberOfCampaignDays,
            setValue: value => ModConfig.NumberOfCampaignDays = value,
            min: 0,
            max: 100,
            interval: 1
        );
    }
}
