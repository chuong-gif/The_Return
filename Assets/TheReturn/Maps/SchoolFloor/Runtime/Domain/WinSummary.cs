/*
 * Mục đích: Tóm tắt thuần C# cho màn hình thắng (T-007): đã thắng chưa, đã xong những khu nào, còn khu nào chưa có nhiệm vụ,
 * và tên hiển thị tiếng Việt của từng khu. Không phụ thuộc Unity.
 * Kiểu: WinSummary. Hàm: Build; DisplayName.
 */
using System.Collections.Generic;

namespace TheReturn
{
    /// <summary>Bản tóm tắt tiến độ map để hiển thị trên màn hình thắng.</summary>
    public sealed class WinSummary
    {
        /// <summary>True khi MapProgress.Won (mọi khu đã đăng ký nhiệm vụ đều xong).</summary>
        public bool Won { get; private set; }

        /// <summary>Số khu đã xong.</summary>
        public int DoneCount { get; private set; }

        /// <summary>Tên hiển thị các khu đã xong, theo thứ tự khai báo khu (phòng trước, hành lang sau).</summary>
        public IReadOnlyList<string> DoneAreaNames { get; private set; }

        /// <summary>Tên hiển thị các khu CHƯA có nhiệm vụ (thiếu thiết kế).</summary>
        public IReadOnlyList<string> MissingAreaNames { get; private set; }

        WinSummary() { }

        /// <summary>Nhận tiến độ map (có thể null); trả bản tóm tắt. Null cho kết quả chưa thắng, không có khu nào.</summary>
        public static WinSummary Build(MapProgress progress)
        {
            var done = new List<string>();
            var missing = new List<string>();
            var summary = new WinSummary { DoneAreaNames = done, MissingAreaNames = missing };
            if (progress == null) return summary;
            foreach (AreaInfo area in progress.Areas)
            {
                if (progress.IsDone(area.Id)) done.Add(DisplayName(area.Id));
                if (!area.HasMission) missing.Add(DisplayName(area.Id));
            }
            summary.DoneCount = done.Count;
            summary.Won = progress.Won;
            return summary;
        }

        /// <summary>Nhận ID khu (Phong_k, Hanh_Lang_k, San); trả tên tiếng Việt (Phòng k, Hành lang k, Sân). ID lạ trả nguyên ID, null trả chuỗi rỗng.</summary>
        public static string DisplayName(string areaId)
        {
            if (string.IsNullOrEmpty(areaId)) return string.Empty;
            if (areaId == MapProgress.Yard) return "Sân";
            if (areaId.StartsWith("Phong_")) return "Phòng " + areaId.Substring(6);
            if (areaId.StartsWith("Hanh_Lang_")) return "Hành lang " + areaId.Substring(10);
            return areaId;
        }
    }
}
