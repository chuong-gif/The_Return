/*
 * Mục đích: Cài đặt IPartyService cho chế độ MỘT MÁY (T-008): giữ PartyState trong bộ nhớ, đóng vai máy chủ giả, có người chơi giả lập để thử
 * luồng 2-4 người khi chưa có mạng. KHÔNG phải mạng thật; 'Vào phòng bằng mã' chỉ vào được phòng đã tạo trong cùng phiên chạy.
 * Kiểu: LocalPartyRegistry (danh sách phòng của 'máy chủ giả'), LocalPartyService.
 * Hàm LocalPartyService: constructor; CreateParty; JoinParty; SetReady; Leave; StartGame; AddSimulatedPlayer; RemoveSimulatedPlayer; HasRealMember; Notify.
 */
using System;
using System.Collections.Generic;

namespace TheReturn
{
    /// <summary>Danh sách phòng đang mở của máy chủ giả một máy.</summary>
    public sealed class LocalPartyRegistry
    {
        public static readonly LocalPartyRegistry Shared = new LocalPartyRegistry();

        readonly Dictionary<string, PartyState> rooms = new Dictionary<string, PartyState>();

        /// <summary>Số phòng đang mở.</summary>
        public int Count { get { return rooms.Count; } }

        /// <summary>Nhận mã; trả phòng hoặc null.</summary>
        public PartyState Find(string code)
        {
            PartyState state;
            return code != null && rooms.TryGetValue(code, out state) ? state : null;
        }

        /// <summary>Nhận phòng; thêm vào danh sách (ghi đè nếu trùng mã).</summary>
        public void Add(PartyState state) { rooms[state.Code] = state; }

        /// <summary>Nhận mã; xóa phòng khỏi danh sách.</summary>
        public void Remove(string code) { if (code != null) rooms.Remove(code); }

        /// <summary>Không nhận tham số; xóa mọi phòng (dùng cho test).</summary>
        public void Clear() { rooms.Clear(); }
    }

    /// <summary>Dịch vụ nhóm một máy với người chơi giả lập.</summary>
    public sealed class LocalPartyService : IPartyService
    {
        const string SimulatedPrefix = "sim-";

        readonly LocalPartyRegistry registry;
        readonly Random rng;
        PartyState state;
        int simulatedCounter;

        public string LocalPlayerId { get; }
        public PartySnapshot Current { get { return state == null ? null : state.Snapshot(LocalPlayerId); } }
        public event Action<PartySnapshot> Changed;

        /// <summary>Nhận mã người chơi trên máy này, danh sách phòng (null = dùng chung) và seed (0 = ngẫu nhiên theo thời gian).</summary>
        public LocalPartyService(string localPlayerId = "local", LocalPartyRegistry registry = null, int seed = 0)
        {
            if (string.IsNullOrEmpty(localPlayerId)) throw new ArgumentException("Thiếu mã người chơi", nameof(localPlayerId));
            LocalPlayerId = localPlayerId;
            this.registry = registry ?? LocalPartyRegistry.Shared;
            rng = seed != 0 ? new Random(seed) : new Random();
        }

        /// <summary>Nhận tên hiển thị; tạo phòng mới với mã chưa trùng, người chơi trên máy này là chủ phòng.</summary>
        public PartyResult CreateParty(string displayName)
        {
            if (state != null) return PartyResult.AlreadyInParty;
            if (string.IsNullOrWhiteSpace(displayName)) return PartyResult.InvalidName;
            string code;
            int guard = 0;
            do { code = PartyCode.Generate(rng); } while (registry.Find(code) != null && ++guard < 1000);
            var room = new PartyState(code);
            PartyResult joined = room.Join(LocalPlayerId, displayName);
            if (joined != PartyResult.Ok) return joined;
            registry.Add(room);
            state = room;
            Notify();
            return PartyResult.Ok;
        }

        /// <summary>Nhận mã phòng người dùng nhập và tên hiển thị; vào phòng đã mở trong cùng phiên chạy.</summary>
        public PartyResult JoinParty(string code, string displayName)
        {
            if (state != null) return PartyResult.AlreadyInParty;
            string normalized = PartyCode.Normalize(code);
            if (normalized == null) return PartyResult.InvalidCode;
            PartyState room = registry.Find(normalized);
            if (room == null) return PartyResult.NotFound;
            PartyResult joined = room.Join(LocalPlayerId, displayName);
            if (joined != PartyResult.Ok) return joined;
            state = room;
            Notify();
            return PartyResult.Ok;
        }

        /// <summary>Nhận trạng thái; đặt sẵn sàng cho người chơi trên máy này.</summary>
        public PartyResult SetReady(bool ready)
        {
            if (state == null) return PartyResult.NoParty;
            PartyResult r = state.SetReady(LocalPlayerId, ready);
            if (r == PartyResult.Ok) Notify();
            return r;
        }

        /// <summary>Không nhận tham số; rời phòng. Phòng rỗng thì đóng. Bắn Changed(null) cho máy này.</summary>
        public PartyResult Leave()
        {
            if (state == null) return PartyResult.NoParty;
            PartyState room = state;
            room.Leave(LocalPlayerId);
            if (!HasRealMember(room)) registry.Remove(room.Code);
            state = null;
            if (Changed != null) Changed(null);
            return PartyResult.Ok;
        }

        /// <summary>Không nhận tham số; chủ phòng (người chơi trên máy này) bắt đầu trò chơi nếu đủ điều kiện.</summary>
        public PartyResult StartGame()
        {
            if (state == null) return PartyResult.NoParty;
            PartyResult r = state.TryStart(LocalPlayerId);
            if (r == PartyResult.Ok) Notify();
            return r;
        }

        /// <summary>Không nhận tham số; THÊM một người chơi giả lập (tự sẵn sàng) để thử khi chưa có mạng. NoParty nếu chưa ở trong nhóm; Full nếu đủ 4.</summary>
        public PartyResult AddSimulatedPlayer()
        {
            if (state == null) return PartyResult.NoParty;
            string id = SimulatedPrefix + (++simulatedCounter);
            PartyResult r = state.Join(id, "Người giả lập " + simulatedCounter);
            if (r != PartyResult.Ok) { simulatedCounter--; return r; }
            state.SetReady(id, true);
            Notify();
            return PartyResult.Ok;
        }

        /// <summary>Không nhận tham số; gỡ người chơi giả lập có vị trí lớn nhất. NoParty nếu chưa ở trong nhóm hoặc không còn người giả lập.</summary>
        public PartyResult RemoveSimulatedPlayer()
        {
            if (state == null) return PartyResult.NoParty;
            PartyMemberInfo target = null;
            foreach (PartyMemberInfo m in state.Snapshot(LocalPlayerId).Members)
                if (m.Id.StartsWith(SimulatedPrefix) && (target == null || m.Slot > target.Slot)) target = m;
            if (target == null) return PartyResult.NoParty;
            PartyResult r = state.Leave(target.Id);
            if (r == PartyResult.Ok) Notify();
            return r;
        }

        /// <summary>Nhận phòng; trả true nếu còn ít nhất một thành viên không phải người giả lập (phòng chỉ còn người giả lập thì đóng).</summary>
        bool HasRealMember(PartyState room)
        {
            foreach (PartyMemberInfo m in room.Snapshot(LocalPlayerId).Members) if (!m.Id.StartsWith(SimulatedPrefix)) return true;
            return false;
        }

        /// <summary>Không nhận tham số; bắn Changed kèm ảnh chụp hiện tại.</summary>
        void Notify()
        {
            if (Changed != null) Changed(Current);
        }
    }
}
