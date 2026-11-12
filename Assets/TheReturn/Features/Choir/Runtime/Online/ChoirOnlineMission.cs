/*
 * Mục đích: Đăng ký nhiệm vụ Hợp xướng (Phong_6) với luồng map online (T-040 Q-014). Cài IOnlineMission như PlankOnlineMission: trên host tạo adapter chạy thật bọc ChoirOnlineHost
 * (Begin = BeginMission, Stop = EndMission, Solved/CanHandOff theo luật host); trên mọi máy bật/tắt ChoirOnlineClient theo việc khu này có đang được làm không.
 * Ở map thật: host autoStart = false và client tắt sẵn. Đặt cùng đối tượng với host và client.
 * Hàm: CreateHostAdapter; SetClientActive; kiểu lồng HostAdapter (IMissionAdapter); thuộc tính AreaId.
 */
using TheReturn.SchoolFloorNet;
using UnityEngine;

namespace TheReturn.ChoirNet.Online
{
    public sealed class ChoirOnlineMission : MonoBehaviour, IOnlineMission
    {
        [SerializeField] private string areaId = "Phong_6";
        [SerializeField] private ChoirOnlineHost host;
        [SerializeField] private ChoirOnlineClient client;

        /// <summary>ID khu của nhiệm vụ.</summary>
        public string AreaId { get { return areaId; } }

        /// <summary>Trả adapter chạy thật cho registry của host.</summary>
        public IMissionAdapter CreateHostAdapter()
        {
            return new HostAdapter(areaId, host);
        }

        /// <summary>Nhận true khi khu của nhiệm vụ đang được làm; bật/tắt phần máy khách (tắt thì thôi nhận gói và vẽ HUD).</summary>
        public void SetClientActive(bool active)
        {
            if (client != null) client.enabled = active;
        }

        private sealed class HostAdapter : IMissionAdapter
        {
            private readonly ChoirOnlineHost host;

            public HostAdapter(string area, ChoirOnlineHost hostComponent)
            {
                AreaId = area;
                host = hostComponent;
            }

            public string AreaId { get; private set; }
            public bool Active { get { return host != null && host.MissionActive; } }
            public bool Solved { get { return host != null && host.Solved; } }
            public bool CanHandOff { get { return host != null && host.CanHandOff; } }

            public void Begin()
            {
                if (host != null) host.BeginMission();
            }

            public void Stop(bool reset)
            {
                if (host != null) host.EndMission();
            }
        }
    }
}
