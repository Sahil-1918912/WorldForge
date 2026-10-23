using System.Collections.Generic;
using UnityEngine;

namespace WorldForge.EditorTools
{
    // The "Innovation Garden" north of the lab: a stone path from the door to a fountain plaza,
    // trees, benches, lamp posts, a patrolling drone, a fence and distant hills.
    public static partial class SceneBuilder
    {
        static readonly Vector3 Fountain = new Vector3(6f, 0f, 18.5f);

        static void BuildGarden(Transform garden)
        {
            Box("Ground", garden, new Vector3(0, -0.11f, 9f), new Vector3(80f, 0.2f, 80f), Tiled(mGrass, 40, 40));
            Box("Path", garden, new Vector3(6f, 0f, 10.85f), new Vector3(1.8f, 0.04f, 7.1f), Tiled(mPath, 1.5f, 6f));
            Cyl("Plaza", garden, Fountain, 4.2f, 0.04f, Tiled(mPath, 5f, 5f));

            BuildFountain(garden);
            Bench(garden, Fountain + new Vector3(-3.0f, 0, 0), 90f);
            Bench(garden, Fountain + new Vector3(3.0f, 0, 0), -90f);
            Bench(garden, Fountain + new Vector3(0, 0, 3.0f), 180f);

            foreach (var p in new[] { new Vector3(4.6f, 0, 9f), new Vector3(7.4f, 0, 12.5f), new Vector3(2.6f, 0, 16f), new Vector3(9.4f, 0, 21f), new Vector3(2.6f, 0, 21f) })
                LampPost(garden, p);

            // Signpost beside the path
            var sign = Group("GardenSign", garden, new Vector3(8.2f, 0, 9.8f));
            Cyl("PostL", sign, new Vector3(-0.7f, 0.8f, 0), 0.05f, 1.6f, mFenceWood);
            Cyl("PostR", sign, new Vector3(0.7f, 0.8f, 0), 0.05f, 1.6f, mFenceWood);
            Box("Board", sign, new Vector3(0, 1.35f, 0), new Vector3(1.6f, 0.42f, 0.05f), mFenceWood);
            Quad("Face", sign, new Vector3(0, 1.35f, -0.027f), new Vector2(1.56f, 0.39f), mSignGarden, Vector3.back);

            PlantTrees(garden);
            Bushes(garden);
            foreach (var p in new[] { new Vector3(4.0f, 0, 12f), new Vector3(8.0f, 0, 10.6f), new Vector3(1.4f, 0, 19.5f), new Vector3(10.6f, 0, 17.5f), new Vector3(6f, 0, 23.3f) })
                FlowerBed(garden, p);

            Fence(garden, -18f, 18f, -14f, 32f);
            Hills(garden);
            Drone();
        }

