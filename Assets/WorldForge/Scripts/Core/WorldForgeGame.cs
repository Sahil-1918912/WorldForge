using System;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

namespace WorldForge
{
    /// <summary>
    /// Scene-level hub: holds references to the main systems and handles the global keys
    /// (T guided tour, N day/night, H help, U hide UI, P screenshot).
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class WorldForgeGame : MonoBehaviour
    {
        public static WorldForgeGame Instance { get; private set; }

        public PlayerController player;
        public PlayerInteractor interactor;
        public CameraTour tour;
        public DayNightController dayNight;
        public HudOverlay hud;

        void Awake() => Instance = this;

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public static void Toast(string message)
        {
            if (Instance != null && Instance.hud != null) Instance.hud.ShowToast(message);
        }

        void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.tKey.wasPressedThisFrame)
            {
                if (tour.Playing) tour.Stop();
                else tour.Play();
            }
            if (keyboard.nKey.wasPressedThisFrame) dayNight.Toggle();
            if (keyboard.hKey.wasPressedThisFrame) hud.showHelp = !hud.showHelp;
            if (keyboard.uKey.wasPressedThisFrame) hud.visible = !hud.visible;
            if (keyboard.pKey.wasPressedThisFrame || keyboard.f12Key.wasPressedThisFrame)
                Toast("Saved " + Path.GetFileName(TakeScreenshot()));
        }

        /// <summary>Saves a PNG to &lt;project or build folder&gt;/Screenshots and returns its path.</summary>
        public static string TakeScreenshot(string directory = null, string fileName = null)
        {
            directory ??= Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Screenshots"));
            Directory.CreateDirectory(directory);
            fileName ??= $"WorldForge_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            string path = Path.Combine(directory, fileName);
            ScreenCapture.CaptureScreenshot(path);
            return path;
        }
    }
}
