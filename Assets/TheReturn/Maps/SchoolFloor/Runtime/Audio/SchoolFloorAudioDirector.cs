/*
 * Mục đích: Đạo diễn âm thanh của map SchoolFloor (T-018): quan sát trạng thái chung rồi phát cue; không chứa luật gameplay.
 * Hàm: Update theo dõi khu bắt đầu/xong/hỏng (AreaStart/AreaDone/AreaFail + nhạc sân/nhiệm vụ), thắng (Win), đổi vai (UiSwitchPlayer),
 * bước chân người đang điều khiển (Step), cổng mở/khóa gần người chơi (GateOpen/GateLock). SchoolFloorFlow.Start tự thêm component này.
 */
using UnityEngine;

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
