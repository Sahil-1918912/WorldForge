using UnityEngine;

namespace WorldForge
{
    /// <summary>Double glass door whose panels slide apart along the local X axis.</summary>
    public class SlidingDoor : Interactable
    {
        public Transform leftPanel;
        public Transform rightPanel;
        public float slideDistance = 1.15f;
        public float speed = 1.4f;

        Vector3 leftClosed, rightClosed;
        bool open;
        float amount;

        public bool IsOpen => open;
        /// <summary>0 = closed, 1 = fully open.</summary>
        public float OpenAmount => amount;
        public override string Prompt => open ? "Close door" : "Open door";

        void Awake()
        {
            leftClosed = leftPanel.localPosition;
            rightClosed = rightPanel.localPosition;
        }

        public override void Interact(PlayerInteractor interactor) => SetOpen(!open);

        public void SetOpen(bool value) => open = value;

        void Update()
        {
            amount = Mathf.MoveTowards(amount, open ? 1f : 0f, Time.deltaTime * speed / slideDistance);
            float s = Mathf.SmoothStep(0f, 1f, amount) * slideDistance;
            leftPanel.localPosition = leftClosed + Vector3.left * s;
            rightPanel.localPosition = rightClosed + Vector3.right * s;
        }
    }
}
