/*
 * Mục đích: Nối luật «Cầu ván và thẻ từ» (PlankSession) với người chơi, tấm ván, thẻ từ và hai cần gạt ở Hanh_Lang_6 (T-006b2).
 * Ván chỉ chở tối đa 2 người và chỉ hạ khi thẻ từ nằm trên ván. Người chơi đứng lên ván rồi gạt cần (E): ván chạy sang bờ bên kia
 * trong thời gian của người chậm hơn, người trên ván được đưa đi theo ván. Hết hạn thì cả đội về đầu khe. HUD chỉ cho vai ĐANG ĐIỀU KHIỂN
 * biết tốc độ cặp sách của chính mình (thông tin riêng, phải hỏi đồng đội).
 * Hệ tọa độ (laneOrigin): z+ từ bờ A sang bờ B, x+ là bên PHẢI người đi, y=0 là mặt sàn. «Bờ đầu khe» là bờ nơi đội đứng lúc bắt đầu.
 * Hàm: Begin bắt đầu; Retry đặt lại; StopSession ngừng; PickUpCard, DropCard, UseLever (hành động người chơi, gọi được từ test);
 * Update xử lý phím và thời gian; AdvanceCrossing, OnArrived, BeginCrossingVisual (ván chạy); PlaceAttempt, StandPoint, DropPoint, IsNearSideA,
 * InBox, ReferencesOk (nội bộ); LateUpdate thẻ theo người cầm và xoay nhãn; OnGUI vẽ HUD.
 */
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheReturn
{
    [DefaultExecutionOrder(50)]
    public sealed class PlankAndKeyPrototype : MonoBehaviour
    {
        public PrototypePartyController party;
        [Tooltip("Gốc hành lang: z+ từ bờ A sang bờ B, x+ là bên PHẢI người đi theo z+, y=0 là mặt sàn.")]
        public Transform laneOrigin;
        [Tooltip("Tấm ván (di chuyển giữa hai điểm đậu).")]
        public Transform deck;
        [Tooltip("Hộp (trigger) trên ván: ai đứng trong hộp này là đang trên ván.")]
        public BoxCollider deckZone;
        public Transform deckRestA;
        public Transform deckRestB;
        [Tooltip("Điểm đứng chuẩn của đội ở mỗi bờ (dùng khi về đầu khe).")]
        public Transform anchorA;
        public Transform anchorB;
        public Transform card;
        public Transform cardHomeA;
        public Transform cardHomeB;
        [Tooltip("Các nhãn chữ (TextMesh) quay về phía camera.")]
        public TextMesh[] labels;
        public PlankSettings settings;
        [Tooltip("0 = đề ngẫu nhiên mỗi lần; khác 0 = đề cố định để thử.")]
        public int fixedSeed;
        public float interactDistance = 3.2f;
        public float failResetSeconds = 2.5f;
        public float hintSeconds = 14f;

        public PlankSession Session { get; private set; }
        public bool Active { get; private set; }
        public string Status { get; private set; }

        bool nearIsA = true;
        bool deckAtNear = true;
        int cardHolder = -1;
        bool cardOnDeck;
        Vector3 cardDeckLocal;
        Vector3 deckFrom;
        Vector3 deckTo;
        int[] riders = new int[0];
        Vector3[] riderLocal = new Vector3[0];
        float failTimer;
        float hintTimer;
        bool warnedMissing;

        /// <summary>Không nhận tham số; tạo đề mới theo số người và thiết lập rồi bắt đầu nhiệm vụ ở bờ nơi đội đang đứng.</summary>
        public void Begin()
        {
            if (!ReferencesOk())
            {
                if (!warnedMissing) Debug.LogError("PlankAndKeyPrototype: thiếu tham chiếu trong scene (party, laneOrigin, deck, deckZone, deckRest, anchor, card, cardHome).", this);
                warnedMissing = true;
                Active = false;
                Status = "Thiếu tham chiếu trong scene.";
                return;
            }
            int players = Mathf.Clamp(party.PartySize, PlankRound.MinPlayers, PlankRound.MaxPlayers);
            float perUnit = settings != null ? settings.secondsPerUnit : 3f;
            int slack = settings != null ? settings.slackPercent : PlankGenerator.DefaultSlackPercent;
            int seed = fixedSeed != 0 ? fixedSeed : System.Environment.TickCount;
            Session = new PlankSession(PlankGenerator.Generate(players, seed, slack), perUnit);
            nearIsA = IsNearSideA();
            PlaceAttempt(false);
            Active = true;
            hintTimer = hintSeconds;
            Status = "Đặt thẻ từ lên ván, đứng lên ván (tối đa 2 người) rồi gạt cần để qua khe.";
        }

        /// <summary>Không nhận tham số; làm lại cùng đề: cả đội, ván và thẻ về đầu khe, đồng hồ về 0.</summary>
        public void Retry()
        {
            if (Session == null || !ReferencesOk()) return;
            Session.Reset();
            PlaceAttempt(true);
            Status = "Đã đặt lại: cả đội về đầu khe.";
        }

        /// <summary>Nhận cờ reset; ngừng nhiệm vụ. reset=false giữ kết quả đã giải khi chuyển nhiệm vụ.</summary>
        public void StopSession(bool reset = true)
        {
            Active = false;
            failTimer = 0f;
            riders = new int[0];
            if (Session != null && reset && ReferencesOk())
            {
                Session.Reset();
                PlaceAttempt(false);
            }
        }

        /// <summary>Nhận chỉ số người; thử nhặt thẻ từ đang nằm trên sàn. Trả true nếu nhặt được.</summary>
        public bool PickUpCard(int player)
        {
            if (!Active || Session == null) return false;
            if (cardHolder >= 0) { Status = "Vai " + (cardHolder + 1) + " đang cầm thẻ từ."; return false; }
            if (cardOnDeck) { Status = "Thẻ đang trên ván, không nhặt được lúc ván chạy."; return false; }
            if (Vector3.Distance(party.players[player].position, card.position) > interactDistance + 0.8f) { Status = "Đứng gần thẻ hơn."; return false; }
            cardHolder = player;
            SetCardCollider(false);
            Status = "Vai " + (player + 1) + " đã cầm thẻ từ.";
            return true;
        }

        /// <summary>Nhận chỉ số người; thả thẻ từ xuống sàn hoặc lên ván nếu người đó đang cầm. Trả true nếu thả được.</summary>
        public bool DropCard(int player)
        {
            if (!Active || Session == null || cardHolder != player) return false;
            if (Session.Phase == PlankPhase.Crossing) { Status = "Không thả thẻ khi ván đang chạy."; return false; }
            card.position = DropPoint(player);
            cardHolder = -1;
            SetCardCollider(true);
            Status = InBox(deckZone, card.position) ? "Thẻ từ đã nằm trên ván." : "Vai " + (player + 1) + " đã thả thẻ từ.";
            return true;
        }

        /// <summary>
        /// Nhận bờ của cần gạt (true = bờ A); gạt cần: nếu ván đang đậu ở bờ đó, có 1-2 người và thẻ từ trên ván thì cho ván chạy.
        /// Trả true nếu ván bắt đầu chạy.
        /// </summary>
        public bool UseLever(bool sideA)
        {
            if (!Active || Session == null) return false;
            bool leverNear = sideA == nearIsA;
            if (leverNear != deckAtNear) { Status = "Ván đang ở bờ bên kia. Gạt cần ở bờ nơi ván đang đậu."; return false; }
            if (Session.Phase == PlankPhase.Crossing) { Status = "Ván đang chạy."; return false; }
            var on = new List<int>();
            int n = Mathf.Min(party.PartySize, Session.State.Round.PlayerCount);
            for (int p = 0; p < n; p++)
            {
                Transform body = party.players[p];
                if (body.gameObject.activeInHierarchy && InBox(deckZone, body.position)) on.Add(p);
            }
            if (on.Count == 0) { Status = "Cần có người đứng trên ván."; return false; }
            if (on.Count > 2) { Status = "Ván chỉ chở tối đa 2 người (đang có " + on.Count + " người)."; return false; }
            bool cardAboard = cardHolder >= 0 ? on.Contains(cardHolder) : InBox(deckZone, card.position);
            if (!cardAboard) { Status = "Ván chỉ hạ xuống khi thẻ từ nằm trên ván: người trên ván cầm thẻ hoặc đặt thẻ lên ván."; return false; }
            PlankStartResult r = on.Count == 1 ? Session.TryStartCrossing(on[0]) : Session.TryStartCrossing(on[0], on[1]);
            switch (r)
            {
                case PlankStartResult.Started:
                    BeginCrossingVisual(on);
                    Status = "Ván chạy sang " + (deckAtNear ? "bờ bên kia" : "bờ đầu khe") + " trong " + Session.CrossingSecondsTotal.ToString("0") + " giây.";
                    return true;
                case PlankStartResult.TimedOut:
                    Status = settings != null ? settings.failure : "Hết thời gian! Cả đội về đầu khe.";
                    failTimer = failResetSeconds;
                    return false;
                case PlankStartResult.Busy:
                    Status = "Ván đang chạy.";
                    return false;
                case PlankStartResult.Finished:
                    Status = "Nhiệm vụ đã xong hoặc đã thất bại; bấm R để làm lại.";
                    return false;
                default:
                    Status = "Không thể qua lúc này (" + r + ").";
                    return false;
            }
        }

        /// <summary>Không nhận tham số; xử lý phím (đổi vai, E, G, R), thời gian của ván và di chuyển của người chơi.</summary>
        void Update()
        {
            if (!Active || Session == null) return;
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
            if (Cursor.lockState != CursorLockMode.Locked) return;
            float dt = Time.deltaTime;
            if (hintTimer > 0f) hintTimer -= dt;
            if (failTimer > 0f)
            {
                failTimer -= dt;
                if (failTimer <= 0f) Retry();
                party.TickInput(true);
                return;
            }
            if (keyboard != null)
            {
                if (keyboard.rKey.wasPressedThisFrame) { Retry(); return; }
                if (keyboard.eKey.wasPressedThisFrame) Interact();
                if (keyboard.gKey.wasPressedThisFrame) DropCard(party.ActivePlayer);
            }
            bool seated = false;
            if (Session.Phase == PlankPhase.Crossing)
            {
                AdvanceCrossing(dt);
                seated = IsRider(party.ActivePlayer);
            }
            party.TickInput(seated);
        }

        /// <summary>Không nhận tham số; tìm vật đang nhìn tới và thực hiện hành động tương ứng.</summary>
        void Interact()
        {
            RaycastHit hit;
            if (!party.TryGetInteractionHit(interactDistance, out hit)) return;
            PlankInteractable item = hit.collider.GetComponentInParent<PlankInteractable>();
            if (item == null) return;
            if (item.kind == PlankInteractKind.Card) PickUpCard(party.ActivePlayer);
            else UseLever(item.kind == PlankInteractKind.LeverA);
        }

        /// <summary>Nhận danh sách người trên ván; ghi vị trí gốc của ván, điểm đến và vị trí của từng người so với ván.</summary>
        void BeginCrossingVisual(List<int> on)
        {
            deckFrom = deck.position;
            deckTo = RestOf(!deckAtNear).position;
            riders = on.ToArray();
            riderLocal = new Vector3[riders.Length];
            for (int i = 0; i < riders.Length; i++) riderLocal[i] = deck.InverseTransformPoint(party.players[riders[i]].position);
            cardOnDeck = cardHolder < 0;
            if (cardOnDeck) cardDeckLocal = deck.InverseTransformPoint(card.position);
        }

        /// <summary>Nhận số giây trôi qua; đẩy thời gian của lượt, đặt ván và người trên ván theo tiến độ, xử lý khi đến nơi.</summary>
        void AdvanceCrossing(float dt)
        {
            bool arrived = Session.Tick(dt);
            float total = Session.CrossingSecondsTotal;
            float progress = arrived || total <= 0f ? 1f : Mathf.Clamp01(1f - Session.CrossingSecondsLeft / total);
            deck.position = Vector3.Lerp(deckFrom, deckTo, progress);
            for (int i = 0; i < riders.Length; i++) party.Teleport(riders[i], deck.TransformPoint(riderLocal[i]), false);
            if (cardOnDeck) card.position = deck.TransformPoint(cardDeckLocal);
            if (arrived) OnArrived();
        }

        /// <summary>Không nhận tham số; ván đã tới bờ đích: thả người, cập nhật bờ ván và thông báo.</summary>
        void OnArrived()
        {
            deckAtNear = !deckAtNear;
            riders = new int[0];
            cardOnDeck = false;
            if (Session.Solved) Status = settings != null ? settings.success : "Cả đội đã qua khe!";
            else Status = "Ván đã tới " + (deckAtNear ? "bờ đầu khe" : "bờ bên kia") + ".  Còn " + Session.DisplayRemainingSeconds.ToString("0") + " giây.";
        }

        /// <summary>Nhận chỉ số người; trả true nếu người đó đang ngồi trên ván chạy.</summary>
        bool IsRider(int player)
        {
            for (int i = 0; i < riders.Length; i++) if (riders[i] == player) return true;
            return false;
        }

        /// <summary>Nhận cờ đưa người; đặt ván, thẻ về đầu khe và (nếu cần) đưa cả đội về điểm đứng chuẩn ở bờ đầu khe.</summary>
        void PlaceAttempt(bool teleportParty)
        {
            deckAtNear = true;
            Transform rest = RestOf(true);
            deck.position = rest.position;
            deck.rotation = rest.rotation;
            riders = new int[0];
            cardHolder = -1;
            cardOnDeck = false;
            card.position = HomeOf(true).position;
            SetCardCollider(true);
            failTimer = 0f;
            if (!teleportParty) return;
            int n = Mathf.Min(party.PartySize, Session.State.Round.PlayerCount);
            Vector3 toward = RestOf(false).position - rest.position;
            toward.y = 0f;
            float yaw = Mathf.Atan2(toward.x, toward.z) * Mathf.Rad2Deg;
            for (int p = 0; p < n; p++)
            {
                party.Teleport(p, StandPoint(p, n));
                party.players[p].rotation = Quaternion.Euler(0f, yaw, 0f);
            }
        }

        /// <summary>Nhận chỉ số người và số người; trả điểm đứng ở bờ đầu khe (trải đều theo chiều ngang).</summary>
        Vector3 StandPoint(int player, int count)
        {
            Transform a = AnchorOf(true);
            float spread = (player - (count - 1) * 0.5f) * 1.2f;
            return a.position + a.right * spread;
        }

        /// <summary>Nhận chỉ số người đang cầm thẻ; trả điểm đặt thẻ (trước mặt, tránh vùng khe; nằm trên ván nếu đứng trên ván).</summary>
        Vector3 DropPoint(int player)
        {
            Transform body = party.players[player];
            Vector3 feet = body.position;
            Vector3 ahead = feet + body.forward * 0.7f;
            Vector3 local = laneOrigin.InverseTransformPoint(ahead);
            bool aheadOk = InBox(deckZone, ahead) || Mathf.Abs(local.z) > 4.4f;
            Vector3 pick = aheadOk ? ahead : feet;
            float y = InBox(deckZone, pick) ? deck.position.y + 0.3f : laneOrigin.position.y + 0.12f;
            return new Vector3(pick.x, y, pick.z);
        }

        /// <summary>Không nhận tham số; trả true nếu người điều khiển đang đứng ở nửa z nhỏ hơn (bờ A) của hành lang.</summary>
        bool IsNearSideA()
        {
            Transform body = party.players[Mathf.Clamp(party.ActivePlayer, 0, party.players.Length - 1)];
            return laneOrigin.InverseTransformPoint(body.position).z <= 0f;
        }

        Transform RestOf(bool near) { return near == nearIsA ? deckRestA : deckRestB; }
        Transform AnchorOf(bool near) { return near == nearIsA ? anchorA : anchorB; }
        Transform HomeOf(bool near) { return near == nearIsA ? cardHomeA : cardHomeB; }

        /// <summary>Nhận cờ bật; bật/tắt va chạm của thẻ (tắt khi đang được cầm để không cản người cầm).</summary>
        void SetCardCollider(bool enabled)
        {
            Collider c = card != null ? card.GetComponent<Collider>() : null;
            if (c != null) c.enabled = enabled;
        }

        /// <summary>Không nhận tham số; trả true nếu mọi tham chiếu bắt buộc đã được nối.</summary>
        bool ReferencesOk()
        {
            return party != null && party.players != null && party.players.Length > 0 && laneOrigin != null && deck != null && deckZone != null
                && deckRestA != null && deckRestB != null && anchorA != null && anchorB != null && card != null && cardHomeA != null && cardHomeB != null;
        }

        /// <summary>Nhận hộp và điểm thế giới; trả true nếu điểm nằm trong hộp (tính theo tọa độ local của hộp).</summary>
        static bool InBox(BoxCollider box, Vector3 world)
        {
            if (box == null) return false;
            return new Bounds(box.center, box.size).Contains(box.transform.InverseTransformPoint(world));
        }

        /// <summary>Không nhận tham số; thẻ bay theo người cầm (hoặc theo ván), nhãn quay về camera người chơi.</summary>
        void LateUpdate()
        {
            if (!Active || party == null || !ReferencesOk()) return;
            if (cardHolder >= 0 && cardHolder < party.players.Length)
            {
                Transform b = party.players[cardHolder];
                card.position = b.position + Vector3.up * 1.15f + b.right * 0.35f + b.forward * 0.45f;
                card.rotation = Quaternion.Euler(0f, b.eulerAngles.y, 0f);
            }
            if (labels == null || party.viewCamera == null) return;
            foreach (TextMesh label in labels)
            {
                if (label == null) continue;
                Vector3 look = party.viewCamera.transform.position - label.transform.position;
                look.y = 0f;
                if (look.sqrMagnitude > .01f) label.transform.rotation = Quaternion.LookRotation(-look);
            }
        }

        /// <summary>Không nhận tham số; mô tả chỗ thẻ từ đang ở (người cầm, trên ván, hoặc bờ nào).</summary>
        string CardText()
        {
            if (cardHolder >= 0) return "vai " + (cardHolder + 1) + " đang cầm";
            if (InBox(deckZone, card.position)) return "đang nằm trên ván";
            bool onSideA = laneOrigin.InverseTransformPoint(card.position).z < 0f;
            return "đang nằm ở " + (onSideA == nearIsA ? "bờ đầu khe" : "bờ bên kia");
        }

        /// <summary>Không nhận tham số; vẽ đồng hồ, tốc độ riêng của vai đang điều khiển, thẻ, người trên ván, trạng thái và phím.</summary>
        void OnGUI()
        {
            if (!Active || Session == null || !ReferencesOk()) return;
            PlankRound round = Session.State.Round;
            int me = Mathf.Min(party.ActivePlayer, round.PlayerCount - 1);
            var sb = new StringBuilder();
            sb.Append(settings != null ? settings.title : "CẦU VÁN VÀ THẺ TỪ").Append("  (lần ").Append(Session.State.Attempts).Append(")\n");
            sb.Append("Thời gian qua khe: ").Append(Session.DisplayUsedSeconds.ToString("0.0")).Append(" / ").Append(Session.DeadlineSeconds.ToString("0"))
              .Append(" giây (còn ").Append(Session.DisplayRemainingSeconds.ToString("0.0")).Append(")\n");
            sb.Append("Vai ").Append(me + 1).Append(": cặp sách của bạn mất ").Append((round.CrossUnits(me) * Session.SecondsPerUnit).ToString("0"))
              .Append(" giây để qua (đi đôi thì tính người chậm hơn). Chỉ bạn biết, hãy hỏi đồng đội.\n");
            sb.Append("Thẻ từ: ").Append(CardText()).Append('\n');
            sb.Append(Status).Append("\n[E] nhặt thẻ / gạt cần  [G] thả thẻ  [R] làm lại  [1-4] đổi vai");
            GUI.Box(new Rect(16, 16, 640, 150), sb.ToString());
            if (hintTimer > 0f && settings != null) GUI.Box(new Rect(16, 176, 640, 90), settings.instructions);
        }
    }
}