        static void BuildFountain(Transform garden)
        {
            var f = Group("Fountain", garden, Fountain);
            Cyl("Basin", f, new Vector3(0, 0.275f, 0), 1.8f, 0.55f, mStone);
            Cyl("Water", f, new Vector3(0, 0.555f, 0), 1.62f, 0.02f, mWater, false);
            Cyl("Column", f, new Vector3(0, 0.95f, 0), 0.22f, 0.8f, mStone);
            Cyl("UpperBowl", f, new Vector3(0, 1.38f, 0), 0.7f, 0.14f, mStone);
            Cyl("UpperWater", f, new Vector3(0, 1.455f, 0), 0.62f, 0.02f, mWater, false);
            Ball("Finial", f, new Vector3(0, 1.55f, 0), 0.22f, mStone, false);

            var sprayGo = new GameObject("Spray");
            sprayGo.transform.SetParent(f, false);
            sprayGo.transform.localPosition = new Vector3(0, 1.62f, 0);
            sprayGo.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            var ps = sprayGo.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 1.1f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(2.2f, 2.8f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.13f);
            main.gravityModifier = 0.9f;
            main.maxParticles = 1200;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = ps.emission;
            emission.rateOverTime = 260f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 14f;
            shape.radius = 0.04f;
            var renderer = sprayGo.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = mSpray;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        static void Bench(Transform parent, Vector3 position, float yaw)
        {
            var b = Group("Bench", parent, position, yaw);
            foreach (float z in new[] { -0.15f, 0f, 0.15f })
                Box("SeatSlat", b, new Vector3(0, 0.45f, z), new Vector3(1.6f, 0.04f, 0.12f), mWood);
            Box("BackSlatLow", b, new Vector3(0, 0.66f, -0.23f), new Vector3(1.6f, 0.1f, 0.03f), mWood, euler: new Vector3(-10, 0, 0));
            Box("BackSlatHigh", b, new Vector3(0, 0.82f, -0.26f), new Vector3(1.6f, 0.1f, 0.03f), mWood, euler: new Vector3(-10, 0, 0));
            foreach (float x in new[] { -0.7f, 0.7f })
            {
                Box("Leg", b, new Vector3(x, 0.22f, 0), new Vector3(0.05f, 0.45f, 0.42f), mFrame);
                Box("BackLeg", b, new Vector3(x, 0.66f, -0.24f), new Vector3(0.05f, 0.44f, 0.04f), mFrame, false, new Vector3(-10, 0, 0));
            }
        }

        static void LampPost(Transform parent, Vector3 position)
        {
            var l = Group("LampPost", parent, position);
            Cyl("Base", l, new Vector3(0, 0.15f, 0), 0.14f, 0.3f, mFrame);
            Cyl("Post", l, new Vector3(0, 1.6f, 0), 0.06f, 3.2f, mFrame);
            Box("Head", l, new Vector3(0, 3.28f, 0), new Vector3(0.34f, 0.12f, 0.34f), mFrame, false);
            var bulb = Ball("Bulb", l, new Vector3(0, 3.16f, 0), 0.2f, mBulb, false);
            lampBulbs.Add(bulb.GetComponent<Renderer>());
            var light = AddLight("LampLight", l, new Vector3(0, 3.0f, 0), LightType.Point, new Color(1f, 0.8f, 0.55f), 3.5f, 9f);
            light.enabled = false;
            lampLights.Add(light);
        }

        static void PlantTrees(Transform garden)
        {
            var trees = Group("Trees", garden);
            var placed = new List<Vector2>();
            // Keep clear of the lab, the path / plaza and the spots the screenshot cameras use.
            var keepClear = new[] { new Vector2(15f, 28f), new Vector2(10.2f, 23.5f), new Vector2(-6f, 17f), new Vector2(-7f, 14.5f), new Vector2(5f, 27f), new Vector2(10.5f, 20f) };
            int attempts = 0;
            while (placed.Count < 26 && attempts++ < 4000)
            {
                var p = new Vector2(Range(-17f, 17f), Range(-13f, 31f));
                if (p.x > -11.5f && p.x < 11.5f && p.y > -8.8f && p.y < 13.5f) continue; // lab + clear view of the glass facade
                if (Vector2.Distance(p, new Vector2(Fountain.x, Fountain.z)) < 5.8f) continue;
                if (Mathf.Abs(p.x - 6f) < 2.6f && p.y < 15f) continue;
                if (System.Array.Exists(keepClear, k => Vector2.Distance(k, p) < 3f)) continue;
                if (placed.Exists(q => Vector2.Distance(q, p) < 3.4f)) continue;
                placed.Add(p);

                var pos = new Vector3(p.x, 0, p.y);
                if (rng.NextDouble() < 0.35) Conifer(trees, pos, Range(0.85f, 1.3f));
                else RoundTree(trees, pos, Range(0.85f, 1.25f));
            }
        }

        static void RoundTree(Transform parent, Vector3 position, float scale)
        {
            var t = Group("Tree", parent, position, Range(0f, 360f));
            t.localScale = Vector3.one * scale;
            float h = Range(2.2f, 3.0f);
            Cyl("Trunk", t, new Vector3(0, h / 2, 0), Range(0.16f, 0.22f), h, mBark);
            var leaves = rng.NextDouble() < 0.25 ? mLeavesLight : mLeaves;
            Ball("Crown", t, new Vector3(0, h + 0.7f, 0), Range(2.4f, 3.0f), leaves, false);
            for (int i = 0; i < 3; i++)
            {
                float a = i * 120f * Mathf.Deg2Rad + Range(0f, 1f);
                var offset = new Vector3(Mathf.Cos(a) * 0.9f, h + Range(0.1f, 0.9f), Mathf.Sin(a) * 0.9f);
                Ball("Crown", t, offset, Range(1.5f, 2.1f), leaves, false);
            }
        }

        static void Conifer(Transform parent, Vector3 position, float scale)
        {
            var t = Group("Conifer", parent, position, Range(0f, 360f));
            t.localScale = Vector3.one * scale;
            Cyl("Trunk", t, new Vector3(0, 0.6f, 0), 0.15f, 1.2f, mBark);
            float[] radius = { 2.8f, 2.2f, 1.6f };
            for (int i = 0; i < 3; i++)
                MeshObject("Tier", t, coneMesh, new Vector3(0, 0.9f + i * 0.95f, 0), new Vector3(radius[i], 1.9f, radius[i]), mLeavesDark);
        }

        static void Bushes(Transform garden)
        {
            var group = Group("Bushes", garden);
            for (float x = -9.6f; x <= 9.8f; x += Range(1.2f, 1.6f))
            {
                if (Mathf.Abs(x - 6f) < 1.7f) continue;
                Ball("Bush", group, new Vector3(x, 0.3f, 7.95f + Range(0f, 0.25f)),
                    new Vector3(Range(1.0f, 1.4f), Range(0.7f, 1.0f), Range(0.8f, 1.0f)), rng.NextDouble() < 0.3 ? mLeavesLight : mLeaves);
            }
        }

        static void FlowerBed(Transform parent, Vector3 position)
        {
            var bed = Group("FlowerBed", parent, position);
            Ball("Mound", bed, new Vector3(0, 0.05f, 0), new Vector3(1.0f, 0.35f, 1.0f), mLeaves, false);
            var colours = new[] { mRed, mYellow, mPurple, mWhitePlastic };
            for (int i = 0; i < 8; i++)
            {
                var p = new Vector3(Range(-0.35f, 0.35f), Range(0.18f, 0.24f), Range(-0.35f, 0.35f));
                Ball("Flower", bed, p, 0.12f, colours[rng.Next(colours.Length)], false);
            }
        }

        static void Fence(Transform parent, float xMin, float xMax, float zMin, float zMax)
        {
            var fence = Group("Fence", parent);
            void Side(Vector3 a, Vector3 b)
            {
                float length = Vector3.Distance(a, b);
                var dir = (b - a).normalized;
                int posts = Mathf.CeilToInt(length / 2.5f);
                for (int i = 0; i <= posts; i++)
                    Box("Post", fence, a + dir * (length * i / posts) + Vector3.up * 0.55f, new Vector3(0.12f, 1.1f, 0.12f), mFenceWood);
                var mid = (a + b) / 2;
                var size = Mathf.Abs(dir.x) > 0.5f ? new Vector3(length, 0.08f, 0.05f) : new Vector3(0.05f, 0.08f, length);
                Box("RailLow", fence, mid + Vector3.up * 0.45f, size, mFenceWood, false);
                Box("RailHigh", fence, mid + Vector3.up * 0.88f, size, mFenceWood, false);

                // Invisible wall so the player cannot leave the garden.
                var wall = new GameObject("Boundary");
                wall.transform.SetParent(fence, false);
                wall.transform.localPosition = mid + Vector3.up * 1.5f;
                wall.AddComponent<BoxCollider>().size = new Vector3(Mathf.Max(size.x, 0.3f), 3f, Mathf.Max(size.z, 0.3f));
            }
            Side(new Vector3(xMin, 0, zMin), new Vector3(xMax, 0, zMin));
            Side(new Vector3(xMin, 0, zMax), new Vector3(xMax, 0, zMax));
            Side(new Vector3(xMin, 0, zMin), new Vector3(xMin, 0, zMax));
            Side(new Vector3(xMax, 0, zMin), new Vector3(xMax, 0, zMax));
        }

        static void Hills(Transform garden)
        {
            var hills = Group("Hills", garden);
            for (int i = 0; i < 9; i++)
            {
                float a = (i / 9f) * Mathf.PI * 2f + Range(-0.2f, 0.2f);
                float r = Range(75f, 95f);
                var hill = Ball("Hill", hills, new Vector3(Mathf.Cos(a) * r, -4f, 9f + Mathf.Sin(a) * r),
                    new Vector3(Range(55f, 75f), Range(18f, 30f), Range(40f, 55f)), mHill, false);
                NoShadows(hill);
            }
        }

        static void Drone()
        {
            var root = Group("Drone", interactive, Fountain);
            var flight = root.gameObject.AddComponent<DroneFlight>();
            var body = Group("Body", root, new Vector3(5.5f, 3.6f, 0));
            Box("Hull", body, Vector3.zero, new Vector3(0.36f, 0.1f, 0.36f), mDarkPlastic, false);
            Box("ArmA", body, Vector3.zero, new Vector3(0.75f, 0.03f, 0.05f), mDarkPlastic, false, new Vector3(0, 45, 0));
            Box("ArmB", body, Vector3.zero, new Vector3(0.75f, 0.03f, 0.05f), mDarkPlastic, false, new Vector3(0, -45, 0));
            for (int i = 0; i < 4; i++)
            {
                float a = (45f + i * 90f) * Mathf.Deg2Rad;
                var rotor = Cyl("Rotor", body, new Vector3(Mathf.Cos(a) * 0.26f, 0.05f, Mathf.Sin(a) * 0.26f), 0.13f, 0.01f, mSteel, false);
                var spin = rotor.AddComponent<Spinner>();
                spin.degreesPerSecond = 1440f;
            }
            var beacon = Ball("Beacon", body, new Vector3(0, -0.06f, 0.18f), 0.05f, mLedRed, false);
            var blink = beacon.AddComponent<BlinkingLight>();
            blink.target = beacon.GetComponent<Renderer>();
            blink.color = new Color(1f, 0.15f, 0.1f);
            blink.frequency = 1.2f;
            blink.squareWave = true;
            flight.body = body;
        }
    }
}
