/*
 * Mục đích: Tiếng BƯỚC CHÂN và tiếng NHẢY của người đang điều khiển trong sảnh chờ, giống map MapSchoolF (T-052).
 * Phát qua AudioHub (cue Step/Jump); clip do GameAudioSlots (object Audio_Slots) của scene quyết định, để trống = âm mặc định.
 * Đây là bản sao gọn của SchoolFloorAudioDirector.TickSteps/TickJump (đạo diễn đó cần SchoolFloorFlow nên sảnh không dùng được): đổi luật tiếng bước/nhảy thì sửa cả hai nơi.
 * Hàm: Awake; Update; PlayerPos; TickSteps; TickJump.
 */
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheReturn
{
    /// <summary>Phát tiếng bước chân và tiếng nhảy cho người chơi trong sảnh chờ.</summary>
    [DefaultExecutionOrder(100)]
    public sealed class LobbyLocomotionAudio : MonoBehaviour
    {
        public PrototypePartyController party;
        [Min(0.5f)] public float strideMeters = 1.7f;

        int lastActive = -1;
        Vector3 lastPos;
        float stepAcc;
        int jumpWatch;
        float jumpStartY;
        float jumpPrevY;
        bool jumpWasGrounded;
        Transform jumpBody;
        CharacterController jumpController;

        /// <summary>Không nhận tham số; tự tìm bộ điều khiển đội nếu chưa gán.</summary>
        void Awake()
        {
            if (party == null) party = FindFirstObjectByType<PrototypePartyController>();
        }

        /// <summary>Mỗi khung hình: theo dõi người đang điều khiển, phát bước chân và tiếng nhảy; chạy sau LobbyReadyController (nơi gọi TickInput).</summary>
        void Update()
        {
            if (party == null || party.players == null || party.PartySize == 0) return;
            int active = party.ActivePlayer;
            if (active != lastActive) { lastActive = active; stepAcc = 0f; lastPos = PlayerPos(active); }
            TickSteps(active);
            TickJump(active);
        }

        /// <summary>Nhận chỉ số người chơi; trả vị trí thế giới của người đó, hoặc zero nếu chưa gắn.</summary>
        Vector3 PlayerPos(int index)
        {
            return index >= 0 && index < party.players.Length && party.players[index] != null ? party.players[index].position : Vector3.zero;
        }

        /// <summary>Nhận chỉ số người điều khiển; cộng quãng đường ngang đã đi, đủ một sải thì phát một tiếng bước (bỏ qua dịch chuyển tức thời và đi quá chậm).</summary>
        void TickSteps(int active)
        {
            Vector3 pos = PlayerPos(active);
            Vector3 d = pos - lastPos; d.y = 0f; lastPos = pos;
            float dist = d.magnitude;
            if (Time.deltaTime <= 0f || dist > 1.5f || dist / Time.deltaTime < 0.4f) return;
            stepAcc += dist;
            if (stepAcc >= strideMeters) { stepAcc = 0f; AudioHub.Player.Play(AudioCues.Step); }
        }

        /// <summary>Nhận chỉ số người điều khiển; khi bấm Space lúc đang (hoặc vừa) đứng trên đất và chuột đã khóa thì theo dõi vài khung hình, nếu thân người thật sự nhích lên thì phát tiếng nhảy một lần (cùng cách làm như map MapSchoolF).</summary>
        void TickJump(int active)
        {
            if (active < 0 || active >= party.players.Length) return;
            Transform body = party.players[active];
            if (body == null) return;
            if (body != jumpBody) { jumpBody = body; jumpController = body.GetComponent<CharacterController>(); jumpWatch = 0; jumpPrevY = body.position.y; jumpWasGrounded = false; }
            if (jumpController == null) return;
            bool groundedNow = jumpController.isGrounded;
            float y = body.position.y;
            if (jumpWatch > 0)
            {
                jumpWatch--;
                if (y - jumpStartY > 0.04f) { jumpWatch = 0; AudioHub.Player.Play(AudioCues.Jump); }
            }
            else
            {
                Keyboard keyboard = Keyboard.current;
                if (keyboard != null && party.allowJump && Cursor.lockState == CursorLockMode.Locked && keyboard.Control(GameAction.Jump).wasPressedThisFrame && (groundedNow || jumpWasGrounded))
                {
                    jumpStartY = jumpPrevY;
                    if (y - jumpStartY > 0.04f) AudioHub.Player.Play(AudioCues.Jump);
                    else jumpWatch = 4;
                }
            }
            jumpWasGrounded = groundedNow;
            jumpPrevY = y;
        }
    }
}
