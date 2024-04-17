using UnityEngine;
using HarmonyLib;

namespace LuisRandomness.BBPClassicBaldi.Patches.Hearing
{
    [HarmonyPatch(typeof(Baldi))]
    [HarmonyPatch("Hear")]
    public class BaldiHearPatch
    {
        static void Prefix(Vector3[] ___soundLocations, ref int ___currentSoundVal, int value, ref bool indicator)
        {
            if (ClassicBaldiPlugin.config_disableBaldicator.Value)
                indicator = false;

            if (!ClassicBaldiPlugin.config_overridePreviousNoises.Value) return;

            // Bump up the current sound value
            if (value > ___currentSoundVal || value == 127)
                ___currentSoundVal = value;

            // Clear all previously cached noises, ONLY focus on the highest priority noise
            for (int i = 0; i < ___currentSoundVal && i < 127; i++)
                if (___soundLocations[i] != Vector3.zero)
                    ___soundLocations[i] = Vector3.zero;
        }
    }

    [HarmonyPatch(typeof(EnvironmentController))]
    [HarmonyPatch("MakeNoise")]
    public class NoiseHandlingPatch
    {
        internal static int overrideValue = -1;

        static bool Prefix(ref int value)
        {
            if (overrideValue >= 0)
                value = overrideValue;
            overrideValue = -1;

            // Cancel if there is any noise with values 0 or lower is made
            if (value <= 0) return false;

            // Run script as normal if Classic Noises are disabled or the value is either 1 or 127
            if (!ClassicBaldiPlugin.config_classicNoiseValues.Value || value == 1 || value == 127)
                return true;

            /* Simplifies all noises into 3 tiers:
             * 1 (1-31) - Standard/Swinging Doors
             * 2 (32-79) - Chalkles, Arts and Crafters, 1st Prize, Blue Locker, Bad Math
             * 3 (80-126) - Elevator, Detention, Alarm Clock, Window, Mrs. Pomp
             * This is done to mostly play nicely with other mods.
             */
            if (value > 79)
                value = 3;
            else
                value = value > 31 ? 2 : 1;

            return true;
        }
    }

    // Cannot override normally... Can guarantee incompatibilites
    [HarmonyPatch(typeof(Elevator))]
    [HarmonyPatch("Close")]
    public class ElevatorCloseChange
    {
        static void Prefix()
        {
            if (ClassicBaldiPlugin.config_classicNoiseValues.Value)
                NoiseHandlingPatch.overrideValue = 126;
        }
    }

    [HarmonyPatch(typeof(MathMachine), "Start")]
    public class MathMachineInit
    {
        static void Prefix(int ___wrongNoiseVal) {
            // Classic noise values
            if (ClassicBaldiPlugin.config_classicNoiseValues.Value && ___wrongNoiseVal < 80)
                ___wrongNoiseVal = 80;
        }
    }
}
