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
            GameObject val1 = assetBundle.LoadAsset<GameObject>("Valuable MagniGlass_SP");
            GameObject val2 = assetBundle.LoadAsset<GameObject>("Valuable Telescope_SP");
            GameObject val3 = assetBundle.LoadAsset<GameObject>("Valuable Stopwatch_SP");
            GameObject val4 = assetBundle.LoadAsset<GameObject>("Valuable HeadSet_SP");
            List<string> list = ["Valuables - Generic"];
            Valuables.RegisterValuable(val1, list);
            Valuables.RegisterValuable(val2, list);
            Valuables.RegisterValuable(val3, list);
            Valuables.RegisterValuable(val4, list);
            GameObject item1 = assetBundle.LoadAsset<GameObject>("Item Melee Pickaxe_SP");
            Item item = assetBundle.LoadAsset<Item>("Item Melee Pickaxe_SP");
            Items.RegisterItem(item);
        }
    }
}