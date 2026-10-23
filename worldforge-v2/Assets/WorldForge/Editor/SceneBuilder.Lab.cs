using UnityEngine;

namespace WorldForge.EditorTools
{
    // The lab: interior is x in [-10, 10], z in [-7, 7], 4 m high.
    // The north wall (z = +7) is a continuous glass wall looking out over the green ground.
    public static partial class SceneBuilder
    {
        static void BuildLabShell(Transform lab)
        {
            Box("Floor", lab, new Vector3(0, -0.1f, 0), new Vector3(20.6f, 0.2f, 14.6f), Tiled(mFloor, 10.3f, 7.3f));
            Box("Ceiling", lab, new Vector3(0, 4.1f, 0), new Vector3(20.6f, 0.2f, 14.6f), Tiled(mCeiling, 6, 4), collider: false);
            Box("Roof", lab, new Vector3(0, 4.32f, 0), new Vector3(21.2f, 0.25f, 15.2f), mRoof);
            Box("WallWest", lab, new Vector3(-10.15f, 2, 0), new Vector3(0.3f, 4, 14.6f), Tiled(mConcrete, 4, 1.2f));
            Box("WallEast", lab, new Vector3(10.15f, 2, 0), new Vector3(0.3f, 4, 14.6f), Tiled(mConcrete, 4, 1.2f));
            Box("WallSouth", lab, new Vector3(0, 2, -7.15f), new Vector3(20.6f, 4, 0.3f), Tiled(mBrick, 20.6f, 8));

            // Glass wall: low curb, eight panes between metal mullions, top beam.
            var facade = Group("GlassWall", lab);
            Box("Curb", facade, new Vector3(0, 0.15f, 7.15f), new Vector3(20.6f, 0.3f, 0.3f), mFrame);
            Box("TopBeam", facade, new Vector3(0, 3.95f, 7.15f), new Vector3(20.6f, 0.1f, 0.26f), mFrame);
            for (int i = 0; i < 8; i++)
            {
                float a = -10f + i * 2.5f;
                if (i > 0) Box($"Mullion_{i}", facade, new Vector3(a, 2, 7.15f), new Vector3(0.1f, 4, 0.22f), mFrame);
                NoShadows(Box($"Glass_{i}", facade, new Vector3(a + 1.25f, 2.1f, 7.15f), new Vector3(2.4f, 3.6f, 0.03f), mGlass));
            }
        }

        static void BuildLabInterior(Transform lab)
        {
            Quad("Sign_Main", lab, new Vector3(4.2f, 3.05f, -6.99f), new Vector2(5.6f, 0.875f), mSignMain, Vector3.forward);
            Quad("Sign_Conveyor", lab, new Vector3(-5.1f, 2.45f, -6.99f), new Vector2(2.4f, 0.6f), mSignConveyor, Vector3.forward);

            Box("WhiteboardFrame", lab, new Vector3(-9.97f, 1.65f, -2.4f), new Vector3(0.04f, 1.3f, 2.5f), mSteel, false);
            Quad("Whiteboard", lab, new Vector3(-9.945f, 1.65f, -2.4f), new Vector2(2.4f, 1.2f), mWhiteboard, Vector3.right);

            Shelf(lab, new Vector3(-9.7f, 0, 1.8f), 90f);
            PottedPlant(lab, new Vector3(-9.4f, 0, 6.4f));
            PottedPlant(lab, new Vector3(9.4f, 0, 6.4f));

            BuildLightSwitch(new Vector3(9.99f, 1.3f, 4.6f));
        }

