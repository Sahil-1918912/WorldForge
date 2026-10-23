using UnityEngine;

namespace WorldForge
{
    /// <summary>
    /// Scrolls the belt texture, carries the crates along the conveyor's local X axis and recycles
    /// them inside the machines at either end. The scanner arch turns green while a crate passes.
    /// </summary>
    public class ConveyorBelt : Interactable
    {
        public Renderer belt;
        public float beltLength = 8.4f;
        public Vector2 beltTiling = new Vector2(8.4f, 1f);
        public Transform[] items;
        public Transform[] rollers;
        public float rollerRadius = 0.06f;
        public float speed = 0.45f;
        public float minX = -3.7f;
        public float maxX = 3.7f;
        public bool running = true;

        [Header("Scanner")]
        public Renderer scannerBeam;
        public Light scannerLight;

        float offset;
        MaterialPropertyBlock beltBlock, beamBlock;

        public override string Prompt => running ? "Stop conveyor" : "Start conveyor";

        public override void Interact(PlayerInteractor interactor)
        {
            running = !running;
            WorldForgeGame.Toast(running ? "Conveyor started" : "Conveyor stopped");
        }

        void Update()
        {
            if (running)
            {
                float dx = speed * Time.deltaTime;
                offset = Mathf.Repeat(offset - dx * beltTiling.x / beltLength, 1f);

                foreach (var item in items)
                {
                    var p = item.localPosition;
                    p.x += dx;
                    if (p.x > maxX) p.x -= maxX - minX;
                    item.localPosition = p;
                }
                foreach (var roller in rollers)
                    roller.Rotate(Vector3.up, -dx / rollerRadius * Mathf.Rad2Deg, Space.Self);
            }

            if (belt != null)
            {
                beltBlock ??= new MaterialPropertyBlock();
                belt.GetPropertyBlock(beltBlock);
                beltBlock.SetVector("_BaseMap_ST", new Vector4(beltTiling.x, beltTiling.y, offset, 0f));
                belt.SetPropertyBlock(beltBlock);
            }

            UpdateScanner();
        }

        void UpdateScanner()
        {
            bool scanning = false;
            foreach (var item in items)
                if (Mathf.Abs(item.localPosition.x) < 0.3f) scanning = true;

            Color c = scanning ? new Color(0.2f, 1f, 0.3f) : new Color(1f, 0.15f, 0.1f);
            if (scannerBeam != null)
            {
                beamBlock ??= new MaterialPropertyBlock();
                scannerBeam.GetPropertyBlock(beamBlock);
                beamBlock.SetColor("_EmissionColor", c * 4f);
                beamBlock.SetColor("_BaseColor", c);
                scannerBeam.SetPropertyBlock(beamBlock);
            }
            if (scannerLight != null)
            {
                scannerLight.color = c;
                scannerLight.intensity = scanning ? 2.5f : 0.8f;
            }
        }
    }
}
