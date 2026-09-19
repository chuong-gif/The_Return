/*
 * Mục đích: Giao diện đọc chứng từ và sửa bảng điểm; chỉ gửi lệnh qua bộ xác thực.
 * Hàm: OnGUI bố trí HUD; Prepare tạo style; Panel vẽ nền; DrawDocument hiển thị tài liệu;
 * DrawBoard chọn phiếu/nguồn/đích; DrawMenu điều khiển thử lại; RowName đổi ID thành tên hồ sơ.
 */
using UnityEngine;
namespace TheReturn
{
    public sealed class GradeRepairHud : MonoBehaviour
    {
        public SchoolFloorFlow flow;
        GUIStyle title, body, small, button;
        int voucher, source, target;
        Vector2 scroll;

        /// <summary>Không nhận tham số; tạo các style chữ một lần.</summary>
        void Prepare()
        {
            if (body != null) return;
            body = new GUIStyle(GUI.skin.label) { fontSize = 19, wordWrap = true };
            title = new GUIStyle(body) { fontSize = 27, fontStyle = FontStyle.Bold };
            small = new GUIStyle(body) { fontSize = 16 };
            button = new GUIStyle(GUI.skin.button) { fontSize = 18, wordWrap = true };
        }

        /// <summary>Nhận khung; vẽ nền tối và giữ màu GUI của người gọi.</summary>
        void Panel(Rect rect)
        {
            Color previous = GUI.color;
            GUI.color = new Color(.035f, .06f, .08f, .98f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
        }

        /// <summary>Nhận chỉ số hồ sơ; trả nhãn A–D để giao diện không tiết lộ mã từ sổ đối chiếu.</summary>
        string RowName(int row) => "Hồ sơ " + (char)('A' + row);

        /// <summary>Không nhận tham số; vẽ HUD theo khung 1280×720 và phục hồi ma trận sau khi vẽ.</summary>
        void OnGUI()
        {
            if (flow.useCanvas) return;
            var game = flow.gradeRepair;
            if (game == null || !game.Active || game.State == null) return;
            Prepare();
            Matrix4x4 previous = GUI.matrix;
            float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1280 * scale) / 2,
                (Screen.height - 720 * scale) / 2), Quaternion.identity, Vector3.one * scale);
            Panel(new Rect(20, 20, 1240, 90));
            GUI.Label(new Rect(38, 32, 780, 36), game.catalog.title + " / PHÒNG GIÁO VIÊN", title);
            GUI.Label(new Rect(38, 76, 780, 27), "Tổng cần đạt: 32 • Mỗi hồ sơ 6–10 • Mỗi chứng từ chỉ dùng một lần", small);
            GUI.Label(new Rect(930, 36, 308, 34), "VAI " + (game.party.ActivePlayer + 1) + " / ĐỘI " + game.party.PartySize, title);
            Panel(new Rect(20, 585, 1240, 115));
            GUI.Label(new Rect(38, 597, 1204, 58), game.Status, body);
            GUI.Label(new Rect(38, 663, 1204, 27), game.catalog.controls, small);

