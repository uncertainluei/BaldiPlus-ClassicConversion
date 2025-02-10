using BepInEx;
using HarmonyLib;

using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;

using BepInEx.Configuration;

using System.IO;

using UnityEngine;

namespace LuisRandomness.BBPClassicExits
{
    [BepInPlugin(ModGuid, "Classic Conversion: Exits", ModVersion)]
    [BepInDependency("mtm101.rulerp.bbplus.baldidevapi", BepInDependency.DependencyFlags.HardDependency)]
    public class ClassicExitsPlugin : BaseUnityPlugin
    {
        public const string ModGuid = "io.github.uncertainluei.baldiplus.classicconversion.exits";
        public const string ModVersion = "2024.1.0.0";

        internal static AssetManager assetMan = new AssetManager();

        void Awake()
        {
            InitConfigValues();
            LoadAssets();

            new Harmony(ModGuid).PatchAllConditionals();
        }

        internal static ConfigEntry<ClassicGateMode> config_gateMode;

        internal static ConfigEntry<ClassicFinaleMode> config_lightMode;
        internal static ConfigEntry<Color> config_lightColor;
        internal static ConfigEntry<Color> config_fogColor;

        internal static ConfigEntry<ClassicFinaleMode> config_audioMode;

        internal static ConfigEntry<bool> config_escapeMusic;


        void InitConfigValues()
        {
            config_gateMode = Config.Bind(
                "General",
                "GateMode",
                ClassicGateMode.Wall,
                "Whether to replace elevator gates with walls and if a map of the level is provided alongside it.");
            config_lightMode = Config.Bind(
                "General",
                "LightMode",
                ClassicFinaleMode.Remastered,
                "The mode of the red light that will appear when an elevator closes. 'Archaic' mode will add in a dark red fog.");
            config_audioMode = Config.Bind(
                "General",
                "AudioMode",
                ClassicFinaleMode.Remastered,
                "The audio mode that will be used for playing the ambient loops after an elevator closes. 'Archaic' mode removes the 1st exit loop.");

            config_escapeMusic = Config.Bind(
                "General",
                "EscapeMusic",
                true,
                "(NOT ADDED YET) Will play Schoolhouse Escape upon all notebooks being collected. If false, it will also disable the Endless Floors music patch.");

            config_lightColor = Config.Bind(
                "Colors",
                "lightColor",
                Color.red,
                "The color of the red light. Affects the color in Remastered and Classic modes.");
            config_fogColor = Config.Bind(
                "Colors",
                "fogColor",
                new Color(0.2431373F, 0F, 0F),
                "The color of the fog that will appear in the Archaic light mode.");
        }

        void LoadAssets()
        {
            string path = AssetLoader.GetModPath(this);
            AddAudioToAssetMan("Classic_", path, "ClassicLoops");
            AddAudioToAssetMan("Cr_", path, "RemasteredLoops");

            AddLoopingObject("Loop_Classic_Quiet", "Aud_Classic_QuietLoop");
            AddLoopingObject("Loop_Classic_Early", "Aud_Classic_Loud_EarlyLoop");
            AddLoopingObject("Loop_Classic_Buildup", "Aud_Classic_Loud_Buildup", "Aud_Classic_Loud_FinalLoop");

            AddLoopingObject("Loop_Cr_Early", "Aud_Cr_Chaos_EarlyLoopStart", "Aud_Cr_Chaos_EarlyLoop");
            AddLoopingObject("Loop_Cr_Buildup", "Aud_Cr_Chaos_Buildup", "Aud_Cr_Chaos_FinalLoop");

            assetMan.Add("ArchaicFog", new Fog() { color = config_fogColor.Value, priority = 15, startDist = 5, maxDist = 80, strength = 1});

            assetMan.Add("Sfx_BigSwitch", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(path, "BigSwitch.wav")), "Sfx_BigSwitch", SoundType.Effect, Color.white, 0.6676F));
        }

        void AddAudioToAssetMan(string prefix, params string[] path)
        {
            string[] paths = Directory.GetFiles(Path.Combine(path));
            string name;
            for (int i = 0; i < paths.Length; i++)
            {
                name = Path.GetFileNameWithoutExtension(paths[i]);
                assetMan.Add("Aud_" + prefix + name, AssetLoader.AudioClipFromFile(paths[i]));
            }
        }

        void AddLoopingObject(string loopName, params string[] clips)
        {
            LoopingSoundObject loop = ScriptableObject.CreateInstance<LoopingSoundObject>();
            loop.name = loopName;
            int len = clips.Length;

            AudioClip[] auds = new AudioClip[len];
            for (int i = 0; i < len; i++)
            {
                auds[i] = assetMan.Get<AudioClip>(clips[i]);
            }

            loop.clips = auds;
            assetMan.Add(loopName, loop);
        }
    }

    public enum ClassicGateMode : byte
    {
        Default,
        Wall,
        WallWithMap
    }

    public enum ClassicFinaleMode : byte
    {
        Default,
        Archaic,
        Classic,
        Remastered
    }
}
