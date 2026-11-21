using MayorMod.Constants;
using MayorMod.Data.Handlers;
using MayorMod.Data.Menu;
using MayorMod.Data.Models;
using MayorMod.Data.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;

namespace MayorMod.Data.TileActions;

public static class ManorHouseTileActions
{
    /// <summary>
    /// Handles actions for the the mayor desk
    /// </summary>
    /// <param name="farmer"></param>
    public static void MayorDeskAction(IModHelper helper, Farmer farmer, MayorModConfig modConfig)
    {
        //Sit at the desk
        var seat = MapSeat.FromData("2/1/down/custom 0.5 0.1 0/-1/-1/false", 19, 5);
        farmer.BeginSitting(seat);

        ShowDeskMenu(helper, modConfig);
    }

    /// <summary>
    ///  Show the desk menu
    /// </summary>
    /// <param name="helper"></param>
    /// <param name="modConfig"></param>
    private static void ShowDeskMenu(IModHelper helper, MayorModConfig modConfig)
    {
        var menuItems = new List<string>{ModUtils.GetTranslationForKey(helper, UIMenuKeys.SelectCouncilMeeting)};

        if (Game1.MasterPlayer.eventsSeen.Contains(ProgressFlags.TaxIntroEvent))
        {
            menuItems.Add(ModUtils.GetTranslationForKey(helper, UIMenuKeys.Budget));
        }

        //Show menu
        var menu = new MayorModMenu(helper, 0.8f, 0.9f, TreasuryDisplayHandler.HUD_HEIGHT + 8);
        menu.MenuItems = new List<IMenuItem>()
        {
           new TextMenuItem(menu, Game1.content.LoadString(DialogueKeys.MayorDesk.Title), new Margin(0, 15, 0, 0)){ IsBold = true, Align = TextMenuItem.MenuItemAlign.Center },
           new TextMenuItem(menu, Game1.content.LoadString(DialogueKeys.MayorDesk.Description), new Margin(20, 60, 0, 0)),
           new BigButtonListMenuItem(menu, new Margin(30, 110, 60, 130), menuItems, (i)  => OnMayorMenuItemSelected(helper, modConfig, i)),
           new ButtonMenuItem(menu, new Vector2(-84, 20), () => { Game1.exitActiveMenu(); })
           {
               ButtonTypeSelected = ButtonMenuItem.ButtonType.Cancel
           }
        };
        Game1.activeClickableMenu = menu;
    }

    /// <summary>
    /// Handles the selection of a mayor menu items and displays the selected menu.
    /// </summary>
    /// <param name="selectedIndex">The index of the selected menu item.</param>
    private static void OnMayorMenuItemSelected(IModHelper helper, MayorModConfig modConfig, int selectedIndex)
    {
        switch (selectedIndex)
        {
            case 0:
                ShowCouncilMeetingMenu(helper, modConfig);
                break;
            case 1:
                ShowBudgetMenu(helper, modConfig);
                break;
        }
    }

