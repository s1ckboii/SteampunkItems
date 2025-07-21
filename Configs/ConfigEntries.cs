using BepInEx.Configuration;

namespace SteampunkItems.Configs;

public class ConfigEntries
{
    #region Headset
    public ConfigEntry<bool> ConfigPromptEnable { get; private set; }
    public ConfigEntry<bool> ConfigFirstGrab { get; private set; }
    public ConfigEntry<float> ConfigGrabbedMusicVolume { get; private set; }
    public ConfigEntry<float> ConfigUngrabbedMusicVolume { get; private set; }
    public ConfigEntry<string> ConfigBlacklistedSongs { get; private set; }
    #endregion

    #region Stopwatch
    public ConfigEntry<string> ConfigStopwatchONMaterial { get; private set; }
    public ConfigEntry<string> ConfigStopwatchOFFMaterial { get; private set; }
    public ConfigEntry<float> ConfigOwnVoicePitchMultiplier { get; private set; }
    public ConfigEntry<float> ConfigOthersVoicePitchMultiplier { get; private set; }
    public ConfigEntry<float> ConfigOverridePlayerSpeed { get; private set; }
    public ConfigEntry<float> ConfigOverridePlayerLookSpeed { get; private set; }
    public ConfigEntry<float> ConfigOverrideStopwatchZoomSet {  get; private set; }
    public ConfigEntry<float> ConfigSaturationOverride { get; private set; }
    public ConfigEntry<float> ConfigOverrideStopwatchPupilSize { get; private set; }
    public ConfigEntry<float> ConfigOverrideDrag { get; private set; }
    public ConfigEntry<float> ConfigOverrideAngularDrag { get; private set; }
    #endregion
/*
    #region MagniGlass
    public ConfigEntry<float> ConfigOverrideGrabDistance { get; private set; }
    public ConfigEntry<float> ConfigOverrideMagniGlassPupilSize { get; private set; }
    public ConfigEntry<float> ConfigOverrideMagniGlassZoomSet { get; private set; }
    #endregion
*/
    public void ConfigManager(ConfigFile configFile)
    {
        #region Headset
        ConfigPromptEnable = configFile.Bind("Headset Options",
            "Tool tip | Text",
            true,
            "Allows you to disable tool tip");
        ConfigFirstGrab = configFile.Bind("Headset Options",
            "Music | First Grab",
            true,
            "Allows you to disable first grab feature where a random music-particle pair is toggled on.");
        ConfigGrabbedMusicVolume = configFile.Bind("Headset Options",
            "Music | Grabbed Volume",
            0.5f,
            new ConfigDescription("Allows you to change the volume when its playing while grabbed (default: 0.5).",
            new AcceptableValueRange<float>(0, 1f)));
        ConfigUngrabbedMusicVolume = configFile.Bind("Headset Options",
            "Music | Ungrabbed Volume",
            0.1f,
            new ConfigDescription("Allows you to change the volume when its playing on the floor (default: 0.1).",
            new AcceptableValueRange<float>(0, 1f)));
        ConfigBlacklistedSongs = configFile.Bind("Headset Options",
            "Music | Blacklisted Song-Particle pair",
            "",
            "Allows you to remove song-particle pairs. There are 8 pairs (0-7), if you want to remove first, third, and fourth you would need to type '0, 2, 3'.");
        #endregion

        #region Stopwatch
        ConfigStopwatchONMaterial = configFile.Bind("Stopwatch Options",
            "Material | Color ON",
            "#420069",
            "Allows you to change the material's color in HEX format while not grabbing the valuable (Default: #FFFFFF)");
        ConfigStopwatchOFFMaterial = configFile.Bind("Stopwatch Options",
            "Material | Color OFF",
            "#690420",
            "Allows you to change the material's color in HEX format while grabbing the valuable (Default: #FFFFF)");
        ConfigOwnVoicePitchMultiplier = configFile.Bind("Stopwatch Options",
            "Sounds | Own Voice Pitch Multiplier",
            0.65f,
            new ConfigDescription("Allows you to adjust the pitch multiplier for your own voice while grabbing the valuable (default: 0.65).",
            new AcceptableValueRange<float>(0.55f, 1.15f)));
        ConfigOthersVoicePitchMultiplier = configFile.Bind("Stopwatch Options",
            "Sounds | Other Grabbers Pitch Multiplier",
            0.65f,
            new ConfigDescription("Allows you to adjust the pitch multiplier for other players' voices while grabbing the valuable (default: 0.65).",
            new AcceptableValueRange<float>(0.55f, 1.15f)));
        ConfigOverridePlayerSpeed = configFile.Bind("Stopwatch Options",
            "Overrides | Speed",
            0.5f,
            new ConfigDescription("Allows you to adjust override speed (LOOKSPEED SCALES WITH THIS) for the player while grabbing the valuable (default: 0.5).",
            new AcceptableValueRange<float>(0.35f, 2f)));
        ConfigOverridePlayerLookSpeed = configFile.Bind("Stopwatch Options",
            "Overrides | Look Speed",
            0.5f,
            new ConfigDescription("Allows you to adjust override look speed (Playerspeed + LookSpeedConfig = overall lookspeed) override for the player while grabbing the valuable (default: 0.5).",
            new AcceptableValueRange<float>(0.1f, 2f)));
        ConfigOverrideStopwatchZoomSet = configFile.Bind("Stopwatch Options",
            "Overrides | Zoom",
            50f,
            new ConfigDescription("Allows you to adjust override zoom for the player while grabbing the valuable [NOTE: 30 is closer, 100 is farther] (default: 50).",
            new AcceptableValueRange<float>(30f, 100f)));
        ConfigSaturationOverride = configFile.Bind("Stopwatch Options",
            "Overrides | Saturation",
            50f,
            new ConfigDescription("Allows you to adjust override saturation for the player while grabbing the valuable (default: 50).",
            new AcceptableValueRange<float>(0.1f, 100f)));
        ConfigOverrideStopwatchPupilSize = configFile.Bind("Stopwatch Options",
            "Overrides | Pupil Size",
            3f,
            new ConfigDescription("Allow you to adjust override pupil size for the player while grabbing the valuable (default: 3).",
            new AcceptableValueRange<float>(0.1f, 5f)));
        ConfigOverrideDrag = configFile.Bind("Stopwatch Options",
            "Overrides | Drag",
            20f,
            new ConfigDescription("Allow you to adjust override drag value for the player while grabbing the valuable (default: 20).",
            new AcceptableValueRange<float>(0.1f, 25f)));
        ConfigOverrideAngularDrag = configFile.Bind("Stopwatch Options",
            "Overrides | Angular Drag",
            40f,
            new ConfigDescription("Allow you to adjust override angular drag value for the player while grabbing the valuable (default: 40).",
            new AcceptableValueRange<float>(0.1f, 45f)));
        #endregion
/*
        #region MagniGlass
        ConfigOverrideGrabDistance = configFile.Bind("MagniGlass Options",
            "Overrides | Grab Distance",
            0.5f,
            new ConfigDescription("Allow you to adjust override grab distance for the player while grabbing the valuable (default: 0.5).",
            new AcceptableValueRange<float>(0.1f, 3f)));
        ConfigOverrideMagniGlassPupilSize = configFile.Bind("MagniGlass Options",
            "Overrides | Pupil Size",
            3f,
            new ConfigDescription("Allow you to adjust override pupil size for the player while grabbing the valuable (default: 3).",
            new AcceptableValueRange<float>(0.1f, 5f)));
        ConfigOverrideMagniGlassZoomSet = configFile.Bind("MagniGlass Options",
            "Overrides | Zoom",
            40f,
            new ConfigDescription("Allows you to adjust override zoom for the player while grabbing the valuable (default: 40).",
            new AcceptableValueRange<float>(0.1f, 100f)));
        #endregion
*/
    }
}