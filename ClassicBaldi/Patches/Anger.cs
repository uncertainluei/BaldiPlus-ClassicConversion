using HarmonyLib;
using UnityEngine;

namespace LuisRandomness.BBPClassicBaldi.Patches.Anger
{
    [HarmonyPatch(typeof(Baldi))]
    [HarmonyPatch("Delay", MethodType.Getter)]
    public class AdjustDelay
    {
        private static bool Prefix(Baldi __instance, ref float __result, float ___anger, float ___extraAnger)
        {
            if (ClassicBaldiPlugin.config_baldiSpecificMovement.Value && !__instance.IsVanilla())
                return true;

            switch (ClassicBaldiPlugin.config_speedMode.Value)
            {
                case BaldiSpeedMode.Classic:
                    __result = -3f * ___anger / (___anger + 2f / __instance.slapSpeedScale) + 3f - ___extraAnger * 0.25f;
                    return false;

                case BaldiSpeedMode.Linear:
                    float b = ClassicBaldiPlugin.config_linearBaseDelay.Value;

                    // x - (x-y)* (a+e)/(t+b)
                    __result = b - (b - ClassicBaldiPlugin.config_linearFinalDelay.Value) *
                        (___anger + ___extraAnger) / (Mathf.Max(ClassicBaldiPlugin.config_linearMinTotal.Value, __instance.ec.notebookTotal) + __instance.baseAnger);

                    return false;
                default:
                    return true;
            }
        }
    }

    [HarmonyPatch(typeof(Baldi))]
    [HarmonyPatch("Speed", MethodType.Getter)]
    public class ConstantSpeed
    {
        private static bool Prefix(Baldi __instance, ref float __result)
        {
            if (!ClassicBaldiPlugin.config_constDistance.Value || ClassicBaldiPlugin.config_baldiSpecificMovement.Value && !__instance.IsVanilla())
                return true;

            __result = ClassicBaldiPlugin.config_movementSpeed.Value;
            return false;
        }
    }

    [HarmonyPatch(typeof(Baldi))]
    [HarmonyPatch("Slap")]
    public class ConstantDistance
    {
        private static void Postfix(Baldi __instance, ref float ___slapDistance)
        {
            if (ClassicBaldiPlugin.config_constDistance.Value && (!ClassicBaldiPlugin.config_baldiSpecificMovement.Value || __instance.IsVanilla()))
            {
                __instance.Navigator.SetSpeed(__instance.Speed);
                ___slapDistance = __instance.Speed * ClassicBaldiPlugin.config_movementTime.Value;
            }
        }
    }
}
