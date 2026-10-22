/*
 * Mục đích: Mô hình bản đồ nhỏ của sân (T-011): trạng thái từng khu trên vòng phòng/hành lang, không phụ thuộc Unity.
 * Hàm: Build(MapProgress) trả danh sách ô theo thứ tự vòng Phong_1, Hanh_Lang_1, Phong_2, ...; ShortLabel rút gọn tên.
 */
using System.Collections.Generic;

namespace TheReturn
{
    /// <summary>Trạng thái hiển thị của một khu trên bản đồ nhỏ.</summary>
    public enum MiniMapState { NoMission, Available, Active, Done, Blocked }

    /// <summary>Một ô của bản đồ nhỏ: ID khu, nhãn ngắn, góc trên vòng (độ, 0 = ô đầu) và trạng thái.</summary>
    public sealed class MiniMapCell
    {
        public string Id { get; }
        public string Label { get; }
        public float AngleDegrees { get; }
        public bool IsCorridor { get; }
        public MiniMapState State { get; }

        public MiniMapCell(string id, string label, float angleDegrees, bool isCorridor, MiniMapState state)
        {
            Id = id; Label = label; AngleDegrees = angleDegrees; IsCorridor = isCorridor; State = state;
        }
    }

    /// <summary>Dựng bản đồ nhỏ từ MapProgress (chỉ đọc).</summary>
    public static class MiniMapModel
    {
        /// <summary>Nhận tiến độ; trả các ô theo vòng (phòng và hành lang xen kẽ), cách đều nhau 360/n độ. Tiến độ null trả danh sách rỗng.</summary>
        public static List<MiniMapCell> Build(MapProgress progress)
        {
            var cells = new List<MiniMapCell>();
            if (progress == null) return cells;
            var ordered = new List<AreaInfo>();
            for (int k = 1; ; k++)
            {
                var room = progress.Find(MapProgress.RoomId(k));
                if (room == null) break;
                ordered.Add(room);
                var corridor = progress.Find(MapProgress.CorridorId(k));
                if (corridor != null) ordered.Add(corridor);
            }
            float step = ordered.Count > 0 ? 360f / ordered.Count : 0f;
            bool anyActive = progress.ActiveAreaId != null;
            for (int i = 0; i < ordered.Count; i++)
            {
                var a = ordered[i];
                MiniMapState s;
                if (progress.IsDone(a.Id)) s = MiniMapState.Done;
                else if (progress.IsActive(a.Id)) s = MiniMapState.Active;
                else if (!a.HasMission) s = MiniMapState.NoMission;
                else s = anyActive ? MiniMapState.Blocked : MiniMapState.Available;
                cells.Add(new MiniMapCell(a.Id, ShortLabel(a.Id), i * step, a.Kind == AreaKind.Corridor, s));
            }
            return cells;
        }

        /// <summary>Nhận ID khu; trả nhãn ngắn: Phong_3 -> P3, Hanh_Lang_2 -> H2, ID khác giữ nguyên.</summary>
        public static string ShortLabel(string id)
        {
            if (id == null) return "";
            if (id.StartsWith("Phong_")) return "P" + id.Substring(6);
            if (id.StartsWith("Hanh_Lang_")) return "H" + id.Substring(10);
            return id;
        }
    }
}
