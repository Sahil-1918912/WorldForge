using UnityEngine;

namespace WorldForge
{
    /// <summary>Flies the garden drone in a gently bobbing circle around its parent.</summary>
    public class DroneFlight : MonoBehaviour
    {
        public Transform body;
        public float radius = 5.5f;
        public float height = 3.6f;
        public float degreesPerSecond = 22f;
        public float bobAmplitude = 0.25f;

        void Update()
        {
            float a = Time.time * degreesPerSecond * Mathf.Deg2Rad;
            var offset = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * radius;
            body.localPosition = offset + Vector3.up * (height + Mathf.Sin(Time.time * 1.7f) * bobAmplitude);

            // Face along the direction of travel, banking slightly into the turn.
            var tangent = new Vector3(-Mathf.Sin(a), 0f, Mathf.Cos(a));
            body.localRotation = Quaternion.LookRotation(tangent) * Quaternion.Euler(4f, 0f, 10f);
        }
    }
}