        static void Shelf(Transform parent, Vector3 position, float yaw)
        {
            var s = Group("Shelf", parent, position, yaw);
            foreach (float x in new[] { -0.88f, 0.88f })
            foreach (float z in new[] { -0.23f, 0.23f })
                Box("Upright", s, new Vector3(x, 1.0f, z), new Vector3(0.04f, 2.0f, 0.04f), mFrame);
            float[] levels = { 0.12f, 0.75f, 1.38f, 2.0f };
            foreach (float y in levels)
                Box("Board", s, new Vector3(0, y, 0), new Vector3(1.8f, 0.03f, 0.5f), mSteel);

            // A few boxes and bins on each of the lower three boards.
            Material[] boxMats = { mCardboard, mBlue, mYellow, mCrate };
            for (int level = 0; level < 3; level++)
            for (int k = 0; k < 3; k++)
            {
                float h = Range(0.2f, 0.35f);
                var mat = boxMats[rng.Next(boxMats.Length)];
                Box("Box", s, new Vector3(-0.55f + k * 0.55f, levels[level] + 0.015f + h / 2, 0), new Vector3(0.38f, h, 0.36f), mat);
            }
        }

        static void PottedPlant(Transform parent, Vector3 position)
        {
            var p = Group("PottedPlant", parent, position);
            Cyl("Pot", p, new Vector3(0, 0.225f, 0), 0.25f, 0.45f, mTerracotta);
            Cyl("Soil", p, new Vector3(0, 0.455f, 0), 0.23f, 0.02f, mSoil, false);
            Ball("Leaves", p, new Vector3(0, 0.95f, 0), 0.8f, mLeaves, false);
        }

        static void BuildLightSwitch(Vector3 position)
        {
            var root = Group("LightSwitch", interactive, position, -90f);
            lightSwitch = root.gameObject.AddComponent<LightSwitch>();
            lightSwitch.displayName = "Lab light switch";
            var hit = root.gameObject.AddComponent<BoxCollider>();
            hit.size = new Vector3(0.3f, 0.35f, 0.1f);
            Box("Plate", root, Vector3.zero, new Vector3(0.12f, 0.18f, 0.02f), mWhitePlastic, false);
            var pivot = Group("Pivot", root, new Vector3(0, 0, 0.01f));
            Box("Lever", pivot, new Vector3(0, 0, 0.025f), new Vector3(0.03f, 0.07f, 0.04f), mDarkPlastic, false);
            lightSwitch.lever = pivot;
        }

        // ------------------------------------------------------------------ robot cell

        static void BuildRobotCell(Transform lab)
        {
            var cell = Group("RobotCellFloor", lab, new Vector3(-4.5f, 0, 0.6f));
            Box("TreadPlate", cell, new Vector3(0, 0.005f, 0), new Vector3(4.6f, 0.01f, 4.4f), Tiled(mTread, 4.6f, 4.4f), false);
            Box("HazardN", cell, new Vector3(0, 0.008f, 2.3f), new Vector3(5f, 0.012f, 0.2f), Tiled(mHazard, 25, 1), false);
            Box("HazardS", cell, new Vector3(0, 0.008f, -2.3f), new Vector3(5f, 0.012f, 0.2f), Tiled(mHazard, 25, 1), false);
            Box("HazardW", cell, new Vector3(-2.4f, 0.008f, 0), new Vector3(0.2f, 0.012f, 4.4f), Tiled(mHazard, 1, 22), false);
            Box("HazardE", cell, new Vector3(2.4f, 0.008f, 0), new Vector3(0.2f, 0.012f, 4.4f), Tiled(mHazard, 1, 22), false);

            var padA = PadTable(lab, "PadA", new Vector3(-6.2f, 0, 0.5f));
            var padB = PadTable(lab, "PadB", new Vector3(-4.5f, 0, 2.2f));
            BuildRobotArm(padA, padB);

            // Hanging zone sign (double sided)
            var sign = Group("RobotCellSign", lab, new Vector3(-4.5f, 3.0f, -1.9f));
            Box("Backing", sign, Vector3.zero, new Vector3(2.45f, 0.64f, 0.02f), mDarkPlastic, false);
            Quad("Front", sign, new Vector3(0, 0, -0.012f), new Vector2(2.4f, 0.6f), mSignRobot, Vector3.back);
            Quad("Back", sign, new Vector3(0, 0, 0.012f), new Vector2(2.4f, 0.6f), mSignRobot, Vector3.forward);
            Cyl("RodL", sign, new Vector3(-1f, 0.65f, 0), 0.01f, 0.7f, mSteel, false);
            Cyl("RodR", sign, new Vector3(1f, 0.65f, 0), 0.01f, 0.7f, mSteel, false);
        }

