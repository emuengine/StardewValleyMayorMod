using MayorMod.Constants;
using StardewModdingAPI.Events;
using StardewModdingAPI;
using StardewValley.GameData.Locations;
using System.Text.Json;
using MayorMod.Data.Models;
using StardewModdingAPI.Utilities;
using StardewValley.GameData;

namespace MayorMod.Data.Handlers;

public class AssetUpdateHandler
{
    private readonly IModHelper _helper;
    private readonly IMonitor _monitor;
    private Dictionary<string, Dictionary<string, List<StringPatch>>> _mayorStringReplacements = new();

    public AssetUpdateHandler(IModHelper helper, IMonitor monitor)
    {
        _helper = helper;
        _monitor= monitor;

        var stringPatchLocation = Path.Join(_helper.DirectoryPath, ModKeys.REPLACEMENT_STRING_CONFIG);
        if (!File.Exists(stringPatchLocation))
        {
            _monitor.Log($"Error: File not found {stringPatchLocation}", LogLevel.Error);
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
    public void AssetSubstringPatch(AssetRequestedEventArgs e)
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
                                data[KeyInt] = updates[key].Aggregate(data[KeyInt], (current, patch) => patch.PatchString(_helper, current));
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
                                data[keyMatch] = updates[key].Aggregate(data[keyMatch], (current, patch) => patch.PatchString(_helper, current));
                            }
                        }
                    }
                }, AssetEditPriority.Late);
            }
        }
        catch (Exception ex)
        {
            _monitor.Log($"Failed to patch asset - {ex.Message}", LogLevel.Error);
        }
    }

    /// <summary>
    /// Updates dialogue so that all characters not specifically designated will reject election leaflets
    /// </summary>
    public void AssetUpdatesForLeafletDialogue(AssetRequestedEventArgs e)
    {
        e.Edit(dialogues =>
        {
            var data = dialogues.AsDictionary<string, string>().Data;
            if (!data.ContainsKey($"AcceptGift_(O){ModItemKeys.Leaflet}") &&
                !data.ContainsKey($"RejectItem_(O){ModItemKeys.Leaflet}"))
            {
                data[$"RejectItem_(O){ModItemKeys.Leaflet}"] = ModUtils.GetTranslationForKey(_helper, $"{ModKeys.MAYOR_MOD_CPID}_Gifting.Default.Leaflet");
            }
        });
    }

    /// <summary>
    /// Update the Locations default fish catch rate
    /// </summary>
    public void RemoveTrashFromRivers(AssetRequestedEventArgs e)
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
    /// Updates Passive Festivals assets that depend on voting day
    /// </summary>
    public void AssetUpdatesForPassiveFestivals(AssetRequestedEventArgs e, SDate votingDate)
    {
        e.Edit(festivals =>
        {
            var data = festivals.AsDictionary<string, PassiveFestivalData>().Data;
            var votingDay = new PassiveFestivalData()
            {
                DisplayName = ModUtils.GetTranslationForKey(_helper, $"{ModKeys.MAYOR_MOD_CPID}_Festival.VotingDay.Name"),
                StartMessage = ModUtils.GetTranslationForKey(_helper, $"{ModKeys.MAYOR_MOD_CPID}_Festival.VotingDay.Message"),
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
