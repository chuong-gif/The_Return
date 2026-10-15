/*
 * Mục đích: Luật thuần C# cho luồng nhiệm vụ tự chọn của tầng học: khu nào đang làm, khu nào đã xong,
 * ai được bắt đầu khu nào, khi nào thắng map. Không phụ thuộc Unity; chưa nối vào scene (T-002).
 * Kiểu: AreaKind, BeginResult, AreaInfo, MapProgressSnapshot, MapProgress.
 * Hàm MapProgress: constructor; CreateRing; RoomId / CorridorId; Find; IsDone / IsActive;
 * SetHasMission; CheckBegin / TryBegin; Complete; Abandon; Reset;
 * DoneCount / Won / MissingMissionAreas; ToSnapshot / RestoreSnapshot.
 */
using System;
using System.Collections.Generic;

namespace TheReturn
{
    /// <summary>Loại khu: phòng hoặc hành lang.</summary>
    public enum AreaKind
    {
        Room,
        Corridor
    }

    /// <summary>Kết quả kiểm tra/bắt đầu một khu.</summary>
    public enum BeginResult
    {
        Started,
        UnknownArea,
        NoMission,
        AlreadyDone,
        AnotherActive,
        InvalidEntry,
        TeamNotReady
    }

    /// <summary>Mô tả một khu: ID (tên object trong scene), loại, đã có nhiệm vụ chưa, các khu kề.</summary>
    public sealed class AreaInfo
    {
        public string Id { get; }
        public AreaKind Kind { get; }
        public bool HasMission { get; internal set; }
        public IReadOnlyList<string> Neighbors { get; }

