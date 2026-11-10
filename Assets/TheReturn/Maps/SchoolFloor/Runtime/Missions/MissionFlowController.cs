/*
 * Mục đích: Điều phối luồng nhiệm vụ TỰ CHỌN của tầng học (T-004). Giữ MissionRegistry; cho đội đi lại tự do khi chưa có
 * nhiệm vụ nào chạy; bắt đầu một khu khi CẢ ĐỘI đứng đủ trong vùng chuẩn bị của một cổng; hoàn thành khu khi nhiệm vụ xong;
 * cho SchoolFloorAccessController biết cửa nào mở/khóa. KHÔNG sửa logic từng nhiệm vụ (chỉ gọi qua adapter).
 * Vị trí đội lấy từ preparationZone của cổng trong scene, không hardcode tọa độ.
 * Kiểu: GateBinding, MissionFlowController.
 * LUẬT CỬA (T-016, chủ dự án chốt): lúc đầu MỌI cửa đóng. Cả đội đứng đủ ở khu chuẩn bị NGOÀI cửa (SchoolPreparationGate.CountOutside)
 * thì cửa đó 'armed' = mở. Cả đội vào đủ vùng chuẩn bị bên trong thì nhiệm vụ bắt đầu và khóa mọi cửa. Xong khu thì cả ba lối của khu mở lại.
 * T-033: «vào đủ» tính bằng CountEntered (vùng chuẩn bị bên trong HOẶC phía trong cửa, tầm enteredDepth/enteredSideMargin), vì người chơi thật đi sâu vào khu.
 * Khu chưa có nhiệm vụ: cửa đóng hẳn. Cửa armed tự đóng lại nếu cả đội rời cả hai vùng quá DisarmSeconds giây.
 * Hàm MissionFlowController: Awake; Start; Update; ResetFlow; TryGetGateState; GateHasMission; AreaOfGate; GateGatherCount; GatherSummary;
 * AnyMissionActive; FreeRoam; UpdateArming; TryStartFromGates.
 * T-028 (chơi online): FlowNetRole (Offline = luồng một máy cũ, MẶC ĐỊNH; Host = chạy luật và gửi trạng thái; Client = chỉ hiển thị trạng thái host gửi);
 * EnterOnline; ExitOnline; BuildState; ApplyRemoteState. Online không gọi FreeRoam (di chuyển do OnlinePartyLocalDriver) và chỉ đăng ký nhiệm vụ đã có bản online.
 */
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheReturn
{
    /// <summary>Vai của máy này trong luồng map khi chơi online (T-028).</summary>
    public enum FlowNetRole
    {
        /// <summary>Chơi một máy: luồng cũ, không đổi.</summary>
        Offline,
        /// <summary>Máy chủ: chạy luật, bắt đầu/hoàn thành khu, gửi trạng thái cho máy khách.</summary>
        Host,
        /// <summary>Máy khách: không chạy luật, chỉ áp dụng trạng thái host gửi.</summary>
        Client
    }

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
        [Tooltip("Map mới (T-040): nếu có thì dùng topology của nó thay vòng 12 khu cũ. Để trống = hành vi cũ.")]
        public MapTopologyProvider topology;

        public MissionRegistry Registry { get; private set; }
        /// <summary>True khi có nhiệm vụ nào đang chạy (kể cả nhiệm vụ do công cụ test bắt đầu ngoài registry).</summary>
        public bool AnyActive { get; private set; }

        /// <summary>Số giây cả đội rời cả hai vùng (ngoài + trong) của cửa đang mở thì cửa đóng lại.</summary>
        public float disarmSeconds = 3f;

        /// <summary>Cổng đang được mở vì cả đội đã tập hợp ở khu chuẩn bị ngoài cửa (null = chưa cửa nào).</summary>
        public SchoolPreparationGate ArmedGate { get; private set; }

        /// <summary>Vai online của máy này; Offline là mặc định (luồng một máy cũ).</summary>
        public FlowNetRole NetRole { get; private set; }
        float disarmTimer;

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
            Registry = NewRegistry();
            byGate.Clear();
            if (bindings != null)
                foreach (GateBinding b in bindings)
                    if (b != null && b.gate != null) byGate[b.gate] = b;
        }

        /// <summary>Không nhận tham số; trả MapProgress mới: từ topology (nếu có) hoặc vòng 6 phòng/6 hành lang cũ.</summary>
        MapProgress NewProgress()
        {
            return topology != null ? topology.CreateProgress() : MapProgress.CreateRing(6, null);
        }

        /// <summary>Không nhận tham số; dựng registry mặc định từ flow trên topology hiện dùng (vòng cũ khi topology trống, nên hành vi cũ không đổi).</summary>
        MissionRegistry NewRegistry()
        {
            return MissionRegistryFactory.Create(NewProgress(), flow);
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
            if (NetRole == FlowNetRole.Client)
            {
                // Máy khách chỉ hiển thị trạng thái host gửi (ApplyRemoteState), không chạy luật.
                AnyActive = Registry.Progress.ActiveAreaId != null;
                return;
            }
            if (Registry.Tick() && NetRole == FlowNetRole.Offline) PrototypePartyController.SetCursor(true);
            AnyActive = AnyMissionActive();
            if (AnyActive) { ArmedGate = null; return; }
            if (NetRole == FlowNetRole.Offline) FreeRoam();
            UpdateArming();
            TryStartFromGates();
        }

        /// <summary>Không nhận tham số; xóa mọi tiến độ khu (dừng khu đang làm có reset). Gọi từ SchoolFloorFlow.RestartMap.</summary>
        public void ResetFlow()
        {
            if (Registry == null) return;
            Registry.Reset();
            AnyActive = false;
            ArmedGate = null;
            disarmTimer = 0;
        }

        /// <summary>
        /// Nhận vai online và các adapter của những nhiệm vụ ĐÃ có bản online (Host: adapter chạy thật; Client: adapter giả chỉ để khu có nhiệm vụ).
        /// Dựng lại registry với vòng 6 phòng/6 hành lang rỗng rồi chỉ đăng ký các adapter này, nên khu chưa có bản online có cửa đóng hẳn.
        /// Vai Offline tương đương ExitOnline. Gọi trước khi cả đội vào chơi.
        /// </summary>
        public void EnterOnline(FlowNetRole role, IEnumerable<IMissionAdapter> adapters)
        {
            if (role == FlowNetRole.Offline)
            {
                ExitOnline();
                return;
            }
            MissionRegistry registry = new MissionRegistry(NewProgress());
            if (adapters != null)
            {
                foreach (IMissionAdapter adapter in adapters)
                {
                    if (adapter != null) registry.Register(adapter);
                }
            }
            Registry = registry;
            NetRole = role;
            AnyActive = false;
            ArmedGate = null;
            disarmTimer = 0;
        }

        /// <summary>Không nhận tham số; quay về luồng một máy cũ (registry mặc định với mọi nhiệm vụ offline).</summary>
        public void ExitOnline()
        {
            NetRole = FlowNetRole.Offline;
            ArmedGate = null;
            AnyActive = false;
            disarmTimer = 0;
            if (flow != null) Registry = NewRegistry();
        }

        /// <summary>Không nhận tham số; trả ảnh chụp luồng map để host gửi cho máy khách (null nếu chưa có registry).</summary>
        public MapFlowState BuildState()
        {
            if (Registry == null) return null;
            return MapFlowState.From(Registry.Progress, IndexOfGate(ArmedGate));
        }

        /// <summary>
        /// Nhận ảnh chụp từ host; CHỈ máy khách áp dụng (khôi phục tiến độ, cổng đang mở). Trả false và không đổi gì nếu vai không phải Client,
        /// ảnh rỗng hoặc ảnh không khớp map (khu lạ, khu đang làm chưa có nhiệm vụ).
        /// </summary>
        public bool ApplyRemoteState(MapFlowState state)
        {
            if (NetRole != FlowNetRole.Client || Registry == null || state == null) return false;
            if (!Registry.Progress.RestoreSnapshot(state.ToSnapshot())) return false;
            AnyActive = Registry.Progress.ActiveAreaId != null;
            ArmedGate = GateAt(state.ArmedGateIndex);
            return true;
        }

        /// <summary>Nhận cổng (null được); trả chỉ số cổng trong bảng bindings, hoặc -1 nếu null/không có.</summary>
        int IndexOfGate(SchoolPreparationGate gate)
        {
            if (gate == null || bindings == null) return -1;
            for (int i = 0; i < bindings.Length; i++)
            {
                if (bindings[i] != null && bindings[i].gate == gate) return i;
            }
            return -1;
        }

        /// <summary>Nhận chỉ số trong bảng bindings; trả cổng tương ứng hoặc null nếu ngoài khoảng.</summary>
        SchoolPreparationGate GateAt(int index)
        {
            if (bindings == null || index < 0 || index >= bindings.Length || bindings[index] == null) return null;
            return bindings[index].gate;
        }

        /// <summary>
        /// Nhận cổng; trả false nếu cổng không có trong bảng (để AccessController dùng luồng cũ). Ngược lại điền open (cửa mở),
        /// active (khu của cổng đang làm) và done (khu đã xong). Luật: khu chưa có nhiệm vụ luôn ĐÓNG; khu đã xong luôn mở;
        /// khu chưa xong chỉ mở ở đúng cổng đang armed (cả đội đã tập hợp ngoài cửa) khi chưa có khu nào đang làm; khu đang làm đóng.
        /// </summary>
        public bool TryGetGateState(SchoolPreparationGate gate, out bool open, out bool active, out bool done)
        {
            open = false; active = false; done = false;
            GateBinding b;
            if (Registry == null || gate == null || !byGate.TryGetValue(gate, out b)) return false;
            AreaInfo area = Registry.Progress.Find(b.areaId);
            if (area == null || !area.HasMission) return true;
            IMissionAdapter adapter = Registry.Find(b.areaId);
            active = Registry.Progress.IsActive(b.areaId) || (adapter != null && adapter.Active);
            done = Registry.Progress.IsDone(b.areaId);
            open = done || (ArmedGate == gate && !AnyActive && Registry.Progress.ActiveAreaId == null);
            return true;
        }

        /// <summary>Nhận cổng; trả true nếu cổng có trong bảng và khu của cổng ĐÃ có nhiệm vụ (dùng cho nhãn cổng, độc lập chỉ số mission cũ).</summary>
        public bool GateHasMission(SchoolPreparationGate gate)
        {
            GateBinding b;
            if (Registry == null || gate == null || !byGate.TryGetValue(gate, out b)) return false;
            AreaInfo area = Registry.Progress.Find(b.areaId);
            return area != null && area.HasMission;
        }

        /// <summary>Nhận cổng; trả ID khu của cổng hoặc null nếu cổng không có trong bảng.</summary>
        public string AreaOfGate(SchoolPreparationGate gate)
        {
            GateBinding b;
            return gate != null && byGate.TryGetValue(gate, out b) ? b.areaId : null;
        }

        /// <summary>Nhận cổng; trả số người đang đứng ở khu chuẩn bị NGOÀI cửa, hoặc -1 nếu cổng không có trong bảng hay khu chưa có nhiệm vụ (dùng cho nhãn cổng).</summary>
        public int GateGatherCount(SchoolPreparationGate gate)
        {
            if (!GateHasMission(gate)) return -1;
            return gate.CountOutside(flow.attendance.party);
        }

        /// <summary>Không nhận tham số; trả câu ngắn cho UI: khu đang được tập hợp nhiều người nhất ở khu chuẩn bị ('Khu chuẩn bị Phong 1: 2/4') hoặc câu mặc định.</summary>
        public string GatherSummary()
        {
            PrototypePartyController party = flow.attendance.party;
            if (Registry == null || bindings == null || party == null) return "";
            int best = 0; string label = null;
            foreach (GateBinding b in bindings)
            {
                if (b == null || b.gate == null) continue;
                AreaInfo area = Registry.Progress.Find(b.areaId);
                if (area == null || !area.HasMission || Registry.Progress.IsDone(b.areaId)) continue;
                int c = b.gate.CountOutside(party);
                if (c > best) { best = c; label = b.areaId; }
            }
            return label == null ? "Chưa ai ở khu chuẩn bị" : "Khu chuẩn bị " + label.Replace('_', ' ') + ": " + best + "/" + party.PartySize;
        }

        /// <summary>Không nhận tham số; trả câu hướng dẫn cho UI theo trạng thái cổng: cổng đã mở thì nhắc cả đội cùng vào trong (kèm số người đã vào), chưa thì nhắc tập hợp trước cổng.</summary>
        public string GuideHint()
        {
            PrototypePartyController party = flow.attendance.party;
            if (party == null || party.PartySize < 2) return "Cần ít nhất 2 người trong đội";
            if (ArmedGate != null)
                return "Cổng đã mở: cả đội cùng đi vào bên trong để bắt đầu (" + ArmedGate.CountEntered(party) + "/" + party.PartySize + " đã vào)";
            return "Cả đội đứng đủ ở khu chuẩn bị trước cổng của khu muốn chơi để mở cổng";
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
        /// Không nhận tham số; khi chưa nhiệm vụ nào chạy: nếu cả đội (từ 2 người) đứng đủ ở khu chuẩn bị NGOÀI một cổng thuộc khu chưa xong thì cổng đó
        /// 'armed' (cửa mở). Cổng đang armed tự đóng nếu cả đội rời cả hai vùng quá disarmSeconds giây.
        /// Cổng đang armed được GIỮ NGUYÊN chừng nào còn người ở vùng ngoài/đã vào: không chuyển sang cổng khác dù vùng ngoài hai cổng chồng nhau
        /// (lỗi 'Phong 2_Cua 1' ↔ 'Hanh lang 2_Ra': người đầu bước vào làm armed nhảy sang cổng kia, cửa đóng sập).
        /// </summary>
        void UpdateArming()
        {
            PrototypePartyController party = flow.attendance.party;
            if (party == null || party.PartySize < 2 || bindings == null) { ArmedGate = null; return; }
            if (ArmedGate != null && ArmedGate.CountOutside(party) + ArmedGate.CountEntered(party) > 0) { disarmTimer = 0; return; }
            foreach (GateBinding b in bindings)
            {
                if (b == null || b.gate == null) continue;
                AreaInfo area = Registry.Progress.Find(b.areaId);
                if (area == null || !area.HasMission || Registry.Progress.IsDone(b.areaId)) continue;
                if (b.gate.CountOutside(party) != party.PartySize) continue;
                ArmedGate = b.gate;
                disarmTimer = 0;
                break;
            }
            if (ArmedGate == null) return;
            if (ArmedGate.CountOutside(party) + ArmedGate.CountEntered(party) > 0) { disarmTimer = 0; return; }
            disarmTimer += Time.deltaTime;
            if (disarmTimer > disarmSeconds) ArmedGate = null;
        }

        /// <summary>
        /// Không nhận tham số; nếu có cổng armed và cả đội (từ 2 người) đã đứng đủ trong vùng chuẩn bị BÊN TRONG của chính cổng đó,
        /// nhờ registry kiểm luật (lối vào, một khu một lúc) rồi bắt đầu nhiệm vụ thật; thành công thì cửa khóa.
        /// </summary>
        void TryStartFromGates()
        {
            GateBinding b;
            if (ArmedGate == null || !byGate.TryGetValue(ArmedGate, out b)) return;
            PrototypePartyController party = flow.attendance.party;
            if (party == null || party.PartySize < 2) return;
            if (ArmedGate.CountEntered(party) != party.PartySize) return;
            if (Registry.TryBegin(b.areaId, b.entryFromId, true) == BeginResult.Started) ArmedGate = null;
        }
    }
}
