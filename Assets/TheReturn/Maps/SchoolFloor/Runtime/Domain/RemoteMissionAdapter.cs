/*
 * Mục đích: Adapter GIẢ cho máy khách khi chơi online (T-028): nhiệm vụ thật chạy trên host, máy khách chỉ cần khu có đăng ký nhiệm vụ để
 * MapProgress.RestoreSnapshot khôi phục được khu đang làm / đã xong. Không bao giờ Active hay Solved; Begin/Stop không làm gì.
 * Thuần C#, không phụ thuộc Unity. Thành viên: AreaId, Active, Solved, CanHandOff, Begin, Stop.
 */
using System;

namespace TheReturn
{
    /// <summary>Đại diện nhiệm vụ chạy ở máy khác (host) cho registry của máy khách.</summary>
    public sealed class RemoteMissionAdapter : IMissionAdapter
    {
        /// <summary>ID khu của nhiệm vụ.</summary>
        public string AreaId { get; }

        /// <summary>Luôn false: nhiệm vụ không chạy ở máy này.</summary>
        public bool Active { get { return false; } }

        /// <summary>Luôn false: kết quả do host báo qua trạng thái luồng map.</summary>
        public bool Solved { get { return false; } }

        /// <summary>Luôn false.</summary>
        public bool CanHandOff { get { return false; } }

        /// <summary>Nhận ID khu khác rỗng; ném ArgumentException nếu rỗng.</summary>
        public RemoteMissionAdapter(string areaId)
        {
            if (string.IsNullOrEmpty(areaId)) throw new ArgumentException("areaId rỗng", nameof(areaId));
            AreaId = areaId;
        }

        /// <summary>Không làm gì (nhiệm vụ do host bắt đầu).</summary>
        public void Begin() { }

        /// <summary>Nhận cờ reset; không làm gì.</summary>
        public void Stop(bool reset) { }
    }
}
