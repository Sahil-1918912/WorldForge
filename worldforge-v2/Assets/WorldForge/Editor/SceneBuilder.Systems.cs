using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace WorldForge.EditorTools
{
    // Sun, sky, ambient light, fog, interior lights, reflection probes, post-processing,
    // the player rig and the scene-level systems (HUD, tour, capture director).
    public static partial class SceneBuilder
    {
        static void BuildLighting(Transform lab)
        {
            var sunGo = new GameObject("Sun");
            sunGo.transform.rotation = Quaternion.Euler(48f, -35f, 0f);
            var sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.95f, 0.86f);
            sun.intensity = 1.7f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.9f;
            RenderSettings.sun = sun;

            var sky = new Material(Shader.Find("Skybox/Procedural")) { name = "ProceduralSky" };
            sky.SetFloat("_SunDisk", 2f);
            sky.SetFloat("_SunSize", 0.035f);
            sky.SetFloat("_SunSizeConvergence", 5f);
            sky.SetFloat("_AtmosphereThickness", 0.9f);
            sky.SetColor("_SkyTint", new Color(0.5f, 0.55f, 0.65f));
            sky.SetColor("_GroundColor", new Color(0.38f, 0.37f, 0.35f));
            sky.SetFloat("_Exposure", 1.25f);
            AssetDatabase.CreateAsset(sky, $"{MaterialFolder}/ProceduralSky.mat");
            RenderSettings.skybox = sky;

            // Trilight ambient needs no baking and is easy to blend for day / night.
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.52f, 0.60f, 0.72f);
            RenderSettings.ambientEquatorColor = new Color(0.42f, 0.42f, 0.40f);
            RenderSettings.ambientGroundColor = new Color(0.30f, 0.29f, 0.27f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 30f;
            RenderSettings.fogEndDistance = 170f;
            RenderSettings.fogColor = new Color(0.70f, 0.78f, 0.88f);

            var lighting = new LightingSettings { name = "WorldForgeLighting", bakedGI = false, realtimeGI = false };
            AssetDatabase.CreateAsset(lighting, $"{SettingsFolder}/WorldForgeLighting.lighting");
            Lightmapping.lightingSettings = lighting;

            // Ceiling light panels, each with a spot light below it.
            var ceiling = Group("CeilingLights", lab);
            foreach (float x in new[] { -6f, 0f, 6f })
            foreach (float z in new[] { -3.2f, 3.0f })
            {
                Box("Housing", ceiling, new Vector3(x, 3.97f, z), new Vector3(1.5f, 0.06f, 0.6f), mFrame, false);
                var panel = Box("Panel", ceiling, new Vector3(x, 3.935f, z), new Vector3(1.4f, 0.02f, 0.5f), mPanel, false);
                NoShadows(panel);
                ceilingPanels.Add(panel.GetComponent<Renderer>());
                var spot = AddLight("CeilingSpot", ceiling, new Vector3(x, 3.85f, z), LightType.Spot, new Color(1f, 0.95f, 0.88f), 9f, 9f,
                    LightShadows.Soft, new Vector3(90, 0, 0));
                spot.spotAngle = 130f;
                spot.innerSpotAngle = 60f;
                spot.shadowStrength = 0.7f;
                ceilingLights.Add(spot);
            }

            Probe("ReflectionProbe_Lab", new Vector3(0, 2f, 0), new Vector3(20.4f, 4.2f, 14.4f), 256, true);

            BuildPostProcessing();
            TunePipelineAsset();
        }

        static ReflectionProbe Probe(string name, Vector3 position, Vector3 size, int resolution, bool boxProjection)
        {
            var go = new GameObject(name);
            go.transform.position = position;
            var probe = go.AddComponent<ReflectionProbe>();
            probe.mode = ReflectionProbeMode.Realtime;
            probe.refreshMode = ReflectionProbeRefreshMode.OnAwake;
            probe.timeSlicingMode = ReflectionProbeTimeSlicingMode.AllFacesAtOnce;
            probe.size = size;
            probe.resolution = resolution;
            probe.boxProjection = boxProjection;
            probe.importance = boxProjection ? 2 : 1;
            probe.blendDistance = 1f;
            return probe;
        }

        static void BuildPostProcessing()
        {
            string path = $"{SettingsFolder}/WorldForgeVolume.asset";
            AssetDatabase.DeleteAsset(path);
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, path);

            var tonemapping = profile.Add<Tonemapping>(true);
            tonemapping.mode.Override(TonemappingMode.ACES);
            var bloom = profile.Add<Bloom>(true);
            bloom.threshold.Override(1.0f);
            bloom.intensity.Override(0.6f);
            bloom.scatter.Override(0.65f);
            var colour = profile.Add<ColorAdjustments>(true);
            colour.postExposure.Override(0.25f);
            colour.contrast.Override(12f);
            colour.saturation.Override(10f);
            var vignette = profile.Add<Vignette>(true);
            vignette.intensity.Override(0.22f);
            vignette.smoothness.Override(0.45f);
            foreach (var component in profile.components)
            {
                component.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
                AssetDatabase.AddObjectToAsset(component, profile);
            }
            EditorUtility.SetDirty(profile);

            var go = new GameObject("PostProcessing");
            var volume = go.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 1f;
            volume.sharedProfile = profile;
        }

        static void TunePipelineAsset()
        {
            // Allow more than four lights to touch one object (the lab has six ceiling spots).
            foreach (var guid in AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset", new[] { "Assets/Settings" }))
            {
                var asset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(AssetDatabase.GUIDToAssetPath(guid));
                var so = new SerializedObject(asset);
                var limit = so.FindProperty("m_AdditionalLightsPerObjectLimit");
                if (limit != null) limit.intValue = 8;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        static void BuildPlayerAndSystems()
        {
            // --- Player rig -----------------------------------------------------------------
            var player = new GameObject("Player");
            player.layer = IgnoreRaycastLayer;
            player.transform.SetPositionAndRotation(new Vector3(4.2f, 0.05f, -5.3f), Quaternion.Euler(0, -25f, 0));
            var cc = player.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.3f;
            cc.center = new Vector3(0, 0.9f, 0);
            cc.stepOffset = 0.35f;
            cc.slopeLimit = 50f;
            cc.skinWidth = 0.03f;

            // A small robot-like avatar, visible in third person.
            var avatar = Group("Avatar", player.transform);
            Primitive(PrimitiveType.Capsule, "Body", avatar, new Vector3(0, 0.75f, 0), new Vector3(0.55f, 0.72f, 0.42f), mWhitePlastic, false, Vector3.zero);
            Box("Chest", avatar, new Vector3(0, 1.05f, 0.18f), new Vector3(0.3f, 0.2f, 0.06f), mLedBlue, false);
            Box("Backpack", avatar, new Vector3(0, 1.0f, -0.24f), new Vector3(0.34f, 0.4f, 0.14f), mOrange, false);
            Ball("Head", avatar, new Vector3(0, 1.62f, 0), 0.34f, mWhitePlastic, false);
            Box("Visor", avatar, new Vector3(0, 1.64f, 0.14f), new Vector3(0.24f, 0.08f, 0.08f), mLedBlue, false);
            foreach (var t in avatar.GetComponentsInChildren<Transform>()) t.gameObject.layer = IgnoreRaycastLayer;

            var pivot = Group("CameraPivot", player.transform, new Vector3(0, 1.62f, 0));
            var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
            camGo.transform.SetParent(pivot, false);
            var cam = camGo.AddComponent<Camera>();
            cam.fieldOfView = 70f;
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 400f;
            camGo.AddComponent<AudioListener>();
            var camData = camGo.AddComponent<UniversalAdditionalCameraData>();
            camData.renderPostProcessing = true;
            camData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            camData.antialiasingQuality = AntialiasingQuality.High;

            var controller = player.AddComponent<PlayerController>();
            controller.cameraPivot = pivot;
            controller.playerCamera = cam;
            controller.avatarRenderers = avatar.GetComponentsInChildren<Renderer>();
            var interactor = player.AddComponent<PlayerInteractor>();
            interactor.controller = controller;

            // --- Wiring for environment controllers --------------------------------------
            lightSwitch.lights = ceilingLights.ToArray();
            lightSwitch.panels = ceilingPanels.ToArray();
            lightSwitch.panelEmission = new Color(3f, 2.9f, 2.7f);

            // --- Systems --------------------------------------------------------------------
            var systems = new GameObject("WorldForge");
            var game = systems.AddComponent<WorldForgeGame>();
            var hud = systems.AddComponent<HudOverlay>();
            var tour = systems.AddComponent<CameraTour>();
            systems.AddComponent<CaptureDirector>();

            hud.player = controller;
            hud.interactor = interactor;
            hud.tour = tour;

            tour.player = controller;
            tour.arm = arm;
            tour.conveyor = conveyor;
            tour.lights = lightSwitch;
            tour.shots = new[]
            {
                Shot(4.2f, 1.7f, -5.6f, 0f, 1.2f, 1f),
                Shot(0.5f, 2.1f, -2.2f, -4.5f, 1.2f, 0.5f),
                Shot(-1.8f, 1.7f, 2.9f, -4.8f, 1.0f, 0.8f),
                Shot(-4.0f, 1.9f, -2.4f, -5.5f, 0.9f, -5.3f),
                Shot(-0.3f, 1.8f, -1.9f, 1.2f, 0.95f, -3.4f),
                Shot(5.8f, 1.6f, -3.6f, 9.3f, 1.1f, -1.0f),
                Shot(4.0f, 1.7f, 3.5f, 4.0f, 1.4f, 12f),
                Shot(6.5f, 3.0f, 5.0f, -4.0f, 0.8f, -1.0f),
            };

            game.player = controller;
            game.interactor = interactor;
            game.tour = tour;
            game.hud = hud;
        }

        static CameraTour.Shot Shot(float x, float y, float z, float lx, float ly, float lz) =>
            new CameraTour.Shot { position = new Vector3(x, y, z), lookAt = new Vector3(lx, ly, lz) };
    }
}
