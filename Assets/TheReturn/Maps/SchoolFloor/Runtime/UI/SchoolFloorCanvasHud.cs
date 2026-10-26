/*
 * Mục đích: Phần HUD theo khu (T-011): sổ tay chỉ hiện khi bấm N, bản đồ nhỏ của sân dựng lúc chạy (không sửa scene).
 * Hàm: UpdateNotebookKey đọc phím N; UpdateMiniMap hiện/ẩn và tô màu 12 ô theo MiniMapModel; BuildMiniMap tạo UI một lần.
 */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TheReturn
{
    public sealed partial class SchoolFloorCanvasPresenter
    {
        bool notebookOpen;
        GameObject miniMap;
        readonly List<Image> miniCells = new List<Image>();
        readonly List<Text> miniLabels = new List<Text>();
        int lastMiniStamp = -1;
        readonly AreaHudModel extraModel = new AreaHudModel();
        int extraFrame = -1;
        string extraArea;
        IAreaHudProvider extraProvider;

        /// <summary>Nhận ID khu; trả model HUD của nhiệm vụ mới (rỗng nếu khu không có nhà cung cấp). Điền một lần mỗi frame và mỗi khu để tránh gọi lặp.</summary>
        AreaHudModel ExtraModel(string area)
        {
            if (extraFrame == Time.frameCount && extraArea == area) return extraModel;
            if (extraArea != area || extraProvider == null) { extraArea = area; extraProvider = AreaHudProviders.Find(flow, area); }
            extraFrame = Time.frameCount;
            extraModel.Clear();
            if (extraProvider != null) extraProvider.FillHud(extraModel, flow.attendance.party.ActivePlayer, Time.timeAsDouble);
            return extraModel;
        }

        /// <summary>Nhận ID khu; trả true nếu khu có nhà cung cấp HUD (dùng model thay đường text cũ).</summary>
        bool HasHudProvider(string area) { ExtraModel(area); return extraProvider != null; }

        /// <summary>Không nhận tham số; N đổi trạng thái hiện/ẩn sổ tay. Trả true nếu sổ tay đang mở.</summary>
        bool UpdateNotebookKey()
        {
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null && keyboard.nKey.wasPressedThisFrame) notebookOpen = !notebookOpen;
            return notebookOpen;
        }

        /// <summary>Nhận cờ hiện; dựng bản đồ nhỏ nếu cần, ẩn khi không phải sân, tô lại màu chỉ khi tiến độ đổi.</summary>
        void UpdateMiniMap(bool show)
        {
            if (flow == null || flow.missionFlow == null || flow.missionFlow.Registry == null || view == null || view.hud == null) return;
            if (miniMap == null) { if (!show) return; BuildMiniMap(); }
            SchoolFloorCanvasView.SetVisible(miniMap, show);
            if (!show) return;
            var progress = flow.missionFlow.Registry.Progress;
            var cells = MiniMapModel.Build(progress);
            int stamp = progress.DoneCount * 100 + (progress.ActiveAreaId == null ? 0 : 50) + cells.Count;
            if (stamp == lastMiniStamp) return;
            lastMiniStamp = stamp;
            for (int i = 0; i < cells.Count && i < miniCells.Count; i++)
            {
                miniCells[i].color = MiniColor(cells[i].State);
                SchoolFloorCanvasView.SetText(miniLabels[i], cells[i].Label);
            }
        }

        static Color MiniColor(MiniMapState state)
        {
            switch (state)
            {
                case MiniMapState.Done: return new Color(0.25f, 0.75f, 0.35f, 0.95f);
                case MiniMapState.Active: return new Color(1f, 0.85f, 0.2f, 0.95f);
                case MiniMapState.Available: return new Color(0.9f, 0.9f, 0.9f, 0.95f);
                case MiniMapState.Blocked: return new Color(0.5f, 0.5f, 0.55f, 0.8f);
                default: return new Color(0.35f, 0.2f, 0.2f, 0.7f);
            }
        }

        /// <summary>Không nhận tham số; tạo khung bản đồ nhỏ góc phải trên với 12 ô xếp vòng tròn (đỉnh = ô đầu, chiều kim đồng hồ).</summary>
        void BuildMiniMap()
        {
            var cells = MiniMapModel.Build(flow.missionFlow.Registry.Progress);
            miniMap = new GameObject("MiniMap", typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)miniMap.transform;
            rt.SetParent(view.hud.transform, false);
            rt.anchorMin = rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-24f, -120f);
            rt.sizeDelta = new Vector2(230f, 250f);
            miniMap.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.45f);
            var font = view.title != null ? view.title.font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            AddMiniText(rt, "MiniTitle", "BẢN ĐỒ", font, 14, new Vector2(0f, 112f), new Vector2(200f, 24f), Color.white);
            AddMiniText(rt, "MiniLegend", "Vàng đang chơi • Xanh đã xong • Trắng chọn được", font, 10, new Vector2(0f, -112f), new Vector2(220f, 24f), new Color(1f, 1f, 1f, 0.8f));
            for (int i = 0; i < cells.Count; i++)
            {
                float a = cells[i].AngleDegrees * Mathf.Deg2Rad;
                var cell = new GameObject("Cell_" + cells[i].Id, typeof(RectTransform), typeof(Image));
                var crt = (RectTransform)cell.transform;
                crt.SetParent(rt, false);
                float size = cells[i].IsCorridor ? 28f : 38f;
                crt.sizeDelta = new Vector2(size, size);
                crt.anchoredPosition = new Vector2(Mathf.Sin(a) * 82f, Mathf.Cos(a) * 82f - 4f);
                var img = cell.GetComponent<Image>();
                img.color = MiniColor(cells[i].State);
                miniCells.Add(img);
                var label = AddMiniText(crt, "Label", cells[i].Label, font, cells[i].IsCorridor ? 11 : 13, Vector2.zero, new Vector2(size, size), Color.black);
                miniLabels.Add(label);
            }
            lastMiniStamp = -1;
        }

        static Text AddMiniText(RectTransform parent, string name, string value, Font font, int size, Vector2 pos, Vector2 box, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.sizeDelta = box;
            rt.anchoredPosition = pos;
            var t = go.GetComponent<Text>();
            t.font = font; t.fontSize = size; t.alignment = TextAnchor.MiddleCenter;
            t.color = color; t.text = value; t.raycastTarget = false;
            return t;
        }

        readonly List<TextMesh> nameTags = new List<TextMesh>();
        Font tagFont;

        /// <summary>Không nhận tham số; trả ' • Tên' của vai đang điều khiển (tên từ đề Điểm danh) hoặc rỗng nếu chưa có đề.</summary>
        string RoleNameSuffix()
        {
            var a = flow.attendance;
            if (a == null || a.State == null || a.party == null) return "";
            var names = a.State.Round.Names; int p = a.party.ActivePlayer;
            return p >= 0 && p < names.Count ? " • " + names[p] : "";
        }

        /// <summary>Không nhận tham số; hiện biển tên ('số • tên') trên đầu các đồng đội đang trong đội (không hiện cho vai đang điều khiển), xoay theo camera. Để người chơi đối chiếu manh mối 'Minh ngồi cạnh An' với nhân vật thật.</summary>
        void UpdateNameTags()
        {
            var a = flow.attendance;
            if (a == null || a.State == null || a.party == null || a.party.players == null) return;
            var players = a.party.players; var names = a.State.Round.Names;
            Camera cam = a.party.viewCamera != null ? a.party.viewCamera : Camera.main;
            if (tagFont == null) tagFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            while (nameTags.Count < players.Length) nameTags.Add(null);
            for (int i = 0; i < players.Length; i++)
            {
                Transform pl = players[i]; if (pl == null) continue;
                var tag = nameTags[i];
                if (tag == null)
                {
                    var go = new GameObject("NameTag_" + (i + 1), typeof(TextMesh), typeof(MeshRenderer));
                    tag = go.GetComponent<TextMesh>();
                    tag.font = tagFont; tag.fontSize = 64; tag.characterSize = 0.09f;
                    tag.anchor = TextAnchor.MiddleCenter; tag.alignment = TextAlignment.Center;
                    tag.color = new Color(1f, 0.95f, 0.6f);
                    go.GetComponent<MeshRenderer>().sharedMaterial = tagFont.material;
                    nameTags[i] = tag;
                }
                bool show = cam != null && i < names.Count && i < a.PartySize && i != a.party.ActivePlayer && pl.gameObject.activeInHierarchy;
                if (tag.gameObject.activeSelf != show) tag.gameObject.SetActive(show);
                if (!show) continue;
                string label = (i + 1) + " • " + names[i];
                if (tag.text != label) tag.text = label;
                var cc = pl.GetComponent<CharacterController>();
                float top = cc != null ? cc.center.y + cc.height * 0.5f + 0.35f : 2.2f;
                tag.transform.position = pl.position + Vector3.up * top;
                tag.transform.rotation = Quaternion.LookRotation(tag.transform.position - cam.transform.position);
            }
        }
    }
}
