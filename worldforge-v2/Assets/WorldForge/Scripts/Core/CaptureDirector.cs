using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;

namespace WorldForge
{
    /// <summary>
    /// Automated smoke test + screenshot set for the report. Only active when the player is
    /// started with  -wfCapture &lt;output folder&gt;. It exercises every interaction, visits fixed
    /// viewpoints, writes PNGs and a log of checks, then quits.
    /// </summary>
    public class CaptureDirector : MonoBehaviour
    {
        StringBuilder log;
        string outDir;
        int failures;

        IEnumerator Start()
        {
            outDir = ArgumentAfter("-wfCapture");
            if (outDir == null)
            {
                enabled = false;
                yield break;
            }
            Directory.CreateDirectory(outDir);
            log = new StringBuilder();

            var game = WorldForgeGame.Instance;
            var player = game.player;
            var hud = game.hud;
            var arm = FindFirstObjectByType<RobotArm>();
            var conveyor = FindFirstObjectByType<ConveyorBelt>();
            var lights = FindFirstObjectByType<LightSwitch>();
            var grabbable = FindFirstObjectByType<Grabbable>();

            Check("Renderers in scene >= 5", FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Length >= 5);
            yield return Frames(30);

            // --- Interactions -------------------------------------------------------------
            Vector3 crateStart = arm.crate.position;
            arm.speedMultiplier = 3f;
            arm.StartCycle();
            float t0 = Time.time;
            while (arm.Busy && Time.time - t0 < 15f) yield return null;
            Check("Robot arm completed a pick & place cycle", arm.CyclesCompleted == 1);
            Check("Robot arm moved the crate to the other pad", Vector3.Distance(arm.crate.position, crateStart) > 1f);
            arm.speedMultiplier = 1f;

            float before = conveyor.items[0].localPosition.x;
            yield return new WaitForSeconds(0.5f);
            Check("Conveyor moves crates", !Mathf.Approximately(before, conveyor.items[0].localPosition.x));

            lights.Set(false);
            Check("Light switch turns lights off", !lights.lights[0].enabled);
            lights.Set(true);

            // --- Walk test (scripted, through the CharacterController) ------------------
            var cc = player.GetComponent<CharacterController>();
            player.Teleport(new Vector3(4.2f, 0.05f, -5.0f), 0f);
            yield return Frames(5);
            Vector3 p0 = player.transform.position;
            for (int i = 0; i < 30; i++)
            {
                cc.Move(player.transform.forward * 3f * Time.deltaTime + Vector3.down * 0.5f * Time.deltaTime);
                yield return null;
            }
            Check("Player can walk", Vector3.Distance(p0, player.transform.position) > 0.5f);
            player.Teleport(new Vector3(5f, 0.05f, 5.5f), 0f);
            for (int i = 0; i < 40; i++)
            {
                cc.Move(Vector3.forward * 4f * Time.deltaTime);
                yield return null;
            }
            Check("Glass wall blocks the player", player.transform.position.z < 7f);

            // --- Screenshots ------------------------------------------------------------
            arm.SetAuto(true);
            hud.visible = false;
            player.SetExternalCameraControl(true);
            var cam = player.playerCamera.transform;

            yield return Shot(cam, "01_interior_overview", new Vector3(8.4f, 3.1f, -4.7f), new Vector3(-3f, 0.4f, 2.6f));
            yield return Shot(cam, "02_robot_cell", new Vector3(-1.6f, 2.0f, 2.9f), new Vector3(-4.8f, 0.9f, 0.4f));
            yield return Shot(cam, "03_conveyor", new Vector3(-2.6f, 1.9f, -2.3f), new Vector3(-5.6f, 0.8f, -5.3f));
            yield return Shot(cam, "04_workstations", new Vector3(5.6f, 1.7f, -2.6f), new Vector3(9.4f, 1.0f, 0.4f));
            yield return Shot(cam, "05_window_view", new Vector3(-6f, 1.7f, 2.5f), new Vector3(2f, 1.0f, 12f));
            yield return Shot(cam, "06_exterior", new Vector3(14f, 4f, 22f), new Vector3(0f, 1.5f, 3f));

            // Gameplay views with the HUD visible.
            player.SetExternalCameraControl(false);
            hud.visible = true;
            hud.showHelp = true;
            arm.SetAuto(false);
            yield return new WaitForSeconds(4f);
            player.Teleport(new Vector3(-2.4f, 0.05f, 0.5f), -90f, 5f);
            yield return Frames(20);
            yield return Capture("07_fp_interact_prompt");

            player.Teleport(new Vector3(1.2f, 0.05f, -1.8f), 180f, 20f);
            yield return Frames(10);
            game.interactor.Grab(grabbable);
            player.Teleport(new Vector3(1.2f, 0.05f, -1.8f), 20f, 5f);
            yield return new WaitForSeconds(1f);
            Check("Player holds an object", game.interactor.Held == grabbable);
            yield return Capture("08_fp_holding_object");
            game.interactor.Drop(Vector3.zero);
            yield return new WaitForSeconds(1.5f);
            Check("Dropped object falls back down", grabbable.transform.position.y < 1.2f);

            hud.showHelp = false;
            player.SetThirdPerson(true);
            player.Teleport(new Vector3(0.4f, 0.05f, -1.0f), -75f, 12f);
            yield return Frames(30);
            yield return Capture("09_third_person");
            player.SetThirdPerson(false);

            game.tour.metresPerSecond = 6f;
            game.tour.Play();
            yield return new WaitForSeconds(3f);
            Check("Guided tour is playing", game.tour.Playing);
            yield return Capture("10_guided_tour");
            game.tour.Stop();

            log.AppendLine(failures == 0 ? "ALL CHECKS PASSED" : $"{failures} CHECK(S) FAILED");
            File.WriteAllText(Path.Combine(outDir, "capture_log.txt"), log.ToString());
            Debug.Log("[WorldForge] capture finished\n" + log);
            yield return Frames(5);
            Application.Quit(failures == 0 ? 0 : 1);
        }

        IEnumerator Shot(Transform cam, string name, Vector3 position, Vector3 lookAt)
        {
            cam.SetPositionAndRotation(position, Quaternion.LookRotation(lookAt - position));
            yield return Frames(15);
            yield return Capture(name);
        }

        IEnumerator Capture(string name)
        {
            yield return new WaitForEndOfFrame();
            var tex = ScreenCapture.CaptureScreenshotAsTexture();
            File.WriteAllBytes(Path.Combine(outDir, name + ".png"), tex.EncodeToPNG());
            Destroy(tex);
            log.AppendLine("captured " + name);
        }

        void Check(string label, bool ok)
        {
            if (!ok) failures++;
            log.AppendLine((ok ? "PASS  " : "FAIL  ") + label);
        }

        static IEnumerator Frames(int n)
        {
            for (int i = 0; i < n; i++) yield return null;
        }

        static string ArgumentAfter(string flag)
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
                if (args[i] == flag) return args[i + 1];
            return null;
        }
    }
}
