using System.IO;
using UnityEngine;
using BepInEx;
using REPOLib.Modules;
using System.Collections.Generic;

namespace SteampunkItems
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency(REPOLib.MyPluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.HardDependency)]
    public class Plugins : BaseUnityPlugin
    {
        private void Awake()
        {
            string pluginFolderPath = Path.GetDirectoryName(Info.Location);
            string assetBundleFilePath = Path.Combine(pluginFolderPath, "steampunkitems");
            AssetBundle assetBundle = AssetBundle.LoadFromFile(assetBundleFilePath);

            if (assetBundle == null)
            {
                Logger.LogError("Failed to load SteampunkItems assetbundle.");
                return;
            }

            List<string> valuabaleAssetNames = 
            [
                "Valuable MagniGlass_SP",
                "Valuable Telescope_SP",
                "Valuable Stopwatch_SP",
                "Valuable HeadSet_SP"
            ];
            List<string> genericList = ["Valuables - Generic"];
            foreach (var assetNames in valuabaleAssetNames)
            {
                RegisterValuable(assetBundle, assetNames, genericList);
            }
            Item meleePickaxe = assetBundle.LoadAsset<Item>("Item Melee Pickaxe_SP");
            Items.RegisterItem(meleePickaxe);
        }
        private void RegisterValuable(AssetBundle assetBundle, string assetName, List<string> list)
        {
            GameObject valuables = assetBundle.LoadAsset<GameObject>(assetName);
            Valuables.RegisterValuable(valuables, list);
        }
    }
}