        /// <summary>Small table the arm picks from / places on. Returns the crate rest point.</summary>
        static Transform PadTable(Transform parent, string name, Vector3 position)
        {
            var t = Group(name, parent, position);
            Box("Top", t, new Vector3(0, 0.77f, 0), new Vector3(0.6f, 0.06f, 0.6f), mSteel);
            Cyl("Column", t, new Vector3(0, 0.37f, 0), 0.06f, 0.74f, mFrame);
            Cyl("Foot", t, new Vector3(0, 0.02f, 0), 0.25f, 0.04f, mFrame);
            Box("Marker", t, new Vector3(0, 0.801f, 0), new Vector3(0.34f, 0.002f, 0.34f), mYellow, false);
            return Group("CrateRestPoint", t, new Vector3(0, 0.93f, 0));
        }

        static void BuildRobotArm(Transform padA, Transform padB)
        {
            var root = Group("RobotArm", interactive, new Vector3(-4.5f, 0, 0.5f));
            arm = root.gameObject.AddComponent<RobotArm>();
            arm.displayName = "Robot arm";

            Cyl("Pedestal", root, new Vector3(0, 0.3f, 0), 0.45f, 0.6f, mJoint);
            var lamp = Ball("StatusLamp", root, new Vector3(0, 0.45f, -0.44f), 0.09f, mStatus, false);
            arm.statusLamp = lamp.GetComponent<Renderer>();

            var turntable = Group("Turntable", root, new Vector3(0, 0.6f, 0));
            Cyl("TurntableBody", turntable, new Vector3(0, 0.1f, 0), 0.36f, 0.2f, mOrange);
            Box("BracketL", turntable, new Vector3(-0.2f, 0.32f, 0), new Vector3(0.08f, 0.3f, 0.3f), mOrange);
            Box("BracketR", turntable, new Vector3(0.2f, 0.32f, 0), new Vector3(0.08f, 0.3f, 0.3f), mOrange);

            var shoulder = Group("Shoulder", turntable, new Vector3(0, 0.35f, 0));
            Cyl("ShoulderAxle", shoulder, Vector3.zero, 0.13f, 0.36f, mJoint, true, new Vector3(0, 0, 90));
            Box("UpperArm", shoulder, new Vector3(0, 0.55f, 0), new Vector3(0.18f, 1.1f, 0.2f), mOrange);

            var elbow = Group("Elbow", shoulder, new Vector3(0, 1.1f, 0));
            Cyl("ElbowAxle", elbow, Vector3.zero, 0.11f, 0.3f, mJoint, true, new Vector3(0, 0, 90));
            Box("Forearm", elbow, new Vector3(0, 0.5f, 0), new Vector3(0.14f, 1.0f, 0.16f), mOrange);

            var wrist = Group("Wrist", elbow, new Vector3(0, 1.0f, 0));
            Ball("WristJoint", wrist, Vector3.zero, 0.17f, mJoint);
            Box("GripperBody", wrist, new Vector3(0, 0.1f, 0), new Vector3(0.48f, 0.07f, 0.14f), mFrame);
            var fingerL = Box("FingerL", wrist, new Vector3(-0.22f, 0.24f, 0), new Vector3(0.03f, 0.2f, 0.1f), mSteel);
            var fingerR = Box("FingerR", wrist, new Vector3(0.22f, 0.24f, 0), new Vector3(0.03f, 0.2f, 0.1f), mSteel);
            var tip = Group("Tip", wrist, new Vector3(0, 0.28f, 0));

            var crate = Box("RobotCrate", root, root.InverseTransformPoint(padA.position), Vector3.one * 0.26f, mCrate);

            arm.turntable = turntable;
            arm.shoulder = shoulder;
            arm.elbow = elbow;
            arm.wrist = wrist;
            arm.fingerLeft = fingerL.transform;
            arm.fingerRight = fingerR.transform;
            arm.tip = tip;
            arm.crate = crate.transform;
            arm.padA = padA;
            arm.padB = padB;
            arm.ApplyHomePose();
        }

