﻿using BepInEx;
using BepInEx.Configuration;
using REPOLib.Modules;
using SteampunkItems.Configs;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SteampunkItems;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(REPOLib.MyPluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.HardDependency)]
public class Plugins : BaseUnityPlugin
{
    public static ConfigFile ConfigFile { get; private set; } = null!;
    public static ConfigEntries ModConfig { get; private set; } = null!;
    private void Awake()
    {
        ConfigFile = this.Config;
        ModConfig = new ConfigEntries();

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
            //"Valuable MagniGlass_SP",
            "Valuable Telescope_SP",
            "Valuable Stopwatch_SP",
            "Valuable HeadSet_SP",
            "Valuable Chronometer_SP",
            "Valuable Logpose_SP"
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

        ModConfig.ConfigManager(ConfigFile);
    }
    private void RegisterValuable(AssetBundle assetBundle, string valuableName, List<string> valuableAssetNames)
    {
        GameObject valuables = assetBundle.LoadAsset<GameObject>(valuableName);
        Valuables.RegisterValuable(valuables, valuableAssetNames);
    }
    private void RegisterItem(AssetBundle assetBundle, string itemName)
    {
        Item items = assetBundle.LoadAsset<Item>(itemName);
        Items.RegisterItem(items);
    }
}