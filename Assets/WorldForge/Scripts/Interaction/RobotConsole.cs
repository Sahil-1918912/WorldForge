using UnityEngine;

namespace WorldForge
{
    /// <summary>Control desk next to the robot cell: toggles the arm's continuous (automatic) mode.</summary>
    public class RobotConsole : Interactable
    {
        public RobotArm arm;
        public Renderer button;

        MaterialPropertyBlock block;

        public override string Prompt => arm.autoMode ? "Stop automatic mode" : "Start automatic mode";

        void Start() => Refresh();

        public override void Interact(PlayerInteractor interactor)
        {
            arm.SetAuto(!arm.autoMode);
            Refresh();
            WorldForgeGame.Toast(arm.autoMode ? "Robot arm: automatic mode ON" : "Robot arm: automatic mode OFF");
        }

        void Refresh()
        {
            if (button == null) return;
            block ??= new MaterialPropertyBlock();
            button.GetPropertyBlock(block);
            Color c = arm.autoMode ? new Color(0.1f, 1f, 0.3f) : new Color(1f, 0.15f, 0.1f);
            block.SetColor("_BaseColor", c);
            block.SetColor("_EmissionColor", c * 2.5f);
            button.SetPropertyBlock(block);
        }
    }
}
