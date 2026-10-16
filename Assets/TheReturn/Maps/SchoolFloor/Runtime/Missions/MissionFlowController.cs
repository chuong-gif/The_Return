/*
 * Mục đích: Điều phối luồng nhiệm vụ TỰ CHỌN của tầng học (T-004). Giữ MissionRegistry; cho đội đi lại tự do khi chưa có
 * nhiệm vụ nào chạy; bắt đầu một khu khi CẢ ĐỘI đứng đủ trong vùng chuẩn bị của một cổng; hoàn thành khu khi nhiệm vụ xong;
 * cho SchoolFloorAccessController biết cửa nào mở/khóa. KHÔNG sửa logic từng nhiệm vụ (chỉ gọi qua adapter).
 * Vị trí đội lấy từ preparationZone của cổng trong scene, không hardcode tọa độ.
 * Kiểu: GateBinding, MissionFlowController.
 * Hàm MissionFlowController: Awake; Start; Update; ResetFlow; TryGetGateState; AnyMissionActive; FreeRoam; TryStartFromGates.
 */
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheReturn
{
    /// <summary>Gắn một cổng trong scene với khu của nó và với khu chuẩn bị mà đội đi từ đó tới (San, Phong_k hoặc Hanh_Lang_k).</summary>
    [Serializable]
    public sealed class GateBinding
    {
        public SchoolPreparationGate gate;
        [Tooltip("ID khu (tên object trong scene), ví dụ Phong_1, Hanh_Lang_1.")]
        public string areaId;
        [Tooltip("ID nơi đội đi vào cổng này: San (sân), hoặc phòng/hành lang kề.")]
        public string entryFromId;
    }

    /// <summary>Bộ điều phối luồng chọn khu: bắt đầu khu khi đủ đội trong vùng chuẩn bị, hoàn thành khu khi xong, quyết định cửa.</summary>
    [DefaultExecutionOrder(-50)]
    public sealed class MissionFlowController : MonoBehaviour
    {
        public SchoolFloorFlow flow;
        [Tooltip("Một dòng cho mỗi SchoolPreparationGate (22 cổng): khu của cổng và nơi đội đi từ đó tới.")]
        public GateBinding[] bindings;

        public MissionRegistry Registry { get; private set; }
        /// <summary>True khi có nhiệm vụ nào đang chạy (kể cả nhiệm vụ do công cụ test bắt đầu ngoài registry).</summary>
        public bool AnyActive { get; private set; }

        readonly Dictionary<SchoolPreparationGate, GateBinding> byGate = new Dictionary<SchoolPreparationGate, GateBinding>();

        /// <summary>Không nhận tham số; dựng registry từ flow và bảng cổng. Thiếu flow thì tự tắt (luồng cũ tiếp tục chạy).</summary>
        void Awake()
        {
            if (flow == null || flow.attendance == null)
            {
                Debug.LogError("MissionFlowController thiếu flow/attendance; luồng tuyến tính cũ vẫn dùng.", this);
                enabled = false;
                return;
            }
            Registry = MissionRegistryFactory.CreateDefault(flow);
            byGate.Clear();
            if (bindings != null)
                foreach (GateBinding b in bindings)
                    if (b != null && b.gate != null) byGate[b.gate] = b;
        }

        /// <summary>Không nhận tham số; Attendance KHÔNG tự chạy: nhường quyền và tắt HUD cho tới khi đội chọn Phong_1.</summary>
        void Start()
        {
            if (Registry == null) return;
            flow.attendance.ExternalControl = true;
            if (flow.attendanceHud != null) flow.attendanceHud.enabled = false;
        }

        /// <summary>Không nhận tham số; mỗi frame: hoàn thành khu đã xong, rồi (khi chưa nhiệm vụ nào chạy) cho đi lại và thử bắt đầu khu.</summary>
        void Update()
        {
            if (Registry == null) return;
            if (Registry.Tick()) PrototypePartyController.SetCursor(true);
            AnyActive = AnyMissionActive();
            if (AnyActive) return;
            FreeRoam();
            TryStartFromGates();
        }

        /// <summary>Không nhận tham số; xóa mọi tiến độ khu (dừng khu đang làm có reset). Gọi từ SchoolFloorFlow.RestartMap.</summary>
        public void ResetFlow()
        {
            if (Registry == null) return;
            Registry.Reset();
            AnyActive = false;
        }

        /// <summary>
        /// Nhận cổng; trả false nếu cổng không có trong bảng (để AccessController dùng luồng cũ). Ngược lại điền open (cửa mở),
        /// active (khu của cổng đang làm) và done (khu đã xong). Khu chưa có nhiệm vụ luôn mở; khu đã xong luôn mở;
        /// khu chưa xong chỉ mở khi không có khu nào đang làm.
        /// </summary>
        public bool TryGetGateState(SchoolPreparationGate gate, out bool open, out bool active, out bool done)
        {
            open = true; active = false; done = false;
            GateBinding b;
            if (Registry == null || gate == null || !byGate.TryGetValue(gate, out b)) return false;
            AreaInfo area = Registry.Progress.Find(b.areaId);
            if (area == null || !area.HasMission) return true;
            IMissionAdapter adapter = Registry.Find(b.areaId);
            active = Registry.Progress.IsActive(b.areaId) || (adapter != null && adapter.Active);
            done = Registry.Progress.IsDone(b.areaId);
            open = done || (!AnyActive && Registry.Progress.ActiveAreaId == null);
            return true;
        }

        /// <summary>Không nhận tham số; trả true nếu có khu đang làm trong registry hoặc bất kỳ adapter nào báo Active.</summary>
        bool AnyMissionActive()
        {
            if (Registry.Progress.ActiveAreaId != null) return true;
            foreach (AreaInfo area in Registry.Progress.Areas)
            {
                IMissionAdapter adapter = Registry.Find(area.Id);
                if (adapter != null && adapter.Active) return true;
            }
            return false;
        }

        /// <summary>Không nhận tham số; khi chưa có nhiệm vụ nào chạy: Tab/Esc thả-khóa chuột, phím 1-4 đổi vai, di chuyển/nhìn khi chuột đã khóa.</summary>
        void FreeRoam()
        {
            PrototypePartyController party = flow.attendance.party;
            if (party == null || party.PartySize == 0) return;
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.tabKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame)
                    PrototypePartyController.SetCursor(Cursor.lockState != CursorLockMode.Locked);
                if (keyboard.digit1Key.wasPressedThisFrame) party.SwitchRole(0, false);
                if (keyboard.digit2Key.wasPressedThisFrame) party.SwitchRole(1, false);
                if (keyboard.digit3Key.wasPressedThisFrame) party.SwitchRole(2, false);
                if (keyboard.digit4Key.wasPressedThisFrame) party.SwitchRole(3, false);
            }
            if (Cursor.lockState == CursorLockMode.Locked) party.TickInput(false);
        }

        /// <summary>
        /// Không nhận tham số; duyệt bảng cổng: nếu cả đội (từ 2 người) đứng đủ trong vùng chuẩn bị của một cổng thuộc khu chưa xong,
        /// nhờ registry kiểm luật (lối vào, một khu một lúc) rồi bắt đầu nhiệm vụ thật. Bắt đầu tối đa một khu mỗi frame.
        /// </summary>
        void TryStartFromGates()
        {
            if (bindings == null) return;
            PrototypePartyController party = flow.attendance.party;
            if (party == null || party.PartySize < 2) return;
            foreach (GateBinding b in bindings)
            {
                if (b == null || b.gate == null) continue;
                AreaInfo area = Registry.Progress.Find(b.areaId);
                if (area == null || !area.HasMission || Registry.Progress.IsDone(b.areaId)) continue;
                if (b.gate.CountInside(party) != party.PartySize) continue;
                if (Registry.TryBegin(b.areaId, b.entryFromId, true) == BeginResult.Started) return;
            }
        }
    }
}
