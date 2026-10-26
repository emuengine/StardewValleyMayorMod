using MayorMod.Constants;
using StardewModdingAPI.Events;
using StardewModdingAPI;
using StardewValley.GameData.Buildings;
using StardewValley.GameData.Locations;
using System.Text.Json;
using MayorMod.Data.Models;
using StardewValley.GameData;
using MayorMod.Data.Utilities;
using System.Collections;

namespace MayorMod.Data.Handlers;

public static class AssetUpdateHandler
{
    private static IMod _mod = null!;
    private static Dictionary<string, Dictionary<string, List<StringPatch>>> _mayorStringReplacements = new();
    private const string DISABLED_EVENT_PRECONDITION = "/G FALSE";

    public static void Init(IMod mod)
    {
        _mod = mod;

        var stringPatchLocation = Path.Join(_mod.Helper.DirectoryPath, ModKeys.REPLACEMENT_STRING_CONFIG);
        if (!File.Exists(stringPatchLocation))
        {
            _mod.Monitor.Log($"Error: File not found {stringPatchLocation}", LogLevel.Error);
        }
        var stringPatchFile = File.ReadAllText(stringPatchLocation);

        _mayorStringReplacements = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, List<StringPatch>>>>(stringPatchFile, new JsonSerializerOptions
        {
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        }) ?? new Dictionary<string, Dictionary<string, List<StringPatch>>>();
    }

    /// <summary>
    /// Replaces substrings in assets from patches loaded from json 
    /// NOTE: Can possibly be done by content patcher but I couldn't work out how.
    /// </summary>
    public static void AssetSubstringPatch(AssetRequestedEventArgs e)
    {
        try
        {
            if (e.NameWithoutLocale is not null &&
                e.NameWithoutLocale.BaseName is not null &&
                //_mayorStringReplacements.Keys.Any(sr => e.NameWithoutLocale.IsEquivalentTo(sr)))
                _mayorStringReplacements.ContainsKey(e.NameWithoutLocale.BaseName.ToLower()))
            {
                e.Edit((asset) =>
                {
                    var updates = _mayorStringReplacements[e.NameWithoutLocale.BaseName.ToLower()];
                    if (e.NameWithoutLocale.BaseName.Equals(XNBPathKeys.SECRET_NOTES, StringComparison.InvariantCultureIgnoreCase))
                    {
                        var data = asset.AsDictionary<int, string>().Data;
                        foreach (var key in updates.Keys)
                        {
                            if (int.TryParse(key, out int KeyInt) && data.ContainsKey(KeyInt))
                            {
                                data[KeyInt] = updates[key].Aggregate(data[KeyInt], (current, patch) => patch.PatchString(_mod.Helper, current));
                            }
                        }
                    }
                    else
                    {
                        var data = asset.AsDictionary<string, string>().Data;
                        foreach (var key in updates.Keys)
                        {
                            if (data.Keys.FirstOrDefault(k => k.StartsWith(key)) is { } keyMatch)
                            {
                                data[keyMatch] = updates[key].Aggregate(data[keyMatch], (current, patch) => patch.PatchString(_mod.Helper, current));
                            }
                        }
                    }
                }, AssetEditPriority.Late);
            }
        }
        catch (Exception ex)
        {
            _mod.Monitor.Log($"Failed to patch asset - {ex.Message}", LogLevel.Error);
        }
    }

    /// <summary>
    /// Updates dialogue so that all characters not specifically designated will reject election leaflets
    /// </summary>
    public static void AssetUpdatesForLeafletDialogue(AssetRequestedEventArgs e)
    {
        e.Edit(dialogues =>
        {
            var data = dialogues.AsDictionary<string, string>().Data;
            if (!data.ContainsKey($"AcceptGift_(O){ModItemKeys.Leaflet}") &&
                !data.ContainsKey($"RejectItem_(O){ModItemKeys.Leaflet}"))
            {
                data[$"RejectItem_(O){ModItemKeys.Leaflet}"] = ModUtils.GetTranslationForKey(_mod.Helper, $"{ModKeys.MAYOR_MOD_CPID}_Gifting.Default.Leaflet");
            }
        });
    }

