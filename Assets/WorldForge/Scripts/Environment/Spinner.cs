using UnityEngine;

namespace WorldForge
{
    /// <summary>Rotates a transform at a constant speed (fans, rotors, decorative parts).</summary>
    public class Spinner : MonoBehaviour
    {
        public Vector3 axis = Vector3.up;
        public float degreesPerSecond = 90f;

        void Update() => transform.Rotate(axis, degreesPerSecond * Time.deltaTime, Space.Self);
    }
}
