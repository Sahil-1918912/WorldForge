using System;
using System.Collections;
using UnityEngine;

namespace WorldForge
{
    /// <summary>
    /// Guided camera fly-through for recording a smooth video. The camera follows a Catmull-Rom
    /// spline through the shot positions while looking at a second spline of targets.
    /// Press T to start / stop.
    /// </summary>
    public class CameraTour : MonoBehaviour
    {
        [Serializable]
        public struct Shot
        {
            public Vector3 position;
            public Vector3 lookAt;
        }

        public Shot[] shots;
        public float metresPerSecond = 1.7f;
        public PlayerController player;
        public RobotArm arm;
        public ConveyorBelt conveyor;
        public LightSwitch lights;

        public bool Playing { get; private set; }
        public float Progress { get; private set; }

        Coroutine routine;

        public void Play()
        {
            if (Playing || shots == null || shots.Length < 2) return;
            routine = StartCoroutine(Run());
        }

        public void Stop()
        {
            if (!Playing) return;
            if (routine != null) StopCoroutine(routine);
            Finish();
        }

        IEnumerator Run()
        {
            Playing = true;
            Progress = 0f;
            player.SetExternalCameraControl(true);

            // Make sure everything worth filming is running.
            if (arm != null) arm.SetAuto(true);
            if (conveyor != null) conveyor.running = true;
            if (lights != null) lights.Set(true);

            var cam = player.playerCamera.transform;
            int n = shots.Length;
            var cumulative = new float[n];
            for (int i = 1; i < n; i++)
                cumulative[i] = cumulative[i - 1] + Vector3.Distance(shots[i - 1].position, shots[i].position);
            float total = cumulative[n - 1];
            float duration = total / metresPerSecond;

            cam.SetPositionAndRotation(shots[0].position, Quaternion.LookRotation(shots[0].lookAt - shots[0].position));
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                Progress = t / duration;
                // Ease in and out of the whole move, constant speed in between.
                float distance = EaseInOut(Progress) * total;
                int seg = 0;
                while (seg < n - 2 && cumulative[seg + 1] < distance) seg++;
                float u = Mathf.InverseLerp(cumulative[seg], cumulative[seg + 1], distance);

                Vector3 pos = Spline(seg, u, s => s.position);
                Vector3 look = Spline(seg, u, s => s.lookAt);
                cam.position = pos;
                cam.rotation = Quaternion.Slerp(cam.rotation, Quaternion.LookRotation(look - pos), 1f - Mathf.Exp(-4f * Time.deltaTime));
                yield return null;
            }
            Finish();
        }

        void Finish()
        {
            Playing = false;
            Progress = 1f;
            routine = null;
            player.SetExternalCameraControl(false);
        }

        Vector3 Spline(int seg, float u, Func<Shot, Vector3> get)
        {
            Vector3 p0 = get(shots[Mathf.Max(seg - 1, 0)]);
            Vector3 p1 = get(shots[seg]);
            Vector3 p2 = get(shots[seg + 1]);
            Vector3 p3 = get(shots[Mathf.Min(seg + 2, shots.Length - 1)]);
            float u2 = u * u, u3 = u2 * u;
            return 0.5f * (2f * p1 + (p2 - p0) * u + (2f * p0 - 5f * p1 + 4f * p2 - p3) * u2 + (3f * p1 - p0 - 3f * p2 + p3) * u3);
        }

        static float EaseInOut(float x)
        {
            // Accelerate over the first 6 %, cruise, decelerate over the last 6 %.
            const float edge = 0.06f;
            const float cruise = 1f / (1f - edge);
            if (x < edge) return cruise * x * x / (2f * edge);
            if (x > 1f - edge) return 1f - cruise * (1f - x) * (1f - x) / (2f * edge);
            return cruise * (x - edge * 0.5f);
        }
    }
}
