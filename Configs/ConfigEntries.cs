using BepInEx.Configuration;


namespace SteampunkItems.Configs;

public class ConfigEntries
{
    #region Headset
    public ConfigEntry<bool> ConfigFirstGrab { get; private set; }
    public ConfigEntry<float> ConfigGrabbedMusicVolume { get; private set; }
    public ConfigEntry<float> ConfigUngrabbedMusicVolume { get; private set; }
    #endregion

    public void ConfigManager(ConfigFile configFile)
    {
        #region Headset
        ConfigFirstGrab = configFile.Bind("Headset Options",
            "Headset | First Grab",
            true ,
            "Allows you to disable first grab feature where a random music-particle pair is toggled on.");
        ConfigGrabbedMusicVolume = configFile.Bind("Headset Options",
            "Headset | Grabbed Music Volume",
            0.5f,
            new ConfigDescription("Allows you to change the volume when its playing while grabbed.",
            new AcceptableValueRange<float>(0, 1f)));
        ConfigUngrabbedMusicVolume = configFile.Bind("Headset Options",
            "Headset | Ungrabbed Music Volume",
            0.1f,
            new ConfigDescription("Allows you to change the volume when its playing on the floor.",
            new AcceptableValueRange<float>(0, 1f)));
        #endregion
    }
}
