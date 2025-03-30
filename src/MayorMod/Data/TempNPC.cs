using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace MayorMod.Data
{
    internal class TempNPC
    {
        private readonly IModHelper _helper;
        public IList<AssetReplacement> EventAssetsReplacement { get; set; } = [];

        public TempNPC(IContentEvents contentEvents, IModHelper helper)
        {
            _helper= helper;
            contentEvents.AssetRequested += this.OnAssetRequested;
        }

        private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            var replace = EventAssetsReplacement.FirstOrDefault(a => e.NameWithoutLocale.IsEquivalentTo(a.EventName) || e.NameWithoutLocale.IsEquivalentTo(a.AssetName));
            if (replace == null)
            {
                return;
            }
            if (e.NameWithoutLocale.IsEquivalentTo(replace.EventName))
            {
                replace.ReplaceAsset = true;
            }
            else if (replace.ReplaceAsset)
            {
                e.Edit(asset => { asset.ReplaceWith(_helper.GameContent.Load<Texture2D>(replace.AssetName)); });
                replace.ReplaceAsset = false;
            }
        }

        public void RegisterAssetReplacementForEvent(string eventName, string assetName)
        {
            EventAssetsReplacement.Add(new AssetReplacement()
            {
                ReplaceAsset = false,
                EventName = eventName,
                AssetName = assetName
            });
        }

        public class AssetReplacement: IEquatable<AssetReplacement>
        {
            public bool ReplaceAsset {get; set; }
            public string EventName { get; set; } = string.Empty;
            public string AssetName { get; set; } = string.Empty;

            bool IEquatable<AssetReplacement>.Equals(AssetReplacement? other)
            {
                if (other == null)
                {
                    return false; 
                }
                return EventName.ToLower() == other.EventName.ToLower() &&
                       AssetName.ToLower() == other.AssetName.ToLower();
            }
        }
    }
}
