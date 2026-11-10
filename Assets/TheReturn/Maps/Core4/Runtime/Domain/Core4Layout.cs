/*
 * Mục đích: ĐẶC TẢ LAYOUT của map mới 4 nhiệm vụ (T-040) dưới dạng dữ liệu thuần C# KIỂM ĐƯỢC: danh sách khu + khu kề, và khu nào giữ nhiệm vụ nào
 * (Attendance, BreakerPanel, Choir, PlankAndKey). Thiết kế map (do phiên #13 lo) chỉ cần điền dữ liệu này; Problems() báo mọi lỗi thiết kế
 * (trùng/thiếu khu, kề không đối xứng, hành lang không kề phòng nào nên không vào được, quá 32 khu của MapFlowState...) và CreateProgress()
 * dựng MapProgress đúng cho MissionRegistry. Dùng lại luật có sẵn (MapProgress, AreaInfo, MapFlowState), KHÔNG đổi chúng.
 * Luật vào khu (MapProgress.CheckBegin): PHÒNG vào được từ Sân hoặc hành lang kề; HÀNH LANG chỉ vào được từ hai phòng kề.
 * Hàm: constructor; AreaOf; DefaultAreaOf; DefaultMissionAreas; EntriesFor; Problems; CreateProgress; thuộc tính Areas.
 */
using System;
using System.Collections.Generic;

namespace TheReturn.Core4
{
    /// <summary>Bốn nhiệm vụ được giữ lại ở map mới.</summary>
    public enum Core4Mission
    {
        Attendance,
        BreakerPanel,
        Choir,
        PlankAndKey
    }

    /// <summary>Layout map mới: các khu, khu kề và khu của từng nhiệm vụ.</summary>
    public sealed class Core4Layout
    {
        /// <summary>Số nhiệm vụ của map mới.</summary>
        public const int MissionCount = 4;

        readonly List<AreaInfo> areas = new List<AreaInfo>();
        readonly Dictionary<Core4Mission, string> missionArea = new Dictionary<Core4Mission, string>();

        /// <summary>Các khu theo thứ tự khai báo (sao chép, chỉ đọc).</summary>
        public IReadOnlyList<AreaInfo> Areas { get { return areas; } }

        /// <summary>
        /// Nhận danh sách khu và ánh xạ nhiệm vụ -> ID khu (null = dùng DefaultMissionAreas). Chỉ lưu dữ liệu, KHÔNG ném lỗi vì thiết kế sai:
        /// gọi Problems() để biết lỗi. Cờ HasMission của khu đầu vào bị bỏ qua (CreateProgress tự đặt đúng theo ánh xạ).
        /// </summary>
        public Core4Layout(IEnumerable<AreaInfo> areaList, IDictionary<Core4Mission, string> missionAreas)
        {
            if (areaList != null)
                foreach (AreaInfo a in areaList) areas.Add(a);
            IDictionary<Core4Mission, string> map = missionAreas ?? DefaultMissionAreas();
            foreach (KeyValuePair<Core4Mission, string> pair in map) missionArea[pair.Key] = pair.Value;
        }

        /// <summary>Nhận nhiệm vụ; trả ID khu được gán (null nếu chưa gán).</summary>
        public string AreaOf(Core4Mission mission)
        {
            string id;
            return missionArea.TryGetValue(mission, out id) ? id : null;
        }

        /// <summary>
        /// Nhận nhiệm vụ; trả ID khu KHUYẾN NGHỊ giữ nguyên như map cũ để registry/âm thanh/UI cũ dùng lại không sửa:
        /// Phong_1, Phong_5, Phong_6, Hanh_Lang_6.
        /// </summary>
        public static string DefaultAreaOf(Core4Mission mission)
        {
            switch (mission)
            {
                case Core4Mission.Attendance: return "Phong_1";
                case Core4Mission.BreakerPanel: return "Phong_5";
                case Core4Mission.Choir: return "Phong_6";
                default: return "Hanh_Lang_6";
            }
        }

        /// <summary>Trả ánh xạ mặc định nhiệm vụ -> ID khu (xem DefaultAreaOf).</summary>
        public static Dictionary<Core4Mission, string> DefaultMissionAreas()
        {
            Dictionary<Core4Mission, string> map = new Dictionary<Core4Mission, string>();
            foreach (Core4Mission m in new[] { Core4Mission.Attendance, Core4Mission.BreakerPanel, Core4Mission.Choir, Core4Mission.PlankAndKey })
                map[m] = DefaultAreaOf(m);
            return map;
        }

