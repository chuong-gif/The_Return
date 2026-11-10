/*
 * Mục đích: Ảnh chụp luồng map để HOST gửi cho máy khách khi chơi online (T-028): khu đang làm, các khu đã xong và cổng đang được mở
 * (cả đội đã tập hợp ngoài cửa). Máy khách khôi phục MapProgress từ ảnh này (RestoreSnapshot) nên mọi luật hiển thị cũ
 * (cửa mở/khóa, bản đồ nhỏ, điều kiện thắng) dùng lại nguyên. Bất biến; mã hóa thành chuỗi ngắn; giải mã KHÔNG tin dữ liệu từ mạng
 * (sai định dạng, ID lạ ký tự, quá dài, trùng đều trả false, không ném lỗi). Thuần C#, không phụ thuộc Unity.
 * Hàm: constructor; From; ToSnapshot; Encode; TryDecode; thuộc tính ActiveAreaId, DoneAreaIds, ArmedGateIndex.
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace TheReturn
{
    /// <summary>Trạng thái luồng map: khu đang làm, khu đã xong, khu có cổng đang mở.</summary>
    public sealed class MapFlowState
    {
        /// <summary>Số khu tối đa chấp nhận khi giải mã (vòng 6 phòng + 6 hành lang = 12, chừa dư).</summary>
        public const int MaxAreas = 32;
        const int MaxIdLength = 24;
        const int MaxGateIndex = 255;
        const char Sep = '|';
        const char ListSep = ',';

        readonly string[] done;

        /// <summary>ID khu đang làm; chuỗi rỗng nếu chưa khu nào.</summary>
        public string ActiveAreaId { get; }

        /// <summary>Chỉ số (trong bảng cổng của MissionFlowController, giống nhau trên mọi máy vì cùng scene) của cổng đang mở vì cả đội đã tập hợp ngoài cửa; -1 nếu chưa cổng nào.</summary>
        public int ArmedGateIndex { get; }

        /// <summary>ID các khu đã xong theo thứ tự khai báo.</summary>
        public IReadOnlyList<string> DoneAreaIds { get { return done; } }

        /// <summary>Nhận khu đang làm (null = không), các khu đã xong (null = không) và chỉ số cổng đang mở (âm = không); sao chép dữ liệu.</summary>
        public MapFlowState(string activeAreaId, IEnumerable<string> doneAreaIds, int armedGateIndex)
        {
            ActiveAreaId = activeAreaId ?? string.Empty;
            ArmedGateIndex = armedGateIndex < 0 ? -1 : armedGateIndex;
            List<string> list = new List<string>();
            if (doneAreaIds != null)
            {
                foreach (string id in doneAreaIds) list.Add(id);
            }
            done = list.ToArray();
        }

        /// <summary>Nhận tiến độ map và chỉ số cổng đang mở (âm = không); trả ảnh chụp hiện tại.</summary>
        public static MapFlowState From(MapProgress progress, int armedGateIndex)
        {
            if (progress == null) throw new ArgumentNullException("progress");
            MapProgressSnapshot snapshot = progress.ToSnapshot();
            return new MapFlowState(snapshot.ActiveAreaId, snapshot.DoneAreaIds, armedGateIndex);
        }

        /// <summary>Trả ảnh chụp dạng MapProgressSnapshot để MapProgress.RestoreSnapshot khôi phục (ActiveAreaId null nếu không có).</summary>
        public MapProgressSnapshot ToSnapshot()
        {
            return new MapProgressSnapshot
            {
                ActiveAreaId = ActiveAreaId.Length == 0 ? null : ActiveAreaId,
                DoneAreaIds = (string[])done.Clone()
            };
        }

        /// <summary>Trả chuỗi dạng «khuĐangLàm|khu1,khu2|chỉSốCổngMở»; rỗng ở chỗ nào không có.</summary>
        public string Encode()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(ActiveAreaId).Append(Sep);
            for (int i = 0; i < done.Length; i++)
            {
                if (i > 0) sb.Append(ListSep);
                sb.Append(done[i]);
            }
            sb.Append(Sep);
            if (ArmedGateIndex >= 0) sb.Append(ArmedGateIndex.ToString(System.Globalization.CultureInfo.InvariantCulture));
            return sb.ToString();
        }

        /// <summary>Nhận chuỗi từ mạng; trả true và trạng thái nếu hợp lệ, ngược lại false (state = null).</summary>
        public static bool TryDecode(string text, out MapFlowState state)
        {
            state = null;
            if (text == null) return false;
            string[] parts = text.Split(Sep);
            if (parts.Length != 3) return false;
            if (!ValidId(parts[0], true)) return false;
            int armed = -1;
            if (parts[2].Length > 0)
            {
                if (!int.TryParse(parts[2], System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out armed)) return false;
                if (armed > MaxGateIndex) return false;
            }

            List<string> doneIds = new List<string>();
            if (parts[1].Length > 0)
            {
                string[] ids = parts[1].Split(ListSep);
                if (ids.Length > MaxAreas) return false;
                for (int i = 0; i < ids.Length; i++)
                {
                    if (!ValidId(ids[i], false) || doneIds.Contains(ids[i])) return false;
                    doneIds.Add(ids[i]);
                }
            }
            state = new MapFlowState(parts[0], doneIds, armed);
            return true;
        }

        static bool ValidId(string id, bool allowEmpty)
        {
            if (id == null) return false;
            if (id.Length == 0) return allowEmpty;
            if (id.Length > MaxIdLength) return false;
            for (int i = 0; i < id.Length; i++)
            {
                char c = id[i];
                bool ok = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '_';
                if (!ok) return false;
            }
            return true;
        }
    }
}
