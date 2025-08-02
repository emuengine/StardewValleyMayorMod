using MayorMod.Constants;
using MayorMod.Data.Menu;
using MayorMod.Data.Models;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using static StardewValley.Game1;

namespace MayorMod.Data.Handlers;

internal class PollingDataHandler : IPhoneHandler
{
    private const string PollingDataKey = "PollingData";
    private readonly IModHelper _helper;
    private readonly MayorModConfig _mayorModConfig;

    public PollingDataHandler(IModHelper helper, MayorModConfig mayorModConfig)
    {
        _helper = helper;
        _mayorModConfig = mayorModConfig;
    }

    public string CheckForIncomingCall(Random random)
    {
#pragma warning disable CS8603
        return null;
#pragma warning restore CS8603
    }

    public bool TryHandleIncomingCall(string callId, out Action showDialogue)
    {
#pragma warning disable CS8625
        showDialogue = null;
#pragma warning restore CS8625
        return false;
    }

    public IEnumerable<KeyValuePair<string, string>> GetOutgoingNumbers()
    {
        List<KeyValuePair<string, string>> numbers = [];

        if (ModProgressHandler.HasProgressFlag(ProgressFlags.RunningForMayor))
        {
            numbers.Add(new KeyValuePair<string, string>(PollingDataKey, content.LoadString(DialogueKeys.PollingData.PollingDataTitle)));
        }
        return numbers;
    }

    public bool TryHandleOutgoingCall(string callId)
    {
        switch (callId)
        {
            case PollingDataKey: return CallPollingData();
            default: return false;
        }
    }

    private bool CallPollingData()
    {
        int ringTime = 4950;
        currentLocation.playShopPhoneNumberSounds("AdventureGuild");
        player.freezePause = ringTime;
        DelayedAction.functionAfterDelay(delegate
        {
            playSound("bigSelect", null);
            if (ModProgressHandler.HasProgressFlag(ProgressFlags.IsVotingDay))
            {
                var polling = new VotingHandler(player, _mayorModConfig)
                {
                    IsVotingRNG = false,
                };
                DrawDialogue(ModUtils.MarlonNPC, 
                             polling.HasWonElection(_helper) ? DialogueKeys.PollingData.PollingDataVotingDayWinning :
                                                               DialogueKeys.PollingData.PollingDataVotingDayLosing);
            }
            else
            {
                DrawDialogue(ModUtils.MarlonNPC, DialogueKeys.PollingData.PollingDataIntro);

                afterDialogues = new afterFadeFunction(() =>
                {
                    activeClickableMenu = GetPollingDataMenu();
                });
            }
        }, ringTime);
        return true;
    }

    private MayorModMenu GetPollingDataMenu()
    {
        var voters = VotingHandler.GetVotingVillagers(_helper);

        var polling = new VotingHandler(player, _mayorModConfig)
        {
            IsVotingRNG = false,
        };
        var totalVoters = voters.Count;
        var debated = polling.HasWonDebate();
        var leaflets = voters.Sum(v => polling.HasNPCGotLeaflet(v) ? 1 : 0);
        var canvassed = voters.Sum(v => polling.HasNPCBeenCanvassed(v) ? 1 : 0);
        var polls = polling.CalculateTotalVotes(_helper);

        var menu = new MayorModMenu(_helper, 0.4f, 0.5f);
        menu.MenuItems =
        [
            new MenuBorder(menu),
            new TextMenuItem(menu, content.LoadString(DialogueKeys.PollingData.PollingDataTitle), new Margin(0, 30, 0, 0)){ IsBold = true, Align = TextMenuItem.MenuItemAlign.Center },
            new BoolMenuItem(menu, content.LoadString(DialogueKeys.PollingData.HadDebate),debated, new Margin(15, 100, 0, 0)),
            new TextMenuItem(menu, $"{content.LoadString(DialogueKeys.PollingData.Leaflets)} {leaflets}/{totalVoters}", new Margin(15, 150, 0, 0)),
            new TextMenuItem(menu, $"{content.LoadString(DialogueKeys.PollingData.VotersCanvassed)} {canvassed}/{totalVoters}", new Margin(15, 200, 0, 0)),
            new TextMenuItem(menu, $"{content.LoadString(DialogueKeys.PollingData.VotingForYou)} {polls}/{totalVoters}", new Margin(15, 250, 0, 0)),

            new ButtonMenuItem(menu, new Vector2(-84, 20), () => { exitActiveMenu(); })
            {
                ButtonTypeSelected = ButtonMenuItem.ButtonType.Cancel
            },
        ];
        return menu;
    }
}