    /// <summary>
    /// Show the town's tax income and costs.
    /// </summary>
    private static void ShowBudgetMenu(IModHelper helper, MayorModConfig modConfig)
    {
        var budgetMenu = new MayorModMenu(helper, 0.8f, 0.9f, TreasuryDisplayHandler.HUD_HEIGHT + 8);
        var coinIcon = helper.ModContent.Load<Texture2D>("assets/treasuryMoney.png");
        var incomeRows = GetIncomeRows(helper);
        var monthlyRows = GetCostRows(helper);
        var incomeCollected = incomeRows.Sum(r => r.Amount ?? 0);
        var costsTotal = monthlyRows.Sum(r => r.Amount ?? 0);

        var taxTable = new TableMenuItem(budgetMenu, ModUtils.GetTranslationForKey(helper, UIMenuKeys.TaxIncome), incomeRows, 80)
        {
            EndPercent = 0.5f,
            Icon = coinIcon,
            TotalRow = new TableRow(ModUtils.GetTranslationForKey(helper, UIMenuKeys.MonthlyTotal), incomeCollected)
        };

        var monthlyTable = new TableMenuItem(budgetMenu, ModUtils.GetTranslationForKey(helper, UIMenuKeys.MonthlyCosts), monthlyRows, 80)
        {
            StartPercent = 0.5f,
            Icon = coinIcon,
            TotalRow = new TableRow(ModUtils.GetTranslationForKey(helper, UIMenuKeys.MonthlyTotal), costsTotal)
        };

        var outstandingTop = monthlyTable.Top + monthlyTable.Height + 30;
        var netTable = CreateNetTable(helper, budgetMenu, coinIcon, incomeCollected - costsTotal);

        var items = new List<IMenuItem>()
        {
            new TextMenuItem(budgetMenu, ModUtils.GetTranslationForKey(helper, UIMenuKeys.Budget), new Margin(0, 15, 0, 0)){ IsBold = true, Align = TextMenuItem.MenuItemAlign.Center },
            taxTable,
            monthlyTable,
            netTable,
        };

        items.Add(CreateOutstandingCostsItem(helper, modConfig, budgetMenu, coinIcon, outstandingTop, netTable.Top + netTable.RowHeight));
        items.Add(new ButtonMenuItem(budgetMenu, new Vector2(24, 20), () => { ShowDeskMenu(helper, modConfig); })
        {
            ButtonTypeSelected = ButtonMenuItem.ButtonType.Back
        });
        items.Add(new ButtonMenuItem(budgetMenu, new Vector2(-84, 20), () => { Game1.exitActiveMenu(); })
        {
            ButtonTypeSelected = ButtonMenuItem.ButtonType.Cancel
        });

        if (Context.IsMainPlayer && GetAvailableCouncilEffects().Count > 0)
        {
            items.Add(new ButtonMenuItem(budgetMenu, new Vector2(-156, 20), () => ShowCouncilEffectsMenu(helper, modConfig))
            {
                ButtonTypeSelected = ButtonMenuItem.ButtonType.Options,
                HoverText = ModUtils.GetTranslationForKey(helper, UIMenuKeys.CouncilEffects)
            });
        }
        budgetMenu.MenuItems = items;
        budgetMenu.RebuildMenu = () => ShowBudgetMenu(helper, modConfig);
        Game1.activeClickableMenu = budgetMenu;
    }

    /// <summary>
    /// Gets the budget income.
    /// </summary>
    private static IList<TableRow> GetIncomeRows(IModHelper helper)
    {
        var incomeRows = TaxHandler.GetTaxPayers(helper);
        if (TaxHandler.HasFundraiserIncome())
        {
            incomeRows.Add(new TableRow(ModUtils.GetTranslationForKey(helper, UIMenuKeys.IncomeFundraiser), TaxHandler.FUNDRAISER_AMOUNT));
        }
        return incomeRows;
    }

    /// <summary>
    /// Gets the budget costs.
    /// </summary>
    private static IList<TableRow> GetCostRows(IModHelper helper)
    {
        var paidCosts = TaxHandler.GetActiveMonthlyCosts()
                                  .Union(SaveHandler.SaveData?.CostsPaidThisMonth ?? new List<string>())
                                  .ToList();
        var monthlyRows = paidCosts.Select(c => new TableRow(Game1.content.LoadString($"{XNBPathKeys.UI}:{ModKeys.MAYOR_MOD_CPID}_Cost{c}").TrimEnd('.'), TreasuryCostKeys.Costs.GetValueOrDefault(c))).ToList();
        if (monthlyRows.Count == 0)
        {
            monthlyRows.Add(new TableRow(ModUtils.GetTranslationForKey(helper, UIMenuKeys.NoMonthlyCosts)));
        }
        return monthlyRows;
    }

