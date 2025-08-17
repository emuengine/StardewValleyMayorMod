using StardewValley;
using StardewValley.Locations;
using StardewValley.SpecialOrders;
using xTile.Dimensions;
using Microsoft.Xna.Framework;
using MayorMod.Data.Handlers;
using System.Reflection;

namespace MayorMod.Data;

/// <summary>
/// Machines in ManorHouse are tied to that location by the createQuestionDialogue so this decouples them
/// </summary>
public class MayorMachines
{
    private static Dictionary<string, Farmer> MoneyToSend = new Dictionary<string, Farmer>();

    private const string QuestionKey_DivorceCancel = "divorceCancel"; 
    private const string QuestionKey_Divorce = "divorce"; 
    private const string QuestionKey_LostAndFound = "lostAndFound";
    private const string QuestionKey_ChooseRecipient = "chooseRecipient";
    private const string QuestionKey_LedgerOptions = "ledgerOptions";

    private const string ResponseKey_SendMoney = "SendMoney";
    private const string ResponseKey_MergeWallets = "MergeWallets";
    private const string ResponseKey_Transfer = "Transfer";
    private const string ResponseKey_CheckDonations = "CheckDonations";
    private const string ResponseKey_RetrieveFarmhandItems = "RetrieveFarmhandItems";
    private const string ResponseKey_SeparateWallets = "separateWallets";
    private const string ResponseKey_CancelSeparateWallets = "cancelSeparateWallets";
    private const string ResponseKey_Leave = "Leave";
    private const string ResponseKey_CancelMerge = "CancelMerge";
    private const string ResponseKey_Cancel = "Cancel";
    private static Response[] YesNoResponse => Game1.currentLocation.createYesNoResponses();

    /// <summary>
    /// Displays a question dialogue with a set of responses and handles the answer via a delayed callback.
    /// </summary>
    /// <param name="question">The question text to display.</param>
    /// <param name="answerChoices">The array of response options the player can choose from.</param>
    /// <param name="questionKey">A key used to identify the question context when processing the answer.</param>
    private static void QuestionDialogueDelegateAnswer(string question, Response[] answerChoices, string questionKey)
    {
        Game1.player.currentLocation.createQuestionDialogue(question, answerChoices, (who, whichAnswer) =>
        {
            DelayedAction.functionAfterDelay(() => // Hack to chain question dialogues
            {
                AnswerDialogueAction(questionKey, whichAnswer);
            }, 1);
        });
    }

