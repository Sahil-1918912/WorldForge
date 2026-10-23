using UnityEngine;

namespace WorldForge
{
    /// <summary>
    /// Scrolls the belt texture and carries the crates along the conveyor's local X axis,
    /// recycling them inside the boxes at either end. Use it to stop / start the line.
    /// </summary>
    public class ConveyorBelt : Interactable
    {
        public Renderer belt;
        public float beltLength = 8f;
        public Transform[] items;
        public float speed = 0.45f;
        public float minX = -3.5f;
        public float maxX = 3.5f;
        public bool running = true;

        float offset;
        MaterialPropertyBlock block;

        public override string Prompt => running ? "Stop conveyor" : "Start conveyor";

        public override void Interact(PlayerInteractor interactor)
        {
            running = !running;
            WorldForgeGame.Toast(running ? "Conveyor started" : "Conveyor stopped");
        }

        void Update()
        {
            if (!running) return;

            float dx = speed * Time.deltaTime;
            foreach (var item in items)
            {
                var p = item.localPosition;
                p.x += dx;
                if (p.x > maxX) p.x -= maxX - minX;
                item.localPosition = p;
            }

            // One texture repeat per metre, so the belt surface moves at the same speed as the crates.
            offset = Mathf.Repeat(offset - dx, 1f);
            block ??= new MaterialPropertyBlock();
            belt.GetPropertyBlock(block);
            block.SetVector("_BaseMap_ST", new Vector4(beltLength, 1f, offset, 0f));
            belt.SetPropertyBlock(block);
        }
    }
}