    /// <summary>
    /// Get net total.
    /// </summary>
    private static TableMenuItem CreateNetTable(IModHelper helper, MayorModMenu budgetMenu, Texture2D coinIcon, int netThisMonth)
    {
        var netTable = new TableMenuItem(budgetMenu, string.Empty, new List<TableRow>(), 0)
        {
            Icon = coinIcon,
            TotalRow = new TableRow(ModUtils.GetTranslationForKey(helper, UIMenuKeys.NetTotal), netThisMonth) 
            { 
                IsBold = true, 
                AmountColor = netThisMonth < 0 ? Color.Red : netThisMonth > 0 ? Color.Green : Game1.textColor 
            }
        };
        netTable.Top = budgetMenu.MenuRect.Height - netTable.Height - 20;
        return netTable;
    }

    /// <summary>
    /// Creates outstanding costs section
    /// </summary>
    private static IMenuItem CreateOutstandingCostsItem(IModHelper helper, MayorModConfig modConfig, MayorModMenu budgetMenu, Texture2D coinIcon, int outstandingTop, int netLineTop)
    {
        var costs = SaveHandler.SaveData?.OutstandingCouncilCosts ?? new List<string>();
        if (costs.Count == 0)
        {
            var noCostsTable = new TableMenuItem(budgetMenu, 
                                                 Game1.content.LoadString(DialogueKeys.CouncilMeeting.OutstandingCosts), 
                                                 new List<TableRow>() { new(Game1.content.LoadString(DialogueKeys.CouncilMeeting.NoOutstandingCosts)) }, 0) { StartPercent = 0.5f };
            noCostsTable.Top = Math.Max(outstandingTop, netLineTop - noCostsTable.Height - 16);
            return noCostsTable;
        }

        var buttonHeight = 72;
        var buttonTop = Math.Max(outstandingTop, netLineTop - buttonHeight - 16);
        var buttonLeft = budgetMenu.MenuRect.Width / 2 + 20;
        var summary = string.Format(ModUtils.GetTranslationForKey(helper, UIMenuKeys.OutstandingSummary), costs.Count);
        var owed = costs.Sum(c => TreasuryCostKeys.Costs.GetValueOrDefault(c)).ToString();
        return new BigButtonListMenuItem(budgetMenu, 
                                         new Margin(buttonLeft, 
                                                    buttonTop, 
                                                    buttonLeft - 20, 
                                                    budgetMenu.MenuRect.Height - buttonHeight), 
                                         new List<string>() { summary }, (i) => ShowOutstandingCostsMenu(helper, modConfig))
                                         { 
                                            Icon = coinIcon, 
                                            RightText = new List<string?>() { owed }, 
                                            Font = Game1.smallFont, NumberOfButtons = 1 
                                         };
    }

    /// <summary>
    /// Show what the town still owes so it can be paid.
    /// </summary>
    private static void ShowOutstandingCostsMenu(IModHelper helper, MayorModConfig modConfig)
    {
        var costsMenu = new MayorModMenu(helper, 0.8f, 0.9f, TreasuryDisplayHandler.HUD_HEIGHT + 8);
        var costs = SaveHandler.SaveData?.OutstandingCouncilCosts ?? new List<string>();
        var coinIcon = helper.ModContent.Load<Texture2D>("assets/treasuryMoney.png");
        costsMenu.MenuItems = new List<IMenuItem>()
        {
            new TextMenuItem(costsMenu, ModUtils.GetTranslationForKey(helper, UIMenuKeys.Budget), new Margin(0, 15, 0, 0))
            { 
                IsBold = true, 
                Align = TextMenuItem.MenuItemAlign.Center 
            },
            new TextMenuItem(costsMenu, Game1.content.LoadString(DialogueKeys.CouncilMeeting.OutstandingCosts), new Margin(20, 60, 0, 0)),
            new BigButtonListMenuItem(costsMenu,
                                      new Margin(30, 110, 60, 130),
                                      costs.Select(c => Game1.content.LoadString($"{XNBPathKeys.UI}:{ModKeys.MAYOR_MOD_CPID}_Cost{c}")).ToList(),
                                      (i) => OnOutstandingCostSelected(costs[i]))
            { 
                ScrollBarEnabled = costs.Count > 4, 
                Icon = coinIcon, RightText = costs.Select(c => TreasuryCostKeys.Costs.GetValueOrDefault(c).ToString()).ToList<string?>() 
            },
            new ButtonMenuItem(costsMenu, new Vector2(24, 20), () => { ShowBudgetMenu(helper, modConfig); })
            {
                ButtonTypeSelected = ButtonMenuItem.ButtonType.Back
            },
            new ButtonMenuItem(costsMenu, new Vector2(-84, 20), () => { Game1.exitActiveMenu(); })
            {
                ButtonTypeSelected = ButtonMenuItem.ButtonType.Cancel
            }
        };
        Game1.activeClickableMenu = costsMenu;
    }

