/*
 * Mục đích: Nối luật hành lang với người chơi, nút, cửa an toàn và checkpoint.
 * Hàm: Begin bắt đầu; Retry đặt lại riêng hành lang; Update xử lý input;
 * TickSimulation kiểm tra nút/ồn/đích; TryToggleStation xác thực tương tác;
 * IsHolding đọc quyền giữ; GateOccupied kiểm tra chống kẹp; AllAcross kiểm tra đích;
 * RefreshPresentation cập nhật nút/loa; StopSession ngừng nhiệm vụ.
 */
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheReturn
{
    [DefaultExecutionOrder(50)]
    public sealed class QuietCorridorPrototype : MonoBehaviour
    {
        public PrototypePartyController party;
        public QuietCorridorSettings settings;
        public QuietHoldStation[] stations;
        public PuzzleDoor gate;
        public BoxCollider safetyZone, goalZone;
        public Transform[] checkpoints;
        public Renderer speakerIndicator;
        public QuietCorridorState State { get; private set; }
        public bool Active { get; private set; }
        public string Status { get; private set; }
        public int TargetStation { get; private set; } = -1;
        public bool SafetyHolding { get; private set; }
        public int Failures { get; private set; }
        MaterialPropertyBlock speakerProperties;

        /// <summary>Không nhận tham số; lấy số người hiện có, tạo lượt và bắt đầu ở vị trí tập hợp.</summary>
        public void Begin()
        {
            State = new QuietCorridorState(party.PartySize, settings.graceSeconds, settings.sprintNoisePerSecond,
                settings.speakerNoisePerSecond, settings.recoveryPerSecond);
            Active = true; Failures = 0;
            Status = settings.instructions;
            RefreshPresentation();
        }

        /// <summary>Nhận cờ do quá ồn; reset riêng nhiệm vụ 2, đưa đội về checkpoint, giữ nguyên điểm danh.</summary>
        public void Retry(bool failed = false)
        {
            if (State == null) return;
            if (failed) Failures++;
            State.Reset();
            for (int i = 0; i < State.PlayerCount; i++) party.Teleport(i, checkpoints[i].position);
            party.SwitchRole(party.ActivePlayer, false);
            gate.SetOpen(false, true);
            Status = failed ? settings.failure : "Đã đặt lại hành lang; điểm danh vẫn được giữ.";
            RefreshPresentation();
        }

        /// <summary>Không nhận tham số; xử lý đổi vai, giữ nút và di chuyển; menu tạm dừng mô phỏng hành lang.</summary>
        void Update()
        {
            if (!Active || State == null) return;
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.tabKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame)
                    PrototypePartyController.SetCursor(Cursor.lockState != CursorLockMode.Locked);
                if (keyboard.digit1Key.wasPressedThisFrame) party.SwitchRole(0, false);
                if (keyboard.digit2Key.wasPressedThisFrame) party.SwitchRole(1, false);
                if (keyboard.digit3Key.wasPressedThisFrame) party.SwitchRole(2, false);
                if (keyboard.digit4Key.wasPressedThisFrame) party.SwitchRole(3, false);
            }
            if (Cursor.lockState != CursorLockMode.Locked) return;
            if (keyboard != null && keyboard.rKey.wasPressedThisFrame) Retry();
            TargetStation = -1;
            RaycastHit hit;
            if (party.TryGetInteractionHit(2.6f, out hit))
            {
                var station = hit.collider.GetComponentInParent<QuietHoldStation>();
                if (station != null) TargetStation = station.index;
            }
            if (keyboard != null && keyboard.eKey.wasPressedThisFrame)
            {
                if (IsHolding(party.ActivePlayer)) State.Release(party.ActivePlayer);
                else if (TargetStation >= 0) TryToggleStation(TargetStation, party.ActivePlayer);
            }
            party.TickInput(IsHolding(party.ActivePlayer));
            TickSimulation(Time.deltaTime, party.IsSprinting ? 1 : 0);
        }

        /// <summary>Nhận ID người; trả true khi người đó đang đứng giữ một trong hai nút.</summary>
        public bool IsHolding(int player) => State != null && (State.Owner(0) == player || State.Owner(1) == player);

        /// <summary>Nhận nút và người; kiểm tra đội, khoảng cách, vật cản rồi giữ/nhả, trả true nếu hợp lệ.</summary>
        public bool TryToggleStation(int station, int player)
        {
            if (!Active || State == null || station < 0 || station >= stations.Length || player < 0 ||
                player >= State.PlayerCount || !party.players[player].gameObject.activeInHierarchy) return false;
            if (State.Owner(station) == player) { State.Release(player); return true; }
            Vector3 eye = party.players[player].position + Vector3.up * 1.4f;
            Vector3 target = stations[station].transform.position;
            if (Vector3.Distance(eye, target) > 2.8f) return false;
            // Không thể giữ xuyên tường: bỏ collider của chính người gửi lệnh, giữ mọi vật cản khác.
            foreach (RaycastHit hit in Physics.RaycastAll(eye, (target-eye).normalized, Vector3.Distance(eye,target),
                ~0, QueryTriggerInteraction.Ignore))
                if (!hit.transform.IsChildOf(party.players[player]) && !hit.transform.IsChildOf(stations[station].transform))
                    return false;
            return State.Hold(station, player);
        }

        /// <summary>Nhận delta giây và số người chạy; hủy giữ xa/mất người, tính ồn, kiểm tra đích và cập nhật cửa.</summary>
        public void TickSimulation(float seconds, int runners)
        {
            if (!Active || State == null) return;
            for (int i = 0; i < stations.Length; i++)
            {
                int owner = State.Owner(i);
                if (owner >= 0 && (!party.players[owner].gameObject.activeInHierarchy ||
                    Vector3.Distance(party.players[owner].position + Vector3.up * 1.4f, stations[i].transform.position) > 3))
                    State.Release(owner);
            }
            State.Tick(seconds, runners);
            if (State.Failed) { Retry(true); return; }
            if (State.TryComplete(AllAcross())) Status = settings.success;
            SafetyHolding = !State.GateRequested && GateOccupied();
            gate.SetOpen(State.GateRequested || SafetyHolding);
            RefreshPresentation();
        }

        /// <summary>Không nhận tham số; trả true nếu capsule người chơi chạm vùng quét cửa, kể cả mép vùng.</summary>
        public bool GateOccupied()
        {
            for (int i = 0; i < party.PartySize; i++)
            {
                var body = party.players[i].GetComponent<CharacterController>();
                if (body != null && body.enabled && body.bounds.Intersects(safetyZone.bounds)) return true;
            }
            return false;
        }

        /// <summary>Không nhận tham số; trả true nếu đủ đúng đội hiện tại đứng trong vùng sau cửa.</summary>
        public bool AllAcross()
        {
            if (party.PartySize != State.PlayerCount) return false;
            for (int i = 0; i < State.PlayerCount; i++)
                if (!party.players[i].gameObject.activeInHierarchy || !goalZone.bounds.Contains(party.players[i].position)) return false;
            return true;
        }

        /// <summary>Không nhận tham số; cập nhật nhãn nút và màu loa theo trạng thái, không tạo asset mới.</summary>
        void RefreshPresentation()
        {
            for (int i = 0; i < stations.Length; i++) stations[i].Show(State.Owner(i));
            if (speakerProperties == null) speakerProperties = new MaterialPropertyBlock();
            speakerProperties.SetColor("_BaseColor", State.SpeakerOn ? new Color(.95f,.2f,.1f) : new Color(.1f,.8f,.35f));
            speakerIndicator.SetPropertyBlock(speakerProperties);
        }

        /// <summary>Không nhận tham số; ngừng input và thả quyền giữ khi map chuyển sang giai đoạn khác.</summary>
        public void StopSession()
        {
            Active = false;
            if (State != null) State.Reset();
        }
    }
}
