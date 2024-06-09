using UnityEngine;
using HarmonyLib;
using MTM101BaldAPI.Reflection;
using LuisRandomness.BBPClassicExits.Extensions;

namespace LuisRandomness.BBPClassicExits.Patches
{
    [HarmonyPatch(typeof(Elevator))]
    [HarmonyPatch("Initialize")]
    public class OnInitialize
    {
        static void Postfix(Elevator __instance, ref SoundObject ___audGateClose)
        {
            if (!ClassicExitManager.Instance)
            {
                GameObject bgm = BaseGameManager.Instance.gameObject;
                bgm.SetActive(false);
                bgm.AddComponent<ClassicExitManager>().destroyOnLoad = true;
                bgm.SetActive(true);
            }

            if (ClassicExitManager.Instance.gateMode > ClassicGateMode.Default)
                ___audGateClose = ClassicExitsPlugin.assetMan.Get<SoundObject>("Sfx_BigSwitch");

            ElevatorExtension.GetFrom(__instance);
        }
    }

    [HarmonyPatch(typeof(Elevator))]
    [HarmonyPatch("Close")]
    public class OnElevatorClose
    {
        static bool Prefix(Elevator __instance, ref bool ___open, SoundObject ___audGateClose, MapIcon ___mapIcon, Sprite ___lockedIconSprite)
        {
            if (ClassicExitManager.Instance.gateMode == ClassicGateMode.Default) return true;

            ___open = false;

            if (BaseGameManager.Instance.FoundNotebooks >= BaseGameManager.Instance.NotebookTotal)
                CoreGameManager.Instance.audMan.PlaySingle(___audGateClose);

            ElevatorExtension.GetFrom(__instance).SetWalls(false);

            ___mapIcon.spriteRenderer.sprite = ___lockedIconSprite;
            ___mapIcon.spriteRenderer.color = Color.red;

            return false;
        }
    }

    [HarmonyPatch(typeof(Elevator))]
    [HarmonyPatch("Open")]
    public class OnElevatorOpen
    {
        static bool Prefix(Elevator __instance, ref bool ___open, MeshCollider ___gateCollider, MapIcon ___mapIcon, Sprite ___openIconSprite)
        {
            ___mapIcon.spriteRenderer.sprite = ___openIconSprite;
            ___mapIcon.spriteRenderer.color = Color.green;

            if (ClassicExitManager.Instance.gateMode == ClassicGateMode.Default) return true;

            ___open = true;
            ___gateCollider.enabled = false;

            ElevatorExtension.GetFrom(__instance).SetWalls(true);
            return false;
        }
    }

    [HarmonyPatch(typeof(BaseGameManager))]
    [HarmonyPatch("ElevatorClosed")]
    public class OnElevatorReached
    {
        static void Postfix(BaseGameManager __instance, Elevator elevator, int ___elevatorsClosed, int ___elevatorsToClose)
        {
            ClassicExitManager cem = ClassicExitManager.Instance; // Shorthand

            if (___elevatorsClosed == 1)
            {
                //TODO: Tidy up this code ASAP!!                
                switch (cem.lightMode)
                {
                    case ClassicFinaleMode.Default:
                        break;
                    case ClassicFinaleMode.Remastered:
                        __instance.StartCoroutine(cem.RemasteredLights());
                        break;
                    case ClassicFinaleMode.Classic:
                        __instance.StartCoroutine(cem.ClassicRedLights(false));
                        break;
                    case ClassicFinaleMode.Archaic:
                        __instance.StartCoroutine(cem.ClassicRedLights(true));
                        break;
                }

                switch (cem.audioMode)
                {
                    case ClassicFinaleMode.Archaic:
                        MusicManager.Instance.StopMidi();
                        break;
                    case ClassicFinaleMode.Classic:
                        MusicManager.Instance.StopMidi();
                        MusicManager.Instance.StopFile();
                        MusicManager.Instance.QueueFile(ClassicExitsPlugin.assetMan.Get<LoopingSoundObject>("Loop_Classic_Quiet"), true);
                        break;
                    case ClassicFinaleMode.Remastered:
                        MusicManager.Instance.SetSpeed(0.1F);
                        MusicManager.Instance.SetLoop(false);
                        break;
                }
                return;
            }

            if (!cem.earlyLoopPlayed)
            {
                cem.earlyLoopPlayed = true;
                MusicManager.Instance.MidiPlayer.MPTK_Transpose = Random.Range(-24, -12);

                switch (cem.audioMode)
                {
                    case ClassicFinaleMode.Default:
                        break;
                    case ClassicFinaleMode.Remastered:
                        MusicManager.Instance.StopFile();
                        MusicManager.Instance.QueueFile(ClassicExitsPlugin.assetMan.Get<LoopingSoundObject>("Loop_Cr_Early"), true);
                        break;
                    default:
                        MusicManager.Instance.StopFile();
                        MusicManager.Instance.QueueFile(ClassicExitsPlugin.assetMan.Get<LoopingSoundObject>("Loop_Classic_Early"), true);
                        break;
                }
            }
            if (___elevatorsToClose == 0)
            {
                if (cem.lightMode == ClassicFinaleMode.Remastered)
                    cem.RemasteredFinal();

                switch (cem.audioMode)
                {
                    case ClassicFinaleMode.Default:
                        break;
                    case ClassicFinaleMode.Remastered:
                        MusicManager.Instance.QueueFile(ClassicExitsPlugin.assetMan.Get<LoopingSoundObject>("Loop_Cr_Buildup"), true);
                        break;
                    default:
                        MusicManager.Instance.StopFile();
                        MusicManager.Instance.QueueFile(ClassicExitsPlugin.assetMan.Get<LoopingSoundObject>("Loop_Classic_Buildup"), true);
                        break;
                }
            }
        }
    }

    [HarmonyPatch(typeof(BaseGameManager))]
    [HarmonyPatch("PrepareToLoad")]
    public class ListenToLoad
    {
        static void Prefix()
        {
            ClassicExitManager.Instance.Revert();
            MusicManager.Instance.StopFile();
        }
    }
}
