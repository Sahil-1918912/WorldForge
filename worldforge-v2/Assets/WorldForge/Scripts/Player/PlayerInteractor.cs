using UnityEngine;
using UnityEngine.InputSystem;

namespace WorldForge
{
    /// <summary>
    /// Ray-casts from the screen centre, highlights the Interactable under the crosshair and
    /// triggers it with E / left click. Also carries Grabbable objects:
    /// E / left click drops, right click throws, hold R to rotate, mouse wheel changes distance.
    /// Runs before PlayerController so the click that captures the cursor is not also a "use".
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public class PlayerInteractor : MonoBehaviour
    {
        public PlayerController controller;
        public float reach = 3.2f;
        public float holdDistance = 1.6f;
        public float throwSpeed = 7f;
        public float rotateSpeed = 120f;

        public Interactable Hovered { get; private set; }
        public Grabbable Held { get; private set; }

        float holdYaw;
        Quaternion heldBaseRotation;

        Transform Cam => controller.playerCamera.transform;

        void Update()
        {
            if (controller.ExternalCameraControl)
            {
                SetHovered(null);
                return;
            }

            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            bool locked = Cursor.lockState == CursorLockMode.Locked;
            bool use = (keyboard != null && keyboard.eKey.wasPressedThisFrame) ||
                       (locked && mouse != null && mouse.leftButton.wasPressedThisFrame);

            if (Held != null)
            {
                SetHovered(null);
                if (use) Drop(Vector3.zero);
                else if (locked && mouse != null && mouse.rightButton.wasPressedThisFrame) Drop(Cam.forward * throwSpeed);
                else
                {
                    if (keyboard != null && keyboard.rKey.isPressed) holdYaw += rotateSpeed * Time.deltaTime;
                    float scroll = mouse != null ? mouse.scroll.ReadValue().y : 0f;
                    if (Mathf.Abs(scroll) > 0.01f) holdDistance = Mathf.Clamp(holdDistance + Mathf.Sign(scroll) * 0.15f, 1.0f, 3.0f);
                }
                return;
            }

            SetHovered(FindTarget());
            if (use && Hovered != null && Hovered.CanInteract) Hovered.Interact(this);
        }

        void LateUpdate()
        {
            if (Held == null) return;

            // Keep the held object in front of the camera, but never push it through a wall.
            float distance = holdDistance;
            if (Physics.SphereCast(Cam.position, 0.15f, Cam.forward, out var hit, holdDistance,
                    Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                distance = Mathf.Max(0.5f, hit.distance - 0.1f);

            Vector3 target = Cam.position + Cam.forward * distance;
            var t = Held.transform;
            float k = 1f - Mathf.Exp(-20f * Time.deltaTime);
            t.position = Vector3.Lerp(t.position, target, k);
            t.rotation = Quaternion.Slerp(t.rotation, Quaternion.Euler(0f, controller.Yaw + holdYaw, 0f) * heldBaseRotation, k);
        }

        Interactable FindTarget()
        {
            // In third person the camera sits behind the player, so extend the ray by that distance.
            float extra = controller.ThirdPerson ? Vector3.Distance(Cam.position, controller.cameraPivot.position) : 0f;
            if (!Physics.Raycast(Cam.position, Cam.forward, out var hit, reach + extra,
                    Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                return null;
            return hit.collider.GetComponentInParent<Interactable>();
        }

        void SetHovered(Interactable target)
        {
            if (target == Hovered) return;
            Highlighter.ClearAll();
            Hovered = target;
            if (target != null) Highlighter.Apply(target);
        }

        public void Grab(Grabbable item)
        {
            if (Held != null || item == null) return;
            SetHovered(null);
            Held = item;
            holdYaw = 0f;
            heldBaseRotation = Quaternion.Euler(0f, -controller.Yaw, 0f) * item.transform.rotation;
            item.OnGrabbed();
        }

        public void Drop(Vector3 velocity)
        {
            if (Held == null) return;
            var item = Held;
            Held = null;
            item.OnReleased(velocity);
        }
    }
}
