using UnityEngine;

namespace WorldForge
{
    /// <summary>
    /// Base class for everything the player can aim at and use with E / left click.
    /// The interactor finds it with GetComponentInParent, so colliders can live on child objects.
    /// </summary>
    public abstract class Interactable : MonoBehaviour
    {
        public string displayName = "Object";

        /// <summary>Verb shown under the crosshair, e.g. "Open door".</summary>
        public virtual string Prompt => "Use";

        public virtual bool CanInteract => true;

        public abstract void Interact(PlayerInteractor interactor);
    }
}
