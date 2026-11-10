/*
 * Mục đích: Đăng ký nhiệm vụ cầu ván (Hanh_Lang_6) với luồng map online (T-028). Cài IOnlineMission: trên host tạo adapter chạy thật bọc
 * PlankOnlineHost (Begin = BeginMission, Solved/CanHandOff theo luật host, Stop = EndMission); trên mọi máy bật/tắt PlankOnlineClient
 * theo việc khu này có đang được làm không. Đặt cùng PlankOnlineHost và PlankOnlineClient (autoStart/startActive = false ở map thật).
 * Hàm: CreateHostAdapter; SetClientActive; kiểu lồng PlankHostAdapter (IMissionAdapter); thuộc tính AreaId.
 */
using TheReturn.SchoolFloorNet;
using UnityEngine;

namespace TheReturn.PlankNet.Online
{
    public sealed class PlankOnlineMission : MonoBehaviour, IOnlineMission
    {
        [SerializeField] private string areaId = "Hanh_Lang_6";
        [SerializeField] private PlankOnlineHost host;
        [SerializeField] private PlankOnlineClient client;

        /// <summary>ID khu của nhiệm vụ.</summary>
        public string AreaId { get { return areaId; } }

        /// <summary>Trả adapter chạy thật cho registry của host.</summary>
        public IMissionAdapter CreateHostAdapter()
        {
            return new PlankHostAdapter(areaId, host);
        }

        /// <summary>Nhận true khi khu của nhiệm vụ đang được làm; bật/tắt phần máy khách.</summary>
        public void SetClientActive(bool active)
        {
            if (client != null) client.MissionActive = active;
        }

        private sealed class PlankHostAdapter : IMissionAdapter
        {
            private readonly PlankOnlineHost host;

            public PlankHostAdapter(string area, PlankOnlineHost hostComponent)
            {
                AreaId = area;
                host = hostComponent;
            }

            public string AreaId { get; }
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