    /// <summary>
    /// Show the council meeting planning menu.
    /// </summary>
    public static void ShowCouncilMeetingMenu(IModHelper helper, MayorModConfig modConfig, bool reconsider = false)
    {
        if (!Context.IsMainPlayer)
        {
            return;
        }

        if (IsCouncilMeetingPlannded())
        {
            string dayPlanned = ModUtils.GetNextCouncilMeetingDay(helper, modConfig);
            Game1.drawObjectDialogue(Game1.content.LoadString(DialogueKeys.CouncilMeeting.AlreadyPlanned) + $" {dayPlanned}.");
            return;
        }

        //Check if there are any meetings available to plan
        var meetings = GetAvailableCouncilMeetings(reconsider);
        var canReconsider = !reconsider && GetAvailableCouncilMeetings(true).Count > 0;
        if (meetings.Count == 0 && !canReconsider)
        {
            Game1.drawObjectDialogue(Game1.content.LoadString(DialogueKeys.CouncilMeeting.NoNewMeetings));
            return;
        }

        //Show council menu, paid meetings get the treasury coin
        var councilMenu = new MayorModMenu(helper, 0.8f, 0.9f, TreasuryDisplayHandler.HUD_HEIGHT + 8);
        councilMenu.MenuItems = GetCouncilMeetingMenuItems(helper, modConfig, councilMenu, meetings, reconsider);
        if (canReconsider)
        {
            councilMenu.MenuItems.Add(new ButtonMenuItem(councilMenu, new Vector2(-156, 20), () => ShowCouncilMeetingMenu(helper, modConfig, true))
            {
                HoverText = Game1.content.LoadString(DialogueKeys.CouncilMeeting.Reconsider),
                ButtonTypeSelected = ButtonMenuItem.ButtonType.Options
            });
        }

        Game1.activeClickableMenu = councilMenu;
    }

    /// <summary>
    /// Get list of council meetings items.
    /// </summary>
    private static IList<IMenuItem> GetCouncilMeetingMenuItems(IModHelper helper, MayorModConfig modConfig, MayorModMenu councilMenu, IList<CouncilMeetingData> meetings, bool reconsider)
    {
        var coinIcon = helper.ModContent.Load<Texture2D>("assets/treasuryMoney.png");
        var title = reconsider ? DialogueKeys.CouncilMeeting.ReconsiderTitle : DialogueKeys.CouncilMeeting.HoldCouncilMeeting;
        
        string description = string.Empty;
        if (reconsider)
        {
            description = DialogueKeys.CouncilMeeting.ReconsiderDescription;
        }
        else if (meetings.Count == 0)
        {
            description = DialogueKeys.CouncilMeeting.NoNewMeetings;
        }
        else
        {
            description = DialogueKeys.CouncilMeeting.AgendaQuestion;
        }

        var items = new List<IMenuItem>()
        {
            new TextMenuItem(councilMenu, Game1.content.LoadString(title), new Margin(0, 15, 0, 0))
            { 
                IsBold = true, 
                Align = TextMenuItem.MenuItemAlign.Center 
            },
            new TextMenuItem(councilMenu, Game1.content.LoadString(description), new Margin(20, 60, 0, 0)),
            new ButtonMenuItem(councilMenu, new Vector2(24, 20), () =>
            {
                if (reconsider)
                    ShowCouncilMeetingMenu(helper, modConfig);
                else
                    ShowDeskMenu(helper, modConfig);
            })
            {
                ButtonTypeSelected = ButtonMenuItem.ButtonType.Back
            },
            new ButtonMenuItem(councilMenu, new Vector2(-84, 20), () => { Game1.exitActiveMenu(); })
            {
                ButtonTypeSelected = ButtonMenuItem.ButtonType.Cancel
            }
        };

        if (meetings.Count > 0)
        {
            items.Insert(2, new BigButtonListMenuItem(councilMenu, new Margin(30, 110, 60, 150), meetings.Select((cm, index) => $"{index + 1}. " + cm.Name).ToList(), i => OnCoucilMeetingSelected(meetings[i]))
            {
                ScrollBarEnabled = true,
                Icon = coinIcon,
                RightText = meetings.Select(cm => cm.IsPaid ? string.Empty : null).ToList()
            });
        }

        return items;
    }

