/*
 * Mục đích: Phần HUD theo khu (T-011): sổ tay chỉ hiện khi bấm N, bản đồ nhỏ (SchoolFloorMiniMapView, object Canvas trong scene).
 * Hàm: UpdateNotebookKey đọc phím N; UpdateMiniMap hiện/ẩn và tô màu 12 ô theo MiniMapModel; BuildMiniMap tạo UI một lần (kiểu ModernUI, T-053); BuildMiniLegend, AddMiniDot, MiniColor, MiniLabelColor.
 */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TheReturn
{
    public sealed partial class SchoolFloorCanvasPresenter
    {
        bool notebookOpen;
        SchoolFloorMiniMapView miniMap;
        readonly AreaHudModel extraModel = new AreaHudModel();
        int extraFrame = -1;
        string extraArea;
        IAreaHudProvider extraProvider;
        bool extraOnline;
        IOnlineHudSource[] onlineSources = new IOnlineHudSource[0];
        float nextOnlineScan;
        readonly AreaHudModel onlineModel = new AreaHudModel();
        int onlineFrame = -1;
        string onlineArea;
        bool onlineOk;

        /// <summary>Nhận ID khu; khi chơi online và nhiệm vụ đã nhận trạng thái thật từ host, trả model HUD từ dữ liệu đó (dùng chung trong một frame), ngược lại null để dùng đường offline.</summary>
        AreaHudModel OnlineHud(string area)
        {
            if (area == null || flow == null || flow.attendance == null || flow.attendance.party == null || !flow.attendance.party.IsOnline) return null;
            if (onlineFrame == Time.frameCount && onlineArea == area) return onlineOk ? onlineModel : null;
            if (Time.unscaledTime >= nextOnlineScan) { nextOnlineScan = Time.unscaledTime + 1.5f; RescanOnlineSources(); }
            onlineFrame = Time.frameCount;
            onlineArea = area;
            onlineOk = false;
            for (int i = 0; i < onlineSources.Length; i++)
            {
                IOnlineHudSource s = onlineSources[i];
                Object alive = s as Object;
                if (alive == null || s.AreaId != area) continue;
                onlineModel.Clear();
                if (s.TryFillOnlineHud(onlineModel)) { onlineOk = true; break; }
            }
            return onlineOk ? onlineModel : null;
        }

        /// <summary>Không nhận tham số; quét lại các nguồn HUD online trong scene (vài giây một lần, chỉ khi chơi online).</summary>
        void RescanOnlineSources()
        {
            MonoBehaviour[] all = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            List<IOnlineHudSource> found = new List<IOnlineHudSource>();
            for (int i = 0; i < all.Length; i++)
            {
                IOnlineHudSource src = all[i] as IOnlineHudSource;
                if (src != null) found.Add(src);
            }
            onlineSources = found.ToArray();
        }

        /// <summary>Nhận số người dự phòng; trả số người của đội theo dữ liệu online của khu đang làm nếu có, ngược lại trả số dự phòng.</summary>
        int HudTeamSize(int fallback)
        {
            AreaHudModel m = OnlineHud(ActiveArea);
            return m != null && m.TeamSize > 0 ? m.TeamSize : fallback;
        }

        /// <summary>Nhận ID khu; trả model HUD của nhiệm vụ mới (rỗng nếu khu không có nhà cung cấp). Điền một lần mỗi frame và mỗi khu để tránh gọi lặp.</summary>
        AreaHudModel ExtraModel(string area)
        {
            if (extraFrame == Time.frameCount && extraArea == area) return extraModel;
            if (extraArea != area || extraProvider == null) { extraArea = area; extraProvider = AreaHudProviders.Find(flow, area); }
            extraFrame = Time.frameCount;
            extraModel.Clear();
            extraOnline = false;
            AreaHudModel online = OnlineHud(area);
            if (online != null)
            {
                extraOnline = true;
                extraModel.Title = online.Title; extraModel.Progress = online.Progress; extraModel.Objective = online.Objective;
                extraModel.Prompt = online.Prompt; extraModel.Notebook = online.Notebook; extraModel.Controls = online.Controls; extraModel.TeamSize = online.TeamSize;
            }
            else if (extraProvider != null) extraProvider.FillHud(extraModel, flow.attendance.party.ActivePlayer, Time.timeAsDouble);
            return extraModel;
        }

        /// <summary>Nhận ID khu; trả true nếu khu có nhà cung cấp HUD (dùng model thay đường text cũ).</summary>
        bool HasHudProvider(string area) { ExtraModel(area); return extraProvider != null || extraOnline; }

        /// <summary>Không nhận tham số; N đổi trạng thái hiện/ẩn sổ tay. Trả true nếu sổ tay đang mở.</summary>
        bool UpdateNotebookKey()
        {
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null && keyboard.Control(GameAction.Notebook).wasPressedThisFrame) notebookOpen = !notebookOpen;
            return notebookOpen;
        }

        /// <summary>Nhận cờ hiện; tìm bản đồ nhỏ đã đặt sẵn trong HUD (object Canvas chỉnh được), ẩn khi không phải sân, tô lại màu theo tiến độ.</summary>
        void UpdateMiniMap(bool show)
        {
            if (view == null || view.hud == null) return;
            if (miniMap == null) miniMap = view.hud.GetComponentInChildren<SchoolFloorMiniMapView>(true);
            if (miniMap == null) return;
            miniMap.SetVisible(show);
            if (!show || flow == null || flow.missionFlow == null || flow.missionFlow.Registry == null) return;
            miniMap.Apply(flow.missionFlow.Registry.Progress);
        }

        readonly List<TextMesh> nameTags = new List<TextMesh>();
        Font tagFont;

        /// <summary>Nhận chỉ số ô; trả tên người ở ô đó: ưu tiên tên người chơi thật khi chơi online (T-055), không thì tên từ đề Điểm danh, không có thì null.</summary>
        string NameOfSlot(int slot)
        {
            var a = flow.attendance;
            if (a == null || a.party == null) return null;
            string online = a.party.IsOnline ? a.party.SlotName(slot) : null;
            if (!string.IsNullOrEmpty(online)) return online;
            if (a.State != null && slot >= 0 && slot < a.State.Round.Names.Count) return a.State.Round.Names[slot];
            return null;
        }

        /// <summary>Không nhận tham số; trả ' • Tên' của vai đang điều khiển (tên người chơi online, hoặc tên từ đề Điểm danh) hoặc rỗng nếu chưa có.</summary>
        string RoleNameSuffix()
        {
            var a = flow.attendance;
            if (a == null || a.party == null) return "";
            string name = NameOfSlot(a.party.ActivePlayer);
            return name != null ? " • " + name : "";
        }

        /// <summary>Không nhận tham số; hiện biển tên ('số • tên') trên đầu các đồng đội đang trong đội (không hiện cho vai đang điều khiển), xoay theo camera. Để người chơi đối chiếu manh mối 'Minh ngồi cạnh An' với nhân vật thật.</summary>
        void UpdateNameTags()
        {
            var a = flow.attendance;
            if (a == null || a.party == null || a.party.players == null) return;
            if (a.State == null && !a.party.IsOnline) return;
            var players = a.party.players;
            Camera cam = a.party.viewCamera != null ? a.party.viewCamera : Camera.main;
            if (tagFont == null) tagFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            while (nameTags.Count < players.Length) nameTags.Add(null);
            for (int i = 0; i < players.Length; i++)
            {
                Transform pl = players[i]; if (pl == null) continue;
                var tag = nameTags[i];
                if (tag == null)
                {
                    var go = new GameObject("NameTag_" + (i + 1), typeof(TextMesh));
                    tag = go.GetComponent<TextMesh>();
                    tag.font = tagFont; tag.fontSize = 64; tag.characterSize = 0.09f;
                    tag.anchor = TextAnchor.MiddleCenter; tag.alignment = TextAlignment.Center;
                    tag.color = new Color(1f, 0.95f, 0.6f);
                    go.GetComponent<MeshRenderer>().sharedMaterial = tagFont.material;
                    nameTags[i] = tag;
                }
                string who = NameOfSlot(i);
                bool show = cam != null && who != null && (a.party.IsOnline || i < a.PartySize) && i != a.party.ActivePlayer && pl.gameObject.activeInHierarchy;
                if (tag.gameObject.activeSelf != show) tag.gameObject.SetActive(show);
                if (!show) continue;
                string label = (i + 1) + " • " + who;
                if (tag.text != label) tag.text = label;
                var cc = pl.GetComponent<CharacterController>();
                float top = cc != null ? cc.center.y + cc.height * 0.5f + 0.35f : 2.2f;
                tag.transform.position = pl.position + Vector3.up * top;
                tag.transform.rotation = Quaternion.LookRotation(tag.transform.position - cam.transform.position);
            }
        }
    }
}
