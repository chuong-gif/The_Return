/*
 * Mục đích: Vẽ giao diện bản thử, tách khỏi luật điểm danh và bộ điều khiển người chơi.
 * Danh sách hàm:
 * - PrepareStyles: tạo style giao diện một lần.
 * - Panel: vẽ nền phẳng.
 * - OnGUI: trình bày mục tiêu, manh mối riêng và điều khiển.
 * - DrawMenu: chọn 2–4 người, bắt đầu hoặc chơi đề mới.
 */
using UnityEngine;

namespace TheReturn
{
    public sealed class AttendanceHud : MonoBehaviour
    {
        public AttendancePrototype session;
        GUIStyle heading, body, small, button;
        Vector2 clueScroll;

        /// <summary>Không nhận tham số; tạo font/style nếu chưa có, không tạo asset.</summary>
        void PrepareStyles()
        {
            if (body != null) return;
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            body = new GUIStyle(GUI.skin.label) { font = font, fontSize = 18, wordWrap = true };
            body.normal.textColor = new Color(.94f, .95f, .94f);
            heading = new GUIStyle(body) { fontSize = 25, fontStyle = FontStyle.Bold };
            small = new GUIStyle(body) { fontSize = 14 };
            button = new GUIStyle(GUI.skin.button) { font = font, fontSize = 18 };
        }

        /// <summary>Nhận vùng vẽ; tô nền tối rồi phục hồi màu GUI trước đó.</summary>
        void Panel(Rect rect)
        {
            Color previous = GUI.color;
            GUI.color = new Color(.045f, .075f, .09f, .96f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
        }

        /// <summary>Không nhận tham số; đọc session và vẽ UI theo tỷ lệ 1280×720, không quyết định đáp án.</summary>
        void OnGUI()
        {
            if (session == null || session.textCatalog == null) return;
            PrepareStyles();
            Matrix4x4 previous = GUI.matrix;
            float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1280 * scale) / 2, (Screen.height - 720 * scale) / 2),
                Quaternion.identity, Vector3.one * scale);
            Panel(new Rect(20, 20, 400, 126));
            GUI.Label(new Rect(38, 30, 365, 24), "THE RETURN / LỚP 101", small);
            GUI.Label(new Rect(38, 59, 365, 38), session.textCatalog.title, heading);
            GUI.Label(new Rect(38, 103, 365, 30), session.State == null ? session.textCatalog.waiting :
                "Đã ngồi " + session.State.OccupiedCount + "/" + session.PartySize + " người • 24 ghế", small);

            if (session.State != null)
            {
                Panel(new Rect(820, 20, 440, 348));
                GUI.Label(new Rect(840, 35, 400, 38),
                    "VAI " + (session.ActivePlayer + 1) + " / " + session.State.Round.Names[session.ActivePlayer], heading);
                string packet = session.CluePackets[session.ActivePlayer];
                float height = body.CalcHeight(new GUIContent(packet), 375);
                clueScroll = GUI.BeginScrollView(new Rect(840, 85, 400, 244), clueScroll,
                    new Rect(0, 0, 375, Mathf.Max(240, height)));
                GUI.Label(new Rect(0, 0, 375, Mathf.Max(240, height)), packet, body);
                GUI.EndScrollView();
                if (session.Begun && !session.Completed && Cursor.lockState == CursorLockMode.Locked)
                {
                    GUI.Label(new Rect(632, 348, 30, 30), "+", body);
                    string prompt = session.State.SeatOf(session.ActivePlayer) >= 0 ? "[E] Đứng dậy" :
                        session.TargetSeat >= 0 ? "[E] Ngồi " + AttendanceRound.SeatCode(session.TargetSeat) : "Nhìn vào bàn hoặc ghế ở gần để ngồi";
                    Panel(new Rect(340, 510, 600, 44));
                    GUI.Label(new Rect(354, 520, 576, 28), prompt, body);
                }
            }
            Panel(new Rect(20, 574, 1240, 126));
            GUI.Label(new Rect(38, 586, 1200, 52), session.Status ?? "", body);
            GUI.Label(new Rect(38, 642, 1200, 25), "WASD đi • Chuột nhìn • E ngồi/đứng • H gợi ý • Tab/Esc mở menu", small);
            GUI.Label(new Rect(38, 671, 1200, 24), "THỬ MỘT MÁY • 1–" + Mathf.Max(2, session.PartySize) +
                " đổi vai • F5 đề mới • R thử lại cùng đề • Chưa có mạng", small);
            if (!session.Begun || session.Completed || Cursor.lockState != CursorLockMode.Locked) DrawMenu();
            GUI.matrix = previous;
        }

        /// <summary>Không nhận tham số; các nút chuyển số người gọi API roster, không sửa trực tiếp trạng thái ghế.</summary>
        void DrawMenu()
        {
            Panel(new Rect(190, 166, 596, 366));
            GUI.Label(new Rect(216, 184, 544, 40), "Chọn đội 2–4 người", heading);
            GUI.Label(new Rect(216, 229, 544, 105), session.textCatalog.introduction, body);
            for (int count = 2; count <= 4; count++)
            {
                string label = count + " người" + (session.PartySize == count ? " ✓" : "");
                if (GUI.Button(new Rect(216 + (count - 2) * 184, 347, 176, 42), label, button))
                    session.SetParticipantCount(count);
            }
            if (GUI.Button(new Rect(216, 404, 544, 44),
                session.Completed ? "Đề mới và chơi tiếp" : "Bắt đầu / Tiếp tục", button))
            {
                if (session.Completed) session.StartNewRound();
                session.BeginSession();
            }
            GUI.Label(new Rect(216, 464, 544, 51),
                "Đổi đội sẽ tạo lại đề. Hàng tăng khi đi xa bảng; cột tăng từ trái sang phải.", small);
        }
    }
}