    /// <summary>
    /// Get the list of council meeting the player can pick from
    /// </summary>
    /// <returns>list of council meetings</returns>
    private static IList<CouncilMeetingData> GetAvailableCouncilMeetings(bool reconsider = false)
    {
        var meetings = new List<CouncilMeetingData>()
        {
            new(Game1.content.LoadString(DialogueKeys.CouncilMeeting.MeetingIntro), CouncilMeetingKeys.MeetingIntro),
        };
        if (CouncilMeetingData.HasMeetingHappened(CouncilMeetingKeys.MeetingIntro))
        {
            meetings = new List<CouncilMeetingData>()
            {
                new(Game1.content.LoadString(DialogueKeys.CouncilMeeting.MeetingSaloonHours), CouncilMeetingKeys.MeetingSaloonHours),
            };

            //Paid meetings unlock once Lewis has handed over the town's accounts
            if (Game1.MasterPlayer.eventsSeen.Contains(ProgressFlags.TaxIntroEvent))
            {
                if (SaveHandler.SaveData?.TownTreasury < 0)
                {
                    meetings.Add(new CouncilMeetingData(Game1.content.LoadString(DialogueKeys.CouncilMeeting.MeetingFundraiser), CouncilMeetingKeys.MeetingFundraiser));
                }
                meetings.AddRange(new List<CouncilMeetingData>()
                {
                    new(Game1.content.LoadString(DialogueKeys.CouncilMeeting.MeetingTownSecurity), CouncilMeetingKeys.MeetingTownSecurity){ IsPaid = true },
                    new(Game1.content.LoadString(DialogueKeys.CouncilMeeting.MeetingTownCleanup), CouncilMeetingKeys.MeetingTownCleanup){ IsPaid = true },
                    new(Game1.content.LoadString(DialogueKeys.CouncilMeeting.MeetingStrategicReserve), CouncilMeetingKeys.MeetingStrategicReserve){ IsPaid = true },
                    new(Game1.content.LoadString(DialogueKeys.CouncilMeeting.MeetingHealthcareFunding), CouncilMeetingKeys.MeetingHealthcareFunding){ IsPaid = true },
                });
                if (CouncilMeetingData.HasMeetingHappened(CouncilMeetingKeys.MeetingTownCleanup))
                {
                    meetings.Add(new CouncilMeetingData(Game1.content.LoadString(DialogueKeys.CouncilMeeting.MeetingRiverCleanup), CouncilMeetingKeys.MeetingRiverCleanup){ IsPaid = true });
                }
                if (!Game1.MasterPlayer.eventsSeen.Contains(CompatibilityKeys.MorrisIsCampaigningForMayorEventID)) //SVE event for Mayor Morris
                {
                    meetings.Add(new CouncilMeetingData(Game1.content.LoadString(DialogueKeys.CouncilMeeting.MeetingTownRoads), CouncilMeetingKeys.MeetingTownRoads){ IsPaid = true });
                }
                if (Game1.MasterPlayer.mailReceived.Contains(ProgressFlags.CompleteBusRepair))
                {
                    meetings.Add(new CouncilMeetingData(Game1.content.LoadString(DialogueKeys.CouncilMeeting.MeetingBusFare), CouncilMeetingKeys.MeetingBusFare){ IsPaid = true });
                }
            }
        }

        return meetings.Where(m => reconsider ? m.CanReconsiderMeeting : !m.EventHasHappened).ToList();
    }