        /// <summary>Nhận nhiệm vụ; trả các ID mà đội có thể đi từ đó vào khu của nhiệm vụ (Sân nếu là phòng, cộng các khu kề). Rỗng nếu chưa gán hoặc khu lạ.</summary>
        public List<string> EntriesFor(Core4Mission mission)
        {
            List<string> entries = new List<string>();
            AreaInfo a = Find(AreaOf(mission));
            if (a == null) return entries;
            if (a.Kind == AreaKind.Room) entries.Add(MapProgress.Yard);
            foreach (string n in a.Neighbors) entries.Add(n);
            return entries;
        }

        /// <summary>Trả danh sách lỗi thiết kế bằng tiếng Việt; rỗng nghĩa là layout hợp lệ và CreateProgress dùng được.</summary>
        public List<string> Problems()
        {
            List<string> problems = new List<string>();
            Dictionary<string, AreaInfo> byId = new Dictionary<string, AreaInfo>();
            if (areas.Count == 0) problems.Add("Layout chưa có khu nào.");
            foreach (AreaInfo a in areas)
            {
                if (a == null) { problems.Add("Có khu null."); continue; }
                if (byId.ContainsKey(a.Id)) problems.Add("Trùng ID khu: " + a.Id + ".");
                else byId[a.Id] = a;
            }
            if (byId.Count > MapFlowState.MaxAreas)
                problems.Add("Quá nhiều khu (" + byId.Count + " > " + MapFlowState.MaxAreas + "), MapFlowState không gửi được qua mạng.");
            foreach (AreaInfo a in byId.Values)
            {
                foreach (string n in a.Neighbors)
                {
                    AreaInfo other;
                    if (!byId.TryGetValue(n, out other)) problems.Add("Khu " + a.Id + " kề khu không tồn tại: " + n + ".");
                    else if (!Contains(other.Neighbors, a.Id)) problems.Add("Khu kề không đối xứng: " + a.Id + " -> " + n + ".");
                }
            }
            HashSet<string> used = new HashSet<string>();
            foreach (Core4Mission m in new[] { Core4Mission.Attendance, Core4Mission.BreakerPanel, Core4Mission.Choir, Core4Mission.PlankAndKey })
            {
                string id = AreaOf(m);
                if (string.IsNullOrEmpty(id)) { problems.Add("Nhiệm vụ " + m + " chưa được gán khu."); continue; }
                AreaInfo area;
                if (!byId.TryGetValue(id, out area)) { problems.Add("Nhiệm vụ " + m + " gán khu không tồn tại: " + id + "."); continue; }
                if (!used.Add(id)) problems.Add("Hai nhiệm vụ cùng gán khu " + id + ".");
                if (area.Kind == AreaKind.Corridor && !HasRoomNeighbor(area, byId))
                    problems.Add("Nhiệm vụ " + m + " ở hành lang " + id + " nhưng hành lang chỉ vào được từ phòng kề và nó không kề phòng nào.");
            }
            return problems;
        }

        /// <summary>Dựng MapProgress mới: đúng bốn khu có nhiệm vụ (HasMission), các khu khác không. Ném InvalidOperationException (kèm lỗi đầu tiên) nếu Problems() không rỗng.</summary>
        public MapProgress CreateProgress()
        {
            List<string> problems = Problems();
            if (problems.Count > 0) throw new InvalidOperationException("Layout không hợp lệ: " + problems[0]);
            HashSet<string> withMission = new HashSet<string>(missionArea.Values);
            List<AreaInfo> list = new List<AreaInfo>();
            foreach (AreaInfo a in areas) list.Add(new AreaInfo(a.Id, a.Kind, withMission.Contains(a.Id), a.Neighbors));
            return new MapProgress(list);
        }

        AreaInfo Find(string id)
        {
            if (id == null) return null;
            foreach (AreaInfo a in areas) if (a != null && a.Id == id) return a;
            return null;
        }

        static bool Contains(IReadOnlyList<string> list, string id)
        {
            for (int i = 0; i < list.Count; i++) if (list[i] == id) return true;
            return false;
        }

        static bool HasRoomNeighbor(AreaInfo area, Dictionary<string, AreaInfo> byId)
        {
            foreach (string n in area.Neighbors)
            {
                AreaInfo other;
                if (byId.TryGetValue(n, out other) && other.Kind == AreaKind.Room) return true;
            }
            return false;
        }
    }
}
