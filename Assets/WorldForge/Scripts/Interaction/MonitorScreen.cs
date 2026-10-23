using UnityEngine;

namespace WorldForge
{
    /// <summary>Computer monitor: each use shows the next page, and after the last page it switches off.</summary>
    public class MonitorScreen : Interactable
    {
        public Renderer screen;
        public Texture2D[] pages;
        public int page;
        public float brightness = 1.4f;

        MaterialPropertyBlock block;

        bool IsOff => page >= pages.Length;
        public override string Prompt => page + 1 < pages.Length ? "Next screen" : IsOff ? "Turn screen on" : "Turn screen off";

        void Start() => Apply();

        public override void Interact(PlayerInteractor interactor)
        {
            page = (page + 1) % (pages.Length + 1);
            Apply();
        }

        void Apply()
        {
            block ??= new MaterialPropertyBlock();
            screen.GetPropertyBlock(block);
            if (IsOff)
            {
                block.SetColor("_EmissionColor", Color.black);
                block.SetColor("_BaseColor", new Color(0.02f, 0.02f, 0.025f));
            }
            else
            {
                block.SetTexture("_BaseMap", pages[page]);
                block.SetTexture("_EmissionMap", pages[page]);
                block.SetColor("_BaseColor", new Color(0.15f, 0.15f, 0.15f));
                block.SetColor("_EmissionColor", Color.white * brightness);
            }
            screen.SetPropertyBlock(block);
        }
    }
}
