/*
 * Mục đích: Nối luật «Đèn lồng chung» (LanternState) với người chơi, biển, bảng nhập, đèn lồng và HUD (T-006f, Hanh_Lang_4).
 * Vùng sáng = bán kính quanh người cầm đèn (mặt phẳng ngang). Prototype đo vị trí rồi truyền cho Domain; Domain quyết định luật.
 * Hai chiều vào: từ cổng Vao (đầu z-) hoặc cổng Ra (đầu z+); Begin nhận biết đội đang ở đầu nào để chọn điểm đầu và vùng cổng cuối.
 * Hàm: Begin; IsEnteredFromEnd; Retry; Update (input, giờ ngoài vùng sáng, hoàn thành); TryReadSign; TryPanelKey; TryPassLantern; TickLight;
 * CheckCompletion; Near; Refresh; OnGUI; StopSession.
 */
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheReturn
{
    [DefaultExecutionOrder(50)]
    public sealed class SharedLanternPrototype : MonoBehaviour
    {
        public PrototypePartyController party;
        public SharedLanternSettings settings;
        [Tooltip("4 biển, theo thứ tự biển 1-4 trên tường.")]
        public LanternSign[] signs;
        [Tooltip("Các phím của bảng nhập: 6 phím ký hiệu, XÓA, XÁC NHẬN.")]
        public LanternPanelKey[] panelKeys;
        [Tooltip("4 chữ hiện các ô đã nhập.")]
        public TextMesh[] slotLabels;
        public TextMesh statusBoard;
        [Tooltip("Đèn lồng, bám theo người đang cầm.")]
        public Transform lanternVisual;
        public Light lanternLight;
        [Tooltip("Điểm đầu hành lang cho từng người (về đây khi ở ngoài vùng sáng quá lâu).")]
        public Transform[] checkpoints;
        [Tooltip("Vùng cổng cuối hành lang: cả đội ở trong thì hoàn thành (sau khi nhập đúng mã).")]
        public Collider goalZone;
        [Tooltip("Điểm đầu khi đội vào từ đầu CUỐI hành lang (cổng Ra, z+). Để trống thì dùng checkpoints.")]
        public Transform[] checkpointsFromEnd;
        [Tooltip("Vùng cổng khi vào từ đầu CUỐI: là đầu kia (z-) của hành lang.")]
        public Collider goalZoneFromEnd;
        [Tooltip("Bảng trạng thái ở đầu kia (dùng khi vào từ đầu cuối).")]
        public TextMesh statusBoardFromEnd;

        public LanternState State { get; private set; }
        public bool Active { get; private set; }
        /// <summary>Tắt khi Canvas của SchoolFloor vẽ HUD thay cho hộp OnGUI.</summary>
        public bool drawOnGui = true;
        public string Status { get; private set; }
        LanternRound round;
        bool reversed;

        /// <summary>Không nhận tham số; tạo đề theo số người hiện có (nếu chưa có hoặc đổi đội) và bắt đầu nhiệm vụ.</summary>
        public void Begin()
        {
            int players = Mathf.Clamp(party.PartySize, 2, 4);
            if (round == null || round.PlayerCount != players)
            {
                int seed = settings != null && settings.fixedSeed != 0 ? settings.fixedSeed : System.Environment.TickCount;
                round = LanternGenerator.Generate(players, seed);
                State = null;
            }
            if (State == null) State = NewState();
            else State.Reset();
            reversed = IsEnteredFromEnd();
            Active = true;
            Status = settings != null ? settings.instructions : string.Empty;
            Refresh(Time.timeAsDouble);
        }

        /// <summary>Không nhận tham số; trả true nếu đội đang ở nửa z+ của hành lang (vào từ cổng Ra), tính theo vị trí trung bình trong hệ cục bộ của nhiệm vụ.</summary>
        bool IsEnteredFromEnd()
        {
            if (party == null || party.players == null) return false;
            Vector3 sum = Vector3.zero;
            int count = 0;
            for (int i = 0; i < State.Round.PlayerCount && i < party.players.Length; i++) { sum += party.players[i].position; count++; }
            return count > 0 && transform.InverseTransformPoint(sum / count).z > 0f;
        }

        /// <summary>Không nhận tham số; tạo LanternState từ đề hiện có và thông số trong settings.</summary>
        LanternState NewState()
        {
            float outside = settings != null ? settings.outsideSeconds : 5f;
            float pass = settings != null ? settings.passRange : 2f;
            float locked = settings != null ? settings.submitLockSeconds : 10f;
            return new LanternState(round, outside, pass, locked);
        }

        /// <summary>Không nhận tham số; thử lại cùng đề: xóa sổ, bảng nhập, đưa mọi người về điểm đầu.</summary>
        public void Retry()
        {
            if (State == null) return;
            State.Reset();
            SendToStart(-1);
            Status = settings != null ? settings.instructions : string.Empty;
            Refresh(Time.timeAsDouble);
        }

        /// <summary>Không nhận tham số; xử lý đổi vai, tương tác E/F, giờ ngoài vùng sáng và hoàn thành.</summary>
        void Update()
        {
            if (!Active || State == null) return;
            var keyboard = Keyboard.current;
            double now = Time.timeAsDouble;
            if (keyboard != null)
            {
                if (keyboard.tabKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame)
                    PrototypePartyController.SetCursor(Cursor.lockState != CursorLockMode.Locked);
                if (keyboard.digit1Key.wasPressedThisFrame) party.SwitchRole(0, false);
                if (keyboard.digit2Key.wasPressedThisFrame) party.SwitchRole(1, false);
                if (keyboard.digit3Key.wasPressedThisFrame) party.SwitchRole(2, false);
                if (keyboard.digit4Key.wasPressedThisFrame) party.SwitchRole(3, false);
            }
            if (Cursor.lockState != CursorLockMode.Locked) { Refresh(now); return; }
            if (keyboard != null && keyboard.rKey.wasPressedThisFrame) Retry();
            RaycastHit hit;
            if (keyboard != null && keyboard.eKey.wasPressedThisFrame && party.TryGetInteractionHit(3f, out hit))
            {
                var sign = hit.collider.GetComponentInParent<LanternSign>();
                var key = hit.collider.GetComponentInParent<LanternPanelKey>();
                if (sign != null) TryReadSign(sign.index);
                else if (key != null) TryPanelKey(key, now);
            }
            if (keyboard != null && keyboard.fKey.wasPressedThisFrame) TryPassLantern();
            TickLight(now);
            CheckCompletion();
            party.TickInput(false);
            Refresh(now);
        }

        /// <summary>Nhận chỉ số biển; người đang điều khiển đọc biển nếu biển trong vùng sáng, hiển thị kết quả. Trả kết quả Domain.</summary>
        public LanternReadResult TryReadSign(int sign)
        {
            if (!Active || State == null || signs == null || sign < 0 || sign >= signs.Length || signs[sign] == null) return LanternReadResult.InvalidSign;
            var result = State.Read(party.ActivePlayer, sign, Near(signs[sign].transform.position));
            switch (result)
            {
                case LanternReadResult.Read: Status = (settings != null ? settings.readOk : "Đã ghi.") + " (biển " + (sign + 1) + ")"; break;
                case LanternReadResult.NotInLight: Status = settings != null ? settings.readNotInLight : "Ngoài vùng sáng."; break;
                case LanternReadResult.AlreadyRead: Status = settings != null ? settings.readAlready : "Đã đọc rồi."; break;
            }
            return result;
        }

        /// <summary>Nhận phím bảng và thời gian giây; ký hiệu điền vào ô trống đầu tiên, XÓA xóa ô, XÁC NHẬN kiểm mã (sai bị phạt khóa).</summary>
        void TryPanelKey(LanternPanelKey key, double now)
        {
            if (key.kind == LanternPanelKey.Kind.Symbol)
            {
                int slot = -1;
                for (int i = 0; i < LanternRound.SignCount; i++) if (State.Entry(i) < 0) { slot = i; break; }
                if (slot < 0) { Status = "Đủ 4 ô - bấm XÓA hoặc XÁC NHẬN."; return; }
                State.SetEntry(slot, key.symbol);
            }
            else if (key.kind == LanternPanelKey.Kind.Clear) State.ClearEntries();
            else
            {
                switch (State.Submit(now))
                {
                    case LanternSubmitResult.Accepted: Status = settings != null ? settings.accepted : "Đúng mã."; break;
                    case LanternSubmitResult.WrongCode: Status = settings != null ? settings.wrongCode : "Sai mã."; break;
                    case LanternSubmitResult.Locked: Status = (settings != null ? settings.locked : "Đang khóa.") + " Còn " + State.LockRemaining(now).ToString("0.0") + " giây."; break;
                    case LanternSubmitResult.Incomplete: Status = settings != null ? settings.incomplete : "Chưa đủ 4 ký hiệu."; break;
                }
            }
        }

        /// <summary>Không nhận tham số; người đang điều khiển (phải đang cầm đèn) trao đèn cho người GẦN NHẤT trong tầm trao.</summary>
        void TryPassLantern()
        {
            int from = party.ActivePlayer;
            if (from != State.Holder) { Status = settings != null ? settings.passNotHolder : "Không cầm đèn."; return; }
            int best = -1; float bestDistance = float.MaxValue;
            for (int i = 0; i < State.Round.PlayerCount && i < party.players.Length; i++)
            {
                if (i == from) continue;
                float d = Vector3.Distance(party.players[i].position, party.players[from].position);
                if (d < bestDistance) { bestDistance = d; best = i; }
            }
            if (best < 0) return;
            var result = State.TryPass(from, best, bestDistance);
            if (result == LanternPassResult.Passed) Status = (settings != null ? settings.passed : "Đã trao đèn.") + " (vai " + (best + 1) + ")";
            else if (result == LanternPassResult.TooFar) Status = settings != null ? settings.passTooFar : "Quá xa.";
        }

        /// <summary>Nhận thời gian giây; đo ai đang trong vùng sáng, đưa Tick cho Domain và đưa người bị phạt về điểm đầu.</summary>
        void TickLight(double now)
        {
            int count = State.Round.PlayerCount;
            var inLight = new bool[count];
            for (int i = 0; i < count; i++)
            {
                bool skip = settings != null && settings.onlyActivePlayerTimed && i != party.ActivePlayer;
                inLight[i] = skip || (i < party.players.Length && Near(party.players[i].position));
            }
            int sentBack = State.Tick(now, inLight);
            if (sentBack == 0) return;
            for (int i = 0; i < count; i++) if ((sentBack >> i & 1) == 1) SendToStart(i);
            Status = settings != null ? settings.sentBack : "Bị đưa về điểm đầu.";
        }

        /// <summary>Nhận chỉ số người (-1 = cả đội); dịch chuyển về điểm đầu hành lang. Bỏ qua nếu thiếu checkpoint.</summary>
        void SendToStart(int player)
        {
            Transform[] points = reversed && checkpointsFromEnd != null && checkpointsFromEnd.Length > 0 ? checkpointsFromEnd : checkpoints;
            if (points == null) return;
            for (int i = 0; i < State.Round.PlayerCount && i < points.Length; i++)
                if ((player < 0 || i == player) && points[i] != null) party.Teleport(i, points[i]);
        }

        /// <summary>Không nhận tham số; sau khi nhập đúng mã, kiểm cả đội đã trong vùng cổng cuối chưa để hoàn thành.</summary>
        void CheckCompletion()
        {
            Collider zone = reversed && goalZoneFromEnd != null ? goalZoneFromEnd : goalZone;
            if (!State.CodeAccepted || State.Solved || zone == null) return;
            bool all = true;
            for (int i = 0; i < State.Round.PlayerCount && i < party.players.Length; i++)
            {
                if (!InsideZone(zone, party.players[i].position)) { all = false; break; }
            }
            if (State.TryComplete(all)) Status = settings != null ? settings.success : "Xong.";
        }

        /// <summary>Nhận vùng và vị trí thế giới; true nếu vị trí nằm trong vùng trên mặt phẳng ngang. BoxCollider được kiểm trong hệ cục bộ để đúng khi vùng bị xoay; collider khác dùng bounds.</summary>
        static bool InsideZone(Collider zone, Vector3 position)
        {
            var box = zone as BoxCollider;
            if (box != null)
            {
                Vector3 local = box.transform.InverseTransformPoint(position) - box.center;
                Vector3 half = box.size * 0.5f;
                return Mathf.Abs(local.x) <= half.x && Mathf.Abs(local.z) <= half.z;
            }
            return zone.bounds.Contains(new Vector3(position.x, zone.bounds.center.y, position.z));
        }

        /// <summary>Nhận vị trí thế giới; trả true nếu cách người cầm đèn không quá bán kính vùng sáng (mặt phẳng ngang).</summary>
        bool Near(Vector3 position)
        {
            if (State == null || party == null || State.Holder >= party.players.Length) return false;
            Vector3 h = party.players[State.Holder].position;
            float dx = position.x - h.x, dz = position.z - h.z;
            float radius = settings != null ? settings.lightRadius : 4f;
            return dx * dx + dz * dz <= radius * radius;
        }

        /// <summary>Nhận thời gian giây; cập nhật biển, ô nhập, bảng trạng thái, vị trí và độ sáng đèn lồng.</summary>
        void Refresh(double now)
        {
            if (State == null) return;
            for (int i = 0; signs != null && i < signs.Length; i++)
                if (signs[i] != null) signs[i].Show(LanternSymbols.Name(State.Round.SymbolOfSign(i)), Near(signs[i].transform.position));
            for (int i = 0; slotLabels != null && i < slotLabels.Length; i++)
                if (slotLabels[i] != null) slotLabels[i].text = State.Entry(i % LanternRound.SignCount) >= 0 ? LanternSymbols.Name(State.Entry(i % LanternRound.SignCount)) : "_";
            if (statusBoard != null || statusBoardFromEnd != null)
            {
                double left = State.LockRemaining(now);
                string board = State.Solved ? "XONG" : (left > 0 ? "KHÓA " + left.ToString("0") + "s" : (State.CodeAccepted ? "RA CỔNG" : "NHẬP MÃ"));
                if (statusBoard != null) statusBoard.text = board;
                if (statusBoardFromEnd != null) statusBoardFromEnd.text = board;
            }
            if (lanternVisual != null && State.Holder < party.players.Length)
                lanternVisual.position = party.players[State.Holder].position + Vector3.up * 1.1f;
            if (lanternLight != null)
            {
                lanternLight.enabled = Active;
                lanternLight.range = (settings != null ? settings.lightRadius : 4f) * 1.5f;
            }
        }

        /// <summary>Không nhận tham số; vẽ HUD: người cầm đèn, sổ riêng của vai đang điều khiển, giờ ngoài vùng sáng, khóa bảng, trạng thái.</summary>
/// <summary>Nhận chỉ số vai và thời điểm; trả sổ riêng của vai (biển đã đọc), giờ ngoài vùng sáng, khóa bảng và phím. Rỗng nếu chưa chạy.</summary>
        public string Notebook(int player, double now)
        {
            if (!Active || State == null) return "";
            var sb = new System.Text.StringBuilder();
            sb.Append("Vai ").Append(player + 1).Append(State.Holder == player ? "  (đang cầm đèn)" : "  (đèn: vai " + (State.Holder + 1) + ")").Append('\n');
            for (int s = 0; s < LanternRound.SignCount; s++)
                sb.Append("Sổ - biển ").Append(s + 1).Append(": ").Append(State.HasRead(player, s) ? LanternSymbols.Name(State.SymbolSeen(player, s)) : "chưa đọc").Append('\n');
            double outside = State.OutsideSeconds(player, now);
            if (outside > 0) sb.Append("Ngoài vùng sáng: ").Append(outside.ToString("0.0")).Append("/").Append(State.OutsideLimit.ToString("0")).Append(" giây\n");
            double lockLeft = State.LockRemaining(now);
            if (lockLeft > 0) sb.Append("Bảng khóa còn ").Append(lockLeft.ToString("0.0")).Append(" giây\n");
            sb.Append("[E] đọc biển / bấm phím  [F] trao đèn  [R] làm lại  [1-4] đổi vai");
            return sb.ToString();
        }

        /// <summary>Không nhận tham số; vẽ hộp HUD cũ khi drawOnGui bật.</summary>
        void OnGUI()
        {
            if (!drawOnGui || !Active || State == null) return;
            GUI.Box(new Rect(16, 16, 460, 190), "ĐÈN LỒNG CHUNG\n" + Notebook(party.ActivePlayer, Time.timeAsDouble) + "\n" + Status);
        }

        /// <summary>Nhận cờ reset; ngừng input, tắt đèn; reset=false giữ kết quả đã giải khi chuyển nhiệm vụ.</summary>
        public void StopSession(bool reset = true)
        {
            Active = false;
            if (lanternLight != null) lanternLight.enabled = false;
            if (State != null && reset) State.Reset();
        }
    }
}
