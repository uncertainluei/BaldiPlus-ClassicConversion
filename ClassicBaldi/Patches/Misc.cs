using HarmonyLib;
using MTM101BaldAPI;
using System.Collections;
using UnityEngine;

namespace LuisRandomness.BBPClassicBaldi.Patches
{
    [ConditionalPatchConfig(ClassicBaldiPlugin.ModGuid, "Misc", "NoDoorUnlock")]
    [HarmonyPatch(typeof(Baldi_StateBase))]
    [HarmonyPatch("DoorHit")]
    public class BaldiDoorHitPatch
    {
        private static bool Prefix(StandardDoor door)
        {
            if (!door.locked) return true;

            // Just go through the door
            return false;
        }
    }

    [ConditionalPatchConfig(ClassicBaldiPlugin.ModGuid, "Misc", "InstantCountdown")]
    [HarmonyPatch(typeof(HappyBaldi))]
    [HarmonyPatch("Activate")]
    public class HappyBaldiActivatePatch
    {
        private static bool Prefix(HappyBaldi __instance, ref bool ___activated, AudioManager ___audMan, SoundObject ___audHere, SpriteRenderer ___sprite)
        {

            ___activated = true;
            ___audMan.FlushQueue(true);

            MusicManager.Instance.StopMidi();
            MusicManager.Instance.StopFile();

            ___audMan.pitchModifier = Random.Range(0.2f, 3f);
            ___audMan.QueueAudio(ClassicBaldiPlugin.audCountdown);
            ___audMan.QueueAudio(___audHere);

            BaseGameManager.Instance.BeginSpoopMode();
            __instance.Ec.SpawnNPCs();

            Baldi baldi = __instance.Ec.GetBaldi();
            if (baldi)
                switch (CoreGameManager.Instance.currentMode)
                {
                    case Mode.Main:
                        baldi.transform.position = __instance.transform.position;
                        break;
                    case Mode.Free:
                        baldi.Despawn();
                        break;
                }

            __instance.Ec.StartEventTimers();
            ___sprite.enabled = false;

            __instance.StartCoroutine(DestroyWait(__instance.gameObject, ___audMan, CoreGameManager.Instance));
            return false;
        }

        private static IEnumerator DestroyWait(GameObject happyBaldi, AudioManager audMan, CoreGameManager cgm)
        {
            yield return new WaitWhile(() => audMan.QueuedAudioIsPlaying || cgm.Paused);
            Object.Destroy(happyBaldi);
            yield break;
        }
    }

    [ConditionalPatchConfig(ClassicBaldiPlugin.ModGuid, "Jumpscare", "ClassicJumpscare")]
    [HarmonyPatch(typeof(CoreGameManager))]
    [HarmonyPatch("EndGame")]
    public class JumpscarePatch
    {
        private static void Postfix(CoreGameManager __instance, Baldi baldi)
        {
            // Skip if it's a character that inherits Baldi but it's NOT Baldi
            if (ClassicBaldiPlugin.config_baldiSpecificJumpscare.Value && !baldi.IsVanilla()) return;

            __instance.StartCoroutine(ClassicJumpscare(baldi.loseSounds[0].selection, __instance.GetCamera(0), __instance.audMan));
        }

        private static IEnumerator ClassicJumpscare(SoundObject snd, GameCamera cam, AudioManager audMan)
        {
            audMan.FlushQueue(true);
            audMan.volumeModifier = 1f;

            SubtitleManager.Instance.CreateSub(snd, audMan, audMan.sourceId, audMan.audioDevice.maxDistance, false, snd.color);

            while (cam.camCom.cullingMask != 0)
            {
                audMan.PlaySingle(ClassicBaldiPlugin.audBuzz);
                yield return null;
            }
            yield break;
        }
    }
}
