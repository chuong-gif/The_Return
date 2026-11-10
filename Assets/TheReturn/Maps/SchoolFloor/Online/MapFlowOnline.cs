/*
 * Mục đích: Nối luồng map (MissionFlowController) với mạng khi chơi online (T-028). Dùng MissionNetBridge dùng chung. HOST: chuyển
 * MissionFlowController sang vai Host (chạy luật như cũ, chỉ đăng ký nhiệm vụ đã có bản online), mỗi khi trạng thái luồng đổi thì gửi ảnh chụp
 * (MapFlowState) cho từng máy khách. MÁY KHÁCH: vai Client, xin trạng thái cho tới khi nhận được rồi áp dụng mỗi lần host gửi (cửa, bản đồ nhỏ,
 * điều kiện thắng dùng lại code cũ). Cả hai bật/tắt phần máy khách của từng nhiệm vụ online theo khu đang làm (IOnlineMission). Mất mạng
 * hoặc dừng host thì trả luồng về một máy (ExitOnline). Máy khách không gửi lệnh nào lên host ở luồng map.
 * Hàm: Update; OnCommand; OnStateRequested; OnState; OnRejected; kiểu IOnlineMission.
 */
using System.Collections.Generic;
using TheReturn.Network.Online;
using Unity.Netcode;
using UnityEngine;

namespace TheReturn.SchoolFloorNet
{
    /// <summary>Một nhiệm vụ đã có bản online, do scene khai báo cho MapFlowOnline.</summary>
    public interface IOnlineMission
    {
        /// <summary>ID khu của nhiệm vụ (ví dụ Hanh_Lang_6).</summary>
        string AreaId { get; }

        /// <summary>Chỉ host gọi; trả adapter chạy thật để đăng ký vào registry của host.</summary>
        IMissionAdapter CreateHostAdapter();

        /// <summary>Mọi máy gọi; nhận true khi khu của nhiệm vụ đang được làm (bật phần máy khách), false khi không.</summary>
        void SetClientActive(bool active);
    }

    [DefaultExecutionOrder(-40)]
    public sealed class MapFlowOnline : MonoBehaviour, IMissionHostEndpoint, IMissionClientEndpoint
    {
        [SerializeField] private MissionFlowController flow;
        [SerializeField] private MissionNetBridge bridge;
        [Tooltip("Các component cài IOnlineMission (vd PlankOnlineMission).")]
        [SerializeField] private MonoBehaviour[] onlineMissions;

        private readonly List<IOnlineMission> missions = new List<IOnlineMission>();
        private FlowNetRole role = FlowNetRole.Offline;
        private string lastSent;
        private int sentVersion;
        private int receivedVersion = -1;
        private bool gotState;
        private float nextRequest;
        private string lastActiveArea = "";

        /// <summary>Vai hiện tại của máy này trong luồng map online.</summary>
        public FlowNetRole Role { get { return role; } }

        private void Awake()
        {
            missions.Clear();
            if (onlineMissions == null) return;
            for (int i = 0; i < onlineMissions.Length; i++)
            {
                IOnlineMission mission = onlineMissions[i] as IOnlineMission;
                if (mission != null) missions.Add(mission);
                else if (onlineMissions[i] != null) Debug.LogWarning("[MapFlowOnline] " + onlineMissions[i].name + " không cài IOnlineMission.", this);
            }
        }

        private void OnDisable()
        {
            if (role != FlowNetRole.Offline) Leave();
        }

        private void Update()
        {
            NetworkManager manager = NetworkManager.Singleton;
            bool online = manager != null && manager.IsListening && flow != null && bridge != null && bridge.IsSpawned;
            if (!online)
            {
                if (role != FlowNetRole.Offline) Leave();
                return;
            }
            FlowNetRole wanted = manager.IsServer ? FlowNetRole.Host : FlowNetRole.Client;
            if (role != wanted)
            {
                if (role != FlowNetRole.Offline) Leave();
                Enter(wanted);
            }
            if (role == FlowNetRole.Host) PushIfChanged();
            else RequestIfNeeded();
            UpdateMissionClients();
        }

