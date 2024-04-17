using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LuisRandomness.BBPClassicExits.Extensions
{
    public class ClassicExitManager : Singleton<ClassicExitManager>
    {
        private Fog fog;
        private EnvironmentController ec;

        private IntVector2 _updatePos;

        private Color lightColor;
        private bool redLightsRunning;

        public bool earlyLoopPlayed;

        private bool[,] originalLightStates = new bool[0, 0];
        private Color[,] originalLightColors = new Color[0, 0];

        private Color standardDarkLevel;
        private Color skyboxColor;
        
        private bool flickering;

        public ClassicGateMode gateMode { get; private set; }
        public ClassicFinaleMode lightMode {get; private set; }
        public ClassicFinaleMode audioMode { get; private set; }

        protected override void AwakeFunction()
        {
            fog = ClassicExitsPlugin.assetMan.Get<Fog>("ArchaicFog");
            ec = BaseGameManager.Instance.Ec;

            earlyLoopPlayed = false;

            originalLightStates = new bool[ec.levelSize.x, ec.levelSize.z];
            originalLightColors = new Color[ec.levelSize.x, ec.levelSize.z];

            gateMode = ClassicExitsPlugin.config_gateMode.Value;
            lightMode = ClassicExitsPlugin.config_lightMode.Value;
            audioMode = ClassicExitsPlugin.config_audioMode.Value;

            lightColor = ClassicExitsPlugin.config_lightColor.Value;
            fog.color = ClassicExitsPlugin.config_fogColor.Value;
        }

        public void Revert()
        {
            redLightsRunning = false;
        }

        public IEnumerator RemasteredLights()
        {
            redLightsRunning = true;

            skyboxColor = Shader.GetGlobalColor("_SkyboxColor");
            Shader.SetGlobalColor("_SkyboxColor", lightColor);

            List<Cell> lights = ec.lights;
            float time = 0.2F;

            while (lights.Count > 0)
            {
                if (!redLightsRunning)
                    break;

                if (time > 0f)
                {
                    time -= Time.deltaTime;
                    yield return null;
                    continue;
                }

                time = 0.2F;
                int num = UnityEngine.Random.Range(0, lights.Count);

                originalLightColors[lights[num].position.x, lights[num].position.z] = lights[num].lightColor;
                originalLightStates[lights[num].position.x, lights[num].position.z] = lights[num].lightOn;

                lights[num].lightColor = lightColor;
                ec.SetLight(true, lights[num]);
                lights.RemoveAt(num);
                yield return null;
            }

            yield return new WaitWhile(() => redLightsRunning);

            foreach (Cell light in ec.lights)
            {
                light.lightColor = originalLightColors[light.position.x, light.position.z];
                ec.SetLight(originalLightStates[light.position.x, light.position.z], light);
            }

            Shader.SetGlobalColor("_SkyboxColor", skyboxColor);
            ec.standardDarkLevel = standardDarkLevel;
            ec.FlickerLights(flickering);
            yield break;
        }

        public void RemasteredFinal()
        {
            flickering = ec.lightsToFlicker.Count > 0;
            standardDarkLevel = ec.standardDarkLevel;

            if (redLightsRunning
                && !PlayerFileManager.Instance.reduceFlashing)
            {
                ec.FlickerLights(true);
                ec.standardDarkLevel = lightColor * 0.2F;
            }
        }

        public IEnumerator ClassicRedLights(bool enableFog)
        {
            redLightsRunning = true;

            skyboxColor = Shader.GetGlobalColor("_SkyboxColor");
            Shader.SetGlobalColor("_SkyboxColor", lightColor);

            bool overridden = ec.lightingOverride;
            ec.lightingOverride = true;

            if (enableFog)
                ec.AddFog(fog);

            while (redLightsRunning && ec)
            {
                for (int i = 0; i < ec.levelSize.x; i++)
                {
                    for (int j = 0; j < ec.levelSize.z; j++)
                    {
                        _updatePos.x = i;
                        _updatePos.z = j;
                        CoreGameManager.Instance.UpdateLighting(lightColor, _updatePos);
                    }
                }
                yield return new WaitForEndOfFrame();
            }

            ec.lightingOverride = overridden;
            ec.InitializeLighting();

            if (enableFog)
                ec.RemoveFog(fog);

            Shader.SetGlobalColor("_SkyboxColor", skyboxColor);
            yield break;
        }
    }

    public class ElevatorExtension : MonoBehaviour
    {
        static Dictionary<Elevator, ElevatorExtension> exts = new Dictionary<Elevator, ElevatorExtension>();

        private Elevator elevator;
        private EnvironmentController ec;

        public Direction direction { get; private set; }
        private IntVector2 right;

        public IntVector2 gridPosition { get; private set; }

        private void Awake()
        {
            ec = BaseGameManager.Instance.Ec;

            gridPosition = IntVector2.GetGridPosition(transform.position);
            direction = Directions.DirFromVector3(transform.forward, 45f);
            right = Directions.ToIntVector2((Direction)(((int)direction + 1) % 4));
        }

        public void SetWalls(bool open)
        {
            IntVector2 pos = gridPosition - right + Directions.ToIntVector2(direction);
            Direction opp = direction.GetOpposite();
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    if (open)
                        ec.ConnectCells(pos, opp);
                    else
                        ec.CloseCell(pos, opp);
                }
                catch (Exception e)
                {
                    Debug.LogError("Hall could not be changed properly!");
                    Debug.LogException(e);
                }
                pos += right;
            }
        }

        private void OnDisable()
        {
            exts.Remove(elevator);
        }

        public static ElevatorExtension GetFrom(Elevator e)
        {
            if (!exts.ContainsKey(e))
            {
                ElevatorExtension ext = e.gameObject.AddComponent<ElevatorExtension>();
                ext.elevator = e;

                exts.Add(e, ext);
                return ext;
            }

            return exts[e];
        }
    }
}