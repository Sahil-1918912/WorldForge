using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace WorldForge
{
    /// <summary>
    /// First-person / third-person character controller.
    /// WASD / arrows move, mouse looks, Shift sprints, Space jumps, V switches camera mode,
    /// Esc releases the cursor and a left click captures it again.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Rig")]
        public Transform cameraPivot;
        public Camera playerCamera;
        public Renderer[] avatarRenderers;

        [Header("Movement")]
        public float walkSpeed = 3.2f;
        public float sprintSpeed = 6.0f;
        public float acceleration = 18f;
        public float jumpHeight = 1.0f;
        public float gravity = -19.6f;

        [Header("Look")]
        public float lookSensitivity = 0.09f;
        public float lookSmoothing = 25f;
        public float minPitch = -80f;
        public float maxPitch = 80f;

        [Header("Third person")]
        public Vector3 thirdPersonOffset = new Vector3(0.45f, 0.25f, -3.0f);
        public float cameraCollisionRadius = 0.2f;

        public bool ThirdPerson { get; private set; }
        public bool ExternalCameraControl { get; private set; }
        public float Yaw => yaw;
        public float Pitch => pitch;

        CharacterController body;
        float yaw, pitch, verticalVelocity;
        Vector3 planarVelocity;
        Vector2 smoothedLook;

        void Awake()
        {
            body = GetComponent<CharacterController>();
            yaw = transform.eulerAngles.y;
            pitch = NormalizeAngle(cameraPivot.localEulerAngles.x);
            ApplyCameraMode();
        }

        void Start()
        {
            if (Application.isFocused && !Application.isBatchMode) LockCursor(true);
        }

        void Update()
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;

            HandleCursor(keyboard, mouse);
            if (ExternalCameraControl) return;

            Look(mouse);
            Move(keyboard);
            if (keyboard != null && keyboard.vKey.wasPressedThisFrame) SetThirdPerson(!ThirdPerson);
        }

        void LateUpdate()
        {
            if (!ExternalCameraControl) PlaceCamera();
        }

        void HandleCursor(Keyboard keyboard, Mouse mouse)
        {
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame) LockCursor(false);
            else if (mouse != null && mouse.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked)
                LockCursor(true);
        }

        public static void LockCursor(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        void Look(Mouse mouse)
        {
            Vector2 raw = Vector2.zero;
            if (mouse != null && Cursor.lockState == CursorLockMode.Locked)
                raw = mouse.delta.ReadValue() * lookSensitivity;

            smoothedLook = Vector2.Lerp(smoothedLook, raw, 1f - Mathf.Exp(-lookSmoothing * Time.deltaTime));
            yaw += smoothedLook.x;
            pitch = Mathf.Clamp(pitch - smoothedLook.y, minPitch, maxPitch);

            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        void Move(Keyboard keyboard)
        {
            Vector2 input = Vector2.zero;
            bool sprint = false, jump = false;
            if (keyboard != null)
            {
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) input.y += 1f;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) input.y -= 1f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) input.x += 1f;
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) input.x -= 1f;
                sprint = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
                jump = keyboard.spaceKey.wasPressedThisFrame;
            }
            input = Vector2.ClampMagnitude(input, 1f);

            Vector3 target = (transform.forward * input.y + transform.right * input.x) * (sprint ? sprintSpeed : walkSpeed);
            planarVelocity = Vector3.MoveTowards(planarVelocity, target, acceleration * Time.deltaTime);

            if (body.isGrounded)
            {
                if (verticalVelocity < 0f) verticalVelocity = -2f;
                if (jump) verticalVelocity = Mathf.Sqrt(2f * jumpHeight * -gravity);
            }
            verticalVelocity += gravity * Time.deltaTime;

            body.Move((planarVelocity + Vector3.up * verticalVelocity) * Time.deltaTime);
        }

        void PlaceCamera()
        {
            var cam = playerCamera.transform;
            if (!ThirdPerson)
            {
                cam.localPosition = Vector3.zero;
                cam.localRotation = Quaternion.identity;
                return;
            }

            // Pull the camera in when a wall is between the player and the desired position.
            Vector3 origin = cameraPivot.position;
            Vector3 desired = cameraPivot.TransformPoint(thirdPersonOffset);
            Vector3 dir = desired - origin;
            float distance = dir.magnitude;
            if (Physics.SphereCast(origin, cameraCollisionRadius, dir / distance, out var hit, distance,
                    Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                distance = Mathf.Max(0.3f, hit.distance - 0.05f);

            cam.position = origin + dir.normalized * distance;
            cam.rotation = cameraPivot.rotation;
        }

        public void SetThirdPerson(bool on)
        {
            ThirdPerson = on;
            ApplyCameraMode();
            WorldForgeGame.Toast(on ? "Third-person camera" : "First-person camera");
        }

        void ApplyCameraMode()
        {
            // In first person the avatar is invisible but still casts a shadow.
            foreach (var r in avatarRenderers)
                if (r != null) r.shadowCastingMode = ThirdPerson ? ShadowCastingMode.On : ShadowCastingMode.ShadowsOnly;
        }

        /// <summary>Hands the camera to another system (guided tour, screenshot director).</summary>
        public void SetExternalCameraControl(bool external)
        {
            if (ExternalCameraControl == external) return;
            ExternalCameraControl = external;
            var cam = playerCamera.transform;
            if (external)
            {
                cam.SetParent(null, true);
            }
            else
            {
                cam.SetParent(cameraPivot, false);
                PlaceCamera();
            }
        }

        public void Teleport(Vector3 position, float newYaw, float newPitch = 0f)
        {
            body.enabled = false;
            transform.SetPositionAndRotation(position, Quaternion.Euler(0f, newYaw, 0f));
            body.enabled = true;
            yaw = newYaw;
            pitch = newPitch;
            planarVelocity = Vector3.zero;
            verticalVelocity = 0f;
            cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            if (!ExternalCameraControl) PlaceCamera();
        }

        static float NormalizeAngle(float a) => a > 180f ? a - 360f : a;
    }
}