        private void Enter(FlowNetRole newRole)
        {
            List<IMissionAdapter> adapters = new List<IMissionAdapter>();
            for (int i = 0; i < missions.Count; i++)
            {
                adapters.Add(newRole == FlowNetRole.Host ? missions[i].CreateHostAdapter() : new RemoteMissionAdapter(missions[i].AreaId));
            }
            flow.EnterOnline(newRole, adapters);
            if (newRole == FlowNetRole.Host) bridge.HostEndpoint = this;
            else bridge.ClientEndpoint = this;
            role = newRole;
            lastSent = null;
            sentVersion = 0;
            receivedVersion = -1;
            gotState = false;
            lastActiveArea = "";
            Debug.Log("[MapFlowOnline] Vào luồng map online, vai " + newRole + ", " + missions.Count + " nhiệm vụ có bản online.");
        }

        private void Leave()
        {
            if (bridge != null)
            {
                if (bridge.HostEndpoint == (IMissionHostEndpoint)this) bridge.HostEndpoint = null;
                if (bridge.ClientEndpoint == (IMissionClientEndpoint)this) bridge.ClientEndpoint = null;
            }
            if (flow != null) flow.ExitOnline();
            for (int i = 0; i < missions.Count; i++) missions[i].SetClientActive(false);
            role = FlowNetRole.Offline;
            lastActiveArea = "";
            Debug.Log("[MapFlowOnline] Rời luồng map online, về luồng một máy.");
        }

        private void PushIfChanged()
        {
            MapFlowState state = flow.BuildState();
            if (state == null) return;
            string encoded = state.Encode();
            if (encoded == lastSent) return;
            lastSent = encoded;
            sentVersion++;
            OnlinePartySlots slots = OnlinePartySlots.Instance;
            if (slots == null) return;
            for (int slot = 0; slot < slots.TeamSize; slot++)
            {
                ulong clientId;
                if (!slots.TryGetClient(slot, out clientId) || clientId == NetworkManager.ServerClientId) continue;
                bridge.SendState(clientId, sentVersion, encoded, string.Empty);
            }
        }

        private void RequestIfNeeded()
        {
            if (gotState || Time.time < nextRequest) return;
            nextRequest = Time.time + 1f;
            bridge.RequestState();
        }

        private void UpdateMissionClients()
        {
            if (flow.Registry == null) return;
            string active = flow.Registry.Progress.ActiveAreaId ?? string.Empty;
            if (active == lastActiveArea) return;
            lastActiveArea = active;
            for (int i = 0; i < missions.Count; i++) missions[i].SetClientActive(missions[i].AreaId == active);
        }

        /// <summary>Luồng map không nhận lệnh từ máy khách; trả lý do từ chối.</summary>
        public string OnCommand(ulong senderClientId, int sequence, string payload)
        {
            return "Luồng map không nhận lệnh từ máy khách.";
        }

        /// <summary>Nhận mã client vừa hỏi; gửi lại trạng thái luồng map hiện tại cho client đó.</summary>
        public void OnStateRequested(ulong senderClientId)
        {
            if (role != FlowNetRole.Host || flow == null) return;
            MapFlowState state = flow.BuildState();
            if (state == null) return;
            bridge.SendState(senderClientId, Mathf.Max(1, sentVersion), state.Encode(), string.Empty);
        }

        /// <summary>Nhận phiên bản và chuỗi trạng thái từ host; áp dụng vào luồng nếu mới hơn bản đã nhận.</summary>
        public void OnState(int version, string publicPayload, string privatePayload)
        {
            if (role != FlowNetRole.Client || version <= receivedVersion) return;
            MapFlowState state;
            if (!MapFlowState.TryDecode(publicPayload, out state)) return;
            if (!flow.ApplyRemoteState(state)) return;
            receivedVersion = version;
            gotState = true;
        }

        /// <summary>Không dùng (máy khách không gửi lệnh luồng map).</summary>
        public void OnRejected(string detail)
        {
        }
    }
}