    /// <summary>
    /// Selects the next council meeting to run
    /// </summary>
    /// <param name="selectedMeeting">council meeting to plan</param>
    private static void OnCoucilMeetingSelected(CouncilMeetingData selectedMeeting )
    {
        if (!Context.IsMainPlayer || IsCouncilMeetingPlannded())
            return;

        Game1.MasterPlayer.eventsSeen.Remove(CouncilMeetingData.GetMeetingEventName(selectedMeeting.EventMailId));
        ModProgressHandler.AddProgressFlag(CouncilMeetingKeys.PlannedPrefix + selectedMeeting.EventMailId);
        ModProgressHandler.AddProgressFlag(CouncilMeetingKeys.NotToday);
        Game1.exitActiveMenu();
        Game1.drawObjectDialogue(Game1.content.LoadString(DialogueKeys.CouncilMeeting.MeetingPlanned));
    }

    /// <summary>
    /// Pays an outstanding cost from the town treasury
    /// </summary>
    /// <param name="costKey">cost to pay</param>
    private static void OnOutstandingCostSelected(string costKey)
    {
        Game1.exitActiveMenu();
        if (!TaxHandler.TrySpendTreasury(TreasuryCostKeys.Costs.GetValueOrDefault(costKey)))
        {
            Game1.drawObjectDialogue(Game1.content.LoadString(DialogueKeys.CouncilMeeting.NotEnoughFunds));
            return;
        }

        SaveHandler.SaveData?.OutstandingCouncilCosts.Remove(costKey);
        TaxHandler.ApplyPaidCostEffects(costKey);
        Game1.drawObjectDialogue(Game1.content.LoadString(DialogueKeys.CouncilMeeting.CostPaid));
    }

    /// <summary>
    /// Check if council meeting is planned
    /// </summary>
    private static bool IsCouncilMeetingPlannded()
    {
        CheckForCouncilPlanningIssues();
        return Game1.MasterPlayer.mailReceived.Any(p => p.StartsWith(CouncilMeetingKeys.PlannedPrefix));
    }

    /// <summary>
    /// Remove events seen where planned event key exists
    /// </summary>
    private static void CheckForCouncilPlanningIssues()
    {
        var planned = Game1.MasterPlayer.mailReceived
                                        .Where(plannedMail => plannedMail.StartsWith(CouncilMeetingKeys.PlannedPrefix))
                                        .Select(mail => $"{ModKeys.MAYOR_MOD_CPID}_CouncilMeeting{mail[CouncilMeetingKeys.PlannedPrefix.Length..]}Event")
                                        .ToHashSet();
        Game1.MasterPlayer.eventsSeen.RemoveWhere(e => planned.Contains(e));
    }

    /// <summary>
    /// Get available effects from council meetings
    /// </summary>
    /// <returns></returns>
    private static List<(string Name, string Flag)> GetAvailableCouncilEffects()
    {
        var effects = new List<(string Name, string Flag)>
        {
            (Game1.content.LoadString(DialogueKeys.CouncilMeeting.MeetingSaloonHours), ProgressFlags.SaloonHoursEarlyLate)
        };

        var keys = TreasuryCostKeys.Costs.Keys.Where(key => key != TreasuryCostKeys.FloodRepair &&
                                                            key != CouncilMeetingKeys.MeetingTownCleanup &&
                                                            key != CouncilMeetingKeys.MeetingStrategicReserve);

        effects.AddRange(keys.Select(key => (Game1.content.LoadString($"{XNBPathKeys.UI}:{ModKeys.MAYOR_MOD_CPID}_Cost{key}"),
                                     key == CouncilMeetingKeys.MeetingTownSecurity ? 
                                                ProgressFlags.SecurityOnGuard : 
                                                $"{ModKeys.MAYOR_MOD_CPID}_{key}")));
        
        return effects.Where(effect => Game1.MasterPlayer.mailReceived.Contains(effect.Flag) ||
                                       Game1.MasterPlayer.mailReceived.Contains(ProgressFlags.DisabledCouncilEffectPrefix + effect.Flag))
                      .ToList();

    }

