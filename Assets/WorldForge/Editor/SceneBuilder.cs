using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace WorldForge.EditorTools
{
    /// <summary>
    /// Generates the complete WorldForge scene (textures, materials, geometry, lights, player,
    /// interactions) from code and saves it to Assets/Scenes/WorldForge.unity.
    /// Menu: WorldForge > Build Scene.  Batch mode: -executeMethod WorldForge.EditorTools.SceneBuilder.Build
    /// The layout is split across SceneBuilder.Lab.cs, SceneBuilder.Garden.cs and SceneBuilder.Systems.cs.
    /// </summary>
    public static partial class SceneBuilder
    {
        public const string ScenePath = "Assets/Scenes/WorldForge.unity";
        const string MaterialFolder = "Assets/WorldForge/Materials";
        const string SettingsFolder = "Assets/WorldForge/Settings";
        const string SignFolder = "Assets/WorldForge/Textures/Signs";
        const int IgnoreRaycastLayer = 2;

        static Dictionary<string, Texture2D> tex;
        static Dictionary<string, Material> tiledCache;
        static Mesh torusMesh, coneMesh;
        static System.Random rng;

        // Scene parts that other sections need to wire up.
        static Transform interactive;
        static readonly List<Light> ceilingLights = new List<Light>();
        static readonly List<Renderer> ceilingPanels = new List<Renderer>();
        static readonly List<Light> lampLights = new List<Light>();
        static readonly List<Renderer> lampBulbs = new List<Renderer>();
        static readonly List<ReflectionProbe> probes = new List<ReflectionProbe>();
        static SlidingDoor door;
        static RobotArm arm;
        static ConveyorBelt conveyor;
        static LightSwitch lightSwitch;
        static DayNightController dayNight;

        [MenuItem("WorldForge/Build Scene", priority = 0)]
        static void BuildFromMenu()
        {
            if (EditorUtility.DisplayDialog("WorldForge", "Regenerate the WorldForge scene, textures and materials?\nAny manual edits to the scene will be replaced.", "Build", "Cancel"))
                Build();
        }

        [MenuItem("WorldForge/Open Scene", priority = 1)]
        static void OpenScene() => EditorSceneManager.OpenScene(ScenePath);

        public static void Build()
        {
            Reset();
            AssetDatabase.DeleteAsset(MaterialFolder);
            Directory.CreateDirectory(MaterialFolder);
            Directory.CreateDirectory(SettingsFolder);

            tex = TextureFactory.GenerateAll();
            LoadSignTextures();
            CreateMaterials();
            torusMesh = MeshFactory.Torus(0.62f, 0.012f);
            coneMesh = MeshFactory.Cone();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            rng = new System.Random(2026);

            var environment = new GameObject("Environment").transform;
            var lab = Group("Lab", environment);
            var garden = Group("Garden", environment);
            interactive = new GameObject("Interactive").transform;

            BuildLabShell(lab);
            BuildLabInterior(lab);
            BuildRobotCell(lab);
            BuildConveyor();
            BuildWorkstations(lab);
            BuildCentrePieces(lab);
            BuildGarden(garden);
            BuildLighting(lab);
            BuildPlayerAndSystems();

            MarkStatic(environment.gameObject);
            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.DeleteAsset("Assets/Scenes/SampleScene.unity");
            AssetDatabase.SaveAssets();
            Debug.Log($"[WorldForge] Scene built: {ScenePath}");
        }

        static void Reset()
        {
            tiledCache = new Dictionary<string, Material>();
            ceilingLights.Clear();
            ceilingPanels.Clear();
            lampLights.Clear();
            lampBulbs.Clear();
            probes.Clear();
        }

        static void LoadSignTextures()
        {
            foreach (var path in Directory.GetFiles(SignFolder, "*.png"))
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.anisoLevel = 8;
                importer.maxTextureSize = 2048;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.SaveAndReimport();
                tex[Path.GetFileNameWithoutExtension(path)] = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }
        }

        // ------------------------------------------------------------------ materials

        static Material mFloor, mConcrete, mCeiling, mBrick, mGlass, mFrame, mSteel, mMachine, mRoof;
        static Material mOrange, mJoint, mDarkPlastic, mWhitePlastic, mWood, mCrate, mCardboard, mFabric;
        static Material mTread, mHazard, mBelt, mPanel, mGrass, mPath, mBark, mLeaves, mLeavesDark, mLeavesLight;
        static Material mWater, mStone, mTerracotta, mSoil, mGlobe, mEmitter, mBulb, mServer, mStatus;
        static Material mRed, mYellow, mBlue, mPurple, mLedGreen, mLedBlue, mLedRed, mHill, mFenceWood;
        static Material mWhiteboard, mSolar, mScreen, mSpray;
        static Material mSignMain, mSignRobot, mSignConveyor, mSignWork, mSignGarden, mPoster;

        static void CreateMaterials()
        {
            mFloor = Mat("FloorTiles", Color.white, "FloorTiles", "FloorTiles_N", smoothness: 0.55f, normalScale: 0.6f);
            mConcrete = Mat("Concrete", new Color(0.93f, 0.93f, 0.92f), "Concrete", "Concrete_N", smoothness: 0.15f, normalScale: 0.5f);
            mCeiling = Mat("Ceiling", Color.white, "Concrete", smoothness: 0.1f);
            mBrick = Mat("Brick", Color.white, "Brick", "Brick_N", smoothness: 0.12f, normalScale: 0.8f);
            mGlass = Mat("Glass", new Color(0.70f, 0.85f, 0.92f, 0.16f), smoothness: 0.95f);
            mFrame = Mat("DarkMetal", new Color(0.16f, 0.17f, 0.19f), "Metal", "Metal_N", metallic: 0.8f, smoothness: 0.55f);
            mSteel = Mat("Steel", new Color(0.9f, 0.9f, 0.92f), "Metal", "Metal_N", metallic: 0.9f, smoothness: 0.7f);
            mMachine = Mat("MachinePaint", new Color(0.78f, 0.8f, 0.83f), "Metal", metallic: 0.3f, smoothness: 0.5f);
            mRoof = Mat("Roof", new Color(0.3f, 0.31f, 0.33f), "Metal", "Metal_N", metallic: 0.6f, smoothness: 0.4f);
            mOrange = Mat("RobotOrange", new Color(1f, 0.42f, 0.05f), metallic: 0.1f, smoothness: 0.6f);
            mJoint = Mat("JointGrey", new Color(0.22f, 0.23f, 0.25f), "Metal", metallic: 0.7f, smoothness: 0.6f);
            mDarkPlastic = Mat("DarkPlastic", new Color(0.07f, 0.075f, 0.085f), smoothness: 0.45f);
            mWhitePlastic = Mat("WhitePlastic", new Color(0.88f, 0.89f, 0.9f), smoothness: 0.55f);
            mWood = Mat("Wood", Color.white, "Wood", smoothness: 0.35f);
            mCrate = Mat("Crate", Color.white, "Crate", "Crate_N", smoothness: 0.25f);
            mCardboard = Mat("Cardboard", new Color(0.86f, 0.7f, 0.5f), "Crate", "Crate_N", smoothness: 0.15f);
            mFabric = Mat("Fabric", Color.white, "Fabric", smoothness: 0.1f);
            mTread = Mat("TreadPlate", Color.white, "TreadPlate", "TreadPlate_N", metallic: 0.8f, smoothness: 0.5f);
            mHazard = Mat("Hazard", Color.white, "Hazard", smoothness: 0.35f);
            mBelt = Mat("Belt", Color.white, "Belt", "Belt_N", smoothness: 0.3f);
            mPanel = Mat("LightPanel", Color.white, smoothness: 0.5f, emission: new Color(3f, 2.9f, 2.7f));
            mGrass = Mat("Grass", Color.white, "Grass", "Grass_N", smoothness: 0.08f);
            mPath = Mat("StonePath", Color.white, "StonePath", "StonePath_N", smoothness: 0.2f);
            mBark = Mat("Bark", Color.white, "Bark", "Bark_N", smoothness: 0.1f);
            mLeaves = Mat("Leaves", Color.white, "Leaves", "Leaves_N", smoothness: 0.25f);
            mLeavesDark = Mat("LeavesConifer", new Color(0.55f, 0.75f, 0.6f), "Leaves", "Leaves_N", smoothness: 0.2f);
            mLeavesLight = Mat("LeavesLight", new Color(1f, 0.92f, 0.6f), "Leaves", "Leaves_N", smoothness: 0.25f);
            mWater = Mat("Water", new Color(0.8f, 0.95f, 1f), "Water", "Water_N", metallic: 0.1f, smoothness: 0.95f);
            mStone = Mat("Stone", new Color(0.88f, 0.85f, 0.78f), "Concrete", "Concrete_N", smoothness: 0.2f);
            mTerracotta = Mat("Terracotta", new Color(0.66f, 0.32f, 0.18f), smoothness: 0.25f);
            mSoil = Mat("Soil", new Color(0.18f, 0.12f, 0.08f), smoothness: 0.05f);
            mGlobe = Mat("HoloGlobe", new Color(0.3f, 0.85f, 1f, 0.2f), "Globe", smoothness: 0.9f, emission: Color.white * 2f, emissionMap: "Globe");
            mEmitter = Mat("HoloEmitter", new Color(0.1f, 0.45f, 0.6f), smoothness: 0.8f, emission: new Color(0.3f, 1.4f, 2.2f));
            mBulb = Mat("LampBulb", new Color(1f, 0.95f, 0.85f), smoothness: 0.8f, emission: new Color(0.01f, 0.01f, 0.01f));
            mServer = Mat("ServerFront", Color.white, "ServerFront", smoothness: 0.5f, emission: Color.white * 2f, emissionMap: "ServerFront_E");
            mStatus = Mat("StatusLamp", Color.white, smoothness: 0.8f, emission: new Color(1f, 0.6f, 0.1f));
            mRed = Mat("RedPaint", new Color(0.8f, 0.08f, 0.06f), smoothness: 0.55f);
            mYellow = Mat("YellowPaint", new Color(0.95f, 0.75f, 0.1f), smoothness: 0.5f);
            mBlue = Mat("BluePlastic", new Color(0.12f, 0.35f, 0.75f), smoothness: 0.5f);
            mPurple = Mat("PurplePetal", new Color(0.55f, 0.25f, 0.8f), smoothness: 0.3f);
            mLedGreen = Mat("LedGreen", new Color(0.2f, 1f, 0.3f), smoothness: 0.8f, emission: new Color(0.4f, 3f, 0.6f));
            mLedBlue = Mat("LedBlue", new Color(0.3f, 0.6f, 1f), smoothness: 0.8f, emission: new Color(0.6f, 1.5f, 3f));
            mLedRed = Mat("LedRed", new Color(1f, 0.15f, 0.1f), smoothness: 0.8f, emission: new Color(3f, 0.3f, 0.2f));
            mHill = Mat("Hills", new Color(0.62f, 0.75f, 0.6f), "Grass", smoothness: 0.05f, tiling: new Vector2(12, 12));
            mFenceWood = Mat("FenceWood", new Color(0.7f, 0.6f, 0.5f), "Wood", smoothness: 0.2f);
            mWhiteboard = Mat("Whiteboard", Color.white, "Whiteboard", smoothness: 0.85f);
            mSolar = Mat("SolarPanel", new Color(0.08f, 0.15f, 0.4f), "Metal", metallic: 0.6f, smoothness: 0.85f);
            mScreen = Mat("Screen", new Color(0.15f, 0.15f, 0.15f), "Screen_Code", smoothness: 0.9f, emission: Color.white * 1.4f, emissionMap: "Screen_Code");
            mSignMain = Sign("Sign_Main", 0.6f);
            mSignRobot = Sign("Sign_RobotCell", 0.3f);
            mSignConveyor = Sign("Sign_Conveyor", 0.35f);
            mSignWork = Sign("Sign_Workstations", 0.35f);
            mSignGarden = Sign("Sign_Garden", 0.15f);
            mPoster = Sign("Poster_Safety", 0.15f);
            mSpray = ParticleMaterial();
        }

        static Material Sign(string texture, float glow) =>
            Mat(texture, Color.white, texture, smoothness: 0.4f, emission: Color.white * glow, emissionMap: texture);

        static Material Mat(string name, Color color, string albedo = null, string normal = null,
            float metallic = 0f, float smoothness = 0.35f, Color? emission = null, string emissionMap = null,
            float normalScale = 1f, Vector2? tiling = null)
        {
            var m = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name };
            m.SetColor("_BaseColor", color);
            if (albedo != null) m.SetTexture("_BaseMap", tex[albedo]);
            m.SetTextureScale("_BaseMap", tiling ?? Vector2.one);
            if (normal != null)
            {
                m.SetTexture("_BumpMap", tex[normal]);
                m.SetFloat("_BumpScale", normalScale);
            }
            m.SetFloat("_Metallic", metallic);
            m.SetFloat("_Smoothness", smoothness);
            if (emission.HasValue)
            {
                m.SetColor("_EmissionColor", emission.Value);
                if (emissionMap != null) m.SetTexture("_EmissionMap", tex[emissionMap]);
                m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            if (color.a < 1f)
            {
                m.SetFloat("_Surface", 1f); // transparent
                m.SetFloat("_Blend", 0f);   // alpha blend
            }
            BaseShaderGUI.SetMaterialKeywords(m);
            if (emission.HasValue) m.EnableKeyword("_EMISSION");
            AssetDatabase.CreateAsset(m, $"{MaterialFolder}/{name}.mat");
            return m;
        }

        static Material ParticleMaterial()
        {
            var m = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit")) { name = "FountainSpray" };
            m.SetFloat("_Surface", 1f);
            m.SetFloat("_Blend", 0f);
            m.SetTexture("_BaseMap", tex["SoftDot"]);
            m.SetColor("_BaseColor", new Color(0.8f, 0.92f, 1f, 0.55f));
            BaseShaderGUI.SetMaterialKeywords(m, null, UnityEditor.Rendering.Universal.ShaderGUI.ParticleGUI.SetMaterialKeywords);
            AssetDatabase.CreateAsset(m, $"{MaterialFolder}/FountainSpray.mat");
            return m;
        }

        /// <summary>Copy of a material with different texture tiling, so textures keep a real-world scale.</summary>
        static Material Tiled(Material source, float x, float y)
        {
            string name = $"{source.name}_{x:0.#}x{y:0.#}";
            if (tiledCache.TryGetValue(name, out var cached)) return cached;
            var m = new Material(source) { name = name };
            m.SetTextureScale("_BaseMap", new Vector2(x, y));
            AssetDatabase.CreateAsset(m, $"{MaterialFolder}/{name}.mat");
            tiledCache[name] = m;
            return m;
        }

        // ------------------------------------------------------------------ geometry helpers

        static Transform Group(string name, Transform parent, Vector3 position = default, float yaw = 0f)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            return go.transform;
        }

        static GameObject Primitive(PrimitiveType type, string name, Transform parent, Vector3 position, Vector3 scale,
            Material material, bool collider, Vector3 euler)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localRotation = Quaternion.Euler(euler);
            go.transform.localScale = scale;
            go.GetComponent<MeshRenderer>().sharedMaterial = material;

            var col = go.GetComponent<Collider>();
            if (!collider || type == PrimitiveType.Cylinder) Object.DestroyImmediate(col);
            // The default capsule collider is a poor fit for flat cylinders, so use a convex hull instead.
            if (collider && type == PrimitiveType.Cylinder) go.AddComponent<MeshCollider>().convex = true;
            return go;
        }

        static GameObject Box(string name, Transform parent, Vector3 position, Vector3 size, Material material,
            bool collider = true, Vector3 euler = default) =>
            Primitive(PrimitiveType.Cube, name, parent, position, size, material, collider, euler);

        /// <summary>Cylinder by radius and height; the axis is local Y unless rotated.</summary>
        static GameObject Cyl(string name, Transform parent, Vector3 position, float radius, float height, Material material,
            bool collider = true, Vector3 euler = default) =>
            Primitive(PrimitiveType.Cylinder, name, parent, position, new Vector3(radius * 2f, height * 0.5f, radius * 2f), material, collider, euler);

        static GameObject Ball(string name, Transform parent, Vector3 position, Vector3 diameter, Material material, bool collider = true) =>
            Primitive(PrimitiveType.Sphere, name, parent, position, diameter, material, collider, Vector3.zero);

        static GameObject Ball(string name, Transform parent, Vector3 position, float diameter, Material material, bool collider = true) =>
            Ball(name, parent, position, Vector3.one * diameter, material, collider);

        /// <summary>A quad whose visible side faces <paramref name="facing"/> (in the parent's space).</summary>
        static GameObject Quad(string name, Transform parent, Vector3 position, Vector2 size, Material material, Vector3 facing)
        {
            var go = Primitive(PrimitiveType.Quad, name, parent, position, new Vector3(size.x, size.y, 1f), material, false, Vector3.zero);
            go.transform.localRotation = Quaternion.LookRotation(-facing, Vector3.up);
            return go;
        }

        static GameObject MeshObject(string name, Transform parent, Mesh mesh, Vector3 position, Vector3 scale, Material material,
            bool convexCollider = false, Vector3 euler = default)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localRotation = Quaternion.Euler(euler);
            go.transform.localScale = scale;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = material;
            if (convexCollider)
            {
                var mc = go.AddComponent<MeshCollider>();
                mc.sharedMesh = mesh;
                mc.convex = true;
            }
            return go;
        }

        static Light AddLight(string name, Transform parent, Vector3 position, LightType type, Color color, float intensity,
            float range, LightShadows shadows = LightShadows.None, Vector3 euler = default)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localRotation = Quaternion.Euler(euler);
            var light = go.AddComponent<Light>();
            light.type = type;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.shadows = shadows;
            return light;
        }

        static T AddRigidbody<T>(GameObject go, float mass) where T : Component
        {
            var rb = go.AddComponent<Rigidbody>();
            rb.mass = mass;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            return go.AddComponent<T>();
        }

        static void NoShadows(GameObject go)
        {
            foreach (var r in go.GetComponentsInChildren<Renderer>())
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        static float Range(float min, float max) => min + (float)rng.NextDouble() * (max - min);

        static void MarkStatic(GameObject root)
        {
            const StaticEditorFlags flags = StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccluderStatic |
                                            StaticEditorFlags.OccludeeStatic | StaticEditorFlags.ReflectionProbeStatic;
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
                GameObjectUtility.SetStaticEditorFlags(t.gameObject, flags);
        }
    }
}
