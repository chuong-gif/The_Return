/*
 * Mục đích: Nguồn topology của scene MỚI (T-040): người dựng scene điền danh sách khu + khu kề + khu của 4 nhiệm vụ ngay trên component (khớp thiết kế
 * map đã duyệt), MissionFlowController.topology trỏ vào đây. Dùng Core4Layout để kiểm: layout sai thì ghi LỖI rõ ràng (không im lặng) và rơi về
 * vòng cũ để scene không chết. Không chứa luật; không phụ thuộc vị trí vật lý.
 * Hàm: CreateProgress; Layout (dựng Core4Layout từ dữ liệu serialize); Problems; kiểu lồng AreaEntry.
 */
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheReturn.Core4
{
    /// <summary>Topology map mới điền bằng tay trong scene; cung cấp MapProgress cho luồng nhiệm vụ.</summary>
    public sealed class Core4TopologyProvider : MapTopologyProvider
    {
        /// <summary>Một khu: ID (tên object trong scene), loại và các khu kề (phải đối xứng).</summary>
        [Serializable]
        public sealed class AreaEntry
        {
            public string id;
            public AreaKind kind;
            public string[] neighbors = new string[0];
        }

        [Tooltip("Mọi khu của map mới. Sân là 'San' (không khai ở đây). Phòng vào được từ Sân hoặc hành lang kề; hành lang chỉ vào từ phòng kề.")]
        public AreaEntry[] areas = new AreaEntry[0];

        [Header("Khu của từng nhiệm vụ (mặc định giữ ID map cũ để âm thanh/registry/UI cũ dùng lại)")]
        public string attendanceArea = "Phong_1";
        public string breakerPanelArea = "Phong_5";
        public string choirArea = "Phong_6";
        public string plankAndKeyArea = "Hanh_Lang_6";

        /// <summary>Không nhận tham số; dựng Core4Layout từ dữ liệu trên component (chưa kiểm lỗi).</summary>
        public Core4Layout Layout()
        {
            List<AreaInfo> list = new List<AreaInfo>();
            if (areas != null)
            {
                foreach (AreaEntry e in areas)
                    if (e != null && !string.IsNullOrEmpty(e.id)) list.Add(new AreaInfo(e.id, e.kind, false, e.neighbors));
            }
            Dictionary<Core4Mission, string> map = new Dictionary<Core4Mission, string>();
            map[Core4Mission.Attendance] = attendanceArea;
            map[Core4Mission.BreakerPanel] = breakerPanelArea;
            map[Core4Mission.Choir] = choirArea;
            map[Core4Mission.PlankAndKey] = plankAndKeyArea;
            return new Core4Layout(list, map);
        }

        /// <summary>Không nhận tham số; trả các lỗi thiết kế hiện có (rỗng = hợp lệ). Dùng trong test/Editor để kiểm scene.</summary>
        public List<string> Problems()
        {
            return Layout().Problems();
        }

        /// <summary>Không nhận tham số; trả MapProgress mới theo layout. Layout sai: ghi lỗi đầu tiên và rơi về vòng 6 phòng cũ để scene vẫn chạy.</summary>
        public override MapProgress CreateProgress()
        {
            Core4Layout layout = Layout();
            List<string> problems = layout.Problems();
            if (problems.Count == 0) return layout.CreateProgress();
            Debug.LogError("Core4TopologyProvider: layout không hợp lệ (" + problems.Count + " lỗi), ví dụ: " + problems[0] + " Rơi về vòng cũ.", this);
            return MapProgress.CreateRing(6, null);
        }
    }
}
