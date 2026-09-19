/*
 * Mục đích: Hiển thị sa bàn chung và gói chỉ dẫn riêng, gửi bước qua bộ xác thực.
 * Hàm: OnGUI vẽ HUD; Prepare tạo style; Panel vẽ nền; DrawBoard vẽ lưới và hướng;
 * Direction gửi bước; DrawMenu cho thử lại hoặc chơi lại map.
 */
using UnityEngine;
namespace TheReturn
{
    public sealed class NavigationExamHud : MonoBehaviour
    {
        public SchoolFloorFlow flow;
        GUIStyle title, body, small, button;
        Vector2 clueScroll;
        /// <summary>Không nhận tham số; tạo style một lần để hiển thị tiếng Việt.</summary>
        void Prepare()
        {
            if (body != null) return;
            body = new GUIStyle(GUI.skin.label) { fontSize = 19, wordWrap = true };
            title = new GUIStyle(body) { fontSize = 26, fontStyle = FontStyle.Bold };
            small = new GUIStyle(body) { fontSize = 16 };
            button = new GUIStyle(GUI.skin.button) { fontSize = 20, wordWrap = true };
        }
        /// <summary>Nhận khung; tô nền tối và phục hồi màu GUI.</summary>
        void Panel(Rect rect)
        {
            Color old = GUI.color;
            GUI.color = new Color(.035f, .06f, .08f, .98f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = old;
        }
        /// <summary>Không nhận tham số; bố trí giao diện 1280×720, chỉ vẽ khi nhiệm vụ đang điều khiển.</summary>
        void OnGUI()
        {
            if (flow.useCanvas) return;
            var game = flow.navigationExam;
            if (game == null || !game.Active || game.State == null) return;
            Prepare();
            Matrix4x4 old = GUI.matrix;
            float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1280*scale)/2, (Screen.height-720*scale)/2),
                Quaternion.identity, Vector3.one*scale);
            Panel(new Rect(20,20,1240,95));
            GUI.Label(new Rect(38,32,810,38),game.catalog.title,title);
            GUI.Label(new Rect(38,78,810,28),"Từ " + NavigationExamState.Code(game.State.Start) + " tới " +
                NavigationExamState.Code(game.State.Goal) + " • Dấu: " + game.State.Stamps + "/2 • Bước: " + game.State.Steps,small);
            GUI.Label(new Rect(900,34,340,36),"VAI " + (game.party.ActivePlayer+1) + " / ĐỘI " + game.party.PartySize,title);
            Panel(new Rect(875,133,385,429));
            GUI.Label(new Rect(896,149,343,40),"MANH MỐI CỦA BẠN",title);
            string clues = game.catalog.Packet(game.State,game.party.ActivePlayer);
            float height = Mathf.Max(290,body.CalcHeight(new GUIContent(clues),310));
            clueScroll = GUI.BeginScrollView(new Rect(896,202,343,321),clueScroll,new Rect(0,0,310,height));
            GUI.Label(new Rect(0,0,310,height),clues,body);
            GUI.EndScrollView();
            Panel(new Rect(20,580,1240,120));
            GUI.Label(new Rect(38,592,1200,64),game.Status,body);
            GUI.Label(new Rect(38,663,1200,27),game.catalog.controls,small);
            if (game.PanelOpen) DrawBoard(game);
            else if (Cursor.lockState != CursorLockMode.Locked) DrawMenu(game);
            else
            {
                GUI.Label(new Rect(632,347,30,30),"+",body);
                Panel(new Rect(40,477,800,80));
                GUI.Label(new Rect(60,495,760,50),game.State.Solved ? game.catalog.success :
                    game.Targeted ? "[E] Mở sa bàn • Lượt vai " + (game.State.Turn+1) :
                    "Tới sa bàn và nhấn E. Trao đổi manh mối trước khi đi.",body);
            }
            GUI.matrix = old;
        }
        /// <summary>Nhận phiên; vẽ lưới không lộ ô khóa, đánh dấu quân/đích và các nút điều khiển.</summary>
        void DrawBoard(NavigationExamPrototype game)
        {
            Panel(new Rect(20,133,835,429));
            GUI.Label(new Rect(42,145,780,36),game.State.Solved ? "ĐÃ HOÀN THÀNH" :
                "QUÂN Ở " + NavigationExamState.Code(game.State.Position) + " • LƯỢT VAI " + (game.State.Turn+1),title);
            for (int cell=0;cell<9;cell++)
            {
                Rect rect = new Rect(45+(cell%3)*130,198+(cell/3)*88,120,78);
                string value = NavigationExamState.Code(cell);
                if (cell == game.State.Position) value += "\nQUÂN";
                else if (cell == game.State.Goal) value += "\nĐÍCH";
                else if (cell == game.State.Start) value += "\nBẮT ĐẦU";
                GUI.Box(rect,value,button);
            }
            GUI.Label(new Rect(451,198,375,60),"Trên/dưới theo hàng số, trái/phải theo cột chữ; không theo hướng camera.",small);
            GUI.enabled = !game.State.Solved && game.party.ActivePlayer == game.State.Turn;
            Direction(game,new Rect(564,270,132,48),"Lên",0,-1);
            Direction(game,new Rect(451,329,120,48),"Trái",-1,0);
            Direction(game,new Rect(695,329,132,48),"Phải",1,0);
            Direction(game,new Rect(564,388,132,48),"Xuống",0,1);
            GUI.enabled = true;
            GUI.Label(new Rect(45,469,780,37),"Sổ lớp: " + (game.State.Stamps>=1 ? "đã lấy" : "chưa lấy") +
                " • Chìa khóa: " + (game.State.Stamps>=2 ? "đã lấy" : "chưa lấy"),body);
            if (GUI.Button(new Rect(45,513,780,34),"Rời sa bàn [E]",button)) game.ClosePanel();
        }
        /// <summary>Nhận phiên, vị trí nút và vector bước; gửi số bước hiện tại để chặn lệnh UI đã cũ.</summary>
        void Direction(NavigationExamPrototype game,Rect rect,string label,int dx,int dy)
        {
            if (GUI.Button(rect,label,button)) game.Step(dx,dy,game.State.Steps);
        }
        /// <summary>Nhận phiên; hiển thị tiếp tục, thử lại, biến thể và chơi lại cả map.</summary>
        void DrawMenu(NavigationExamPrototype game)
        {
            Panel(new Rect(125,165,625,365));
            GUI.Label(new Rect(150,184,575,35),"TẠM DỪNG",title);
            if(GUI.Button(new Rect(150,238,575,46),"Tiếp tục",button)) PrototypePartyController.SetCursor(true);
            if(GUI.Button(new Rect(150,299,575,46),"Thử lại sa bàn",button)) game.ResetRound();
            if(GUI.Button(new Rect(150,360,575,46),"Biến thể tiếp theo",button)) game.ResetRound(true);
            if(GUI.Button(new Rect(150,421,575,46),"Chơi lại cả map",button)) flow.RestartMap();
        }
    }
}
