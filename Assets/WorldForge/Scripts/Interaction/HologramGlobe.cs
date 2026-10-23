using UnityEngine;

namespace WorldForge
{
    /// <summary>Spinning holographic globe; each use switches to the next spin speed.</summary>
    public class HologramGlobe : Interactable
    {
        public Transform globe;
        public Transform[] rings;
        public Vector3[] ringAxes;
        public float[] speeds = { 20f, 70f, -40f, 0f };
        public float bobAmplitude = 0.04f;

        int index;
        Vector3 globeRest;

        public int SpeedIndex => index;
        public override string Prompt => "Change spin speed";

        void Awake() => globeRest = globe.localPosition;

        public override void Interact(PlayerInteractor interactor)
        {
            index = (index + 1) % speeds.Length;
            WorldForgeGame.Toast(speeds[index] == 0f ? "Globe paused" : $"Globe spin: {speeds[index]:0} deg/s");
        }

        void Update()
        {
            float speed = speeds[index];
            globe.Rotate(Vector3.up, speed * Time.deltaTime, Space.Self);
            globe.localPosition = globeRest + Vector3.up * Mathf.Sin(Time.time * 1.3f) * bobAmplitude;
            for (int i = 0; i < rings.Length; i++)
            {
                var axis = ringAxes != null && i < ringAxes.Length ? ringAxes[i] : Vector3.up;
                rings[i].Rotate(axis, (speed * 1.5f + 15f) * Time.deltaTime * (i % 2 == 0 ? 1f : -1f), Space.World);
            }
        }
    }
}
