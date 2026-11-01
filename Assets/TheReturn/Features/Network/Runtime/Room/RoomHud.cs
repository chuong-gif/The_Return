/*
 * Mục đích: Giao diện PHÒNG online tạm (T-026), IMGUI cùng phong cách NetworkTestHud. Màn 1: tên, chọn đội 2/3/4 người, tạo phòng Relay/LAN,
 * nhập mã để vào. Màn 2 (trong phòng): mã phòng, danh sách ô + tên, nút Bắt đầu (chỉ chủ phòng, chỉ khi đủ người), nút Rời phòng.
 * Hàm: OnGUI; DrawMenu; DrawLobby; NextProtocol.
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
            GUILayout.BeginArea(new Rect(10, 10, 360, 520), GUI.skin.box);
            GUILayout.Label("PHÒNG ONLINE (T-026)");
            NetworkSessionService s = rooms.Session;
            if (s.State == NetworkSessionState.Hosting || s.State == NetworkSessionState.Joined) DrawLobby(s);
            else DrawMenu(s);
            GUILayout.EndArea();
        }

        private void DrawMenu(NetworkSessionService s)
        {
            if (s.State == NetworkSessionState.Working)
            {
                GUILayout.Label(s.StatusText);
                return;
            }
            GUILayout.Label("Tên của bạn (để trống = tự đặt):");
            playerName = GUILayout.TextField(playerName, 20);
            GUILayout.Label("Số người trong đội (chốt lúc tạo phòng):");
            GUILayout.BeginHorizontal();
            for (int n = 2; n <= 4; n++)
            {
                if (GUILayout.Toggle(teamSize == n, n + " người", GUI.skin.button)) teamSize = n;
            }
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Giao thức Relay: " + s.RelayProtocolChoice + " (bấm để đổi)")) s.RelayProtocolChoice = NextProtocol(s.RelayProtocolChoice);
            if (GUILayout.Button("Tạo phòng Relay (qua Internet)")) rooms.CreateRelay(teamSize, playerName);
            if (GUILayout.Button("Tạo phòng LAN (cùng máy/mạng)")) rooms.CreateLan(teamSize, playerName);
            GUILayout.Space(6);
            GUILayout.Label("Mã phòng để vào:");
            joinCode = GUILayout.TextField(joinCode, 8).ToUpperInvariant();
            if (GUILayout.Button("Vào bằng mã (Relay)")) rooms.JoinRelay(joinCode, playerName);
            if (GUILayout.Button("Vào LAN " + s.LanAddress)) rooms.JoinLan(playerName);
            if (!string.IsNullOrEmpty(rooms.Notice)) GUILayout.Label("! " + rooms.Notice);
            if (s.State == NetworkSessionState.Error)
            {
                GUILayout.Label(s.StatusText);
                if (GUILayout.Button("Thử lại")) s.ResetError();
            }
        }

        private void DrawLobby(NetworkSessionService s)
        {
            string where = string.IsNullOrEmpty(s.JoinCode) ? "LAN" : "Mã phòng: " + s.JoinCode;
            GUILayout.Label(where);
            GUILayout.Label(s.StatusText);
            RoomRoster roster = RoomRoster.Current;
            if (roster == null)
            {
                GUILayout.Label("Đang đồng bộ phòng...");
            }
            else
            {
                int team = roster.TeamSize.Value;
                GUILayout.Label("Đội: " + roster.Rows.Count + "/" + team + " người");
                for (int i = 0; i < roster.Rows.Count; i++)
                {
                    var row = roster.Rows[i];
                    GUILayout.Label("  Ô " + (row.Slot + 1) + ": " + row.Name + (row.IsHost ? " (chủ phòng)" : ""));
                }
                if (roster.Started.Value)
                {
                    GUILayout.Label("VÁN ĐÃ BẮT ĐẦU (nạp map sẽ nối ở T-028)");
                }
                else if (s.State == NetworkSessionState.Hosting)
                {
                    bool full = roster.Rows.Count >= team;
                    GUI.enabled = full;
                    if (GUILayout.Button(full ? "Bắt đầu" : "Chờ đủ " + team + " người...")) rooms.StartGame();
                    GUI.enabled = true;
                }
                else
                {
                    GUILayout.Label("Chờ chủ phòng bắt đầu...");
                }
            }
            if (GUILayout.Button("Rời phòng")) rooms.Leave();
        }

        private static RelayProtocol NextProtocol(RelayProtocol p)
        {
            if (p == RelayProtocol.DTLS) return RelayProtocol.WSS;
            if (p == RelayProtocol.WSS) return RelayProtocol.UDP;
            return RelayProtocol.DTLS;
        }
    }
}
