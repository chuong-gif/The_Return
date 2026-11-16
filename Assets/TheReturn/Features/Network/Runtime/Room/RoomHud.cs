/*
 * Mục đích: Giao diện PHÒNG online tạm (T-026), IMGUI nhưng đã tô theo ModernUI (ModernGui, T-053). Màn 1: tên, chọn đội 2/3/4 người, tạo phòng Relay/LAN,
 * nhập mã để vào. Màn 2 (trong phòng): mã phòng, danh sách ô + tên, nút Bắt đầu (chỉ chủ phòng, chỉ khi đủ người), nút Rời phòng.
 * Khi ván đã bắt đầu chỉ còn một thanh nhỏ ở góc (map có HUD riêng).
 * Hàm: OnGUI; DrawBackdrop; DrawMenu; DrawLobby; DrawStartedPill; NextProtocol.
 */
using Unity.Services.Multiplayer;
using UnityEngine;

namespace TheReturn.Network.Room
{
    public sealed class RoomHud : MonoBehaviour
    {
        [SerializeField] private RoomService rooms;

        private string playerName = "";
        private string joinCode = "";
        private int teamSize = 2;

        private void OnGUI()
        {
            if (rooms == null) rooms = FindFirstObjectByType<RoomService>();
            if (rooms == null || rooms.Session == null) return;
            NetworkSessionService s = rooms.Session;
            float vw, vh;
            ModernGui.Begin(out vw, out vh);
            RoomRoster roster = RoomRoster.Current;
            bool started = roster != null && roster.Started.Value;
            if (started)
            {
                DrawStartedPill(s);
            }
            else
            {
                DrawBackdrop(vw, vh);
                bool inLobby = s.State == NetworkSessionState.Hosting || s.State == NetworkSessionState.Joined;
                float w = 540f;
                float h = inLobby ? 500f : 640f;
                GUILayout.BeginArea(new Rect(Mathf.Min(160f, (vw - w) * 0.5f), (vh - h) * 0.5f, w, h), GUI.skin.box);
                GUILayout.Label("PHÒNG ONLINE", ModernGui.Title);
                if (inLobby) DrawLobby(s); else DrawMenu(s);
                GUILayout.EndArea();
            }
            ModernGui.End();
        }

        /// <summary>Nhận kích thước màn hình ảo; phủ nền tối chuyển màu (chỉ khi vẽ lại) để phòng chờ nhìn như menu chính.</summary>
        private static void DrawBackdrop(float vw, float vh)
        {
            Texture2D bg = ModernGui.Backdrop;
            if (bg != null && Event.current.type == EventType.Repaint) GUI.DrawTexture(new Rect(0f, 0f, vw, vh), bg, ScaleMode.StretchToFill);
        }

        private void DrawMenu(NetworkSessionService s)
        {
            if (s.State == NetworkSessionState.Working)
            {
                GUILayout.Label(s.StatusText);
                GUILayout.Space(8f);
                if (GUILayout.Button("← Về menu chính")) BackToMainMenu();
                return;
            }
            GUILayout.Label("Tên của bạn (để trống = tự đặt)", ModernGui.Dim);
            playerName = GUILayout.TextField(playerName, 20);
            GUILayout.Label("Số người trong đội (chốt lúc tạo phòng)", ModernGui.Dim);
            GUILayout.BeginHorizontal();
            for (int n = 2; n <= 4; n++)
            {
                if (GUILayout.Toggle(teamSize == n, n + " người", GUI.skin.button)) teamSize = n;
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(6f);
            if (GUILayout.Button("Tạo phòng Relay (qua Internet)", ModernGui.Primary)) rooms.CreateRelay(teamSize, playerName);
            if (GUILayout.Button("Tạo phòng LAN (cùng máy/mạng)")) rooms.CreateLan(teamSize, playerName);
            GUILayout.Space(8f);
            GUILayout.Label("Mã phòng để vào", ModernGui.Dim);
            joinCode = GUILayout.TextField(joinCode, 8).ToUpperInvariant();
            if (GUILayout.Button("Vào bằng mã (Relay)")) rooms.JoinRelay(joinCode, playerName);
            if (GUILayout.Button("Vào LAN " + s.LanAddress)) rooms.JoinLan(playerName);
            GUILayout.Space(6f);
            if (GUILayout.Button("Giao thức Relay: " + s.RelayProtocolChoice + " (bấm để đổi)")) s.RelayProtocolChoice = NextProtocol(s.RelayProtocolChoice);
            if (!string.IsNullOrEmpty(rooms.Notice)) GUILayout.Label("! " + rooms.Notice);
            GUILayout.Space(6f);
            if (GUILayout.Button("← Về menu chính")) BackToMainMenu();
            if (s.State == NetworkSessionState.Error)
            {
                GUILayout.Label(s.StatusText);
                if (GUILayout.Button("Thử lại")) s.ResetError();
            }
        }

        /// <summary>Không nhận tham số; thoát phòng/phiên mạng và quay về menu chính.</summary>
        private static void BackToMainMenu() { SessionExit.ReturnToMenu(SessionExit.DefaultMenuScene); }

        private void DrawLobby(NetworkSessionService s)
        {
            string where = string.IsNullOrEmpty(s.JoinCode) ? "LAN" : "Mã phòng: " + s.JoinCode;
            GUILayout.Label(where, ModernGui.Title);
            GUILayout.Label(s.StatusText, ModernGui.Dim);
            GUILayout.Space(8f);
            RoomRoster roster = RoomRoster.Current;
            if (roster == null)
            {
                GUILayout.Label("Đang đồng bộ phòng...");
            }
            else
            {
                int team = roster.TeamSize.Value;
                GUILayout.Label("Đội: " + roster.Rows.Count + "/" + team + " người", ModernGui.Dim);
                for (int i = 0; i < roster.Rows.Count; i++)
                {
                    var row = roster.Rows[i];
                    GUILayout.Box("Ô " + (row.Slot + 1) + ":  " + row.Name + (row.IsHost ? "   (chủ phòng)" : ""), GUI.skin.button);
                }
                GUILayout.Space(8f);
                if (s.State == NetworkSessionState.Hosting)
                {
                    bool full = roster.Rows.Count >= team;
                    GUI.enabled = full;
                    if (GUILayout.Button(full ? "Bắt đầu" : "Chờ đủ " + team + " người...", ModernGui.Primary)) rooms.StartGame();
                    GUI.enabled = true;
                }
                else
                {
                    GUILayout.Label("Chờ chủ phòng bắt đầu...");
                }
            }
            if (GUILayout.Button("Rời phòng")) rooms.Leave();
        }

        /// <summary>Nhận phiên mạng; vẽ thanh nhỏ góc trái trên báo ván đang diễn ra kèm mã phòng.</summary>
        private static void DrawStartedPill(NetworkSessionService s)
        {
            string code = string.IsNullOrEmpty(s.JoinCode) ? "LAN" : s.JoinCode;
            GUI.Box(new Rect(16f, 16f, 330f, 40f), "Ván đang diễn ra  •  " + code, ModernGui.Pill);
        }

        private static RelayProtocol NextProtocol(RelayProtocol p)
        {
            if (p == RelayProtocol.DTLS) return RelayProtocol.WSS;
            if (p == RelayProtocol.WSS) return RelayProtocol.UDP;
            return RelayProtocol.DTLS;
        }
    }
}
