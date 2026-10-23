using System.Collections;
using UnityEngine;

namespace WorldForge
{
    /// <summary>
    /// 4-DOF pick-and-place arm: base yaw, shoulder pitch, elbow pitch and a wrist that keeps the
    /// gripper pointing down. Joint angles come from analytic two-link inverse kinematics, so the arm
    /// can reach any point we ask for. Each cycle moves the crate from one pad to the other.
    /// </summary>
    public class RobotArm : Interactable
    {
        [Header("Joints")]
        public Transform turntable;
        public Transform shoulder;
        public Transform elbow;
        public Transform wrist;
        public Transform fingerLeft;
        public Transform fingerRight;
        public Transform tip;

        [Header("Geometry (metres, in the arm's local space)")]
        public float shoulderHeight = 0.95f;
        public float upperArm = 1.1f;
        public float forearm = 1.0f;
        public float wristToTip = 0.28f;
        public float fingerOpen = 0.22f;
        public float fingerClosed = 0.145f;
        public float hoverHeight = 0.45f;

        [Header("Task")]
        public Transform crate;
        public Transform padA;
        public Transform padB;
        public bool autoMode;
        public float speedMultiplier = 1f;

        [Header("Status lamp")]
        public Renderer statusLamp;
        public Light statusLight;

        public bool Busy { get; private set; }
        public int CyclesCompleted { get; private set; }

        public override string Prompt => Busy ? "Robot arm is busy..." : "Run pick & place cycle";
        public override bool CanInteract => !Busy;

        struct Pose
        {
            public float yaw, radius, height, grip;
            public Pose(float yaw, float radius, float height, float grip)
            {
                this.yaw = yaw; this.radius = radius; this.height = height; this.grip = grip;
            }
        }

        static readonly Pose Home = new Pose(-45f, 1.0f, 1.9f, 0f);
        Pose current = Home;
        Transform crateParent;
        MaterialPropertyBlock block;

        void Awake()
        {
            crateParent = crate != null ? crate.parent : null;
            ApplyHomePose();
            SetStatus(false);
        }

        void Start()
        {
            if (autoMode) StartCycle();
        }

        public override void Interact(PlayerInteractor interactor)
        {
            StartCycle();
            WorldForgeGame.Toast("Robot arm: pick & place cycle started");
        }

        public void StartCycle()
        {
            if (!Busy && isActiveAndEnabled) StartCoroutine(Cycle());
        }

        public void SetAuto(bool on)
        {
            autoMode = on;
            SetStatus(Busy || on);
            if (on) StartCycle();
        }

        /// <summary>Also called by the scene builder so the arm looks right in edit mode.</summary>
        public void ApplyHomePose()
        {
            current = Home;
            Apply(current);
        }

        IEnumerator Cycle()
        {
            Busy = true;
            SetStatus(true);

            Transform from = Vector3.Distance(crate.position, padA.position) <= Vector3.Distance(crate.position, padB.position) ? padA : padB;
            Transform to = from == padA ? padB : padA;

            Pose aboveFrom = PoseAt(from.position, hoverHeight, 0f);
            Pose atFrom = PoseAt(from.position, 0f, 0f);
            Pose aboveTo = PoseAt(to.position, hoverHeight, 1f);
            Pose atTo = PoseAt(to.position, 0f, 1f);

            yield return MoveTo(aboveFrom, 1.3f);
            yield return MoveTo(atFrom, 0.8f);
            atFrom.grip = 1f;
            yield return MoveTo(atFrom, 0.4f);
            crate.SetParent(tip, true);
            aboveFrom.grip = 1f;
            yield return MoveTo(aboveFrom, 0.8f);
            yield return MoveTo(aboveTo, 1.5f);
            yield return MoveTo(atTo, 0.8f);
            atTo.grip = 0f;
            yield return MoveTo(atTo, 0.4f);
            crate.SetParent(crateParent, true);
            crate.SetPositionAndRotation(to.position, Quaternion.Euler(0f, crate.eulerAngles.y, 0f));
            aboveTo.grip = 0f;
            yield return MoveTo(aboveTo, 0.8f);
            yield return MoveTo(Home, 1.3f);

            Busy = false;
            CyclesCompleted++;
            SetStatus(autoMode);

            if (autoMode)
            {
                yield return new WaitForSeconds(0.6f / speedMultiplier);
                StartCycle();
            }
        }

        Pose PoseAt(Vector3 worldPoint, float lift, float grip)
        {
            Vector3 p = transform.InverseTransformPoint(worldPoint);
            return new Pose(Mathf.Atan2(p.x, p.z) * Mathf.Rad2Deg, new Vector2(p.x, p.z).magnitude, p.y + lift, grip);
        }

        IEnumerator MoveTo(Pose target, float duration)
        {
            Pose start = current;
            duration /= Mathf.Max(0.05f, speedMultiplier);
            for (float t = 0f; t < 1f; t += Time.deltaTime / duration)
            {
                float s = Mathf.SmoothStep(0f, 1f, t);
                current = new Pose(
                    Mathf.LerpAngle(start.yaw, target.yaw, s),
                    Mathf.Lerp(start.radius, target.radius, s),
                    Mathf.Lerp(start.height, target.height, s),
                    Mathf.Lerp(start.grip, target.grip, s));
                Apply(current);
                yield return null;
            }
            current = target;
            Apply(current);
        }

        void Apply(Pose pose)
        {
            // Wrist position = gripper tip lifted by the wrist-to-tip length (gripper points straight down).
            float r = pose.radius;
            float h = pose.height + wristToTip - shoulderHeight;
            float d = Mathf.Clamp(Mathf.Sqrt(r * r + h * h), Mathf.Abs(upperArm - forearm) + 1e-3f, upperArm + forearm - 1e-3f);

            float cosElbow = (d * d - upperArm * upperArm - forearm * forearm) / (2f * upperArm * forearm);
            float q2 = Mathf.Acos(Mathf.Clamp(cosElbow, -1f, 1f));
            float q1 = Mathf.Atan2(r, h) - Mathf.Atan2(forearm * Mathf.Sin(q2), upperArm + forearm * Mathf.Cos(q2));
            float q3 = Mathf.PI - q1 - q2;

            turntable.localRotation = Quaternion.Euler(0f, pose.yaw, 0f);
            shoulder.localRotation = Quaternion.Euler(q1 * Mathf.Rad2Deg, 0f, 0f);
            elbow.localRotation = Quaternion.Euler(q2 * Mathf.Rad2Deg, 0f, 0f);
            wrist.localRotation = Quaternion.Euler(q3 * Mathf.Rad2Deg, 0f, 0f);

            float finger = Mathf.Lerp(fingerOpen, fingerClosed, pose.grip);
            fingerLeft.localPosition = new Vector3(-finger, fingerLeft.localPosition.y, 0f);
            fingerRight.localPosition = new Vector3(finger, fingerRight.localPosition.y, 0f);
        }

        void SetStatus(bool running)
        {
            Color c = running ? new Color(0.2f, 1f, 0.35f) : new Color(1f, 0.6f, 0.1f);
            if (statusLamp != null)
            {
                block ??= new MaterialPropertyBlock();
                statusLamp.GetPropertyBlock(block);
                block.SetColor("_EmissionColor", c * 3f);
                statusLamp.SetPropertyBlock(block);
            }
            if (statusLight != null) statusLight.color = c;
        }
    }
}
