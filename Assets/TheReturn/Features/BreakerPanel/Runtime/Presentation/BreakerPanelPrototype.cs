/*
 * Mục đích: Nối luật Bảng cầu dao (BreakerState) với người chơi, công tắc, đèn và HUD (T-006a2).
 * Chỉ chủ công tắc bấm được và xem được công tắc đó đảo những đèn nào (HUD chỉ hiện công tắc của vai đang điều khiển).
 * Hàm: Begin bắt đầu; Retry đặt lại công tắc; Update xử lý input và giờ; TryToggle bấm công tắc;
 * RefreshParts cập nhật công tắc/đèn; OnGUI vẽ HUD; StopSession ngừng nhiệm vụ.
 */
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheReturn
{
    [DefaultExecutionOrder(50)]
    public sealed class BreakerPanelPrototype : MonoBehaviour
    {
        public PrototypePartyController party;
        public BreakerSettings settings;
        [Tooltip("Tối đa 6 công tắc (số người + 2); chỉ dùng phần đầu theo số người.")]
        public BreakerSwitch[] switches;
        [Tooltip("Tối đa 6 đèn; chỉ dùng phần đầu theo số người.")]
        public BreakerLamp[] lamps;
        [Tooltip("Vẽ HUD OnGUI dự phòng; Canvas (SchoolFloorCanvasPresenter) tự tắt vì đã hiện sổ tay trong UI.")]
        public bool drawOnGui = true;
        public BreakerState State { get; private set; }
        public bool Active { get; private set; }
        public string Status { get; private set; }
        public int TargetSwitch { get; private set; } = -1;
        BreakerRound round;

        /// <summary>Không nhận tham số; tạo đề theo số người hiện có (nếu chưa có hoặc đổi đội) và bắt đầu.</summary>
        public void Begin()
        {
            int players = Mathf.Clamp(party.PartySize, 2, 4);
            if (round == null || round.PlayerCount != players)
            {
                int seed = settings != null && settings.fixedSeed != 0 ? settings.fixedSeed : System.Environment.TickCount;
                round = BreakerGenerator.Generate(players, seed);
                State = null;
            }
            if (State == null) State = new BreakerState(round, settings != null ? settings.holdSeconds : 3.0);
            else State.Reset();
            Active = true;
            Status = settings != null ? settings.instructions : "";
            RefreshParts();
        }

        /// <summary>Không nhận tham số; tắt mọi công tắc, giữ đề.</summary>
        public void Retry()
        {
            if (State == null) return;
            State.Reset();
            Status = "Đã đặt lại bảng cầu dao.";
            RefreshParts();
        }

        /// <summary>Không nhận tham số; xử lý đổi vai, bấm công tắc (E) và giữ khớp đích.</summary>
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
            if (Cursor.lockState != CursorLockMode.Locked) return;
            if (keyboard != null && keyboard.rKey.wasPressedThisFrame) Retry();
            TargetSwitch = -1;
            RaycastHit hit;
            if (party.TryGetInteractionHit(3f, out hit))
            {
                var sw = hit.collider.GetComponentInParent<BreakerSwitch>();
                if (sw != null) TargetSwitch = sw.index;
            }
            if (keyboard != null && keyboard.eKey.wasPressedThisFrame && TargetSwitch >= 0) TryToggle(TargetSwitch, party.ActivePlayer);
            if (State.Tick(Time.timeAsDouble) && settings != null) Status = settings.success;
            party.TickInput(false);
            RefreshParts();
        }

        /// <summary>Nhận công tắc và người; bấm công tắc, đặt thông báo, trả kết quả của Domain.</summary>
        public BreakerResult TryToggle(int sw, int player)
        {
            if (!Active || State == null) return BreakerResult.InvalidSwitch;
            var result = State.Toggle(player, sw);
            if (result == BreakerResult.Tripped) Status = settings != null ? settings.tripped : "Cầu dao nhảy!";
            else if (result == BreakerResult.NotOwner) Status = settings != null ? settings.notOwner : "Không phải công tắc của bạn.";
            else if (result == BreakerResult.Toggled) Status = "Quá tải: " + State.OnCount + "/" + round.OverloadLimit + " công tắc đang bật.";
            RefreshParts();
            return result;
        }

        /// <summary>Không nhận tham số; cập nhật công tắc và đèn theo trạng thái.</summary>
        void RefreshParts()
        {
            if (State == null) return;
            for (int i = 0; switches != null && i < switches.Length; i++)
            {
                if (switches[i] == null) continue;
                bool used = i < round.Size;
                switches[i].gameObject.SetActive(used);
                if (used) switches[i].Show(State.SwitchOn(i), round.OwnerOf(i));
            }
            for (int i = 0; lamps != null && i < lamps.Length; i++)
            {
                if (lamps[i] == null) continue;
                bool used = i < round.Size;
                lamps[i].gameObject.SetActive(used);
                if (used) lamps[i].Show(State.LampOn(i), State.TargetOn(i));
            }
        }

        /// <summary>Không nhận tham số; vẽ HUD: công tắc của vai đang chọn (đảo đèn nào), quá tải, tiến độ giữ khớp.</summary>
/// <summary>Không nhận tham số; vẽ HUD dự phòng khi drawOnGui bật (Canvas tắt nó và dùng Notebook).</summary>
        void OnGUI()
        {
            if (!drawOnGui || !Active || State == null) return;
            int player = party.ActivePlayer;
            GUI.Box(new Rect(16, 16, 480, 40 + 22 * (round.Size / 2 + 4)), "BẢNG CẦU DAO - Vai " + (player + 1) + "\n" + Notebook(player) + "\n" + Status + "\n[E] bấm công tắc  [R] làm lại  [1-4] đổi vai");
        }

        /// <summary>Nhận ID vai; trả các công tắc của vai đó và đèn mỗi công tắc đảo, kèm quá tải và % giữ khớp. Rỗng nếu chưa bắt đầu.</summary>
        public string Notebook(int player)
        {
            if (State == null || round == null) return "";
            var sb = new System.Text.StringBuilder();
            for (int s = 0; s < round.Size; s++)
            {
                int mask;
                if (!State.Inspect(player, s, out mask)) continue;
                sb.Append("Công tắc ").Append(s + 1).Append(" đảo đèn: ");
                for (int l = 0; l < round.Size; l++) if ((mask >> l & 1) == 1) sb.Append(l + 1).Append(' ');
                sb.Append('\n');
            }
            sb.Append("Đang bật ").Append(State.OnCount).Append('/').Append(round.OverloadLimit).Append("   Giữ khớp: ")
              .Append((State.HoldProgress(Time.timeAsDouble) * 100).ToString("0")).Append('%');
            return sb.ToString();
        }

        /// <summary>Nhận cờ reset; ngừng input; reset=true bỏ đề để lần sau sinh đề mới, false giữ kết quả.</summary>
        public void StopSession(bool reset = true)
        {
            Active = false;
            if (reset) { State = null; round = null; }
        }
    }
}