    /// <summary>
    /// Show council event toggle menu.
    /// </summary>
    /// <param name="helper"></param>
    /// <param name="modConfig"></param>
    private static void ShowCouncilEffectsMenu(IModHelper helper, MayorModConfig modConfig)
    {
        if (!Context.IsMainPlayer)
            return;

        var effects = GetAvailableCouncilEffects();
        if (effects.Count == 0)
        {
            ShowBudgetMenu(helper, modConfig);
            return;
        }

        var menu = new MayorModMenu(helper, 0.8f, 0.9f, TreasuryDisplayHandler.HUD_HEIGHT + 8);
        var items = new List<IMenuItem>
        {
            new TextMenuItem(menu, ModUtils.GetTranslationForKey(helper, UIMenuKeys.CouncilEffects), new Margin(0, 15, 0, 0))
            { 
                IsBold = true, 
                Align = TextMenuItem.MenuItemAlign.Center 
            },
            new TextMenuItem(menu, ModUtils.GetTranslationForKey(helper, UIMenuKeys.CouncilEffectsDescription), new Margin(20, 60, 0, 0)),
            new ButtonMenuItem(menu, new Vector2(24, 20), () => ShowBudgetMenu(helper, modConfig))
            { 
                ButtonTypeSelected = ButtonMenuItem.ButtonType.Back 
            },
            new ButtonMenuItem(menu, new Vector2(-84, 20), () => Game1.exitActiveMenu())
            { 
                ButtonTypeSelected = ButtonMenuItem.ButtonType.Cancel 
            }
        };

        if (effects.Count > 0)
        {
            items.Insert(2, 
                         new BigButtonListMenuItem(menu, new Margin(30, 110, 60, 150), effects.Select(effect => effect.Name).ToList(), 
                         index => ToggleCouncilEffect(helper, modConfig, effects[index].Flag))
                         {
                            ScrollBarEnabled = true,
                            CheckedStates = effects.Select(effect => Game1.MasterPlayer.mailReceived.Contains(effect.Flag)).ToList(),
                            AccessibleLabels = effects.Select(effect => $"{effect.Name}: " +
                                                        ModUtils.GetTranslationForKey(helper, Game1.MasterPlayer.mailReceived.Contains(effect.Flag)? 
                                                            UIMenuKeys.EffectOn : 
                                                            UIMenuKeys.EffectOff)).ToList()
                         });
        }
        menu.MenuItems = items;
        menu.RebuildMenu = () => ShowCouncilEffectsMenu(helper, modConfig);
        Game1.activeClickableMenu = menu;
    }

    /// <summary>
    /// Toggles a council effect and refreshes the effects menu.
    /// </summary>
    private static void ToggleCouncilEffect(IModHelper helper, MayorModConfig modConfig, string flag)
    {
        if (!Context.IsMainPlayer)
        {
            return;
        }

        var disabledFlag = ProgressFlags.DisabledCouncilEffectPrefix + flag;
        if (Game1.MasterPlayer.mailReceived.Contains(flag))
        {
            ModProgressHandler.AddProgressFlag(disabledFlag);
            ModProgressHandler.RemoveProgressFlag(flag);
        }
        else if (Game1.MasterPlayer.mailReceived.Contains(disabledFlag))
        {
            ModProgressHandler.AddProgressFlag(flag);
            ModProgressHandler.RemoveProgressFlag(disabledFlag);
        }
        AssetInvalidationHandler.InvalidateLocationContexts();
        AssetInvalidationHandler.InvalidateModDataIfNeeded();
        if (flag == ProgressFlags.BusFareHalf || flag == ProgressFlags.BusFareFree)
        {
            if (Game1.getLocationFromName(nameof(BusStop)) is BusStop busStop)
            {
                ModProgressHandler.UpdateBusFare(busStop);
            }
        }
        ShowCouncilEffectsMenu(helper, modConfig);
    }

}
