using UnityEngine;

namespace WorldForge
{
    /// <summary>A physics object the player can pick up, carry, rotate and throw.</summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Grabbable : Interactable
    {
        Rigidbody body;
        Collider[] colliders;

        public bool IsHeld { get; private set; }
        public override string Prompt => "Pick up";

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            colliders = GetComponentsInChildren<Collider>();
        }

        public override void Interact(PlayerInteractor interactor) => interactor.Grab(this);

        public void OnGrabbed()
        {
            IsHeld = true;
            body.isKinematic = true;
            body.interpolation = RigidbodyInterpolation.None;
            foreach (var c in colliders) c.enabled = false;
        }

        public void OnReleased(Vector3 velocity)
        {
            IsHeld = false;
            foreach (var c in colliders) c.enabled = true;
            body.isKinematic = false;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.linearVelocity = velocity;
            body.angularVelocity = velocity.sqrMagnitude > 0.01f ? Random.insideUnitSphere * 3f : Vector3.zero;
        }
    }
}