            if (game.PanelOpen)
            {
                if (game.OpenDocument >= 0) DrawDocument(game);
                else DrawBoard(game);
            }
            else if (Cursor.lockState != CursorLockMode.Locked) DrawMenu(game);
            else
            {
                GUI.Label(new Rect(632, 347, 30, 30), "+", body);
                Panel(new Rect(270, 490, 740, 70));
                string prompt = game.Target == null ? "Tới bàn hồ sơ hoặc bảng sửa • Nhìn gần rồi nhấn E" :
                    game.Target.document < 0 ? "[E] Mở bảng sửa điểm" :
                    "[E] Hồ sơ " + (game.Target.document + 1) + " • Vai " + (game.State.Round.OwnerOf(game.Target.document) + 1) + " phụ trách";
                if (game.State.Solved) prompt = game.catalog.success;
                GUI.Label(new Rect(288, 507, 704, 48), prompt, body);
            }
            GUI.matrix = previous;
        }

        /// <summary>Nhận phiên; vẽ tài liệu riêng của vai đã được xác thực cùng nút đóng.</summary>
        void DrawDocument(GradeRepairPrototype game)
        {
            Panel(new Rect(230, 135, 820, 420));
            int doc = game.OpenDocument;
            string heading = game.catalog.documentTitles != null && doc < game.catalog.documentTitles.Length ?
                game.catalog.documentTitles[doc] : "Tài liệu " + (doc + 1);
            GUI.Label(new Rect(254, 150, 760, 40), heading, title);
            string text = game.catalog.DocumentText(game.State.Round, doc);
            float height = Mathf.Max(235, body.CalcHeight(new GUIContent(text), 720));
            scroll = GUI.BeginScrollView(new Rect(254, 207, 770, 252), scroll, new Rect(0, 0, 720, height));
            GUI.Label(new Rect(0, 0, 720, height), text, body);
            GUI.EndScrollView();
            GUI.Label(new Rect(254, 470, 760, 27), "Đã xác nhận đọc • Hãy trao đổi mã hồ sơ với đồng đội ở bảng.", small);
            if (GUI.Button(new Rect(254, 508, 770, 34), "Đóng tài liệu [E]", button)) game.ClosePanel();
        }

        /// <summary>Nhận phiên; vẽ điểm và các lựa chọn, gửi Apply/Undo/Submit qua controller thay vì sửa state trực tiếp.</summary>
        void DrawBoard(GradeRepairPrototype game)
        {
            Panel(new Rect(80, 125, 1120, 443));
            GUI.Label(new Rect(103, 141, 1000, 34), "BẢNG SỬA / ĐỐI CHIẾU CHỨNG TỪ TRƯỚC KHI NỘP", title);
            int total = 0;
            for (int i = 0; i < 4; i++)
            {
                total += game.State.Score(i);
                GUI.Label(new Rect(105 + i * 215, 190, 210, 32),
                    RowName(i) + ": " + game.State.Round.InitialScores[i] + " → " + game.State.Score(i), body);
            }
            GUI.Label(new Rect(990, 190, 180, 32), "Tổng: " + total, body);
            string[] names = { "Phiếu 01: +2", "Phiếu 02: +2", "Biên bản: chuyển 1" };
            GUI.enabled = !game.State.Solved;
            for (int i = 0; i < 3; i++)
            {
                string name = (voucher == i ? "● " : "") + names[i] + (game.State.Used(i) ? " (đã dùng)" : "");
                if (GUI.Button(new Rect(105 + i * 358, 234, 346, 46), name, button)) voucher = i;
            }
            GUI.Label(new Rect(105, 298, 130, 28), "Hồ sơ nhận:", body);
            for (int i = 0; i < 4; i++)
                if (GUI.Button(new Rect(243 + i * 226, 293, 214, 40),
                    (target == i ? "● " : "") + RowName(i), button)) target = i;
            GUI.enabled = !game.State.Solved && voucher == 2;
            GUI.Label(new Rect(105, 352, 130, 30), "Chuyển từ:", body);
            for (int i = 0; i < 4; i++)
                if (GUI.Button(new Rect(243 + i * 226, 347, 214, 40),
                    (source == i ? "● " : "") + RowName(i), button)) source = i;
            GUI.enabled = !game.State.Solved;
            if (GUI.Button(new Rect(105, 408, 342, 46), "Áp dụng chứng từ đã chọn", button))
                game.Apply(voucher, voucher == 2 ? source : -1, target);
            if (GUI.Button(new Rect(459, 408, 342, 46), "Hoàn tác (" + game.State.UndoCount + ")", button)) game.Undo();
            if (GUI.Button(new Rect(813, 408, 360, 46), "Nộp bảng điểm", button)) game.Submit();
            GUI.enabled = true;
            int read = 0;
            for (int i = 0; i < 4; i++) if (game.State.Read(i)) read++;
            GUI.Label(new Rect(105, 473, 750, 31), "Hồ sơ đã đọc: " + read + "/4 • Sai có thể hoàn tác, phiếu không bị mất.", small);
            if (GUI.Button(new Rect(105, 515, 1068, 34), "Rời bảng [E]", button)) game.ClosePanel();
        }

        /// <summary>Nhận phiên; hiển thị tiếp tục, reset nhiệm vụ 3 hoặc chơi lại cả map.</summary>
        void DrawMenu(GradeRepairPrototype game)
        {
            Panel(new Rect(300, 173, 680, 362));
            GUI.Label(new Rect(324, 194, 632, 42), "TẠM DỪNG / SỬA BẢNG ĐIỂM", title);
            if (GUI.Button(new Rect(324, 252, 632, 46), "Tiếp tục", button)) PrototypePartyController.SetCursor(true);
            if (GUI.Button(new Rect(324, 312, 632, 46), "Thử lại bảng — giữ hai nhiệm vụ trước", button)) game.Retry();
            if (GUI.Button(new Rect(324, 372, 632, 46), "Hồ sơ mới — giữ hai nhiệm vụ trước", button)) game.NewCase();
            if (GUI.Button(new Rect(324, 432, 632, 46), "Chơi lại cả map từ điểm danh", button)) flow.RestartMap();
        }
    }
}
