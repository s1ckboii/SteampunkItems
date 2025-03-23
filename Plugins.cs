using System.IO;
using UnityEngine;
using BepInEx;
using REPOLib.Modules;
using System.Collections.Generic;

namespace SteampunkItems;

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
        foreach (var valuableName in valuabaleAssetNames)
        {
            RegisterValuable(assetBundle, valuableName, genericList);
        }
        List<string> itemAssetNames =
        [
            "Item Melee Pickaxe_SP",
            //"Item Bombolver_SP"
        ];
        foreach (var itemName in itemAssetNames)
        {
            RegisterItem(assetBundle, itemName);
        }
    }
    private void RegisterValuable(AssetBundle assetBundle, string valuableName, List<string> valuableAssetNames)
    {
        GameObject valuables = assetBundle.LoadAsset<GameObject>(valuableName);
        //Valuables.RegisterValuable(valuables, valuableAssetNames);
        Valuables.RegisterValuable(valuables, valuableAssetNames);
    }
    private void RegisterItem(AssetBundle assetBundle, string itemName)
    {
        Item items = assetBundle.LoadAsset<Item>(itemName);
        Items.RegisterItem(items);
    }
}