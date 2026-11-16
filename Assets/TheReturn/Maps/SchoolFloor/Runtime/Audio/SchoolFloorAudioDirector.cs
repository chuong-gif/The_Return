/*
 * Mục đích: Đạo diễn âm thanh của map SchoolFloor (T-018): quan sát trạng thái chung rồi phát cue; không chứa luật gameplay.
 * Hàm: Update theo dõi khu bắt đầu/xong/hỏng (AreaStart/AreaDone/AreaFail + nhạc sân/nhiệm vụ), thắng (Win), đổi vai (UiSwitchPlayer),
 * bước chân người đang điều khiển (Step), tiếng nhảy (Jump, T-045: TickJump), cổng mở/khóa gần người chơi (GateOpen/GateLock). SchoolFloorFlow.Start tự thêm component này.
 */
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheReturn
{
    /// <summary>Phát âm thanh chung theo trạng thái MissionRegistry và party.</summary>
    public sealed class SchoolFloorAudioDirector : MonoBehaviour
    {
        public SchoolFloorFlow flow;
        [Min(0.5f)] public float strideMeters = 1.7f;
        [Min(1f)] public float gateHearDistance = 25f;

        bool init;
        string lastArea;
        int lastDone;
        bool lastWon;
        int lastPlayer = -1;
        SchoolPreparationGate[] gates = new SchoolPreparationGate[0];
        bool[] gateOpen = new bool[0];
        Vector3 lastPos;
        float stepAcc;
        int jumpWatch;
        float jumpStartY;
        float jumpPrevY;
        bool jumpWasGrounded;
        Transform jumpBody;
        CharacterController jumpController;

        void Start()
        {
            if (flow == null) flow = GetComponent<SchoolFloorFlow>();
            gates = FindObjectsOfType<SchoolPreparationGate>();
            gateOpen = new bool[gates.Length];
            AudioHub.Player.PlayAmbience(AudioCues.AmbSchool);
            AudioHub.Player.PlayMusic(AudioCues.MusicYard);
        }

        void OnDestroy()
        {
            AudioHub.Player.PlayAmbience("");
            AudioHub.Player.StopMusic();
        }

        void Update()
        {
            if (flow == null || flow.missionFlow == null || flow.missionFlow.Registry == null || flow.attendance == null || flow.attendance.party == null) return;
            var progress = flow.missionFlow.Registry.Progress;
            var party = flow.attendance.party;
            string area = progress.ActiveAreaId;
            int done = progress.DoneCount;
            bool won = progress.Won;
            if (!init)
            {
                init = true; lastArea = area; lastDone = done; lastWon = won; lastPlayer = party.ActivePlayer;
                for (int i = 0; i < gates.Length; i++) gateOpen[i] = gates[i] != null && gates[i].IsOpen;
                if (area != null) AudioHub.Player.PlayMusic(AudioCues.MusicMission);
                return;
            }
            if (area != lastArea)
            {
                if (area != null) { AudioHub.Player.Play(AudioCues.AreaStart); AudioHub.Player.PlayMusic(AudioCues.MusicMission); }
                else
                {
                    if (done > lastDone) AudioHub.Player.Play(AudioCues.AreaDone);
                    else if (done == lastDone) AudioHub.Player.Play(AudioCues.AreaFail);
                    AudioHub.Player.PlayMusic(AudioCues.MusicYard);
                }
                lastArea = area;
            }
            lastDone = done;
            if (won && !lastWon) AudioHub.Player.Play(AudioCues.Win);
            lastWon = won;

            int active = party.ActivePlayer;
            if (active != lastPlayer) { if (lastPlayer >= 0) AudioHub.Player.Play(AudioCues.UiSwitchPlayer); lastPlayer = active; stepAcc = 0f; lastPos = PlayerPos(party, active); }
            TickSteps(party, active);
            TickJump(party, active);
            TickGates(party, active);
        }

        static Vector3 PlayerPos(PrototypePartyController party, int index)
        {
            return party.players != null && index >= 0 && index < party.players.Length && party.players[index] != null ? party.players[index].position : Vector3.zero;
        }

        void TickSteps(PrototypePartyController party, int active)
        {
            var pos = PlayerPos(party, active);
            var d = pos - lastPos; d.y = 0f; lastPos = pos;
            float dist = d.magnitude;
            if (Time.deltaTime <= 0f || dist > 1.5f || dist / Time.deltaTime < 0.4f) return;
            stepAcc += dist;
            if (stepAcc >= strideMeters) { stepAcc = 0f; AudioHub.Player.Play(AudioCues.Step); }
        }

        /// <summary>Nhận party và chỉ số người đang điều khiển; khi bấm Space lúc đang (hoặc vừa) đứng trên đất và chuột đã khóa thì theo dõi vài khung hình, nếu thân người thật sự nhích lên so với khung hình trước thì phát tiếng nhảy một lần (không phát khi ngồi hoặc bị khóa di chuyển). T-045 sửa: MissionFlowController chạy TRƯỚC đạo diễn này (thứ tự -50) nên lúc đạo diễn đọc, nhân vật có thể đã rời đất ở chính khung hình bấm; vì vậy dùng trạng thái đất của khung hình trước và so độ cao với khung hình trước.</summary>
        void TickJump(PrototypePartyController party, int active)
        {
            if (party.players == null || active < 0 || active >= party.players.Length) return;
            var body = party.players[active];
            if (body == null) return;
            if (body != jumpBody) { jumpBody = body; jumpController = body.GetComponent<CharacterController>(); jumpWatch = 0; jumpPrevY = body.position.y; jumpWasGrounded = false; }
            if (jumpController == null) return;
            bool groundedNow = jumpController.isGrounded;
            float y = body.position.y;
            if (jumpWatch > 0)
            {
                jumpWatch--;
                if (y - jumpStartY > 0.04f) { jumpWatch = 0; AudioHub.Player.Play(AudioCues.Jump); }
            }
            else
            {
                var keyboard = Keyboard.current;
                if (keyboard != null && party.allowJump && Cursor.lockState == CursorLockMode.Locked && keyboard.Control(GameAction.Jump).wasPressedThisFrame && (groundedNow || jumpWasGrounded))
                {
                    jumpStartY = jumpPrevY;
                    if (y - jumpStartY > 0.04f) AudioHub.Player.Play(AudioCues.Jump);
                    else jumpWatch = 4;
                }
            }
            jumpWasGrounded = groundedNow;
            jumpPrevY = y;
        }

        void TickGates(PrototypePartyController party, int active)
        {
            var me = PlayerPos(party, active);
            for (int i = 0; i < gates.Length; i++)
            {
                var g = gates[i]; if (g == null) continue;
                bool open = g.IsOpen;
                if (open == gateOpen[i]) continue;
                gateOpen[i] = open;
                var p = g.transform.position;
                if ((p - me).sqrMagnitude > gateHearDistance * gateHearDistance) continue;
                AudioHub.Player.PlayAt(open ? AudioCues.GateOpen : AudioCues.GateLock, p.x, p.y, p.z);
            }
        }
    }
}
