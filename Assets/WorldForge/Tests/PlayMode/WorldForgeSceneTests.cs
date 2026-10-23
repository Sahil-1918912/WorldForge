using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace WorldForge.Tests
{
    /// <summary>
    /// Loads the real WorldForge scene and checks each assignment requirement:
    /// 3D objects, materials / lighting, camera navigation and the interactions.
    /// Keyboard input is simulated with the Input System test fixture.
    /// </summary>
    public class WorldForgeSceneTests : InputTestFixture
    {
        Keyboard keyboard;
        WorldForgeGame game;

        public override void Setup()
        {
            // The fixture resets the input system in Setup, so virtual devices must be added after it.
            base.Setup();
            // Batch-mode runs have no window focus; keep held keys pressed anyway.
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            keyboard = InputSystem.AddDevice<Keyboard>();
            InputSystem.AddDevice<Mouse>();
        }

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            SceneManager.LoadScene("WorldForge");
            yield return null;
            yield return null;
            game = WorldForgeGame.Instance;
            Assert.IsNotNull(game, "WorldForgeGame not found - was the scene built?");
        }

        [Test]
        public void Scene_HasManyTexturedObjectsAndLights()
        {
            var renderers = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
            Assert.Greater(renderers.Length, 100, "expected a richly populated scene");

            var textured = renderers.Select(r => r.sharedMaterial)
                .Where(m => m != null && m.HasProperty("_BaseMap") && m.GetTexture("_BaseMap") != null)
                .Distinct().Count();
            Assert.GreaterOrEqual(textured, 10, "expected at least ten distinct textured materials");

            var lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            Assert.IsTrue(lights.Any(l => l.type == LightType.Directional), "sun light");
            Assert.GreaterOrEqual(lights.Count(l => l.type == LightType.Spot), 6, "ceiling spot lights");
            Assert.IsNotNull(RenderSettings.skybox, "skybox");
        }

        [UnityTest]
        public IEnumerator Player_WalksForwardWithW()
        {
            var player = game.player;
            player.Teleport(new Vector3(4.2f, 0.05f, -5.0f), 0f);
            yield return new WaitForSeconds(0.2f);
            Vector3 start = player.transform.position;

            Press(keyboard.wKey);
            for (float t = 0; t < 1f; t += Time.deltaTime)
            {
                Debug.Log($"WFDBG t={t:0.00} dt={Time.deltaTime:0.000} w={keyboard.wKey.isPressed} cur={Keyboard.current == keyboard} pos={player.transform.position} grounded={player.GetComponent<CharacterController>().isGrounded} focus={Application.isFocused}");
                yield return null;
            }
            Release(keyboard.wKey);

            float moved = player.transform.position.z - start.z;
            Assert.Greater(moved, 1.5f, $"player should walk forward, moved {moved:0.00} m");
        }

        [UnityTest]
        public IEnumerator Player_IsBlockedByWalls()
        {
            var player = game.player;
            player.Teleport(new Vector3(8.0f, 0.05f, 5.5f), 90f);
            Press(keyboard.wKey);
            yield return new WaitForSeconds(1.5f);
            Release(keyboard.wKey);
            Assert.Greater(player.transform.position.x, 9.4f, "player should reach the wall");
            Assert.Less(player.transform.position.x, 9.75f, "east wall should stop the player");
        }

        [UnityTest]
        public IEnumerator V_TogglesThirdPersonCamera()
        {
            Assert.IsFalse(game.player.ThirdPerson);
            PressAndRelease(keyboard.vKey);
            yield return null;
            yield return null;
            Assert.IsTrue(game.player.ThirdPerson);
            float distance = Vector3.Distance(game.player.playerCamera.transform.position, game.player.cameraPivot.position);
            Assert.Greater(distance, 1.0f, "camera should sit behind the avatar");
        }

        [UnityTest]
        public IEnumerator RobotArm_MovesCrateBetweenPads()
        {
            var arm = Object.FindFirstObjectByType<RobotArm>();
            Vector3 start = arm.crate.position;
            arm.speedMultiplier = 4f;
            arm.StartCycle();
            yield return new WaitUntil(() => !arm.Busy);
            Assert.AreEqual(1, arm.CyclesCompleted);
            Assert.Less(Vector3.Distance(arm.crate.position, arm.padB.position), 0.05f, "crate should end on pad B");
            Assert.Greater(Vector3.Distance(arm.crate.position, start), 1f);
        }

        [UnityTest]
        public IEnumerator Interact_E_OpensDoorWhenLookingAtIt()
        {
            var player = game.player;
            var door = Object.FindFirstObjectByType<SlidingDoor>();
            player.Teleport(new Vector3(6f, 0.05f, 5.4f), 0f, 10f);
            yield return null;
            yield return null;
            Assert.AreEqual(door, game.interactor.Hovered, "crosshair should be on the door");

            PressAndRelease(keyboard.eKey);
            yield return new WaitForSeconds(1.5f);
            Assert.Greater(door.OpenAmount, 0.99f);
        }

        [UnityTest]
        public IEnumerator Grabbable_CanBeCarriedAndDropped()
        {
            var item = Object.FindObjectsByType<Grabbable>(FindObjectsSortMode.None).First(g => g.displayName == "Parts crate");
            game.player.Teleport(new Vector3(0.4f, 0.05f, -2.0f), 180f, 10f);
            game.interactor.Grab(item);
            yield return new WaitForSeconds(0.5f);
            game.player.Teleport(new Vector3(0.4f, 0.05f, -2.0f), 0f, 0f);
            yield return new WaitForSeconds(0.5f);
            Assert.Greater(item.transform.position.z, -1.0f, "held item follows the camera");

            game.interactor.Drop(Vector3.zero);
            yield return new WaitForSeconds(1.5f);
            Assert.IsFalse(item.IsHeld);
            Assert.Less(item.transform.position.y, 0.4f, "dropped item falls to the floor");
        }

        [UnityTest]
        public IEnumerator LightSwitch_And_DayNight_Toggle()
        {
            var lights = Object.FindFirstObjectByType<LightSwitch>();
            lights.Interact(game.interactor);
            Assert.IsFalse(lights.lights.Any(l => l.enabled));
            lights.Interact(game.interactor);
            Assert.IsTrue(lights.lights.All(l => l.enabled));

            float daySun = RenderSettings.sun.intensity;
            PressAndRelease(keyboard.nKey);
            yield return new WaitForSeconds(2f);
            Assert.Less(RenderSettings.sun.intensity, daySun * 0.5f, "night should dim the sun");
            Assert.IsTrue(game.dayNight.nightLights.All(l => l.enabled), "lamp posts on at night");
        }

        [UnityTest]
        public IEnumerator T_PlaysGuidedTour()
        {
            PressAndRelease(keyboard.tKey);
            yield return new WaitForSeconds(1f);
            Assert.IsTrue(game.tour.Playing);
            Assert.IsTrue(game.player.ExternalCameraControl);
            PressAndRelease(keyboard.tKey);
            yield return null;
            yield return null;
            Assert.IsFalse(game.tour.Playing);
            Assert.IsFalse(game.player.ExternalCameraControl);
        }
    }
}