        // ------------------------------------------------------------------ conveyor

        static void BuildConveyor()
        {
            var root = Group("Conveyor", interactive, new Vector3(-5.1f, 0, -5.3f));
            conveyor = root.gameObject.AddComponent<ConveyorBelt>();
            conveyor.displayName = "Conveyor belt";

            var belt = Box("Belt", root, new Vector3(0, 0.81f, 0), new Vector3(8f, 0.08f, 0.8f), mBelt);
            Box("RailN", root, new Vector3(0, 0.86f, 0.43f), new Vector3(8f, 0.12f, 0.05f), mSteel);
            Box("RailS", root, new Vector3(0, 0.86f, -0.43f), new Vector3(8f, 0.12f, 0.05f), mSteel);
            foreach (float x in new[] { -2.5f, 0f, 2.5f })
            {
                Box("LegN", root, new Vector3(x, 0.385f, 0.35f), new Vector3(0.06f, 0.77f, 0.06f), mFrame);
                Box("LegS", root, new Vector3(x, 0.385f, -0.35f), new Vector3(0.06f, 0.77f, 0.06f), mFrame);
            }

            // Boxes at both ends hide the crates while they wrap around.
            Box("InputBox", root, new Vector3(-4.0f, 0.75f, 0), new Vector3(1.0f, 1.5f, 1.2f), mMachine);
            Box("InputStripe", root, new Vector3(-4.0f, 1.4f, 0), new Vector3(1.02f, 0.06f, 1.22f), mLedBlue, false);
            Box("OutputBox", root, new Vector3(4.0f, 0.75f, 0), new Vector3(1.0f, 1.5f, 1.2f), mMachine);
            Box("OutputStripe", root, new Vector3(4.0f, 1.4f, 0), new Vector3(1.02f, 0.06f, 1.22f), mLedGreen, false);
            Box("InputMouth", root, new Vector3(-3.495f, 1.05f, 0), new Vector3(0.01f, 0.5f, 0.6f), mDarkPlastic, false);
            Box("OutputMouth", root, new Vector3(3.495f, 1.05f, 0), new Vector3(0.01f, 0.5f, 0.6f), mDarkPlastic, false);

            var items = new Transform[5];
            float spacing = 7.4f / items.Length;
            for (int i = 0; i < items.Length; i++)
            {
                var crate = Box($"Crate_{i}", root, new Vector3(-3.3f + i * spacing, 1.05f, 0), Vector3.one * 0.4f,
                    i % 2 == 0 ? mCrate : mCardboard);
                crate.AddComponent<Rigidbody>().isKinematic = true;
                items[i] = crate.transform;
            }

            conveyor.belt = belt.GetComponent<Renderer>();
            conveyor.beltLength = 8f;
            conveyor.items = items;
            // Wrap points sit fully inside the end boxes so no crate visibly pops.
            conveyor.minX = -3.7f;
            conveyor.maxX = 3.7f;
        }

        // ------------------------------------------------------------------ workstations and workbench