        /// <summary>Nhận ID khác rỗng, loại khu, cờ có nhiệm vụ và danh sách khu kề; tạo mô tả khu (sao chép danh sách kề).</summary>
        public AreaInfo(string id, AreaKind kind, bool hasMission, IEnumerable<string> neighbors)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("Id rỗng", nameof(id));
            Id = id;
            Kind = kind;
            HasMission = hasMission;
            var list = new List<string>();
            if (neighbors != null)
                foreach (string n in neighbors) list.Add(n);
            Neighbors = list.AsReadOnly();
        }
    }

    /// <summary>Ảnh chụp tiến độ để lưu hoặc đồng bộ mạng sau này (chỉ chuỗi ID, không chứa tham chiếu Unity).</summary>
    [Serializable]
    public sealed class MapProgressSnapshot
    {
        public string ActiveAreaId;
        public string[] DoneAreaIds = new string[0];
    }

    /// <summary>
    /// Tiến độ cả map: tối đa một khu đang làm (mọi nhiệm vụ cần cả đội), tập khu đã xong, điều kiện thắng.
    /// Vị trí thật của đội do lớp Presentation quyết định; ở đây chỉ nhận «đội bắt đầu từ khu chuẩn bị nào» và «cả đội đã đủ chưa».
    /// </summary>
    public sealed class MapProgress
    {
        /// <summary>ID quy ước của sân ngoài (lối vào phòng từ sân).</summary>
        public const string Yard = "San";

        readonly List<AreaInfo> areas = new List<AreaInfo>();
        readonly Dictionary<string, AreaInfo> byId = new Dictionary<string, AreaInfo>();
        readonly HashSet<string> done = new HashSet<string>();

        public IReadOnlyList<AreaInfo> Areas => areas;
        public string ActiveAreaId { get; private set; }
        public int DoneCount => done.Count;

        /// <summary>Nhận danh sách khu; kiểm ID duy nhất, khu kề tồn tại và đối xứng; ném ArgumentException nếu sai.</summary>
        public MapProgress(IEnumerable<AreaInfo> areaList)
        {
            if (areaList == null) throw new ArgumentNullException(nameof(areaList));
            foreach (AreaInfo a in areaList)
            {
                if (a == null) throw new ArgumentException("Có khu null", nameof(areaList));
                if (byId.ContainsKey(a.Id)) throw new ArgumentException("Trùng ID khu: " + a.Id, nameof(areaList));
                byId[a.Id] = a;
                areas.Add(a);
            }
            foreach (AreaInfo a in areas)
            {
                foreach (string n in a.Neighbors)
                {
                    AreaInfo other;
                    if (!byId.TryGetValue(n, out other))
                        throw new ArgumentException("Khu " + a.Id + " kề khu không tồn tại: " + n, nameof(areaList));
                    if (!Contains(other.Neighbors, a.Id))
                        throw new ArgumentException("Khu kề không đối xứng: " + a.Id + " -> " + n, nameof(areaList));
                }
            }
        }

        /// <summary>Nhận chỉ số phòng bắt đầu từ 1; trả ID chuẩn của phòng (Phong_k, tên object trong scene).</summary>
        public static string RoomId(int k) { return "Phong_" + k; }

        /// <summary>Nhận chỉ số hành lang bắt đầu từ 1; trả ID chuẩn (Hanh_Lang_k nối Phong_k và Phong_(k+1)).</summary>
        public static string CorridorId(int k) { return "Hanh_Lang_" + k; }

        /// <summary>
        /// Nhận số phòng (≥ 3) và tập ID khu đã có nhiệm vụ; tạo vòng tròn Phong_1..n / Hanh_Lang_1..n.
        /// Hanh_Lang_k nối Phong_k với Phong_(k+1) (Hanh_Lang_n nối Phong_n với Phong_1). Trả MapProgress mới.
        /// </summary>
        public static MapProgress CreateRing(int roomCount, IEnumerable<string> missionAreaIds)
        {
            if (roomCount < 3) throw new ArgumentOutOfRangeException(nameof(roomCount));
            var withMission = new HashSet<string>();
            if (missionAreaIds != null)
                foreach (string id in missionAreaIds) withMission.Add(id);
            var list = new List<AreaInfo>();
            for (int k = 1; k <= roomCount; k++)
            {
                int prev = k == 1 ? roomCount : k - 1;
                string room = RoomId(k);
                list.Add(new AreaInfo(room, AreaKind.Room, withMission.Contains(room), new[] { CorridorId(prev), CorridorId(k) }));
            }
            for (int k = 1; k <= roomCount; k++)
            {
                int next = k == roomCount ? 1 : k + 1;
                string corridor = CorridorId(k);
                list.Add(new AreaInfo(corridor, AreaKind.Corridor, withMission.Contains(corridor), new[] { RoomId(k), RoomId(next) }));
            }
            return new MapProgress(list);
        }

        /// <summary>Nhận ID; trả mô tả khu hoặc null nếu không có.</summary>
        public AreaInfo Find(string areaId)
        {
            AreaInfo a;
            return areaId != null && byId.TryGetValue(areaId, out a) ? a : null;
        }

        /// <summary>Nhận ID; trả true nếu khu đã xong (ID lạ trả false).</summary>
        public bool IsDone(string areaId) { return areaId != null && done.Contains(areaId); }

        /// <summary>Nhận ID; trả true nếu khu đang được làm.</summary>
        public bool IsActive(string areaId) { return areaId != null && ActiveAreaId == areaId; }

        /// <summary>Nhận ID và cờ; đăng ký (hoặc gỡ) nhiệm vụ cho khu. Trả false nếu ID lạ hoặc khu đang làm/đã xong (không đổi giữa chừng).</summary>
        public bool SetHasMission(string areaId, bool hasMission)
        {
            AreaInfo a = Find(areaId);
            if (a == null || IsActive(areaId) || IsDone(areaId)) return false;
            a.HasMission = hasMission;
            return true;
        }

        /// <summary>
        /// Nhận ID khu muốn làm, ID khu chuẩn bị mà đội đang đứng (Yard hoặc ID một phòng/hành lang) và cờ «cả đội đã đủ».
        /// Chỉ kiểm tra, không đổi trạng thái. Thứ tự kiểm: ID lạ, đã xong, chưa có nhiệm vụ, đang có khu khác, lối vào sai, đội chưa đủ.
        /// Phòng vào được từ sân hoặc từ hành lang kề; hành lang chỉ vào được từ hai phòng kề.
        /// </summary>
        public BeginResult CheckBegin(string areaId, string entryFromId, bool teamReady)
        {
            AreaInfo a = Find(areaId);
            if (a == null) return BeginResult.UnknownArea;
            if (IsDone(areaId)) return BeginResult.AlreadyDone;
            if (!a.HasMission) return BeginResult.NoMission;
            if (ActiveAreaId != null) return BeginResult.AnotherActive;
            bool entryOk = Contains(a.Neighbors, entryFromId) || (a.Kind == AreaKind.Room && entryFromId == Yard);
            if (!entryOk) return BeginResult.InvalidEntry;
            if (!teamReady) return BeginResult.TeamNotReady;
            return BeginResult.Started;
        }

        /// <summary>Như CheckBegin nhưng nếu đủ điều kiện thì đặt khu thành đang làm. Trả kết quả (Started nghĩa là đã bắt đầu).</summary>
        public BeginResult TryBegin(string areaId, string entryFromId, bool teamReady)
        {
            BeginResult r = CheckBegin(areaId, entryFromId, teamReady);
            if (r == BeginResult.Started) ActiveAreaId = areaId;
            return r;
        }

        /// <summary>Nhận ID; nếu đúng khu đang làm thì đánh dấu xong và nhả khu đang làm. Trả true nếu thành công.</summary>
        public bool Complete(string areaId)
        {
            if (!IsActive(areaId)) return false;
            done.Add(areaId);
            ActiveAreaId = null;
            return true;
        }

        /// <summary>Nhận ID; nếu đúng khu đang làm thì nhả mà KHÔNG đánh dấu xong, tiến độ khu khác giữ nguyên. Trả true nếu thành công.</summary>
        public bool Abandon(string areaId)
        {
            if (!IsActive(areaId)) return false;
            ActiveAreaId = null;
            return true;
        }

        /// <summary>Xóa mọi tiến độ (không đổi danh sách khu hay cờ nhiệm vụ).</summary>
        public void Reset()
        {
            done.Clear();
            ActiveAreaId = null;
        }

        /// <summary>Trả true khi có ít nhất một khu đã đăng ký nhiệm vụ và mọi khu đã đăng ký đều xong.</summary>
        public bool Won
        {
            get
            {
                int registered = 0;
                foreach (AreaInfo a in areas)
                {
                    if (!a.HasMission) continue;
                    registered++;
                    if (!done.Contains(a.Id)) return false;
                }
                return registered > 0;
            }
        }

        /// <summary>Trả danh sách ID khu CHƯA có nhiệm vụ (thiếu thiết kế), để báo rõ thay vì bỏ sót lặng lẽ.</summary>
        public IReadOnlyList<string> MissingMissionAreas
        {
            get
            {
                var list = new List<string>();
                foreach (AreaInfo a in areas)
                    if (!a.HasMission) list.Add(a.Id);
                return list;
            }
        }

        /// <summary>Trả ảnh chụp tiến độ (khu đang làm + các khu đã xong theo thứ tự khai báo) để lưu/đồng bộ.</summary>
        public MapProgressSnapshot ToSnapshot()
        {
            var ids = new List<string>();
            foreach (AreaInfo a in areas)
                if (done.Contains(a.Id)) ids.Add(a.Id);
            return new MapProgressSnapshot { ActiveAreaId = ActiveAreaId, DoneAreaIds = ids.ToArray() };
        }

        /// <summary>
        /// Nhận ảnh chụp; khôi phục nếu hợp lệ (ID đều có thật, khu đang làm có nhiệm vụ và chưa xong). Trả false và không đổi gì nếu sai.
        /// </summary>
        public bool RestoreSnapshot(MapProgressSnapshot snapshot)
        {
            if (snapshot == null) return false;
            var newDone = new HashSet<string>();
            if (snapshot.DoneAreaIds != null)
                foreach (string id in snapshot.DoneAreaIds)
                {
                    if (Find(id) == null) return false;
                    newDone.Add(id);
                }
            if (snapshot.ActiveAreaId != null)
            {
                AreaInfo active = Find(snapshot.ActiveAreaId);
                if (active == null || !active.HasMission || newDone.Contains(snapshot.ActiveAreaId)) return false;
            }
            done.Clear();
            foreach (string id in newDone) done.Add(id);
            ActiveAreaId = snapshot.ActiveAreaId;
            return true;
        }

        /// <summary>Nhận danh sách và giá trị; trả true nếu danh sách chứa giá trị (so sánh chuỗi chính xác).</summary>
        static bool Contains(IReadOnlyList<string> list, string value)
        {
            if (value == null) return false;
            for (int i = 0; i < list.Count; i++)
                if (list[i] == value) return true;
            return false;
        }
    }
}
