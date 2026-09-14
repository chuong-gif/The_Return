/*
 * Mục đích: Giao diện thử nhiệm vụ 2, hiện ồn, lý do và bàn giao cho đội 2–4 người.
 * Hàm: OnGUI vẽ HUD/menu; Panel vẽ khung có nền tối.
 */
using UnityEngine;
namespace TheReturn
{
    public sealed class QuietCorridorHud : MonoBehaviour
    {
        public SchoolFloorFlow flow;
        GUIStyle title, body, small, button;

        /// <summary>Nhận hình chữ nhật; vẽ nền tối, phục hồi màu GUI.</summary>
        void Panel(Rect rect)
        {
            Color before = GUI.color;
            GUI.color = new Color(.035f,.06f,.08f,.97f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = before;
        }

        /// <summary>Không nhận tham số; vẽ trạng thái và các nút tiếp tục/thử lại, phục hồi ma trận GUI.</summary>
        void OnGUI()
        {
            var game = flow.corridor;
            if (!game.Active || game.State == null) return;
            if (body == null)
            {
                body = new GUIStyle(GUI.skin.label) { fontSize = 19, wordWrap = true };
                title = new GUIStyle(body) { fontSize = 27, fontStyle = FontStyle.Bold };
                small = new GUIStyle(body) { fontSize = 16 };
                button = new GUIStyle(GUI.skin.button) { fontSize = 19 };
            }
            Matrix4x4 before = GUI.matrix;
            float scale = Mathf.Min(Screen.width/1280f, Screen.height/720f);
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width-1280*scale)/2,(Screen.height-720*scale)/2),
                Quaternion.identity, Vector3.one*scale);
            Panel(new Rect(20,20,520,190));
            GUI.Label(new Rect(38,32,484,36), game.settings.title, title);
            GUI.Label(new Rect(38,77,484,30), "Tiếng ồn: " + Mathf.CeilToInt(game.State.Noise) + "/100", body);
            Color color = GUI.color;
            GUI.color = new Color(.15f,.22f,.24f);
            GUI.DrawTexture(new Rect(38,113,480,14), Texture2D.whiteTexture);
            GUI.color = game.State.Noise > 70 ? Color.red : new Color(.2f,.8f,.6f);
            GUI.DrawTexture(new Rect(38,113,480*game.State.Noise/100f,14), Texture2D.whiteTexture);
            GUI.color = color;
            string reason = game.State.Solved ? "Hoàn thành • Cửa giữ mở" :
                game.State.SpeakerOn ? "Loa đang rè: +" + game.settings.speakerNoisePerSecond.ToString("0.#") + "/giây" :
                game.State.Held ? "Loa đã tắt • Đi bộ để giảm ồn" : "Sắp bật loa: " + game.State.Grace.ToString("0.0") + " giây";
            if (game.party.IsSprinting && !game.State.Solved) reason += " • Đang chạy!";
            GUI.Label(new Rect(38,140,484,56), reason, small);
            Panel(new Rect(870,20,390,150));
            GUI.Label(new Rect(889,32,350,32), "VAI " + (game.party.ActivePlayer+1) + " / ĐỘI " + game.party.PartySize, title);
            GUI.Label(new Rect(889,77,350,75),
                "A: " + (game.State.Owner(0)<0 ? "Chưa giữ" : "Vai " + (game.State.Owner(0)+1)) +
                "    B: " + (game.State.Owner(1)<0 ? "Chưa giữ" : "Vai " + (game.State.Owner(1)+1)) +
                "\nBàn giao: " + (game.State.HandedOver ? "Đã xác nhận" : "Cần hai người giữ A và B"), small);
            GUI.Label(new Rect(632,347,30,30), "+", body);
            string prompt = game.State.Solved ? game.settings.success : game.IsHolding(game.party.ActivePlayer) ?
                "[E] Nhả nút • [1–4] Đổi sang đồng đội" : game.TargetStation >= 0 ?
                "[E] Giữ nút " + (game.TargetStation==0 ? "A" : "B") : "Tìm nút A/B • Đi bộ để giữ yên lặng";
            if (game.SafetyHolding) prompt = "Có người trong cửa • Cảm biến đang giữ cửa mở";
            Panel(new Rect(280,485,720,62));
            GUI.Label(new Rect(298,496,684,48), prompt, body);
            Panel(new Rect(20,568,1240,132));
            GUI.Label(new Rect(38,580,1204,66), game.Status, body);
            GUI.Label(new Rect(38,650,1204,44), game.settings.localControls, small);
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                Panel(new Rect(325,218,630,251));
                GUI.Label(new Rect(348,233,580,35), "TẠM DỪNG • Giữ kết quả điểm danh", title);
                if (GUI.Button(new Rect(348,280,584,44), "Tiếp tục", button)) PrototypePartyController.SetCursor(true);
                if (GUI.Button(new Rect(348,337,584,44), "Thử lại riêng hành lang", button))
                { game.Retry(); PrototypePartyController.SetCursor(true); }
                if (GUI.Button(new Rect(348,394,584,44), "Chơi lại cả map từ điểm danh", button)) flow.RestartMap();
            }
            GUI.matrix = before;
        }
    }
}
