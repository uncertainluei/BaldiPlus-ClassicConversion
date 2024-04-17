using UnityEngine;
using HarmonyLib;
using System.Collections;
using System.Drawing;
using UnityEngine.Networking.Types;

namespace LuisRandomness.BBPClassicBaldi.Patches
{
    [HarmonyPatch(typeof(CoreGameManager))]
    [HarmonyPatch("EndGame")]
    public class JumpscarePatch
    {
        static void Postfix(CoreGameManager __instance, Baldi baldi)
        {
            // Skip if it's not enabled
            if (!ClassicBaldiPlugin.config_classicJumpscare.Value) return;

            // Skip if it's a character that inherits Baldi but it's NOT Baldi
            if (ClassicBaldiPlugin.config_baldiSpecificJumpscare.Value &&
                (baldi.Character != Character.Baldi || baldi.GetType() != typeof(Baldi))) return;

            __instance.StartCoroutine(ClassicJumpscare(baldi.loseSounds[0].selection, __instance.GetCamera(0), __instance.audMan));
        }

        static IEnumerator ClassicJumpscare(SoundObject snd, GameCamera cam, AudioManager audMan)
        {
            audMan.FlushQueue(true);
            audMan.volumeModifier = 1f;

            Singleton<SubtitleManager>.Instance.CreateSub(snd, audMan, audMan.sourceId, audMan.audioDevice.maxDistance, false, snd.color);

            while (cam.camCom.cullingMask != 0)
            {
                audMan.PlaySingle(ClassicBaldiPlugin.audBuzz);
                yield return null;
            }
            yield break;
        }
    }
}
