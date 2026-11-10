/*
 * Mục đích: LUẬT ÂM THANH ONLINE của 4 nhiệm vụ ở scene mới (T-040 Q-014 mục 4). MissionAudioDirector cũ đọc state của prototype MỘT MÁY, mà khi chơi online
 * các prototype đó không chạy (trạng thái nằm ở bản sao của OnlineClient), nên cần lớp riêng: so trạng thái chung TRƯỚC và SAU rồi trả danh sách cue (AudioCues)
 * cần phát. Thuần C#, test được không cần scene/âm thanh. Lần đầu nhận trạng thái (prev = null) không phát gì để khỏi có tiếng giả lúc vào map/vào muộn.
 * Khác bản một máy có chủ ý: Cầu dao nhảy chỉ phát tiếng quá tải (không kèm tiếng «tắt công tắc»); Cầu ván phát tiếng khi ván BẮT ĐẦU chạy, tiếng quẹt thẻ
 * khi có người nhặt thẻ, tiếng thất bại khi hết giờ (bản cũ phát PlankPlace theo số lần chơi).
 * Hàm: Attendance; Breaker; Choir; Plank.
 */
using System.Collections.Generic;
using TheReturn.AttendanceNet;
using TheReturn.BreakerNet;
using TheReturn.ChoirNet;
using TheReturn.PlankNet;

namespace TheReturn.Core4.Audio
{
    /// <summary>So hai trạng thái chung liên tiếp của một nhiệm vụ và thêm các cue cần phát vào danh sách.</summary>
    public static class OnlineMissionAudioRules
    {
        static readonly string[] Notes = { AudioCues.Note1, AudioCues.Note2, AudioCues.Note3, AudioCues.Note4 };

        /// <summary>Điểm danh: nhận trạng thái trước/sau và danh sách cue; số ghế có người tăng thì tiếng ngồi, giảm thì tiếng đứng dậy.</summary>
        public static void Attendance(AttendancePublicState prev, AttendancePublicState cur, List<string> cues)
        {
            if (prev == null || cur == null || cues == null) return;
            int delta = cur.OccupiedCount() - prev.OccupiedCount();
            if (delta > 0) cues.Add(AudioCues.SeatSit);
            else if (delta < 0) cues.Add(AudioCues.SeatStand);
        }

        /// <summary>Cầu dao: nhảy cầu dao thì tiếng quá tải; ngược lại số công tắc bật tăng thì tiếng bật, giảm thì tiếng tắt.</summary>
        public static void Breaker(BreakerPublicState prev, BreakerPublicState cur, List<string> cues)
        {
            if (prev == null || cur == null || cues == null) return;
            if (cur.Trips > prev.Trips) { cues.Add(AudioCues.Overload); return; }
            int delta = cur.OnCount - prev.OnCount;
            if (delta > 0) cues.Add(AudioCues.SwitchOn);
            else if (delta < 0) cues.Add(AudioCues.SwitchOff);
        }

        /// <summary>Hợp xướng: tiến thêm nốt thì phát nốt đó (xoay vòng 4 nốt); thêm lỗi sai hoặc quá giờ thì tiếng nốt sai.</summary>
        public static void Choir(ChoirPublicState prev, ChoirPublicState cur, List<string> cues)
        {
            if (prev == null || cur == null || cues == null) return;
            if (cur.Progress > prev.Progress) cues.Add(Notes[(cur.Progress - 1) % Notes.Length]);
            if (cur.Mistakes > prev.Mistakes || cur.Timeouts > prev.Timeouts) cues.Add(AudioCues.NoteWrong);
        }

        /// <summary>Cầu ván: ván bắt đầu chạy thì tiếng ván; thẻ từ vừa được nhặt thì tiếng quẹt thẻ; vừa thất bại (hết giờ) thì tiếng thất bại.</summary>
        public static void Plank(PlankPublicState prev, PlankPublicState cur, List<string> cues)
        {
            if (prev == null || cur == null || cues == null) return;
            if (prev.Phase != PlankPhase.Crossing && cur.Phase == PlankPhase.Crossing) cues.Add(AudioCues.PlankPlace);
            if (prev.CardHolder < 0 && cur.CardHolder >= 0) cues.Add(AudioCues.CardSwipe);
            if (prev.Status != PlankStatus.Failed && cur.Status == PlankStatus.Failed) cues.Add(AudioCues.AreaFail);
        }
    }
}