    /// <summary>
    /// Moves events to another location for compatibility.
    /// </summary>
    public static void MoveEvents(AssetRequestedEventArgs e, EventTransfer transfer)
    {
        if (e.NameWithoutLocale.IsEquivalentTo(transfer.FromEventsAsset))
        {
            e.Edit((asset) =>
            {
                var data = asset.AsDictionary<string, string>().Data;
                foreach (var eventKey in data.Keys.Where(k => transfer.EventIds.Contains(k.Split('/')[0]) && !k.EndsWith(DISABLED_EVENT_PRECONDITION)).ToList())
                {
                    data[eventKey + DISABLED_EVENT_PRECONDITION] = data[eventKey];
                    data.Remove(eventKey);
                }
            }, AssetEditPriority.Late);
        }
        else if (e.NameWithoutLocale.IsEquivalentTo(transfer.ToEventsAsset))
        {
            e.Edit((asset) =>
            {
                var data = asset.AsDictionary<string, string>().Data;
                var fromData = _mod.Helper.GameContent.Load<Dictionary<string, string>>(transfer.FromEventsAsset);
                foreach (var eventId in transfer.EventIds)
                {
                    var eventKey = fromData.Keys.FirstOrDefault(k => k.Split('/')[0] == eventId);
                    if (eventKey != null)
                    {
                        data[eventKey.Replace(DISABLED_EVENT_PRECONDITION, string.Empty)] = fromData[eventKey];
                    }
                }
            });
        }
    }

    /// <summary>
    /// Replaces substrings in the pages of a SpaceCore guidebook.
    /// </summary>
    public static void GuidebookSubstringPatch(AssetRequestedEventArgs e, string guidebookId, string searchKey, string replaceKey)
    {
        var patch = new StringPatch() { SearchKey = searchKey, ReplaceKey = replaceKey, IsTranslationKey = true };
        e.Edit((asset) =>
        {
            if (asset.Data is not IDictionary guidebooks)
            {
                return;
            }
            var guidebook = guidebooks[guidebookId];
            if (guidebook?.GetType().GetProperty("Chapters")?.GetValue(guidebook) is not IDictionary chapters)
            {
                return;
            }
            foreach (var chapter in chapters.Values)
            {
                if (chapter?.GetType().GetProperty("Pages")?.GetValue(chapter) is not IEnumerable pages)
                {
                    continue;
                }
                foreach (var page in pages)
                {
                    var contents = page.GetType().GetProperty("Contents");
                    if (contents?.GetValue(page) is string text)
                    {
                        contents.SetValue(page, patch.PatchString(_mod.Helper, text));
                    }
                }
            }
        }, AssetEditPriority.Late);
    }

    /// <summary>
    /// Update the Locations default fish catch rate
    /// </summary>
    public static void RemoveTrashFromRivers(AssetRequestedEventArgs e)
    {
        e.Edit((asset) =>
        {
            var data = asset.AsDictionary<string, LocationData>().Data;
            var rubbish = data["Default"].Fish.FirstOrDefault(f => f.Id == ModKeys.RUBBISH_ID);
            if (rubbish is not null)
            {
                rubbish.Chance = 0.01f;
            }
        });
    }


    /// <summary>
    /// Robins building upgrades now finish instantly.
    /// </summary>
    public static void RobinBuildsInstantly(AssetRequestedEventArgs e)
    {
        e.Edit((asset) =>
        {
            var data = asset.AsDictionary<string, BuildingData>().Data;
            foreach (var building in data.Values.Where(b => b.Builder == ModNPCKeys.RobinId))
            {
                building.BuildDays = 0;
                foreach (var skin in building.Skins)
                {
                    skin.BuildDays = null;
                }
            }
        });
    }

    /// <summary>
    /// Updates Passive Festivals assets that depend on voting day
    /// </summary>
    public static void AssetUpdatesForPassiveFestivals(AssetRequestedEventArgs e)
    {
        e.Edit(festivals =>
        {
            var votingDate = ModUtils.GetVotingDate();
            if (votingDate is null)
            {
                return;
            }
            var data = festivals.AsDictionary<string, PassiveFestivalData>().Data;
            var votingDay = new PassiveFestivalData()
            {
                DisplayName = ModUtils.GetTranslationForKey(_mod.Helper, $"{ModKeys.MAYOR_MOD_CPID}_Festival.VotingDay.Name"),
                StartMessage = ModUtils.GetTranslationForKey(_mod.Helper, $"{ModKeys.MAYOR_MOD_CPID}_Festival.VotingDay.Message"),
                Season = votingDate.Season,
                StartDay = votingDate.Day,
                EndDay = votingDate.Day,
                StartTime = 610,
                ShowOnCalendar = true,
            };
            data[$"{ModKeys.MAYOR_MOD_CPID}_VotingDayPassiveFestival"] = votingDay;
        });
    }
}
