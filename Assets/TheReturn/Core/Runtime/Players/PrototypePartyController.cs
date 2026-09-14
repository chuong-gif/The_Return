/*
 * Mục đích: Điều khiển góc nhìn thứ nhất và đổi vai trên một máy; có thể dùng lại cho các câu đố khác.
 * Danh sách hàm:
 * - Initialize: lưu vị trí xuất hiện và bộ điều khiển va chạm.
 * - SetPartySize: bật đúng số mô hình người trong đội.
 * - SwitchRole: chuyển camera và độ cao mắt sang một vai.
 * - TickInput: xử lý nhìn/di chuyển của vai hiện tại.
 * - TryGetInteractionHit: tìm va chạm gần nhất, bỏ qua chính người đang điều khiển.
 * - Teleport: đổi vị trí có tắt va chạm tạm thời.
 * - ResetPositions: đưa các vai về vị trí đầu.
 * - SetCursor: khóa/thả chuột; không thay đổi luật câu đố.
 */
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheReturn
{
    public sealed class PrototypePartyController : MonoBehaviour
    {
        public Camera viewCamera;
        public Transform[] players;
        public Renderer[] bodies;
        public float moveSpeed = 3.2f;
        public float sprintMultiplier = 1.5f;
        public bool IsSprinting { get; private set; }
        public float lookSensitivity = .10f;
        public int ActivePlayer { get; private set; }
        public int PartySize { get; private set; }
        CharacterController[] controllers;
        Vector3[] spawns;
        float[] pitches;
        float[] fallSpeeds;

        /// <summary>Không nhận tham số; lấy tham chiếu scene và lưu spawn. Gọi một lần trước khi dùng.</summary>
        public void Initialize()
        {
            controllers = new CharacterController[players.Length];
            spawns = new Vector3[players.Length];
            pitches = new float[players.Length];
            fallSpeeds = new float[players.Length];
            for (int i = 0; i < players.Length; i++)
            {
                controllers[i] = players[i].GetComponent<CharacterController>();
                spawns[i] = players[i].position;
            }
        }

        /// <summary>Nhận số người 0–4; bật/tắt đúng các vai trong scene, không tạo đề.</summary>
        public void SetPartySize(int count)
        {
            PartySize = Mathf.Clamp(count, 0, players.Length);
            for (int i = 0; i < players.Length; i++) players[i].gameObject.SetActive(i < PartySize);
        }

        /// <summary>Nhận ID vai và trạng thái ngồi; trả false nếu vai không trong đội, true sau khi chuyển camera.</summary>
        public bool SwitchRole(int index, bool seated)
        {
            if (index < 0 || index >= PartySize) return false;
            ActivePlayer = index;
            for (int i = 0; i < bodies.Length; i++) bodies[i].enabled = i != index;
            viewCamera.transform.SetParent(players[index], false);
            viewCamera.transform.localPosition = Vector3.up * (seated ? 1.18f : 1.65f);
            viewCamera.transform.localRotation = Quaternion.Euler(pitches[index], 0, 0);
            return true;
        }

        /// <summary>Nhận trạng thái ngồi; đọc keyboard/mouse, cập nhật vị trí và góc nhìn, không trả dữ liệu.</summary>
        public void TickInput(bool seated)
        {
            IsSprinting = false;
            if (PartySize == 0 || Cursor.lockState != CursorLockMode.Locked) return;
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            if (mouse != null)
            {
                Vector2 delta = mouse.delta.ReadValue() * lookSensitivity;
                players[ActivePlayer].Rotate(0, delta.x, 0);
                pitches[ActivePlayer] = Mathf.Clamp(pitches[ActivePlayer] - delta.y, -75, 75);
                viewCamera.transform.localRotation = Quaternion.Euler(pitches[ActivePlayer], 0, 0);
            }
            if (keyboard == null || seated) return;
            Vector2 axis = new Vector2(
                (keyboard.dKey.isPressed ? 1 : 0) - (keyboard.aKey.isPressed ? 1 : 0),
                (keyboard.wKey.isPressed ? 1 : 0) - (keyboard.sKey.isPressed ? 1 : 0));
            axis = Vector2.ClampMagnitude(axis, 1);
            Transform player = players[ActivePlayer];
            fallSpeeds[ActivePlayer] = controllers[ActivePlayer].isGrounded ? -2 :
                fallSpeeds[ActivePlayer] - 20 * Time.deltaTime;
            bool sprinting = keyboard.leftShiftKey.isPressed && axis.sqrMagnitude > .01f;
            Vector3 before = player.position;
            Vector3 motion = (player.right * axis.x + player.forward * axis.y) * moveSpeed * (sprinting ? sprintMultiplier : 1);
            motion.y = fallSpeeds[ActivePlayer];
            controllers[ActivePlayer].Move(motion * Time.deltaTime);
            Vector3 moved = player.position - before;
            moved.y = 0;
            IsSprinting = sprinting && moved.sqrMagnitude > .000001f;
        }

        /// <summary>Nhận tầm với; trả true và hit gần nhất ngoài collider bản thân, giữ vật cản khác.</summary>
        public bool TryGetInteractionHit(float distance, out RaycastHit nearest)
        {
            nearest = default;
            float nearestDistance = float.PositiveInfinity;
            bool found = false;
            RaycastHit[] hits = Physics.RaycastAll(viewCamera.transform.position, viewCamera.transform.forward,
                distance, ~0, QueryTriggerInteraction.Ignore);
            foreach (RaycastHit hit in hits)
            {
                if (hit.transform.IsChildOf(players[ActivePlayer]) || hit.distance >= nearestDistance) continue;
                nearest = hit;
                nearestDistance = hit.distance;
                found = true;
            }
            return found;
        }

        /// <summary>Nhận ID vai, vị trí và có reset hướng hay không; đổi pose an toàn, không trả dữ liệu.</summary>
        public void Teleport(int index, Vector3 position, bool resetLook = true)
        {
            controllers[index].enabled = false;
            players[index].position = position;
            controllers[index].enabled = true;
            fallSpeeds[index] = 0;
            if (resetLook)
            {
                players[index].rotation = Quaternion.identity;
                pitches[index] = 0;
            }
        }

        /// <summary>Không nhận tham số; trả tất cả vai về spawn đã lưu, không reset câu đố.</summary>
        public void ResetPositions()
        {
            for (int i = 0; i < players.Length; i++) Teleport(i, spawns[i]);
        }

        /// <summary>Nhận true để khóa chuột, false để thả; cập nhật con trỏ hệ thống.</summary>
        public static void SetCursor(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
