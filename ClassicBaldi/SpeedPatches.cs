using HarmonyLib;

namespace LuisRandomness.BBPClassicBaldi.Patches.Anger
{
    [HarmonyPatch(typeof(Baldi))]
    [HarmonyPatch("Delay", MethodType.Getter)]
    public class AdjustDelay
    {
        static bool Prefix(Baldi __instance, ref float __result, float ___anger, float ___extraAnger)
        {
            switch (ClassicBaldiPlugin.config_speedMode.Value)
            {
                case BaldiSpeedMode.Classic:
                    __result = -3f * ___anger / (___anger + 2f / __instance.slapSpeedScale) + 3f - ___extraAnger * 0.25f;
                    return false;
                      
                case BaldiSpeedMode.Archaic:
                    float b = ClassicBaldiPlugin.config_archaicBaseDelay.Value;

                    // x - (x-y)* (a+e)/(t+b)
                    __result = b - (b-ClassicBaldiPlugin.config_archaicFinalDelay.Value) *
                        (___anger + ___extraAnger) / (__instance.ec.notebookTotal + __instance.baseAnger);

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
        static bool Prefix(ref float __result)
        {
            if (ClassicBaldiPlugin.config_constDistance.Value)
            {
                __result = ClassicBaldiPlugin.config_movementSpeed.Value;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Baldi))]
    [HarmonyPatch("Slap")]
    public class ConstantDistance
    {
        static void Postfix(Baldi __instance, ref float ___slapDistance)
        {
            if (ClassicBaldiPlugin.config_constDistance.Value)
            {
                __instance.Navigator.SetSpeed(__instance.Speed);
                ___slapDistance = __instance.Speed * ClassicBaldiPlugin.config_movementTime.Value;
            }
        }
    }
}
