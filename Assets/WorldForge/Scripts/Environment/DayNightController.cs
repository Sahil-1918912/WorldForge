using UnityEngine;

namespace WorldForge
{
    /// <summary>
    /// Blends the whole environment between day and night: sun direction / colour, sky exposure,
    /// ambient light, fog and the garden lamp posts. Toggled with N or by using the wall panel.
    /// </summary>
    public class DayNightController : Interactable
    {
        public Light sun;
        public Light[] nightLights;
        public Renderer[] nightBulbs;
        public Color bulbEmission = new Color(4f, 3.1f, 1.8f);
        public ReflectionProbe[] probes;
        public float transitionSeconds = 1.5f;
        public bool night;

        [Header("Day")]
        public Vector3 daySunEuler = new Vector3(48f, -35f, 0f);
        public Color daySunColor = new Color(1f, 0.95f, 0.86f);
        public float daySunIntensity = 1.7f;
        public float daySkyExposure = 1.25f;
        public Color dayAmbientSky = new Color(0.52f, 0.60f, 0.72f);
        public Color dayAmbientEquator = new Color(0.42f, 0.42f, 0.40f);
        public Color dayAmbientGround = new Color(0.30f, 0.29f, 0.27f);
        public Color dayFog = new Color(0.70f, 0.78f, 0.88f);

        [Header("Night")]
        public Vector3 nightSunEuler = new Vector3(35f, 150f, 0f);
        public Color nightSunColor = new Color(0.55f, 0.65f, 1f);
        public float nightSunIntensity = 0.25f;
        public float nightSkyExposure = 0.12f;
        public Color nightAmbientSky = new Color(0.07f, 0.09f, 0.16f);
        public Color nightAmbientEquator = new Color(0.05f, 0.06f, 0.09f);
        public Color nightAmbientGround = new Color(0.02f, 0.02f, 0.03f);
        public Color nightFog = new Color(0.05f, 0.07f, 0.12f);

        float blend;
        Material sky;
        MaterialPropertyBlock block;

        public override string Prompt => night ? "Switch to daytime" : "Switch to night-time";

        void Awake()
        {
            // Work on a copy so play mode never modifies the skybox asset.
            if (RenderSettings.skybox != null)
            {
                sky = new Material(RenderSettings.skybox);
                RenderSettings.skybox = sky;
            }
            blend = night ? 1f : 0f;
            Apply(blend);
        }

        public override void Interact(PlayerInteractor interactor) => Toggle();

        public void Toggle()
        {
            SetNight(!night, false);
            WorldForgeGame.Toast(night ? "Night-time" : "Daytime");
        }

        public void SetNight(bool value, bool instant)
        {
            night = value;
            if (!instant) return;
            blend = night ? 1f : 0f;
            Apply(blend);
            RefreshProbes();
        }

        void Update()
        {
            float target = night ? 1f : 0f;
            if (Mathf.Approximately(blend, target)) return;
            blend = Mathf.MoveTowards(blend, target, Time.deltaTime / transitionSeconds);
            Apply(blend);
            if (Mathf.Approximately(blend, target)) RefreshProbes();
        }

        void Apply(float b)
        {
            float s = Mathf.SmoothStep(0f, 1f, b);
            if (sun != null)
            {
                sun.transform.rotation = Quaternion.Slerp(Quaternion.Euler(daySunEuler), Quaternion.Euler(nightSunEuler), s);
                sun.color = Color.Lerp(daySunColor, nightSunColor, s);
                sun.intensity = Mathf.Lerp(daySunIntensity, nightSunIntensity, s);
            }
            if (sky != null) sky.SetFloat("_Exposure", Mathf.Lerp(daySkyExposure, nightSkyExposure, s));

            RenderSettings.ambientSkyColor = Color.Lerp(dayAmbientSky, nightAmbientSky, s);
            RenderSettings.ambientEquatorColor = Color.Lerp(dayAmbientEquator, nightAmbientEquator, s);
            RenderSettings.ambientGroundColor = Color.Lerp(dayAmbientGround, nightAmbientGround, s);
            RenderSettings.fogColor = Color.Lerp(dayFog, nightFog, s);

            foreach (var l in nightLights)
                if (l != null) l.enabled = s > 0.5f;

            block ??= new MaterialPropertyBlock();
            foreach (var r in nightBulbs)
            {
                if (r == null) continue;
                r.GetPropertyBlock(block);
                block.SetColor("_EmissionColor", bulbEmission * s);
                r.SetPropertyBlock(block);
            }
        }

        void RefreshProbes()
        {
            foreach (var p in probes)
                if (p != null) p.RenderProbe();
        }
    }
}
