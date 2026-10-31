/*
 * Mục đích: Điều khiển góc nhìn thứ nhất và đổi vai trên một máy; có thể dùng lại cho các câu đố khác.
 * Danh sách hàm:
 * - Initialize: lưu vị trí xuất hiện và bộ điều khiển va chạm.
 * - SetPartySize: bật đúng số mô hình người trong đội.
 * - SwitchRole: chuyển camera và độ cao mắt sang một vai, ẩn thân mô hình của vai đang điều khiển.
 * - TickInput: xử lý nhìn/di chuyển/nhảy (phím Space) của vai hiện tại.
 * - NextFallSpeed: (tĩnh) tính vận tốc đứng mới từ trạng thái chạm đất và lệnh nhảy.
 * - TryGetInteractionHit: tìm va chạm gần nhất, bỏ qua chính người đang điều khiển.
 * - Teleport: đổi vị trí có tắt va chạm tạm thời.
 * - ResetPositions: đưa các vai về vị trí đầu.
 * - SetCursor: khóa/thả chuột; không thay đổi luật câu đố.
 * - EnterOnlineMode / ExitOnlineMode: (T-025) chế độ online, người chơi này chỉ điều khiển đúng MỘT ô cố định; mặc định tắt.
 * - BindSlot: (T-025) gắn/gỡ nhân vật đã spawn qua mạng vào một ô, để dùng lại cùng bộ điều khiển.
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
        /// <summary>Cho phép nhảy bằng phím Space khi đang đứng trên mặt đất (không nhảy khi ngồi).</summary>
        public bool allowJump = true;
        /// <summary>Độ cao nhảy tối đa (m); trọng lực cố định 20 m/s².</summary>
        public float jumpHeight = 0.8f;
        public bool IsSprinting { get; private set; }
        public float lookSensitivity = .10f;
        /// <summary>Hệ số nhân độ nhạy chuột từ Settings (T-008); 1 = giữ nguyên lookSensitivity.</summary>
        public static float SensitivityScale = 1f;
        public int ActivePlayer { get; private set; }
        public int PartySize { get; private set; }
        /// <summary>Ô duy nhất người chơi này được điều khiển ở chế độ online; -1 = chế độ 1 máy (đổi vai như cũ).</summary>
        public int OnlineLocalSlot { get; private set; } = -1;
        public bool IsOnline { get { return OnlineLocalSlot >= 0; } }
        CharacterController[] controllers;
        PlayerAnimationDriver[] drivers;
        Vector3[] spawns;
        Quaternion[] spawnRotations;
        float[] pitches;
        float[] fallSpeeds;

        /// <summary>Không nhận tham số; lấy tham chiếu scene và lưu spawn. Gọi một lần trước khi dùng.</summary>
        public void Initialize()
        {
            controllers = new CharacterController[players.Length];
            drivers = new PlayerAnimationDriver[players.Length];
            spawns = new Vector3[players.Length];
            spawnRotations = new Quaternion[players.Length];
            pitches = new float[players.Length];
            fallSpeeds = new float[players.Length];
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] == null) continue; // ô chưa có nhân vật (online: người chưa vào)
                controllers[i] = players[i].GetComponent<CharacterController>();
                drivers[i] = players[i].GetComponentInChildren<PlayerAnimationDriver>(true);
                spawns[i] = players[i].position;
                spawnRotations[i] = players[i].rotation;
            }
        }

        /// <summary>Nhận số người 0–4; bật/tắt đúng các vai trong scene, không tạo đề.</summary>
        public void SetPartySize(int count)
        {
            PartySize = Mathf.Clamp(count, 0, players.Length);
            for (int i = 0; i < players.Length; i++) if (players[i] != null) players[i].gameObject.SetActive(i < PartySize);
        }

        /// <summary>Nhận ID vai và trạng thái ngồi; trả false nếu vai không trong đội, true sau khi chuyển camera.</summary>
        public bool SwitchRole(int index, bool seated)
        {
            if (index < 0 || index >= PartySize) return false;
            if (OnlineLocalSlot >= 0 && index != OnlineLocalSlot) return false; // online: không đổi vai
            if (players[index] == null) return false;
            ActivePlayer = index;
            for (int i = 0; i < bodies.Length; i++) if (bodies[i] != null) bodies[i].enabled = i != index;
            for (int i = 0; i < drivers.Length; i++) if (drivers[i] != null) drivers[i].SetBodyVisible(i != index);
            viewCamera.transform.SetParent(players[index], false);
            viewCamera.transform.localPosition = Vector3.up * (seated ? 1.18f : 1.65f);
            viewCamera.transform.localRotation = Quaternion.Euler(pitches[index], 0, 0);
            return true;
        }

        /// <summary>Nhận trạng thái ngồi; đọc keyboard/mouse, cập nhật vị trí và góc nhìn, không trả dữ liệu.</summary>
        public void TickInput(bool seated)
        {
            IsSprinting = false;
            if (PartySize == 0) return;
            if (players[ActivePlayer] == null || controllers[ActivePlayer] == null) return; // chưa gắn nhân vật
            if (drivers != null && drivers[ActivePlayer] != null) drivers[ActivePlayer].Seated = seated;
            if (Cursor.lockState != CursorLockMode.Locked) return;
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            if (mouse != null)
            {
                Vector2 delta = mouse.delta.ReadValue() * lookSensitivity * SensitivityScale;
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
            fallSpeeds[ActivePlayer] = NextFallSpeed(fallSpeeds[ActivePlayer], controllers[ActivePlayer].isGrounded,
                allowJump && keyboard.spaceKey.wasPressedThisFrame, jumpHeight, Time.deltaTime);
            bool sprinting = keyboard.leftShiftKey.isPressed && axis.sqrMagnitude > .01f;
            Vector3 before = player.position;
            Vector3 motion = (player.right * axis.x + player.forward * axis.y) * moveSpeed * (sprinting ? sprintMultiplier : 1);
            motion.y = fallSpeeds[ActivePlayer];
            controllers[ActivePlayer].Move(motion * Time.deltaTime);
            Vector3 moved = player.position - before;
            moved.y = 0;
            IsSprinting = sprinting && moved.sqrMagnitude > .000001f;
        }

        /// <summary>Trọng lực dùng cho rơi và nhảy (m/s²).</summary>
        public const float Gravity = 20f;

        /// <summary>Nhận vận tốc đứng hiện tại, đang chạm đất, có lệnh nhảy, độ cao nhảy (m), dt (s); trả vận tốc đứng mới (m/s, dương là đi lên). Chỉ nhảy khi đang chạm đất.</summary>
        public static float NextFallSpeed(float current, bool grounded, bool jump, float jumpHeight, float dt)
        {
            if (grounded && jump) return Mathf.Sqrt(2f * Gravity * jumpHeight);
            return grounded ? -2f : current - Gravity * dt;
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
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] == null) continue;
                Teleport(i, spawns[i]);
                players[i].rotation = spawnRotations[i];
                if (drivers[i] != null) drivers[i].Seated = false;
            }
        }

        /// <summary>Nhận ID vai và mốc Transform; đặt vị trí cùng hướng ngang của mốc, dùng được cho phòng đã xoay.</summary>
        public void Teleport(int index, Transform target)
        {
            Teleport(index, target.position);
            players[index].rotation = Quaternion.Euler(0, target.eulerAngles.y, 0);
        }

        /// <summary>
        /// Bật chế độ online: nhận ô CỐ ĐỊNH của người chơi này, số người của đội, và trạng thái ngồi. Từ đó chỉ ô này được điều khiển
        /// (SwitchRole sang ô khác trả false). Trả false (và giữ nguyên chế độ cũ) nếu ô sai hoặc chưa gắn nhân vật bằng BindSlot.
        /// </summary>
        public bool EnterOnlineMode(int localSlot, int teamSize, bool seated = false)
        {
            if (controllers == null) Initialize();
            if (localSlot < 0 || localSlot >= players.Length) return false;
            int previousOnline = OnlineLocalSlot;
            int previousSize = PartySize;
            OnlineLocalSlot = -1; // tạm thả để SwitchRole không tự chặn
            PartySize = Mathf.Clamp(teamSize, 0, players.Length);
            if (!SwitchRole(localSlot, seated))
            {
                OnlineLocalSlot = previousOnline;
                PartySize = previousSize;
                return false;
            }
            OnlineLocalSlot = localSlot;
            return true;
        }

        /// <summary>Không nhận tham số; tắt chế độ online, quay về đổi vai một máy. Không đổi vị trí hay camera.</summary>
        public void ExitOnlineMode()
        {
            OnlineLocalSlot = -1;
        }

        /// <summary>
        /// Nhận ô và nhân vật vừa spawn (null để gỡ khi người chơi rời). Lưu lại CharacterController, bộ lái hoạt họa và điểm xuất phát
        /// của ô. Nếu camera đang gắn vào nhân vật bị gỡ thì thả camera ra trước. Trả false nếu ô nằm ngoài danh sách.
        /// </summary>
        public bool BindSlot(int index, Transform player)
        {
            if (controllers == null) Initialize();
            if (index < 0 || index >= players.Length) return false;
            if (player == null && viewCamera != null && players[index] != null && viewCamera.transform.parent == players[index])
                viewCamera.transform.SetParent(null, true);
            players[index] = player;
            controllers[index] = player != null ? player.GetComponent<CharacterController>() : null;
            drivers[index] = player != null ? player.GetComponentInChildren<PlayerAnimationDriver>(true) : null;
            spawns[index] = player != null ? player.position : Vector3.zero;
            spawnRotations[index] = player != null ? player.rotation : Quaternion.identity;
            pitches[index] = 0;
            fallSpeeds[index] = 0;
            return true;
        }

        /// <summary>Nhận true để khóa chuột, false để thả; cập nhật con trỏ hệ thống.</summary>
        public static void SetCursor(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
