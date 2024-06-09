using BepInEx;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.Registers;
using UnityEngine;
using BepInEx.Configuration;

namespace LegacyPosterConversion
{
    [BepInPlugin(ModGuid, "Legacy Poster Conversion", ModVersion)]
    [BepInDependency("mtm101.rulerp.bbplus.baldidevapi", BepInDependency.DependencyFlags.HardDependency)]
    public class PosterConversionPlugin : BaseUnityPlugin
    {
        public const string ModGuid = "io.github.luisrandomness.bbp_legacy_poster_conversion";
        public const string ModVersion = "2024.1.0.0";

        void Awake()
        {
            LoadingEvents.RegisterOnAssetsLoaded(OnAssetsLoaded, false);
        }

        void OnAssetsLoaded()
        {
            Material[] mats;

            Resources.FindObjectsOfTypeAll<PosterObject>().Do((PosterObject x) =>
            {
                mats = x.material;
                if (!x.name.Contains("_reflex") && mats?.Length > 0)
                {
                    x.baseTexture = (Texture2D)mats[0].mainTexture;
                    x.textData = new PosterTextData[0];
                    x.MarkAsNeverUnload();
                }
            });
        }
    }
}
