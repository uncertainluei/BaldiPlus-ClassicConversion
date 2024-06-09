using BaldiEndless;
using HarmonyLib;
using MTM101BaldAPI;

using UnityEngine;
using UnityEngine.Assertions.Must;

namespace LuisRandomness.BBPClassicBaldi.Patches.Anger
{
    [ConditionalPatchMod("mtm101.rulerp.baldiplus.endlessfloors")]
    [ConditionalPatchConfig(ClassicBaldiPlugin.ModGuid, "Anger", "RemoveArcadeStretch")]
    [HarmonyPatch(typeof(BaseGameManager))]
    [HarmonyPatch("CollectNotebooks")]
    public class RevertArcadeAngerStretch
    {

        [HarmonyAfter("mtm101.rulerp.baldiplus.endlessfloors")]
        private static void Postfix(BaseGameManager __instance, int count, float ___notebookAngerVal)
        {
            if (count != 0)
            {
                // Reverse the patch that's done by Endless Floors
                float standardCount = (EndlessFloorsPlugin.currentFloorData.myFloorBaldi == 1) ? 4F : (EndlessFloorsPlugin.currentFloorData.myFloorBaldi == 2) ? 7F : 9F;

                if (__instance.NotebookTotal > standardCount)
                {
                    __instance.AngerBaldi(count * ___notebookAngerVal);
                    float stretchedTotal = -standardCount / __instance.NotebookTotal;
                    __instance.AngerBaldi(count * ___notebookAngerVal * stretchedTotal);
                }
            }
        }
    }
}
