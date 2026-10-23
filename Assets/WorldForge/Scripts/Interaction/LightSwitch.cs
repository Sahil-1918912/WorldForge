using UnityEngine;

namespace WorldForge
{
    /// <summary>Wall switch that turns the ceiling lights (lamps and glowing panels) on and off.</summary>
    public class LightSwitch : Interactable
    {
        public Light[] lights;
        public Renderer[] panels;
        public Color panelEmission = new Color(3f, 2.9f, 2.7f);
        public Transform lever;
        public bool on = true;

        MaterialPropertyBlock block;

        public override string Prompt => on ? "Turn lab lights off" : "Turn lab lights on";

        void Start() => Apply();

        public override void Interact(PlayerInteractor interactor)
        {
            Set(!on);
            WorldForgeGame.Toast(on ? "Lab lights ON" : "Lab lights OFF");
        }

        public void Set(bool value)
        {
            on = value;
            Apply();
        }

        void Apply()
        {
            foreach (var l in lights)
                if (l != null) l.enabled = on;

            block ??= new MaterialPropertyBlock();
            foreach (var r in panels)
            {
                if (r == null) continue;
                r.GetPropertyBlock(block);
                block.SetColor("_EmissionColor", on ? panelEmission : Color.black);
                r.SetPropertyBlock(block);
            }
            if (lever != null) lever.localRotation = Quaternion.Euler(on ? -25f : 25f, 0f, 0f);
        }
    }
}