    /// <summary>
    /// Handles the answers for Mayor Machines
    /// </summary>
    /// <param name="questionKey">key for the question</param>
    /// <param name="answerKey">answer chosen</param>
    private static void AnswerDialogueAction(string questionKey, string answerKey)
    {
        var questionParams = ArgUtility.SplitBySpace(questionKey);
        var questionAndAnswer = $"{questionParams[0]}_{answerKey}";
        var man = Game1.getLocationFromName(nameof(ManorHouse));

        if (questionAndAnswer == $"ledgerOptions_{ResponseKey_SendMoney}")
        {
            ChooseLedgerRecipient();
        }
        else if (questionAndAnswer == "ledgerOptions_MergeWallets")
        {
            QuestionDialogueDelegateAnswer(Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_SeparateWallets_MergeQuestion"), YesNoResponse, "mergeWallets");
        }
        else if (questionAndAnswer == "ledgerOptions_CancelMerge")
        {
            QuestionDialogueDelegateAnswer(Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_SeparateWallets_CancelQuestion"), YesNoResponse, "cancelMergeWallets");
        }
        else if (questionAndAnswer.Contains(ResponseKey_Transfer))
        {
            string answer = questionAndAnswer.Split('_')[1];
            var method = typeof(ManorHouse).GetMethod("beginSendMoney", BindingFlags.NonPublic | BindingFlags.Instance);
            method?.Invoke(man, new object[] { MoneyToSend[answer] });
            MoneyToSend.Clear();
        }
        else
        {
            man.answerDialogueAction(questionAndAnswer, questionParams);
        }
    }

    /// <summary>
    /// Updates the Divorce Book action
    /// </summary>
    public static void DivorceBook()
    {
        var player = Game1.player;

        if (player.divorceTonight.Value)
        {
            string question = player.hasCurrentOrPendingRoommate()
                ? Game1.content.LoadString("Strings\\Locations:ManorHouse_DivorceBook_CancelQuestion_Krobus", player.getSpouse().displayName)
                : Game1.content.LoadStringReturnNullIfNotFound("Strings\\Locations:ManorHouse_DivorceBook_CancelQuestion");
            QuestionDialogueDelegateAnswer(question, YesNoResponse, QuestionKey_DivorceCancel);
        }
        else if (player.isMarriedOrRoommates())
        {
            var question = player.hasCurrentOrPendingRoommate()
                ? Game1.content.LoadString("Strings\\Locations:ManorHouse_DivorceBook_Question_Krobus", player.getSpouse().displayName)
                : Game1.content.LoadStringReturnNullIfNotFound("Strings\\Locations:ManorHouse_DivorceBook_Question");
            QuestionDialogueDelegateAnswer(question, YesNoResponse, QuestionKey_Divorce);
        }
        else
        {
            Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ManorHouse_DivorceBook_NoSpouse"));
        }
    }

    /// <summary>
    /// Calls the Mayor Fridge action
    /// </summary>
    public static void MayorFridge(Point point)
    {
        var manorHouse = (ManorHouse)Game1.getLocationFromName(nameof(ManorHouse));
        manorHouse.performAction(TileActionHandler.MayorFridgeActionType, Game1.player, new Location(point.X, point.Y));
    }

    /// <summary>
    /// Updates the Lost And Found action
    /// </summary>
    public static void CheckLostAndFound()
    {
        var manorHouse = (ManorHouse)Game1.getLocationFromName(nameof(ManorHouse));
        var playerTeam = Game1.player.team;

        var question = SpecialOrder.IsSpecialOrdersBoardUnlocked()
            ? Game1.content.LoadString("Strings\\Locations:ManorHouse_LAF_Check_OrdersUnlocked")
            : Game1.content.LoadString("Strings\\Locations:ManorHouse_LAF_Check");

        var choices = new List<Response>();
        if (playerTeam.returnedDonations.Count > 0 && !playerTeam.returnedDonationsMutex.IsLocked())
        {
            choices.Add(new Response(ResponseKey_CheckDonations, Game1.content.LoadString("Strings\\Locations:ManorHouse_LAF_DonationItems")));
        }
        if (manorHouse.GetRetrievableFarmers().Count > 0)
        {
            choices.Add(new Response(ResponseKey_RetrieveFarmhandItems, Game1.content.LoadString("Strings\\Locations:ManorHouse_LAF_FarmhandItems")));
        }
        if (choices.Count > 0)
        {
            choices.Add(new Response(ResponseKey_Cancel, Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_TransferCancel")));
            QuestionDialogueDelegateAnswer(question, choices.ToArray(), QuestionKey_LostAndFound);
        }
        else
        {
            Game1.drawObjectDialogue(question);
        }
    }

    /// <summary>
    /// Updates the Ledger Book action
    /// </summary>
    public static void ReadLedgerBook()
    {
        if (Game1.player.useSeparateWallets)
        {
            if (Game1.IsMasterGame)
            {
                List<Response> choices = new List<Response>()
                {
                    new(ResponseKey_SendMoney, Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_SendMoney")),
                    Game1.player.changeWalletTypeTonight.Value ?
                            new Response(ResponseKey_CancelMerge, Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_CancelMerge")) :
                            new Response(ResponseKey_MergeWallets, Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_MergeWallets")),
                    new Response(ResponseKey_Leave, Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_Leave"))
                };

                var question = Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_SeparateWallets_HostQuestion");
                QuestionDialogueDelegateAnswer(question, choices.ToArray(), QuestionKey_LedgerOptions);
            }
            else
            {
                ChooseLedgerRecipient();
            }
            return;
        }

        if (!Game1.getAllFarmhands().Any())
        {
            Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_Singleplayer"));
            return;
        }

        if (Game1.IsMasterGame)
        {
            var question = Game1.player.changeWalletTypeTonight.Value
                ? Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_SharedWallets_CancelQuestion")
                : Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_SharedWallets_SeparateQuestion");
            var responseKey = Game1.player.changeWalletTypeTonight.Value ? ResponseKey_CancelSeparateWallets : ResponseKey_SeparateWallets;
            QuestionDialogueDelegateAnswer(question, YesNoResponse, responseKey);
        }
        else
        {
            Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_SharedWallets_Client"));
        }
    }

    /// <summary>
    /// Choose the recipient for ledger actions
    /// </summary>
    public static void ChooseLedgerRecipient()
    {
        MoneyToSend.Clear();
        var responses = new List<Response>();

        foreach (Farmer farmer in Game1.getAllFarmers())
        {
            if (farmer.UniqueMultiplayerID == Game1.player.UniqueMultiplayerID || farmer.isUnclaimedFarmhand)
            {
                continue;
            }

            var responseKey = $"{ResponseKey_Transfer}{responses.Count + 1}";
            var name = string.IsNullOrEmpty(farmer.Name) ? Game1.content.LoadString("Strings\\UI:Chat_PlayerJoinedNewName") : farmer.Name;
            responses.Add(new Response(responseKey, name));
            MoneyToSend[responseKey] = farmer;
        }

        if (responses.Count == 0)
        {
            Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_NoFarmhands"));
            return;
        }
        responses.Sort((x, y) => string.Compare(x.responseKey, y.responseKey, StringComparison.Ordinal));
        responses.Add(new Response(ResponseKey_Cancel, Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_TransferCancel")));
        var question = Game1.content.LoadString("Strings\\Locations:ManorHouse_LedgerBook_SeparateWallets_TransferQuestion");
        QuestionDialogueDelegateAnswer(question, responses.ToArray(), QuestionKey_ChooseRecipient);
    }
}