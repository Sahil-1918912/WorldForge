#if WORLDFORGE_RECORDER
using System.IO;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Encoder;
using UnityEditor.Recorder.Input;
using UnityEngine;

namespace WorldForge.EditorTools
{
    /// <summary>
    /// One-click video export of the guided tour with Unity Recorder.
    /// Menu: WorldForge > Record Guided Tour (QHD / Full HD). Enters Play mode, starts the tour,
    /// records the Game view at a fixed 60 fps, then stops and leaves Play mode when the tour ends.
    /// Output: &lt;project&gt;/Recordings/. Linux only supports VP8 WebM; Windows / macOS get H.264 MP4.
    /// </summary>
    [InitializeOnLoad]
    static class TourRecorder
    {
        const string PendingKey = "WorldForge.TourRecorder.Pending";
        const string WidthKey = "WorldForge.TourRecorder.Width";
        const string HeightKey = "WorldForge.TourRecorder.Height";

        static RecorderController controller;
        static int warmupFrames;

        static TourRecorder() => EditorApplication.playModeStateChanged += OnPlayModeChanged;

        [MenuItem("WorldForge/Record Guided Tour (QHD 2560x1440)", priority = 40)]
        static void RecordQhd() => Begin(2560, 1440);

        [MenuItem("WorldForge/Record Guided Tour (Full HD 1920x1080)", priority = 41)]
        static void RecordFullHd() => Begin(1920, 1080);

        static void Begin(int width, int height)
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("WorldForge", "Stop Play mode first, then choose the record menu item again.", "OK");
                return;
            }
            SessionState.SetBool(PendingKey, true);
            SessionState.SetInt(WidthKey, width);
            SessionState.SetInt(HeightKey, height);
            EditorApplication.EnterPlaymode();
        }

        static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(PendingKey, false))
            {
                SessionState.SetBool(PendingKey, false);
                warmupFrames = 30; // let reflection probes and the scene settle before rolling
                EditorApplication.update += Tick;
            }
            else if (state == PlayModeStateChange.ExitingPlayMode)
            {
                StopRecording();
            }
        }

        static void Tick()
        {
            var game = WorldForgeGame.Instance;
            if (!EditorApplication.isPlaying || game == null) return;

            if (controller == null)
            {
                if (--warmupFrames > 0) return;
                StartRecording(SessionState.GetInt(WidthKey, 2560), SessionState.GetInt(HeightKey, 1440));
                game.tour.Play();
                return;
            }

            if (!game.tour.Playing)
            {
                StopRecording();
                EditorApplication.ExitPlaymode();
            }
        }

        static void StartRecording(int width, int height)
        {
            var settings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
            var movie = ScriptableObject.CreateInstance<MovieRecorderSettings>();
            movie.name = "WorldForge Tour";
            movie.Enabled = true;
            movie.CaptureAudio = false;
            movie.EncoderSettings = new CoreEncoderSettings
            {
                Codec = Application.platform == RuntimePlatform.LinuxEditor
                    ? CoreEncoderSettings.OutputCodec.WEBM
                    : CoreEncoderSettings.OutputCodec.MP4,
                EncodingQuality = CoreEncoderSettings.VideoEncodingQuality.High,
            };
            movie.ImageInputSettings = new GameViewInputSettings { OutputWidth = width, OutputHeight = height };

            string folder = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Recordings"));
            Directory.CreateDirectory(folder);
            movie.OutputFile = Path.Combine(folder, $"WorldForge_Tour_{height}p_{System.DateTime.Now:yyyyMMdd_HHmmss}");

            settings.AddRecorderSettings(movie);
            settings.SetRecordModeToManual();
            settings.FrameRatePlayback = FrameRatePlayback.Constant;
            settings.FrameRate = 60f;
            settings.CapFrameRate = true;

            controller = new RecorderController(settings);
            controller.PrepareRecording();
            controller.StartRecording();
            Debug.Log($"[WorldForge] Recording guided tour at {width}x{height} -> {movie.OutputFile}");
        }

        static void StopRecording()
        {
            EditorApplication.update -= Tick;
            if (controller == null) return;
            if (controller.IsRecording()) controller.StopRecording();
            controller = null;
            Debug.Log("[WorldForge] Recording finished - see the Recordings folder in the project.");
            EditorUtility.RevealInFinder(Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Recordings")));
        }
    }
}
#endif
