using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Linq;
using System.Reflection;
using MTM101BaldAPI;
using MTM101BaldAPI.Registers;
using UnityEngine;
using MTM101BaldAPI.AssetTools;
using System;
using MTM101BaldAPI.OptionsAPI;
using System.IO;
using System.ComponentModel;

namespace LuisRandomness.BBPClassicBaldi
{
    [BepInPlugin(ModGuid, "Classic Baldi AI", ModVersion)]
    [BepInDependency("mtm101.rulerp.bbplus.baldidevapi", BepInDependency.DependencyFlags.HardDependency)]
    public class ClassicBaldiPlugin : BaseUnityPlugin
    {
        public const string ModGuid = "io.github.luisrandomness.bbp_classic_baldi";
        public const string ModVersion = "2024.1.0.0";

        void Awake()
        {
            InitConfigValues();
            LoadingEvents.RegisterOnAssetsLoaded(LoadAssets, false);

            new Harmony(ModGuid).PatchAllConditionals();
        }

        internal static ConfigEntry<bool> config_overridePreviousNoises;
        internal static ConfigEntry<bool> config_classicNoiseValues;

        internal static ConfigEntry<bool> config_disableBaldicator;
        
        internal static ConfigEntry<bool> config_classicJumpscare;
        internal static ConfigEntry<bool> config_baldiSpecificJumpscare;

        internal static ConfigEntry<BaldiSpeedMode> config_speedMode;

        internal static ConfigEntry<bool> config_constDistance;
        internal static ConfigEntry<float> config_movementSpeed;
        internal static ConfigEntry<float> config_movementTime;

        internal static ConfigEntry<float> config_archaicBaseDelay;
        internal static ConfigEntry<float> config_archaicFinalDelay;
        
        void InitConfigValues()
        {
            config_overridePreviousNoises = Config.Bind(
                "Hearing",
                "overridePreviousNoises",
                true,
                "Makes Baldi no longer track previous sounds once a new sound is heard.");

            config_classicNoiseValues = Config.Bind(
                "Hearing",
                "classicNoiseValues",
                true,
                "Uses noise values equivalent to Classic. Do note that this will mostly override vanilla noises and mods might break.");

            config_disableBaldicator = Config.Bind(
                "Hearing",
                "noBaldicator",
                false,
                "Disables the Baldicator that appears for every sound Baldi hears. Only turn this on if you know what you're doing.");


            config_classicJumpscare = Config.Bind(
                "Jumpscare",
                "enabled",
                true,
                "Replaces the jumpscare noise when Baldi catches you to use the spammed Classic buzz sound.\nNote: If the BUZZ noise is spammed instead, then an error has occurred when loading the sound.");
            config_baldiSpecificJumpscare = Config.Bind(
                "Jumpscare",
                "baldiOnly",
                true,
                "Blacklists NPCs that may inherit Baldi's from using the Classic jumpscare to keep compatibility with modded Baldis.");

            config_speedMode = Config.Bind(
                "Anger",
                "speedMode",
                BaldiSpeedMode.Default,
                "The slapping mode Baldi is in, impacting how his slap frequencies work.\nDefault -> Default/vanilla slap curves\nClassic -> Use equation used in Classic and earlier BB+ releases/demos\nArchaic -> Linear delay reduction, akin to Classic 1.2 and lower");

            config_constDistance = Config.Bind(
                "Anger.Movement",
                "constantDistance",
                true,
                "Makes Baldi's slapping distance independent from his current anger. Ideal when using non-default speed modes as Baldi's distance is normally adjusted to stick with his intended speed.");
            config_movementSpeed = Config.Bind(
                "Anger.Movement",
                "speed",
                112F,
                "Baldi's general movement speed when slapping. Default value is on par with Classic in comparison to the player's running speed.");
            config_movementTime = Config.Bind(
                "Anger.Movement",
                "time",
                0.218F,
                "The movement time Baldi gets when slapping. Default value is equivalent to 10 fixed update ticks in Classic.");

            config_archaicBaseDelay = Config.Bind(
                "Anger.Archaic",
                "baseDelay",
                3F,
                "The base delay.");
            config_archaicFinalDelay = Config.Bind(
                "Anger.Archaic",
                "finalDelay",
                0.66F,
                "The delay when all notebooks are collected. Default value is equivalent to the average speed when all notebooks are collected in Classic 1.2.\nIf you want the speed to be equivalent to 1.0, use 0.57");

        }

        internal static SoundObject audBuzz;

        void LoadAssets()
        {
            // Copy the sound Baldi makes on loss just so there's a fallback if the custom sound is not loaded
            audBuzz = Instantiate(Resources.FindObjectsOfTypeAll<Baldi>().Where((Baldi x) => x.Character == Character.Baldi).First().loseSounds[0].selection);
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
        }
    }

    internal enum BaldiSpeedMode
    {
        Default,
        Classic, // Classic 1.3 - Early BB+
        Archaic // Classic 1.2 - 1.0
    }
}