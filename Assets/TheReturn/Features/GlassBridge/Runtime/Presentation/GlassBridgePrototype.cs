/*
 * Mục đích: Nối luật «Cầu kính» (GlassBridgeState) với người chơi, các ô kính trên cầu, hai đài và hai bảng sơ đồ (T-006e2).
 * Mỗi frame đọc vị trí từng nhân vật: bước vào hàng kế tiếp thì gọi State.Step, lên đài cuối thì State.Disembark, quay lại đài đầu
 * hoặc lùi hàng thì State.Retreat. Té (ô sai): ô vỡ, đội nhìn thấy ô đỏ, nhân vật bị đưa về đài đầu.
 * Quyền xem sơ đồ: bảng chỉ hiện chữ khi vai ĐANG ĐIỀU KHIỂN đứng ở một đài; đang trên cầu chỉ thấy dòng ẩn.
 * Hệ tọa độ cầu (laneOrigin): z+ từ đài đầu sang đài cuối, x+ là bên PHẢI người đi, y=0 là mặt sàn. Ô trái = x<0, ô phải = x>0.
 * Hàm: Begin bắt đầu; Retry đặt lại; Update xử lý input/theo dõi người chơi; TrackPlayers; Locate; HandleStep; StartPoint;
 * ResetTiles; RefreshBoards; LateUpdate xoay bảng về phía camera; OnGUI vẽ thông tin; StopSession ngừng nhiệm vụ.
 */
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheReturn
{
    [DefaultExecutionOrder(50)]
    public sealed class GlassBridgePrototype : MonoBehaviour
    {
        enum Place { None, Start, End, Tile }

        public PrototypePartyController party;
        [Tooltip("Gốc cầu: z+ từ đài đầu sang đài cuối, x+ là bên PHẢI của người đi, y=0 là mặt sàn.")]
        public Transform laneOrigin;
        public BoxCollider startPlatform;
        public BoxCollider endPlatform;
        [Tooltip("Sắp theo hàng*2 + bên (0 trái, 1 phải); số phần tử là số hàng x 2.")]
        public GlassTile[] tiles;
        [Tooltip("Các bảng sơ đồ ở hai đài (TextMesh); quay về phía camera.")]
        public TextMesh[] boards;
        public float laneHalfWidth = 2.8f;
        public float firstRowZ = -11.25f;
        public float rowSpacing = 4.5f;
        public float rowHalfDepth = 1.6f;
        [Tooltip("0 = đề ngẫu nhiên mỗi lần; khác 0 = đề cố định để thử.")]
        public int fixedSeed;

        public GlassBridgeState State { get; private set; }
        public bool Active { get; private set; }
        /// <summary>Tắt khi Canvas của SchoolFloor vẽ HUD thay cho hộp OnGUI.</summary>
        public bool drawOnGui = true;
        public string Status { get; private set; }

        GlassBridgeRound round;
        string diagramText = "";
        int Rows { get { return tiles != null ? tiles.Length / 2 : 0; } }

        /// <summary>Không nhận tham số; tạo đề mới theo số người và số hàng (nếu chưa có hoặc đổi) rồi bắt đầu.</summary>
        public void Begin()
        {
            int players = Mathf.Clamp(party.PartySize, 2, 4);
            if (round == null || round.PlayerCount != players || round.Rows != Rows)
            {
                int seed = fixedSeed != 0 ? fixedSeed : System.Environment.TickCount;
                round = GlassBridgeGenerator.Generate(players, seed, Rows);
                State = new GlassBridgeState(round);
            }
            else State.Reset();
            ResetTiles();
            BuildDiagram();
            Active = true;
            Status = "Một người đi, đồng đội ở đài xem sơ đồ và dẫn. Ô sai sẽ vỡ.";
        }

        /// <summary>Không nhận tham số; đặt lại cầu, đưa mọi người về đài đầu, giữ đề.</summary>
        public void Retry()
        {
            if (State == null) return;
            State.Reset();
            ResetTiles();
            for (int p = 0; p < party.PartySize && p < round.PlayerCount; p++) party.Teleport(p, StartPoint(p));
            Status = "Đã đặt lại cầu.";
        }

        /// <summary>Không nhận tham số; xử lý đổi vai, làm lại (R), theo dõi vị trí và di chuyển.</summary>
        void Update()
        {
            if (!Active || State == null) return;
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.tabKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame)
                    PrototypePartyController.SetCursor(Cursor.lockState != CursorLockMode.Locked);
                if (keyboard.digit1Key.wasPressedThisFrame) party.SwitchRole(0, false);
                if (keyboard.digit2Key.wasPressedThisFrame) party.SwitchRole(1, false);
                if (keyboard.digit3Key.wasPressedThisFrame) party.SwitchRole(2, false);
                if (keyboard.digit4Key.wasPressedThisFrame) party.SwitchRole(3, false);
            }
            if (Cursor.lockState != CursorLockMode.Locked) { RefreshBoards(); return; }
            if (keyboard != null && keyboard.rKey.wasPressedThisFrame) Retry();
            TrackPlayers();
            party.TickInput(false);
            RefreshBoards();
        }

        /// <summary>Không nhận tham số; đọc vị trí từng nhân vật đang hoạt động và báo cho luật.</summary>
        void TrackPlayers()
        {
            for (int p = 0; p < party.PartySize && p < round.PlayerCount; p++)
            {
                Transform body = party.players[p];
                if (!body.gameObject.activeInHierarchy) continue;
                int row, side;
                Place place = Locate(body.position, out row, out side);
                int current = State.RowOf(p);
                if (place == Place.Start)
                {
                    while (State.RowOf(p) >= 0 && State.RowOf(p) < State.EndPlatform)
                        if (!State.Retreat(p)) break;
                }
                else if (place == Place.End)
                {
                    if (current == State.EndPlatform - 1)
                    {
                        State.Disembark(p);
                        Status = State.Solved ? "Cả đội đã qua cầu!" : "Vai " + (p + 1) + " đã qua cầu.";
                    }
                }
                else if (place == Place.Tile)
                {
                    if (row == current + 1) HandleStep(p, row, side, State.Step(p, side));
                    else if (row < current)
                        while (State.RowOf(p) > row)
                            if (!State.Retreat(p)) break;
                }
            }
        }

        /// <summary>Nhận người, hàng, ô và kết quả bước; vỡ ô + đưa người về đài đầu nếu té, nháy ô nếu đi đúng.</summary>
        void HandleStep(int player, int row, int side, GlassBridgeResult result)
        {
            GlassTile tile = tiles[row * 2 + side];
            if (result == GlassBridgeResult.Fell)
            {
                tile.SetBroken(true);
                party.Teleport(player, StartPoint(player));
                Status = "Vai " + (player + 1) + " đạp vỡ ô " + (side == 0 ? "trái" : "phải") + " hàng " + (row + 1) + ", về đài đầu.";
            }
            else if (result == GlassBridgeResult.Advanced)
            {
                tile.Flash(new Color(.4f, 1f, .5f), .35f);
                Status = "Vai " + (player + 1) + " tới hàng " + (row + 1) + ".";
            }
        }

        /// <summary>Nhận tọa độ thế giới; trả loại chỗ đứng (đài đầu, đài cuối, ô kính, ngoài) và gán hàng/ô nếu là ô kính.</summary>
        Place Locate(Vector3 world, out int row, out int side)
        {
            row = -1; side = 0;
            if (InBox(startPlatform, world)) return Place.Start;
            if (InBox(endPlatform, world)) return Place.End;
            Vector3 local = laneOrigin.InverseTransformPoint(world);
            if (Mathf.Abs(local.x) > laneHalfWidth || local.y < -0.6f || local.y > 2.2f) return Place.None;
            int r = Mathf.RoundToInt((local.z - firstRowZ) / rowSpacing);
            if (r < 0 || r >= Rows) return Place.None;
            if (Mathf.Abs(local.z - (firstRowZ + r * rowSpacing)) > rowHalfDepth) return Place.None;
            row = r;
            side = local.x < 0 ? 0 : 1;
            return Place.Tile;
        }

        /// <summary>Nhận hộp và điểm thế giới; trả true nếu điểm nằm trong hộp (tính theo tọa độ local, hỗ trợ hộp xoay).</summary>
        static bool InBox(BoxCollider box, Vector3 world)
        {
            if (box == null) return false;
            return new Bounds(box.center, box.size).Contains(box.transform.InverseTransformPoint(world));
        }

        /// <summary>Nhận chỉ số người; trả điểm đứng trên đài đầu (trải đều theo chỉ số người).</summary>
        Vector3 StartPoint(int player)
        {
            float spread = (player - (party.PartySize - 1) * 0.5f) * 1.2f;
            Vector3 local = startPlatform.center + new Vector3(spread, -startPlatform.size.y * 0.5f + 0.1f, 0);
            return startPlatform.transform.TransformPoint(local);
        }

        /// <summary>Không nhận tham số; đưa mọi ô về nguyên vẹn.</summary>
        void ResetTiles()
        {
            if (tiles == null) return;
            foreach (GlassTile tile in tiles) if (tile != null) tile.SetBroken(false);
        }

        /// <summary>Không nhận tham số; dựng chữ sơ đồ ô an toàn cho bảng.</summary>
        void BuildDiagram()
        {
            var sb = new StringBuilder("SƠ ĐỒ Ô AN TOÀN\n(trái/phải theo người đi từ đài ĐẦU sang đài CUỐI)\n");
            for (int r = 0; r < round.Rows; r++)
                sb.Append("Hàng ").Append(r + 1).Append(": ").Append(round.SafeTile(r) == 0 ? "TRÁI" : "PHẢI").Append('\n');
            diagramText = sb.ToString();
        }

        /// <summary>Không nhận tham số; bảng hiện sơ đồ khi vai đang điều khiển đứng ở đài, ngược lại hiện dòng ẩn.</summary>
        void RefreshBoards()
        {
            if (boards == null || State == null) return;
            bool visible = State.CanSeeDiagram(Mathf.Min(party.ActivePlayer, round.PlayerCount - 1));
            string text = visible ? diagramText : "SƠ ĐỒ BỊ ẨN\nChỉ xem được khi đứng ở đài";
            foreach (TextMesh board in boards) if (board != null) board.text = text;
        }

        /// <summary>Không nhận tham số; xoay bảng sơ đồ quay mặt về camera người chơi.</summary>
        void LateUpdate()
        {
            if (!Active || boards == null || party == null || party.viewCamera == null) return;
            foreach (TextMesh board in boards)
            {
                if (board == null) continue;
                Vector3 look = party.viewCamera.transform.position - board.transform.position;
                look.y = 0;
                if (look.sqrMagnitude > .01f) board.transform.rotation = Quaternion.LookRotation(-look);
            }
        }

        /// <summary>Không nhận tham số; vẽ thông tin vai đang điều khiển, vị trí trên cầu và trạng thái.</summary>
