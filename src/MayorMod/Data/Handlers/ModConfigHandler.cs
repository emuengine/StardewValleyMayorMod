using MayorMod.Constants;
using MayorMod.Data.Interfaces;
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


        configMenu.AddSectionTitle(
            mod: _modManifest,
            text: () => ModUtils.GetTranslationForKey(_helper, $"{ModKeys.MAYOR_MOD_CPID}_UIMenu.CouncilMeetingDays"),
            tooltip: () => ModUtils.GetTranslationForKey(_helper, $"{ModKeys.MAYOR_MOD_CPID}_UIMenu.CouncilMeetingDays.Tooltip")
        );
        configMenu.AddBoolOption(
             mod: _modManifest,
             name: () => ModUtils.GetTranslationForKey(_helper, $"{ModKeys.MAYOR_MOD_CPID}_UIMenu.Monday"),
             getValue: () => ModConfig.MeetingDays[1],
             setValue: value => ModConfig.MeetingDays[1] = value
        );
        configMenu.AddBoolOption(
             mod: _modManifest,
             name: () => ModUtils.GetTranslationForKey(_helper, $"{ModKeys.MAYOR_MOD_CPID}_UIMenu.Tuesday"),
             getValue: () => ModConfig.MeetingDays[2],
             setValue: value => ModConfig.MeetingDays[2] = value
        );
        configMenu.AddBoolOption(
             mod: _modManifest,
             name: () => ModUtils.GetTranslationForKey(_helper, $"{ModKeys.MAYOR_MOD_CPID}_UIMenu.Wednesday"),
             getValue: () => ModConfig.MeetingDays[3],
             setValue: value => ModConfig.MeetingDays[3] = value
        );
        configMenu.AddBoolOption(
             mod: _modManifest,
             name: () => ModUtils.GetTranslationForKey(_helper, $"{ModKeys.MAYOR_MOD_CPID}_UIMenu.Thursday"),
             getValue: () => ModConfig.MeetingDays[4],
             setValue: value => ModConfig.MeetingDays[4] = value
        );
        configMenu.AddBoolOption(
             mod: _modManifest,
             name: () => ModUtils.GetTranslationForKey(_helper, $"{ModKeys.MAYOR_MOD_CPID}_UIMenu.Friday"),
             getValue: () => ModConfig.MeetingDays[5],
             setValue: value => ModConfig.MeetingDays[5] = value
        );
        configMenu.AddBoolOption(
             mod: _modManifest,
             name: () => ModUtils.GetTranslationForKey(_helper, $"{ModKeys.MAYOR_MOD_CPID}_UIMenu.Saturday"),
             getValue: () => ModConfig.MeetingDays[6],
             setValue: value => ModConfig.MeetingDays[6] = value
        );
        configMenu.AddBoolOption(
             mod: _modManifest,
             name: () => ModUtils.GetTranslationForKey(_helper, $"{ModKeys.MAYOR_MOD_CPID}_UIMenu.Sunday"),
             getValue: () => ModConfig.MeetingDays[0],
             setValue: value => ModConfig.MeetingDays[0] = value
        );
    }
}
