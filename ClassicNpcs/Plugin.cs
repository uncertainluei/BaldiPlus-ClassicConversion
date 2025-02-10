using System;
using BepInEx;
using HarmonyLib;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI;
using BepInEx.Configuration;
using System.Reflection;

namespace LuisRandomness.BBPClassicNpcs
{
    [BepInPlugin(ModGuid, "Classic Conversion: NPCs", ModVersion)]
    [BepInDependency("mtm101.rulerp.bbplus.baldidevapi", BepInDependency.DependencyFlags.HardDependency)]
    public class ClassicNpcsPlugin : BaseUnityPlugin
    {
        public const string ModGuid = "io.github.uncertainluei.baldiplus.classicconversion.npcs";
        public const string ModVersion = "2024.1.0.0";

        internal static AssetManager assetMan = new AssetManager();

        private void Awake()
        {
            InitConfigValues();

            new Harmony(ModGuid).PatchAllConditionals();
        }

        private void InitConfigValues()
        {
        }
    }
}
