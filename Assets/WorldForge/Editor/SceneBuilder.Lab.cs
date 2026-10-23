using UnityEngine;

namespace WorldForge.EditorTools
{
    // The lab building: interior is x in [-10, 10], z in [-7, 7], 4 m high.
    // The north wall (z = +7) is glass with a sliding door that leads to the garden.
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

            // Skirting boards
            Box("SkirtWest", lab, new Vector3(-9.99f, 0.06f, 0), new Vector3(0.02f, 0.12f, 14f), mFrame, false);
            Box("SkirtEast", lab, new Vector3(9.99f, 0.06f, 0), new Vector3(0.02f, 0.12f, 14f), mFrame, false);
            Box("SkirtSouth", lab, new Vector3(0, 0.06f, -6.99f), new Vector3(20f, 0.12f, 0.02f), mFrame, false);

            // Glass facade: curb, mullions, panes, top beam. The gap at x 4.8..7.2 is the door.
            var facade = Group("GlassFacade", lab);
            Box("Curb_West", facade, new Vector3(-2.75f, 0.15f, 7.15f), new Vector3(15.1f, 0.3f, 0.3f), mFrame);
            Box("Curb_East", facade, new Vector3(8.75f, 0.15f, 7.15f), new Vector3(3.1f, 0.3f, 0.3f), mFrame);
            Box("TopBeam", facade, new Vector3(0, 3.95f, 7.15f), new Vector3(20.6f, 0.1f, 0.26f), mFrame);
            foreach (float x in new[] { -7.5f, -5f, -2.5f, 0f, 2.5f, 4.8f, 7.2f, 8.6f })
                Box($"Mullion_{x:0.#}", facade, new Vector3(x, 2, 7.15f), new Vector3(0.1f, 4, 0.22f), mFrame);

            float[,] panes = { { -10f, -7.5f }, { -7.5f, -5f }, { -5f, -2.5f }, { -2.5f, 0f }, { 0f, 2.5f }, { 2.5f, 4.8f }, { 7.2f, 8.6f }, { 8.6f, 10f } };
            for (int i = 0; i < panes.GetLength(0); i++)
            {
                float a = panes[i, 0], b = panes[i, 1];
                NoShadows(Box($"Glass_{i}", facade, new Vector3((a + b) / 2, 2.1f, 7.15f), new Vector3(b - a - 0.1f, 3.6f, 0.03f), mGlass));
            }
            Box("DoorHeader", facade, new Vector3(6, 2.68f, 7.15f), new Vector3(2.4f, 0.12f, 0.22f), mFrame);
            NoShadows(Box("Transom", facade, new Vector3(6, 3.32f, 7.15f), new Vector3(2.3f, 1.16f, 0.03f), mGlass));
            Box("Threshold", facade, new Vector3(6, 0.005f, 7.0f), new Vector3(2.4f, 0.01f, 0.6f), mSteel, false);

            BuildDoor();

