/*
 * Mục đích: Bản đồ nhỏ của sân MapSchoolF dưới dạng object Canvas trong scene (sửa được bằng Inspector): chỉ 4 ô nhiệm vụ
 * (Phong_1 Điểm danh, Phong_5 Cầu dao, Phong_6 Hợp xướng, Hanh_Lang_6 Cầu ván). Chỉ tô màu theo tiến độ, không tạo UI lúc chạy.
 * Hàm: Apply(progress) tô màu từng ô theo MiniMapModel; SetVisible; ColorOf/TextColorOf (màu theo trạng thái, chỉnh được trong Inspector).
 */
using System;
using UnityEngine;
using UnityEngine.UI;

namespace TheReturn
{
    /// <summary>Gắn vào object "MiniMap" dưới HUD; mỗi Entry nối một ô nhiệm vụ với ID khu của nó.</summary>
    public sealed class SchoolFloorMiniMapView : MonoBehaviour
    {
        [Serializable]
        public sealed class Entry
        {
            [Tooltip("ID khu trong MapProgress, ví dụ Phong_1.")] public string areaId;
            public Image tile;
            public Text code;
            public Text title;
        }

        public Entry[] entries;

        [Header("Màu theo trạng thái (nền ô)")]
        public Color activeColor = new Color32(52, 211, 153, 255);
        public Color doneColor = new Color32(24, 88, 74, 255);
        public Color availableColor = new Color32(66, 80, 101, 255);
        public Color lockedColor = new Color32(22, 27, 35, 255);

        [Header("Màu chữ theo trạng thái")]
        public Color activeText = new Color32(6, 24, 18, 255);
        public Color doneText = new Color32(52, 211, 153, 255);
        public Color availableText = new Color32(235, 240, 247, 255);
        public Color lockedText = new Color32(96, 108, 125, 255);

        int lastStamp = -1;

        /// <summary>Nhận cờ hiện; bật/tắt cả bản đồ.</summary>
        public void SetVisible(bool show) { if (gameObject.activeSelf != show) gameObject.SetActive(show); }

        /// <summary>Nhận tiến độ (có thể null); tô lại các ô, chỉ làm khi tiến độ đổi.</summary>
        public void Apply(MapProgress progress)
        {
            if (progress == null || entries == null) return;
            var cells = MiniMapModel.Build(progress);
            int stamp = progress.DoneCount * 100 + (progress.ActiveAreaId == null ? 0 : 50) + cells.Count;
            if (stamp == lastStamp) return;
            lastStamp = stamp;
            foreach (var e in entries)
            {
                if (e == null) continue;
                MiniMapState state = MiniMapState.NoMission;
                foreach (var c in cells) if (c.Id == e.areaId) { state = c.State; break; }
                if (e.tile != null) e.tile.color = ColorOf(state);
                Color t = TextColorOf(state);
                if (e.code != null) e.code.color = t;
                if (e.title != null) e.title.color = t;
            }
        }

        /// <summary>Nhận trạng thái; trả màu nền ô.</summary>
        public Color ColorOf(MiniMapState s)
        {
            switch (s)
            {
                case MiniMapState.Done: return doneColor;
                case MiniMapState.Active: return activeColor;
                case MiniMapState.Available: return availableColor;
                default: return lockedColor;
            }
        }

        /// <summary>Nhận trạng thái; trả màu chữ tương phản với nền ô.</summary>
        public Color TextColorOf(MiniMapState s)
        {
            switch (s)
            {
                case MiniMapState.Done: return doneText;
                case MiniMapState.Active: return activeText;
                case MiniMapState.Available: return availableText;
                default: return lockedText;
            }
        }
    }
}
