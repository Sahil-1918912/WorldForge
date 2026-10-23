using UnityEngine;

namespace WorldForge
{
    /// <summary>Pulses a renderer's emission (and an optional light) - server LEDs, drone beacons.</summary>
    public class BlinkingLight : MonoBehaviour
    {
        public Renderer target;
        public Light pointLight;
        public Color color = Color.green;
        public float intensity = 3f;
        public float frequency = 1f;
        [Range(0f, 1f)] public float minimum = 0.15f;
        public bool squareWave;
        public float phase;

        MaterialPropertyBlock block;

        void Update()
        {
            float wave = 0.5f + 0.5f * Mathf.Sin((Time.time * frequency + phase) * Mathf.PI * 2f);
            if (squareWave) wave = wave > 0.5f ? 1f : 0f;
            float k = Mathf.Lerp(minimum, 1f, wave);

            if (target != null)
            {
                block ??= new MaterialPropertyBlock();
                target.GetPropertyBlock(block);
                block.SetColor("_EmissionColor", color * intensity * k);
                target.SetPropertyBlock(block);
            }
            if (pointLight != null) pointLight.intensity = k * 1.5f;
        }
    }
}
