using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;

using System;
using System.Collections;

using UnityEngine;

namespace LuisRandomness.BBPClassicBaldi
{
    [BepInPlugin(ModGuid, "Classic Conversion: Baldi", ModVersion)]
    [BepInDependency("mtm101.rulerp.bbplus.baldidevapi", BepInDependency.DependencyFlags.HardDependency)]
    public class ClassicBaldiPlugin : BaseUnityPlugin
    {
        public const string ModGuid = "io.github.uncertainluei.baldiplus.classicconversion.baldi";
        public const string ModVersion = "2024.1.0.0";

        private void Awake()
        {
            InitConfigValues();
            LoadingEvents.RegisterOnAssetsLoaded(Info, LoadAssets(), false);

            new Harmony(ModGuid).PatchAllConditionals();
        }

        internal static ConfigEntry<bool> config_loudestSoundOnly;
        internal static ConfigEntry<bool> config_classicSoundValues;

        internal static ConfigEntry<bool> config_disableBaldicator;

        internal static ConfigEntry<bool> config_classicJumpscare;
        internal static ConfigEntry<bool> config_baldiSpecificJumpscare;

        internal static ConfigEntry<BaldiSpeedMode> config_speedMode;
        internal static ConfigEntry<bool> config_baldiSpecificMovement;
        internal static ConfigEntry<bool> config_removeArcadeStretch;

        internal static ConfigEntry<bool> config_constDistance;
        internal static ConfigEntry<float> config_movementSpeed;
        internal static ConfigEntry<float> config_movementTime;

        internal static ConfigEntry<float> config_linearBaseDelay;
        internal static ConfigEntry<float> config_linearFinalDelay;
        internal static ConfigEntry<byte> config_linearMinTotal;

        internal static ConfigEntry<bool> config_noDookUnlock;
        internal static ConfigEntry<bool> config_instantCountdown;

        private void InitConfigValues()
        {
            config_loudestSoundOnly = Config.Bind(
                "Hearing",
                "LoudestSoundOnly",
                true,
                "Restores the Classic hearing behavior, where Baldi will only go for the loudest sound and ignore all quieter ones until a louder sound is made, reaches the location of the current sound, or spots the player.");

            config_classicSoundValues = Config.Bind(
                "Hearing",
                "ClassicSoundValues",
                true,
                "Uses noise values equivalent to Classic. Do note that this will mostly override vanilla noises and mods might break.");

            config_disableBaldicator = Config.Bind(
                "Hearing",
                "DisableBaldicator",
                false,
                "Disables the Baldicator that appears for every sound Baldi hears. Only turn this on if you are completely aware of the sound values themselves.");


            config_noDookUnlock = Config.Bind(
                "Misc",
                "NoDoorUnlock",
                true,
                "Baldi no longer permanently unlocks locked doors when hitting them.");
            config_instantCountdown = Config.Bind(
                "Misc",
                "InstantCountdown",
                false,
                "Baldi counts down all numbers at once just like in earlier versions and demos of Plus. May break mods that patch the SpawnWait coroutine in the \"HappyBaldi\" class!");


            config_classicJumpscare = Config.Bind(
                "Jumpscare",
                "ClassicJumpscare",
                true,
                "Replaces the jumpscare noise when Baldi catches you to use the spammed Classic buzz sound.\nNote: If the BUZZ noise is spammed instead, then an error has occurred when loading the sound.");
            config_baldiSpecificJumpscare = Config.Bind(
                "Jumpscare",
                "BaldiOnly",
                true,
                "Blacklists NPCs that may inherit Baldi's from using the Classic jumpscare to keep compatibility with modded Baldis.");

            config_speedMode = Config.Bind(
                "Anger",
                "SpeedMode",
                BaldiSpeedMode.Default,
                "The slapping mode Baldi is in, impacting how his slap frequencies work.\nDefault -> Default/vanilla slap curves\nClassic -> Use equation used in Classic and earlier BB+ releases/demos\nLinear -> Linear delay reduction, akin to Classic 1.2 and lower");
            config_baldiSpecificMovement = Config.Bind(
                "Anger",
                "BaldiOnly",
                true,
                "Blacklists NPCs that may inherit Baldi's class from getting their movement patched.");
            config_removeArcadeStretch = Config.Bind(
                "Anger",
                "RemoveArcadeStretch",
                false,
                "Reverts the anger stretch algorithm from Endless Floors. ");

            config_constDistance = Config.Bind(
                "Anger.Movement",
                "ConstantDistance",
                true,
                "Makes Baldi's slapping distance independent from his current anger. Ideal when using non-default speed modes as Baldi's distance is normally adjusted to stick with his intended speed.");
            config_movementSpeed = Config.Bind(
                "Anger.Movement",
                "Speed",
                112F,
                "Baldi's general movement speed when slapping. Default value is on par with Classic in comparison to the player's running speed.");
            config_movementTime = Config.Bind(
                "Anger.Movement",
                "Time",
                0.218F,
                "The movement time Baldi gets when slapping. Default value is equivalent to 10 fixed update ticks in Classic.");

            config_linearBaseDelay = Config.Bind(
                "Anger.Linear",
                "BaseDelay",
                3F,
                "The base delay.");
            config_linearFinalDelay = Config.Bind(
                "Anger.Linear",
                "FinalDelay",
                0.66F,
                "The delay when all notebooks are collected. Default value is equivalent to the average speed when all notebooks are collected in Classic 1.2.\nIf you want the speed to be equivalent to 1.0, use 0.57");
            config_linearMinTotal = Config.Bind(
                "Anger.Linear",
                "MinNotebooks",
                (byte)6,
                "The minimum amount of notebooks required to meet the final delay's \"all notebooks\" quota.");

        }

        internal static SoundObject audBuzz;
        internal static SoundObject audCountdown;

        private IEnumerator LoadAssets()
        {
            yield return 2;

            yield return "Loading Classic Buzz sound";

            // Copy the sound Baldi makes on loss just so there's a fallback if the custom sound is not loaded
            audBuzz = Instantiate(((Baldi)NPCMetaStorage.Instance.Get(Character.Baldi).value).loseSounds[0].selection);
            audBuzz.name = "Lose_ClassicBuzz";
            audBuzz.subtitle = false;

            try
            {
                AudioClip snd = AssetLoader.AudioClipFromMod(this, "BAL_Screech.wav");
                audBuzz.soundClip = snd;
            }
            catch (Exception e)
            {
                Logger.LogError("Sound \"BAL_Screech\" could not be loaded! Fallbacking to regular Buzz sound...");
                Logger.LogError("Exception info: " + e);
            }

            yield return "Loading Countdown voice clip";
            try
            {
                audCountdown = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, "BAL_Countdown.wav"), "Vfx_BAL_Countdown", SoundType.Voice, Color.green, 1.4F);
            }
            catch (Exception e)
            {
                MTM101BaldiDevAPI.CauseCrash(Info, e);
            }
            yield break;
        }
    }

    internal enum BaldiSpeedMode
    {
        Default,
        Classic, // Classic 1.3 - Early BB+
        Linear // Classic 1.2 - 1.0
    }

    public static class BaldiExtensions
    {
        private static Type baldiType = typeof(Baldi);

        public static bool IsVanilla(this Baldi baldi)
        {
            return baldi.Character == Character.Baldi && baldi.GetType() == baldiType;
        }
    }
}