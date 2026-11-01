/*
 * Mục đích: Lái Animator của mô hình người chơi theo chuyển động thật (T-010); dùng được cho người đang điều khiển lẫn người khác.
 * Danh sách hàm:
 * - Awake / OnEnable: tìm Animator, người chơi cha (CharacterController) và các Renderer; nhớ vị trí đầu.
 * - LateUpdate: tính vận tốc ngang từ độ dời vị trí rồi cập nhật tham số Animator (Speed, Forward, Airborne, Seated).
 * - SetBodyVisible: ẩn/hiện thân mô hình (ẩn thân người đang điều khiển ở góc nhìn thứ nhất, vẫn đổ bóng).
 * - Seated: true thì phát pose ngồi.
 * - HorizontalVelocity: (T-027) hàm thuần tính vận tốc ngang từ độ dời, có thể test không cần scene.
 * - SuppressLocomotion: (T-027) true thì coi như đứng yên dù vị trí đổi (người được phương tiện chở, vd ván chạy); mặc định false.
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
        public float airborneDelay = 0.06f;
        public bool Seated { get; set; }
        /// <summary>Nhiệm vụ đặt true khi nhân vật được phương tiện chở đi (vị trí đổi nhưng người đứng yên): không phát animation đi/chạy.</summary>
        public bool SuppressLocomotion { get; set; }
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
            Vector3 velocity = HorizontalVelocity(motionRoot.position - lastPosition, dt, SuppressLocomotion);
            lastPosition = motionRoot.position;

            float speed = velocity.magnitude;
            float forward = speed > moveThreshold ? Vector3.Dot(velocity / speed, motionRoot.forward) : 0f;
            float k = 1f - Mathf.Exp(-smoothing * dt);
            smoothSpeed = Mathf.Lerp(smoothSpeed, speed, k);
            smoothForward = Mathf.Lerp(smoothForward, forward, k);

            bool grounded = Physics.Raycast(motionRoot.position + Vector3.up * 0.1f, Vector3.down, 0.3f, ~0, QueryTriggerInteraction.Ignore);
            airTime = grounded ? 0f : airTime + dt;

            animator.SetFloat(SpeedId, smoothSpeed);
            animator.SetFloat(ForwardId, smoothForward);
            animator.SetBool(AirborneId, airTime > airborneDelay && !Seated);
            animator.SetBool(SeatedId, Seated);
        }

        /// <summary>
        /// Nhận độ dời vị trí trong một khung hình, thời gian khung hình (giây) và cờ đang bị chở; trả vận tốc ngang (m/s).
        /// Trả 0 khi dt không dương, khi đang bị chở (SuppressLocomotion), hoặc khi dịch chuyển tức thời (Teleport, đứng dậy khỏi ghế: trên 20 m/s).
        /// </summary>
        public static Vector3 HorizontalVelocity(Vector3 displacement, float dt, bool suppress)
        {
            if (dt <= 0f || suppress) return Vector3.zero;
            Vector3 velocity = displacement / dt;
            velocity.y = 0f;
            return velocity.magnitude > 20f ? Vector3.zero : velocity;
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
