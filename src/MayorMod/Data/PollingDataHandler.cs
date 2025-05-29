using MayorMod.Constants;
using MayorMod.Data.Menu;
using MayorMod.Data.Models;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using static StardewValley.Game1;

namespace MayorMod.Data;

internal class PollingDataHandler : IPhoneHandler
{
    private const string PollingDataKey = "PollingData";
    private readonly IModHelper _helper;

    public PollingDataHandler(IModHelper helper)
    {
        _helper = helper;
    }

    public string CheckForIncomingCall(Random random)
    {
        return null;
    }

    public bool TryHandleIncomingCall(string callId, out Action showDialogue)
    {
        showDialogue = null;
        return false;
    }

    public IEnumerable<KeyValuePair<string, string>> GetOutgoingNumbers()
    {
        List<KeyValuePair<string, string>> numbers = [];

        if (ModProgressManager.HasProgressFlag(ModProgressManager.RunningForMayor))
        {
            numbers.Add(new KeyValuePair<string, string>(PollingDataKey, Game1.content.LoadString(DialogueKeys.PollingData.PollingDataTitle)));
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
            if (ModProgressManager.HasProgressFlag(ModProgressManager.IsVotingDay))
            {
                var polling = new VotingManager(Game1.player)
                {
                    IsVotingRNG = false,
                };
                DrawDialogue(ModUtils.MarlonNPC, polling.HasWonElection()? DialogueKeys.PollingData.PollingDataVotingDayWinning:
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
        var polling = new VotingManager(Game1.player)
        {
            IsVotingRNG = false,
        };
        var totalVoters = VotingManager.Voters.Count;
        var debated = polling.HasWonDebate();
        var leaflets = VotingManager.Voters.Sum(v => polling.HasNPCGotLeaflet(v) ? 1 : 0);
        var canvassed = VotingManager.Voters.Sum(v => polling.HasNPCBeenCanvassed(v) ? 1 : 0);
        var polls = polling.CalculateTotalVotes();

        var menu = new MayorModMenu(_helper, 0.4f, 0.5f);
        menu.MenuItems =
        [
            new MenuBorder(menu),
            new TextMenuItem(menu, Game1.content.LoadString(DialogueKeys.PollingData.PollingDataTitle), new Margin(0, 30, 0, 0)){ IsBold = true, Align = TextMenuItem.MenuItemAlign.Center },
            new TextMenuItem(menu, $"{Game1.content.LoadString(DialogueKeys.PollingData.HadDebate)} {debated}", new Margin(15, 100, 0, 0)),
            new TextMenuItem(menu, $"{Game1.content.LoadString(DialogueKeys.PollingData.Leaflets)} {leaflets}/{totalVoters}", new Margin(15, 150, 0, 0)),
            new TextMenuItem(menu, $"{Game1.content.LoadString(DialogueKeys.PollingData.VotersCanvassed)} {canvassed}/{totalVoters}", new Margin(15, 200, 0, 0)),
            new TextMenuItem(menu, $"{Game1.content.LoadString(DialogueKeys.PollingData.VotingForYou)} {polls}/{totalVoters}", new Margin(15, 250, 0, 0)),

            new ButtonMenuItem(menu, new Vector2(-84, 20), () => { exitActiveMenu(); })
            {
                ButtonTypeSelected = ButtonMenuItem.ButtonType.Cancel
            },
        ];
        return menu;
    }
}