        static void BuildWorkstations(Transform lab)
        {
            float[] zs = { -2.6f, 0.6f };
            Material[] screens = { mScreenCode, mScreenDash };
            for (int i = 0; i < zs.Length; i++)
            {
                // Desk faces +x (towards the east wall); the user sits on its west side.
                var desk = Group($"Desk_{i}", lab, new Vector3(9.35f, 0, zs[i]), 90f);
                Box("Top", desk, new Vector3(0, 0.75f, 0), new Vector3(1.8f, 0.05f, 0.85f), mWood);
                foreach (float x in new[] { -0.85f, 0.85f })
                foreach (float z in new[] { -0.38f, 0.38f })
                    Box("Leg", desk, new Vector3(x, 0.365f, z), new Vector3(0.05f, 0.73f, 0.05f), mFrame);
                Box("Keyboard", desk, new Vector3(0, 0.785f, -0.15f), new Vector3(0.45f, 0.02f, 0.15f), mDarkPlastic, false);

                Box("MonitorStand", desk, new Vector3(0, 0.95f, 0.26f), new Vector3(0.05f, 0.35f, 0.04f), mFrame);
                Box("MonitorBezel", desk, new Vector3(0, 1.2f, 0.22f), new Vector3(0.95f, 0.56f, 0.04f), mDarkPlastic);
                Quad("Screen", desk, new Vector3(0, 1.2f, 0.198f), new Vector2(0.9f, 0.506f), screens[i], Vector3.back);

                Chair(desk, new Vector3(0, 0, -0.8f));
            }
        }

        static void Chair(Transform parent, Vector3 position)
        {
            var c = Group("Chair", parent, position);
            Box("Seat", c, new Vector3(0, 0.46f, 0), new Vector3(0.48f, 0.06f, 0.46f), mFabric);
            Box("Back", c, new Vector3(0, 0.78f, -0.21f), new Vector3(0.46f, 0.5f, 0.05f), mFabric);
            foreach (float x in new[] { -0.2f, 0.2f })
            foreach (float z in new[] { -0.19f, 0.19f })
                Box("Leg", c, new Vector3(x, 0.215f, z), new Vector3(0.04f, 0.43f, 0.04f), mFrame);
        }

        static void BuildWorkbench(Transform lab)
        {
            var bench = Group("Workbench", lab, new Vector3(1.2f, 0, -3.4f));
            Box("Top", bench, new Vector3(0, 0.9f, 0), new Vector3(2.2f, 0.06f, 0.9f), mWood);
            foreach (float x in new[] { -1.02f, 1.02f })
            foreach (float z in new[] { -0.38f, 0.38f })
                Box("Leg", bench, new Vector3(x, 0.435f, z), new Vector3(0.07f, 0.87f, 0.07f), mFrame);
            Box("LowerShelf", bench, new Vector3(0, 0.25f, 0), new Vector3(2.1f, 0.03f, 0.8f), mSteel);

            // Loose items the player can pick up.
            float top = 0.93f;
            var crate = Box("SmallCrate", interactive, new Vector3(0.5f, top + 0.15f, -3.35f), Vector3.one * 0.3f, mCrate);
            AddRigidbody<Grabbable>(crate, 2f).displayName = "Small crate";

            var ball = Ball("RubberBall", interactive, new Vector3(1.2f, top + 0.12f, -3.3f), 0.22f, mRed);
            var bouncy = new PhysicsMaterial("Bouncy") { bounciness = 0.6f, bounceCombine = PhysicsMaterialCombine.Maximum };
            UnityEditor.AssetDatabase.CreateAsset(bouncy, $"{MaterialFolder}/Bouncy.physicMaterial");
            ball.GetComponent<Collider>().sharedMaterial = bouncy;
            AddRigidbody<Grabbable>(ball, 0.4f).displayName = "Rubber ball";

            var toolbox = Group("Toolbox", interactive, new Vector3(1.9f, top, -3.4f));
            Box("Case", toolbox, new Vector3(0, 0.1f, 0), new Vector3(0.42f, 0.2f, 0.22f), mBlue);
            Box("Handle", toolbox, new Vector3(0, 0.23f, 0), new Vector3(0.24f, 0.04f, 0.04f), mDarkPlastic, false);
            AddRigidbody<Grabbable>(toolbox.gameObject, 2.5f).displayName = "Toolbox";
        }
    }
}
