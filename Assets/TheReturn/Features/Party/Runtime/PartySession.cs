/*
 * Mục đích: Giữ nhóm chơi hiện tại xuyên các scene (T-008): MainMenu tạo nhóm, Sanh_Cho và SchoolFloor đọc số người từ đây.
 * Static nên sống qua LoadScene (mất khi tắt game hoặc nạp lại domain). Khi chưa có nhóm đã bắt đầu thì HasStartedParty = false và
 * PlayerCount trả 4 (bản thử chạy thẳng một scene như trước).
 * Hàm: Service (lấy/đặt dịch vụ nhóm, mặc định LocalPartyService); HasStartedParty; PlayerCount; LocalSlot; Reset.
 */
using TheReturn;

namespace TheReturn
{
    /// <summary>Phiên nhóm chơi dùng chung giữa các scene.</summary>
    public static class PartySession
    {
        const int DefaultPlayerCount = 4;

        static IPartyService service;

        /// <summary>Dịch vụ nhóm hiện dùng; mặc định LocalPartyService (một máy). Gán dịch vụ khác khi có mạng.</summary>
        public static IPartyService Service
        {
            get { if (service == null) service = new LocalPartyService("local"); return service; }
            set { service = value; }
        }

        /// <summary>True khi đang có nhóm và chủ phòng đã bấm Bắt đầu.</summary>
        public static bool HasStartedParty
        {
            get { PartySnapshot s = Service.Current; return s != null && s.Started; }
        }

        /// <summary>Số người của nhóm đã bắt đầu (2-4); chưa có nhóm đã bắt đầu thì 4 như mặc định cũ.</summary>
        public static int PlayerCount
        {
            get
            {
                PartySnapshot s = Service.Current;
                if (s == null || !s.Started) return DefaultPlayerCount;
                return s.Count < 2 ? 2 : (s.Count > 4 ? 4 : s.Count);
            }
        }

        /// <summary>Vị trí (0-3) của người chơi trên máy này trong nhóm; -1 nếu chưa có nhóm.</summary>
        public static int LocalSlot
        {
            get { PartySnapshot s = Service.Current; return s == null ? -1 : s.LocalSlot; }
        }

        /// <summary>Không nhận tham số; rời nhóm (nếu có) và bỏ dịch vụ để lần sau tạo mới.</summary>
        public static void Reset()
        {
            if (service != null && service.Current != null) service.Leave();
            service = null;
        }
    }
}
