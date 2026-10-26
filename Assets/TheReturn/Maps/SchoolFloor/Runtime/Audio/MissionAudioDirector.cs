/*
 * Mục đích: Tiếng riêng của từng khu (T-018 S4): quan sát trạng thái nhiệm vụ đang chạy rồi phát cue; không sửa file nhiệm vụ nào.
 * Hàm: Update gọi Tick(area); Delta(key, value) trả chênh lệch so với lần trước (lần đầu của khu = 0, tránh tiếng giả lúc bắt đầu);
 * Tick* theo từng khu phát cue qua AudioHub.Player. SchoolFloorFlow.Start tự thêm component này.
 */
using System.Collections.Generic;
using UnityEngine;

namespace TheReturn
{
    /// <summary>Phát âm thanh riêng của 12 khu dựa trên thay đổi trạng thái.</summary>
    public sealed class MissionAudioDirector : MonoBehaviour
    {
        public SchoolFloorFlow flow;

        readonly Dictionary<string, int> last = new Dictionary<string, int>();
        string lastArea = "";
        bool announced;

        static readonly string[] Notes = { AudioCues.Note1, AudioCues.Note2, AudioCues.Note3, AudioCues.Note4 };

        /// <summary>Nhận khóa và giá trị hiện tại; trả chênh lệch so với lần gọi trước (lần đầu của khu trả 0).</summary>
        int Delta(string key, int value)
        {
            int prev;
            if (!last.TryGetValue(key, out prev)) { last[key] = value; return 0; }
            last[key] = value;
            return value - prev;
        }

        static void Play(string cue) { AudioHub.Player.Play(cue); }

        void Update()
        {
            if (flow == null || flow.missionFlow == null || flow.missionFlow.Registry == null) return;
            string area = flow.missionFlow.Registry.Progress.ActiveAreaId ?? "";
            if (area != lastArea) { last.Clear(); lastArea = area; announced = false; }
            switch (area)
            {
                case "Phong_1": TickAttendance(); break;
                case "Hanh_Lang_1": TickQuiet(); break;
                case "Phong_2": TickGrade(); break;
                case "Phong_3": TickExam(); break;
                case "Hanh_Lang_3": TickBack(); break;
                case "Phong_4": TickTruth(); break;
                case "Phong_5": TickBreaker(); break;
                case "Phong_6": TickChoir(); break;
                case "Hanh_Lang_2": TickGlass(); break;
                case "Hanh_Lang_4": TickLantern(); break;
                case "Hanh_Lang_5": TickTeacherSays(); break;
                case "Hanh_Lang_6": TickPlank(); break;
            }
        }

        void TickAttendance()
        {
            var a = flow.attendance; if (a == null || a.State == null) return;
            int d = Delta("seat", a.State.OccupiedCount);
            if (d > 0) Play(AudioCues.SeatSit); else if (d < 0) Play(AudioCues.SeatStand);
        }

        void TickQuiet()
        {
            var q = flow.corridor; if (q == null || q.State == null) return;
            if (Delta("buzz", q.State.SpeakerOn ? 1 : 0) > 0) Play(AudioCues.SpeakerBuzz);
            if (Delta("held", q.State.Held ? 1 : 0) > 0) Play(AudioCues.ButtonHold);
            if (Delta("failed", q.State.Failed ? 1 : 0) > 0) Play(AudioCues.Warning);
        }

        void TickGrade()
        {
            var g = flow.gradeRepair; if (g == null || g.State == null) return;
            if (Delta("rev", g.State.Revision) > 0) Play(AudioCues.Paper);
        }

        void TickExam()
        {
            var e = flow.navigationExam; if (e == null || e.State == null) return;
            if (Delta("stamps", e.State.Stamps) > 0) Play(AudioCues.Stamp);
        }

        void TickBack()
        {
            var b = flow.dontLookBack; if (b == null || b.State == null) return;
            if (Delta("pen", b.State.Penalties) > 0) Play(AudioCues.WrongTurn);
            if (Delta("wit", b.State.WitnessedCount) > 0) Play(AudioCues.UiClick);
        }

        void TickTruth()
        {
            var t = flow.teacherTruth; if (t == null || t.State == null) return;
            if (Delta("win", t.State.WindowActive ? 1 : 0) > 0) Play(AudioCues.TeacherVoice);
            if (Delta("conf", t.State.ConfirmedCount) > 0) Play(AudioCues.UiClick);
        }

        void TickBreaker()
        {
            var p = flow.breakerPanel; if (p == null || p.State == null) return;
            int d = Delta("on", p.State.OnCount);
            if (d > 0) Play(AudioCues.SwitchOn); else if (d < 0) Play(AudioCues.SwitchOff);
            if (Delta("trips", p.State.Trips) > 0) Play(AudioCues.Overload);
        }

        void TickChoir()
        {
            var c = flow.choir; if (c == null || c.State == null) return;
            int d = Delta("prog", c.State.Progress);
            if (d > 0) Play(Notes[Mathf.Clamp(c.State.Progress - 1, 0, 1000) % Notes.Length]);
            if (Delta("mis", c.State.Mistakes) > 0) Play(AudioCues.NoteWrong);
            if (Delta("tmo", c.State.Timeouts) > 0) Play(AudioCues.NoteWrong);
        }

        void TickGlass()
        {
            var g = flow.glassBridge; if (g == null || g.State == null) return;
            if (Delta("across", g.State.AcrossCount) > 0) Play(AudioCues.GlassStep);
            if (Delta("falls", g.State.Falls) > 0) Play(AudioCues.GlassCrack);
        }

        void TickLantern()
        {
            var l = flow.sharedLantern; if (l == null || l.State == null) return;
            if (Delta("holder", l.State.Holder) != 0) Play(AudioCues.LanternPass);
            if (Delta("wrong", l.State.WrongSubmits) > 0) Play(AudioCues.CodeWrong);
            if (Delta("sent", l.State.SentBackCount) > 0) Play(AudioCues.Warning);
            if (Delta("ok", l.State.CodeAccepted ? 1 : 0) > 0) Play(AudioCues.LanternPass);
        }

        void TickTeacherSays()
        {
            var t = flow.teacherSays; if (t == null || t.State == null) return;
            var s = t.State;
            bool newSegment = Delta("seg", s.Segment) > 0;
            if (!announced && s.Started) { announced = true; newSegment = true; }
            if (newSegment && !s.Solved)
            {
                Play(AudioCues.TeacherSays);
                Play(s.RequiredNow == TeacherCommand.Stand ? AudioCues.StandCommand : AudioCues.StepCommand);
            }
            if (Delta("warn", s.TotalWarnings) > 0) Play(AudioCues.Warning);
            if (Delta("sent", s.SentBacks) > 0) Play(AudioCues.AreaFail);
        }

        void TickPlank()
        {
            var p = flow.plankAndKey; if (p == null || p.Session == null) return;
            if (Delta("att", p.Session.State.Attempts) > 0) Play(AudioCues.PlankPlace);
        }
    }
}
