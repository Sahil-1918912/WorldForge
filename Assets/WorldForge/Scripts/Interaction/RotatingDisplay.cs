using UnityEngine;

namespace WorldForge
{
    /// <summary>Exhibit turntable: each use rotates the platform a further 90 degrees.</summary>
    public class RotatingDisplay : Interactable
    {
        public Transform platform;
        public float step = 90f;
        public float degreesPerSecond = 120f;

        float targetYaw, currentYaw;

        public float CurrentYaw => currentYaw;
        public override string Prompt => "Rotate exhibit 90°";

        void Awake() => targetYaw = currentYaw = platform.localEulerAngles.y;

        public override void Interact(PlayerInteractor interactor) => targetYaw += step;

        void Update()
        {
            currentYaw = Mathf.MoveTowards(currentYaw, targetYaw, degreesPerSecond * Time.deltaTime);
            platform.localRotation = Quaternion.Euler(0f, currentYaw, 0f);
        }
    }
}
