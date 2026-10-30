/*
 * Mục đích: Lái Animator của mô hình người chơi theo chuyển động thật (T-010); dùng được cho người đang điều khiển lẫn người khác.
 * Danh sách hàm:
 * - Awake / OnEnable: tìm Animator, người chơi cha (CharacterController) và các Renderer; nhớ vị trí đầu.
 * - LateUpdate: tính vận tốc ngang từ độ dời vị trí rồi cập nhật tham số Animator (Speed, Forward, Airborne, Seated).
 * - SetBodyVisible: ẩn/hiện thân mô hình (ẩn thân người đang điều khiển ở góc nhìn thứ nhất, vẫn đổ bóng).
 * - Seated: true thì phát pose ngồi.
 */
using UnityEngine;

namespace TheReturn
{
    [DisallowMultipleComponent]
    public sealed class PlayerAnimationDriver : MonoBehaviour
    {
        public Animator animator;
        /// <summary>Transform của người chơi (cha có CharacterController); mặc định là cha trực tiếp.</summary>
        public Transform motionRoot;
        /// <summary>Tốc độ ngang (m/s) từ mức này trở lên dùng animation chạy.</summary>
        public float runSpeed = 4f;
        /// <summary>Dưới mức này (m/s) coi là đứng yên.</summary>
        public float moveThreshold = 0.15f;
        public float smoothing = 12f;
        /// <summary>Thời gian rời mặt đất (giây) trước khi phát animation nhảy/rơi.</summary>
        public float airborneDelay = 0.25f;
        public bool Seated { get; set; }
        public bool BodyVisible { get; private set; } = true;

        static readonly int SpeedId = Animator.StringToHash("Speed");
        static readonly int ForwardId = Animator.StringToHash("Forward");
        static readonly int AirborneId = Animator.StringToHash("Airborne");
        static readonly int SeatedId = Animator.StringToHash("Seated");

        Renderer[] renderers;
        Vector3 lastPosition;
        float airTime;
        float smoothSpeed;
        float smoothForward;

        void Awake()
        {
            if (animator == null) animator = GetComponentInChildren<Animator>(true);
            if (motionRoot == null) motionRoot = transform.parent != null ? transform.parent : transform;
            renderers = GetComponentsInChildren<Renderer>(true);
            lastPosition = motionRoot.position;
        }

        void OnEnable()
        {
            if (motionRoot != null) lastPosition = motionRoot.position;
            airTime = 0;
        }

        void LateUpdate()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f || animator == null || motionRoot == null) return;
            Vector3 velocity = (motionRoot.position - lastPosition) / dt;
            lastPosition = motionRoot.position;
            velocity.y = 0f;
            // Dịch chuyển tức thời (Teleport, đứng dậy khỏi ghế): không tính là chạy.
            if (velocity.magnitude > 20f) velocity = Vector3.zero;

            float speed = velocity.magnitude;
            float forward = speed > moveThreshold ? Vector3.Dot(velocity / speed, motionRoot.forward) : 0f;
            float k = 1f - Mathf.Exp(-smoothing * dt);
            smoothSpeed = Mathf.Lerp(smoothSpeed, speed, k);
            smoothForward = Mathf.Lerp(smoothForward, forward, k);

            bool grounded = Physics.Raycast(motionRoot.position + Vector3.up * 0.1f, Vector3.down, 0.5f, ~0, QueryTriggerInteraction.Ignore);
            airTime = grounded ? 0f : airTime + dt;

            animator.SetFloat(SpeedId, smoothSpeed);
            animator.SetFloat(ForwardId, smoothForward);
            animator.SetBool(AirborneId, airTime > airborneDelay && !Seated);
            animator.SetBool(SeatedId, Seated);
        }

        /// <summary>Nhận true để hiện thân, false để ẩn (vẫn đổ bóng); không trả dữ liệu.</summary>
        public void SetBodyVisible(bool visible)
        {
            BodyVisible = visible;
            if (renderers == null) renderers = GetComponentsInChildren<Renderer>(true);
            foreach (Renderer r in renderers)
            {
                if (r == null) continue;
                r.shadowCastingMode = visible ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            }
        }
    }
}