            // Name sign on the roof edge, readable from the garden.
            Box("RoofSignBoard", lab, new Vector3(0, 5.0f, 7.45f), new Vector3(7.2f, 1.25f, 0.12f), mFrame);
            Box("RoofSignPostL", lab, new Vector3(-3f, 4.6f, 7.3f), new Vector3(0.1f, 0.5f, 0.1f), mFrame, false);
            Box("RoofSignPostR", lab, new Vector3(3f, 4.6f, 7.3f), new Vector3(0.1f, 0.5f, 0.1f), mFrame, false);
            Quad("RoofSign", lab, new Vector3(0, 5.0f, 7.515f), new Vector2(6.9f, 1.08f), mSignMain, Vector3.forward);
        }

        static void BuildDoor()
        {
            var root = Group("GlassDoor", interactive, new Vector3(6f, 0f, 6.92f));
            door = root.gameObject.AddComponent<SlidingDoor>();
            door.displayName = "Sliding glass door";
            door.leftPanel = DoorPanel(root, "PanelLeft", -0.6f, 0.45f);
            door.rightPanel = DoorPanel(root, "PanelRight", 0.6f, -0.45f);
        }

        static Transform DoorPanel(Transform root, string name, float x, float handleX)
        {
            var panel = Group(name, root, new Vector3(x, 0, 0));
            // Full panel width so the two panels' colliders meet in the middle.
            NoShadows(Box("Glass", panel, new Vector3(0, 1.32f, 0), new Vector3(1.2f, 2.56f, 0.03f), mGlass));
            Box("StileL", panel, new Vector3(-0.57f, 1.32f, 0), new Vector3(0.05f, 2.6f, 0.05f), mFrame, false);
            Box("StileR", panel, new Vector3(0.57f, 1.32f, 0), new Vector3(0.05f, 2.6f, 0.05f), mFrame, false);
            Box("RailTop", panel, new Vector3(0, 2.6f, 0), new Vector3(1.19f, 0.05f, 0.05f), mFrame, false);
            Box("RailBottom", panel, new Vector3(0, 0.05f, 0), new Vector3(1.19f, 0.1f, 0.05f), mFrame, false);
            Cyl("Handle", panel, new Vector3(handleX, 1.1f, -0.05f), 0.018f, 0.6f, mSteel, false);
            return panel;
        }

        static void BuildLabInterior(Transform lab)
        {
            // Ceiling duct along the brick wall
            Cyl("Duct", lab, new Vector3(0, 3.62f, -6.4f), 0.22f, 19.6f, mSteel, false, new Vector3(0, 0, 90));
            for (float x = -8f; x <= 8f; x += 4f)
                Box($"DuctHanger_{x}", lab, new Vector3(x, 3.92f, -6.4f), new Vector3(0.05f, 0.2f, 0.05f), mFrame, false);

            // Signage
            Quad("Sign_Main", lab, new Vector3(4.2f, 3.05f, -6.99f), new Vector2(5.6f, 0.875f), mSignMain, Vector3.forward);
            Quad("Sign_Conveyor", lab, new Vector3(-5.1f, 2.45f, -6.99f), new Vector2(2.4f, 0.6f), mSignConveyor, Vector3.forward);
            Quad("Sign_Workstations", lab, new Vector3(9.99f, 2.7f, -0.6f), new Vector2(2.6f, 0.65f), mSignWork, Vector3.left);

            // Whiteboard on the west wall
            Box("WhiteboardFrame", lab, new Vector3(-9.97f, 1.65f, -2.4f), new Vector3(0.04f, 1.3f, 2.5f), mSteel, false);
            Quad("Whiteboard", lab, new Vector3(-9.945f, 1.65f, -2.4f), new Vector2(2.4f, 1.2f), mWhiteboard, Vector3.right);
            Box("MarkerTray", lab, new Vector3(-9.92f, 0.98f, -2.4f), new Vector3(0.08f, 0.03f, 1.2f), mSteel, false);
            Box("MarkerBlue", lab, new Vector3(-9.91f, 1.005f, -2.7f), new Vector3(0.02f, 0.02f, 0.13f), mBlue, false);
            Box("MarkerRed", lab, new Vector3(-9.91f, 1.005f, -2.5f), new Vector3(0.02f, 0.02f, 0.13f), mRed, false);

            // Storage shelves on the west wall
            Shelf(lab, new Vector3(-9.7f, 0, 0.6f), 90f);
            Shelf(lab, new Vector3(-9.7f, 0, 2.9f), 90f);

            // Server racks in the south-east corner
            ServerRack(lab, new Vector3(9.55f, 0, -5.6f));
            ServerRack(lab, new Vector3(9.55f, 0, -6.35f));

            // Plants
            PottedPlant(lab, new Vector3(-9.4f, 0, 6.4f), 1.1f);
            PottedPlant(lab, new Vector3(9.4f, 0, 6.4f), 1.0f);
            PottedPlant(lab, new Vector3(-0.9f, 0, 6.45f), 0.9f);
            PottedPlant(lab, new Vector3(3.6f, 0, 6.45f), 1.0f);
            PottedPlant(lab, new Vector3(9.4f, 0, -4.6f), 0.8f);

            // Wall switches (interactive) on the east wall, next to the garden door
            BuildLightSwitch(new Vector3(9.99f, 1.3f, 4.6f));
            BuildDayNightPanel(new Vector3(9.99f, 1.3f, 5.3f));
        }

        static void Shelf(Transform parent, Vector3 position, float yaw)
        {
            var s = Group("Shelf", parent, position, yaw);
            foreach (float x in new[] { -0.88f, 0.88f })
            foreach (float z in new[] { -0.23f, 0.23f })
                Box("Upright", s, new Vector3(x, 1.0f, z), new Vector3(0.04f, 2.0f, 0.04f), mFrame);
            float[] levels = { 0.12f, 0.62f, 1.12f, 1.62f, 2.0f };
            foreach (float y in levels)
                Box("Board", s, new Vector3(0, y, 0), new Vector3(1.8f, 0.03f, 0.5f), mSteel);

            for (int level = 0; level < 4; level++)
            {
                float x = -0.75f;
                while (x < 0.6f)
                {
                    int kind = rng.Next(4);
                    float w = kind == 0 ? Range(0.25f, 0.4f) : kind == 1 ? 0.35f : 0.18f;
                    float y = levels[level] + 0.015f;
                    var pos = new Vector3(x + w / 2, y, Range(-0.08f, 0.08f));
                    switch (kind)
                    {
                        case 0:
                            float h = Range(0.18f, 0.36f);
                            Box("Box", s, pos + Vector3.up * h / 2, new Vector3(w, h, Range(0.25f, 0.38f)), mCardboard, euler: new Vector3(0, Range(-6, 6), 0));
                            break;
                        case 1:
                            Box("Bin", s, pos + Vector3.up * 0.11f, new Vector3(w, 0.22f, 0.4f), rng.Next(2) == 0 ? mBlue : mYellow);
                            break;
                        case 2:
                            Cyl("Motor", s, pos + Vector3.up * 0.12f, 0.08f, 0.24f, rng.Next(2) == 0 ? mSteel : mOrange);
                            break;
                        default:
                            Ball("Ball", s, pos + Vector3.up * 0.09f, 0.18f, rng.Next(2) == 0 ? mRed : mBlue);
                            break;
                    }
                    x += w + Range(0.04f, 0.12f);
                }
            }
        }

        static void ServerRack(Transform parent, Vector3 position)
        {
            var r = Group("ServerRack", parent, position, -90f);
            Box("Cabinet", r, new Vector3(0, 1.05f, 0), new Vector3(0.7f, 2.1f, 0.8f), mDarkPlastic);
            Quad("Front", r, new Vector3(0, 1.05f, 0.401f), new Vector2(0.62f, 1.95f), mServer, Vector3.forward);
            for (int i = 0; i < 4; i++)
            {
                var led = Ball($"Beacon_{i}", r, new Vector3(0.28f, 0.3f + i * 0.45f, 0.405f), 0.025f, mLedGreen, false);
                var blink = led.AddComponent<BlinkingLight>();
                blink.target = led.GetComponent<Renderer>();
                blink.color = i % 2 == 0 ? new Color(0.2f, 1f, 0.3f) : new Color(0.3f, 0.6f, 1f);
                blink.frequency = Range(0.7f, 2.5f);
                blink.phase = Range(0f, 1f);
                blink.squareWave = true;
            }
        }

        static void PottedPlant(Transform parent, Vector3 position, float scale)
        {
            var p = Group("PottedPlant", parent, position);
            p.localScale = Vector3.one * scale;
            Cyl("Pot", p, new Vector3(0, 0.225f, 0), 0.25f, 0.45f, mTerracotta);
            Cyl("Rim", p, new Vector3(0, 0.44f, 0), 0.27f, 0.05f, mTerracotta, false);
            Cyl("Soil", p, new Vector3(0, 0.455f, 0), 0.23f, 0.02f, mSoil, false);
            for (int i = 0; i < 4; i++)
            {
                var offset = new Vector3(Range(-0.15f, 0.15f), Range(0.75f, 1.1f), Range(-0.15f, 0.15f));
                Ball("Leaves", p, offset, Range(0.45f, 0.7f), i % 2 == 0 ? mLeaves : mLeavesLight, false);
            }
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

        static void BuildDayNightPanel(Vector3 position)
        {
            var root = Group("DayNightPanel", interactive, position, -90f);
            dayNight = root.gameObject.AddComponent<DayNightController>();
            dayNight.displayName = "Day / night control";
            Box("Panel", root, Vector3.zero, new Vector3(0.32f, 0.32f, 0.03f), mDarkPlastic);
            Cyl("Sun", root, new Vector3(-0.07f, 0.03f, 0.02f), 0.05f, 0.01f, mYellow, false, new Vector3(90, 0, 0));
            Cyl("Moon", root, new Vector3(0.07f, -0.03f, 0.02f), 0.05f, 0.01f, mLedBlue, false, new Vector3(90, 0, 0));
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
            BuildRobotConsole();

            // Hanging zone sign (double sided)
            var sign = Group("RobotCellSign", lab, new Vector3(-4.5f, 3.0f, -1.9f));
            Box("Backing", sign, Vector3.zero, new Vector3(2.45f, 0.64f, 0.02f), mDarkPlastic, false);
            Quad("Front", sign, new Vector3(0, 0, -0.012f), new Vector2(2.4f, 0.6f), mSignRobot, Vector3.back);
            Quad("Back", sign, new Vector3(0, 0, 0.012f), new Vector2(2.4f, 0.6f), mSignRobot, Vector3.forward);
            Cyl("RodL", sign, new Vector3(-1f, 0.65f, 0), 0.01f, 0.7f, mSteel, false);
            Cyl("RodR", sign, new Vector3(1f, 0.65f, 0), 0.01f, 0.7f, mSteel, false);

            // Safety poster on a stand at the corner of the cell
            var stand = Group("SafetyStand", lab, new Vector3(-2.3f, 0, -2.0f));
            Cyl("PoleL", stand, new Vector3(-0.33f, 0.9f, 0), 0.025f, 1.8f, mSteel);
            Cyl("PoleR", stand, new Vector3(0.33f, 0.9f, 0), 0.025f, 1.8f, mSteel);
            Box("Board", stand, new Vector3(0, 1.3f, 0), new Vector3(0.66f, 0.98f, 0.02f), mWhitePlastic);
            Quad("Poster", stand, new Vector3(0, 1.3f, -0.012f), new Vector2(0.6f, 0.9f), mPoster, Vector3.back);
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
            Cyl("PedestalRing", root, new Vector3(0, 0.58f, 0), 0.47f, 0.04f, mOrange, false);
            var lamp = Ball("StatusLamp", root, new Vector3(0, 0.45f, -0.44f), 0.09f, mStatus, false);
            arm.statusLamp = lamp.GetComponent<Renderer>();
            arm.statusLight = AddLight("StatusLight", root, new Vector3(0, 0.45f, -0.6f), LightType.Point, new Color(1f, 0.6f, 0.1f), 0.8f, 1.5f);

            var turntable = Group("Turntable", root, new Vector3(0, 0.6f, 0));
            Cyl("TurntableBody", turntable, new Vector3(0, 0.1f, 0), 0.36f, 0.2f, mOrange);
            Box("BracketL", turntable, new Vector3(-0.2f, 0.32f, 0), new Vector3(0.08f, 0.3f, 0.3f), mOrange);
            Box("BracketR", turntable, new Vector3(0.2f, 0.32f, 0), new Vector3(0.08f, 0.3f, 0.3f), mOrange);

            var shoulder = Group("Shoulder", turntable, new Vector3(0, 0.35f, 0));
            Cyl("ShoulderAxle", shoulder, Vector3.zero, 0.13f, 0.36f, mJoint, true, new Vector3(0, 0, 90));
            Box("UpperArm", shoulder, new Vector3(0, 0.55f, 0), new Vector3(0.18f, 1.1f, 0.2f), mOrange);
            Box("UpperArmCable", shoulder, new Vector3(0, 0.55f, -0.11f), new Vector3(0.05f, 0.9f, 0.03f), mDarkPlastic, false);

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

        static void BuildRobotConsole()
        {
            var root = Group("RobotConsole", interactive, new Vector3(-1.6f, 0, 1.9f), 90f);
            var console = root.gameObject.AddComponent<RobotConsole>();
            console.displayName = "Robot control desk";
            console.arm = arm;
            Box("Body", root, new Vector3(0, 0.5f, 0), new Vector3(0.7f, 1.0f, 0.5f), mDarkPlastic);
            Box("Top", root, new Vector3(0, 1.04f, 0.02f), new Vector3(0.72f, 0.04f, 0.56f), mMachine, euler: new Vector3(15, 0, 0));
            var button = Cyl("Button", root, new Vector3(0.17f, 1.08f, 0.08f), 0.07f, 0.05f, mRed, false, new Vector3(15, 0, 0));
            console.button = button.GetComponent<Renderer>();
            Quad("Screen", root, new Vector3(-0.13f, 1.065f, 0.03f), new Vector2(0.32f, 0.2f), mScreen, new Vector3(0, 0.97f, 0.26f));
        }

        // ------------------------------------------------------------------ conveyor

        static void BuildConveyor()
        {
            var root = Group("Conveyor", interactive, new Vector3(-5.1f, 0, -5.3f));
            conveyor = root.gameObject.AddComponent<ConveyorBelt>();
            conveyor.displayName = "Conveyor belt";

            var belt = Box("Belt", root, new Vector3(0, 0.81f, 0), new Vector3(8.4f, 0.08f, 0.8f), mBelt);
            Box("RailN", root, new Vector3(0, 0.86f, 0.43f), new Vector3(8.4f, 0.12f, 0.05f), mSteel);
            Box("RailS", root, new Vector3(0, 0.86f, -0.43f), new Vector3(8.4f, 0.12f, 0.05f), mSteel);
            foreach (float x in new[] { -3.0f, -1.0f, 1.0f, 3.0f })
            {
                Box("LegN", root, new Vector3(x, 0.385f, 0.35f), new Vector3(0.06f, 0.77f, 0.06f), mFrame);
                Box("LegS", root, new Vector3(x, 0.385f, -0.35f), new Vector3(0.06f, 0.77f, 0.06f), mFrame);
                Box("Brace", root, new Vector3(x, 0.25f, 0), new Vector3(0.05f, 0.05f, 0.7f), mFrame, false);
            }

            // Machines at both ends hide the crates while they wrap around.
            Box("Dispenser", root, new Vector3(-4.2f, 0.85f, 0), new Vector3(1.2f, 1.7f, 1.3f), mMachine);
            Box("DispenserStripe", root, new Vector3(-4.2f, 1.55f, 0), new Vector3(1.22f, 0.06f, 1.32f), mLedBlue, false);
            Box("DispenserMouth", root, new Vector3(-3.595f, 1.05f, 0), new Vector3(0.02f, 0.5f, 0.6f), mDarkPlastic, false);
            Box("Packer", root, new Vector3(4.2f, 0.75f, 0), new Vector3(1.2f, 1.5f, 1.3f), mMachine);
            Box("PackerStripe", root, new Vector3(4.2f, 1.35f, 0), new Vector3(1.22f, 0.06f, 1.32f), mLedGreen, false);
            Box("PackerMouth", root, new Vector3(3.595f, 1.05f, 0), new Vector3(0.02f, 0.5f, 0.6f), mDarkPlastic, false);

            // Scanner arch in the middle
            Box("ScannerPostN", root, new Vector3(0, 0.75f, 0.5f), new Vector3(0.08f, 1.5f, 0.08f), mFrame);
            Box("ScannerPostS", root, new Vector3(0, 0.75f, -0.5f), new Vector3(0.08f, 1.5f, 0.08f), mFrame);
            Box("ScannerBeamHousing", root, new Vector3(0, 1.5f, 0), new Vector3(0.12f, 0.12f, 1.08f), mFrame);
            var beam = Box("ScannerBeam", root, new Vector3(0, 1.43f, 0), new Vector3(0.02f, 0.02f, 0.9f), mLedRed, false);
            conveyor.scannerBeam = beam.GetComponent<Renderer>();
            conveyor.scannerLight = AddLight("ScannerLight", root, new Vector3(0, 1.3f, 0), LightType.Point, Color.red, 0.8f, 2f);

            var items = new Transform[6];
            float spacing = 7.4f / items.Length;
            for (int i = 0; i < items.Length; i++)
            {
                float size = i % 3 == 1 ? 0.34f : 0.4f;
                var crate = Box($"Crate_{i}", root, new Vector3(-3.4f + i * spacing, 0.85f + size / 2, Range(-0.08f, 0.08f)),
                    Vector3.one * size, i % 3 == 1 ? mCardboard : mCrate, euler: new Vector3(0, Range(-8f, 8f), 0));
                crate.AddComponent<Rigidbody>().isKinematic = true;
                items[i] = crate.transform;
            }

            conveyor.belt = belt.GetComponent<Renderer>();
            conveyor.beltLength = 8.4f;
            conveyor.beltTiling = new Vector2(8.4f, 1f);
            conveyor.items = items;
            conveyor.rollers = new Transform[0];
            conveyor.minX = -3.7f;
            conveyor.maxX = 3.7f;
        }

        // ------------------------------------------------------------------ workstations

        static void BuildWorkstations(Transform lab)
        {
            float[] zs = { -3.4f, -0.6f, 2.2f };
            for (int i = 0; i < zs.Length; i++)
            {
                // Desk faces +x (towards the east wall); the user sits on its west side.
                var desk = Group($"Desk_{i}", lab, new Vector3(9.35f, 0, zs[i]), 90f);
                Box("Top", desk, new Vector3(0, 0.75f, 0), new Vector3(1.8f, 0.05f, 0.85f), mWood);
                foreach (float x in new[] { -0.85f, 0.85f })
                foreach (float z in new[] { -0.38f, 0.38f })
                    Box("Leg", desk, new Vector3(x, 0.365f, z), new Vector3(0.05f, 0.73f, 0.05f), mFrame);
                Box("ModestyPanel", desk, new Vector3(0, 0.5f, 0.4f), new Vector3(1.7f, 0.4f, 0.02f), mMachine);
                Box("Keyboard", desk, new Vector3(0, 0.785f, -0.15f), new Vector3(0.45f, 0.02f, 0.15f), mDarkPlastic, false);
                Box("Mouse", desk, new Vector3(0.33f, 0.785f, -0.15f), new Vector3(0.06f, 0.025f, 0.1f), mDarkPlastic, false);
                Cyl("Mug", desk, new Vector3(-0.6f, 0.825f, -0.05f), 0.04f, 0.1f, i == 1 ? mRed : mWhitePlastic, false);
                Box("Tower", desk, new Vector3(0.7f, 0.225f, 0.1f), new Vector3(0.2f, 0.45f, 0.45f), mDarkPlastic);
                Box("TowerLed", desk, new Vector3(0.7f, 0.4f, -0.126f), new Vector3(0.12f, 0.01f, 0.005f), mLedBlue, false);
                Chair(lab, desk.TransformPoint(new Vector3(0, 0, -0.8f)), 90f + Range(-25f, 25f));
                Monitor(desk, i);
            }
        }

        static void Monitor(Transform desk, int index)
        {
            // Lives under Interactive (not static) but is placed relative to its desk.
            var root = Group($"Monitor_{index}", interactive);
            root.SetPositionAndRotation(desk.position, desk.rotation);
            var monitor = root.gameObject.AddComponent<MonitorScreen>();
            monitor.displayName = "Workstation monitor";
            Box("Base", root, new Vector3(0, 0.785f, 0.2f), new Vector3(0.25f, 0.02f, 0.18f), mFrame);
            Box("Neck", root, new Vector3(0, 0.95f, 0.26f), new Vector3(0.05f, 0.33f, 0.04f), mFrame);
            Box("Bezel", root, new Vector3(0, 1.2f, 0.22f), new Vector3(0.95f, 0.56f, 0.04f), mDarkPlastic);
            var screen = Quad("Screen", root, new Vector3(0, 1.2f, 0.198f), new Vector2(0.9f, 0.506f), mScreen, Vector3.back);
            monitor.screen = screen.GetComponent<Renderer>();
            monitor.pages = new[] { tex["Screen_Code"], tex["Screen_Dashboard"], tex["Screen_Map"] };
            monitor.page = index % 3;
        }

        static void Chair(Transform parent, Vector3 worldPosition, float yaw)
        {
            var c = Group("OfficeChair", parent, parent.InverseTransformPoint(worldPosition), yaw);
            for (int k = 0; k < 5; k++)
            {
                var leg = Group("Leg", c, Vector3.zero, k * 72f);
                Box("Spoke", leg, new Vector3(0, 0.07f, 0.16f), new Vector3(0.05f, 0.03f, 0.32f), mFrame, false);
                Ball("Caster", leg, new Vector3(0, 0.03f, 0.3f), 0.06f, mDarkPlastic, false);
            }
            Cyl("Column", c, new Vector3(0, 0.27f, 0), 0.03f, 0.38f, mSteel);
            Box("Seat", c, new Vector3(0, 0.48f, 0), new Vector3(0.5f, 0.08f, 0.48f), mFabric);
            Box("BackSupport", c, new Vector3(0, 0.6f, -0.25f), new Vector3(0.05f, 0.3f, 0.03f), mFrame, false);
            Box("Back", c, new Vector3(0, 0.85f, -0.26f), new Vector3(0.46f, 0.5f, 0.05f), mFabric, euler: new Vector3(-8, 0, 0));
            Box("ArmL", c, new Vector3(-0.27f, 0.66f, 0), new Vector3(0.04f, 0.04f, 0.32f), mDarkPlastic, false);
            Box("ArmR", c, new Vector3(0.27f, 0.66f, 0), new Vector3(0.04f, 0.04f, 0.32f), mDarkPlastic, false);
        }

        // ------------------------------------------------------------------ centre of the lab

        static void BuildCentrePieces(Transform lab)
        {
            BuildGlobe();
            BuildExhibit();

            // Workbench with loose parts the player can pick up.
            var bench = Group("Workbench", lab, new Vector3(1.2f, 0, -3.4f));
            Box("Top", bench, new Vector3(0, 0.9f, 0), new Vector3(2.2f, 0.06f, 0.9f), mWood);
            foreach (float x in new[] { -1.02f, 1.02f })
            foreach (float z in new[] { -0.38f, 0.38f })
                Box("Leg", bench, new Vector3(x, 0.435f, z), new Vector3(0.07f, 0.87f, 0.07f), mFrame);
            Box("LowerShelf", bench, new Vector3(0, 0.25f, 0), new Vector3(2.1f, 0.03f, 0.8f), mSteel);
            Box("StoredBox1", bench, new Vector3(-0.6f, 0.41f, 0), new Vector3(0.5f, 0.3f, 0.4f), mCardboard);
            Box("StoredBox2", bench, new Vector3(0.1f, 0.38f, 0.05f), new Vector3(0.4f, 0.24f, 0.35f), mBlue);
            Box("Pegboard", bench, new Vector3(0, 1.45f, -0.43f), new Vector3(2.2f, 1.0f, 0.03f), mMachine, false);
            Box("Spanner", bench, new Vector3(-0.6f, 1.5f, -0.41f), new Vector3(0.04f, 0.32f, 0.01f), mSteel, false, new Vector3(0, 0, 20));
            Box("Hammer", bench, new Vector3(-0.3f, 1.5f, -0.41f), new Vector3(0.03f, 0.35f, 0.01f), mWood, false);
            Box("HammerHead", bench, new Vector3(-0.3f, 1.66f, -0.41f), new Vector3(0.12f, 0.04f, 0.03f), mFrame, false);
            Box("Saw", bench, new Vector3(0.3f, 1.45f, -0.41f), new Vector3(0.45f, 0.12f, 0.01f), mSteel, false);

            float top = 0.93f;
            var crate = Box("PartsCrate", interactive, new Vector3(0.4f, top + 0.15f, -3.35f), Vector3.one * 0.3f, mCrate);
            AddRigidbody<Grabbable>(crate, 2f).displayName = "Parts crate";

            var ball = Ball("RubberBall", interactive, new Vector3(0.8f, top + 0.12f, -3.25f), 0.22f, mRed);
            ball.GetComponent<Collider>().sharedMaterial = BouncyMaterial();
            AddRigidbody<Grabbable>(ball, 0.4f).displayName = "Rubber ball";

            var cone = Group("TrafficCone", interactive, new Vector3(1.15f, top, -3.45f));
            Box("Base", cone, new Vector3(0, 0.015f, 0), new Vector3(0.32f, 0.03f, 0.32f), mDarkPlastic);
            MeshObject("Cone", cone, coneMesh, new Vector3(0, 0.03f, 0), new Vector3(0.28f, 0.45f, 0.28f), mOrange, true);
            Cyl("Band", cone, new Vector3(0, 0.22f, 0), 0.085f, 0.06f, mWhitePlastic, false);
            AddRigidbody<Grabbable>(cone.gameObject, 0.8f).displayName = "Traffic cone";

            var battery = Group("BatteryPack", interactive, new Vector3(1.55f, top, -3.3f));
            Box("Case", battery, new Vector3(0, 0.06f, 0), new Vector3(0.3f, 0.12f, 0.18f), mDarkPlastic);
            Box("Gauge", battery, new Vector3(0, 0.08f, -0.091f), new Vector3(0.2f, 0.02f, 0.004f), mLedGreen, false);
            AddRigidbody<Grabbable>(battery.gameObject, 3f).displayName = "Battery pack";

            var toolbox = Group("Toolbox", interactive, new Vector3(1.98f, top, -3.4f));
            Box("Case", toolbox, new Vector3(0, 0.1f, 0), new Vector3(0.42f, 0.2f, 0.22f), mRed);
            Box("HandlePostL", toolbox, new Vector3(-0.12f, 0.23f, 0), new Vector3(0.02f, 0.06f, 0.02f), mDarkPlastic, false);
            Box("HandlePostR", toolbox, new Vector3(0.12f, 0.23f, 0), new Vector3(0.02f, 0.06f, 0.02f), mDarkPlastic, false);
            Box("Handle", toolbox, new Vector3(0, 0.265f, 0), new Vector3(0.28f, 0.02f, 0.03f), mDarkPlastic, false);
            AddRigidbody<Grabbable>(toolbox.gameObject, 2.5f).displayName = "Toolbox";

            var servo = Group("ServoMotor", interactive, new Vector3(0.75f, top, -3.68f));
            Box("Mount", servo, new Vector3(0, 0.03f, 0), new Vector3(0.18f, 0.06f, 0.12f), mFrame);
            Cyl("Body", servo, new Vector3(0, 0.13f, 0), 0.065f, 0.14f, mSteel);
            Cyl("Shaft", servo, new Vector3(0, 0.22f, 0), 0.012f, 0.05f, mSteel, false);
            AddRigidbody<Grabbable>(servo.gameObject, 1.2f).displayName = "Servo motor";
        }

        static PhysicsMaterial BouncyMaterial()
        {
            var pm = new PhysicsMaterial("Bouncy") { bounciness = 0.65f, bounceCombine = PhysicsMaterialCombine.Maximum, dynamicFriction = 0.4f };
            UnityEditor.AssetDatabase.CreateAsset(pm, $"{MaterialFolder}/Bouncy.physicMaterial");
            return pm;
        }

        static void BuildGlobe()
        {
            var root = Group("HoloGlobe", interactive, new Vector3(2.5f, 0, 1.2f));
            var globe = root.gameObject.AddComponent<HologramGlobe>();
            globe.displayName = "Holographic globe";
            Cyl("Base", root, new Vector3(0, 0.06f, 0), 0.55f, 0.12f, mFrame);
            Cyl("Column", root, new Vector3(0, 0.5f, 0), 0.12f, 0.76f, mSteel);
            Cyl("Emitter", root, new Vector3(0, 0.9f, 0), 0.42f, 0.05f, mEmitter);
            var sphere = Ball("Globe", root, new Vector3(0, 1.55f, 0), 0.85f, mGlobe);
            NoShadows(sphere);
            var ring1 = MeshObject("Ring1", root, torusMesh, new Vector3(0, 1.55f, 0), Vector3.one, mEmitter, euler: new Vector3(70, 0, 0));
            var ring2 = MeshObject("Ring2", root, torusMesh, new Vector3(0, 1.55f, 0), Vector3.one * 1.12f, mEmitter, euler: new Vector3(-60, 40, 0));
            AddLight("GlobeLight", root, new Vector3(0, 1.55f, 0), LightType.Point, new Color(0.3f, 0.85f, 1f), 1.5f, 4f);
            globe.globe = sphere.transform;
            globe.rings = new[] { ring1.transform, ring2.transform };
            globe.ringAxes = new[] { new Vector3(0.2f, 1f, 0.1f).normalized, new Vector3(1f, 0.3f, 0f).normalized };
        }

        static void BuildExhibit()
        {
            var root = Group("RoverExhibit", interactive, new Vector3(5.0f, 0, 3.6f));
            var display = root.gameObject.AddComponent<RotatingDisplay>();
            display.displayName = "Rover prototype";
            Box("Pedestal", root, new Vector3(0, 0.35f, 0), new Vector3(1.0f, 0.7f, 1.0f), mWhitePlastic);
            Box("PedestalTrim", root, new Vector3(0, 0.69f, 0), new Vector3(1.02f, 0.02f, 1.02f), mLedBlue, false);
            var platform = Group("Platform", root, new Vector3(0, 0.72f, 0), 30f);
            Cyl("Disc", platform, Vector3.zero, 0.46f, 0.04f, mFrame);
            Box("Chassis", platform, new Vector3(0, 0.15f, 0), new Vector3(0.5f, 0.1f, 0.34f), mWhitePlastic);
            foreach (float x in new[] { -0.18f, 0f, 0.18f })
            foreach (float z in new[] { -0.21f, 0.21f })
                Cyl("Wheel", platform, new Vector3(x, 0.08f, z), 0.075f, 0.06f, mDarkPlastic, false, new Vector3(90, 0, 0));
            Cyl("Mast", platform, new Vector3(0.15f, 0.35f, 0), 0.02f, 0.3f, mSteel, false);
            Box("CameraHead", platform, new Vector3(0.15f, 0.52f, 0), new Vector3(0.12f, 0.07f, 0.1f), mWhitePlastic, false);
            Cyl("Lens", platform, new Vector3(0.22f, 0.52f, 0), 0.025f, 0.02f, mLedBlue, false, new Vector3(0, 0, 90));
            Box("SolarPanel", platform, new Vector3(-0.06f, 0.23f, 0), new Vector3(0.42f, 0.015f, 0.32f), mSolar, false);
            Cyl("Antenna", platform, new Vector3(-0.2f, 0.4f, 0.12f), 0.006f, 0.3f, mSteel, false);
            display.platform = platform;
        }
    }
}