/// <summary>Nhận chỉ số vai; trả sổ tay của vai: vị trí trên cầu và có xem được sơ đồ không (rỗng nếu chưa chạy).</summary>
        public string Notebook(int playerIndex)
        {
            if (!Active || State == null) return "";
            int player = Mathf.Min(playerIndex, round.PlayerCount - 1);
            int row = State.RowOf(player);
            string where = row < 0 ? "đài đầu" : (row >= round.Rows ? "đài cuối" : "hàng " + (row + 1) + "/" + round.Rows);
            return "Vai " + (player + 1) + " đang ở " + where + "\n" + (State.CanSeeDiagram(player) ? "Đang ở đài: xem được bảng sơ đồ" : "Đang trên cầu: không xem được sơ đồ") + "\n[R] làm lại  [1-4] đổi vai";
        }

        /// <summary>Không nhận tham số; vẽ hộp thông tin cũ khi drawOnGui bật.</summary>
        void OnGUI()
        {
            if (!drawOnGui || !Active || State == null) return;
            var sb = new StringBuilder();
            sb.Append("CẦU KÍNH\n");
            sb.Append("Đã qua: ").Append(State.AcrossCount).Append('/').Append(round.PlayerCount).Append("  Số lần té: ").Append(State.Falls).Append('\n');
            sb.Append(Notebook(party.ActivePlayer)).Append('\n').Append(Status);
            GUI.Box(new Rect(16, 16, 460, 130), sb.ToString());
        }

        /// <summary>Nhận cờ reset; ngừng nhiệm vụ; reset=false giữ kết quả đã giải khi chuyển nhiệm vụ.</summary>
        public void StopSession(bool reset = true)
        {
            Active = false;
            if (State != null && reset)
            {
                State.Reset();
                ResetTiles();
            }
        }
    }
}
