/*
 * Mục đích: Một object trong scene (Audio_Slots) để chủ dự án KÉO THẢ clip âm thanh thay cho âm mặc định (T-045),
 * không phải sửa code hay mở AudioCueBank. Ô để TRỐNG = giữ âm mặc định (âm giữ chỗ). Rê chuột vào tên ô để đọc ghi chú tiếng Việt.
 * Cách hoạt động: lúc scene chạy, bảng cue đang dùng được nhân bản rồi ghi đè các cue có clip; asset AudioCueBank_Main KHÔNG bị sửa.
 * Hàm: Awake (áp dụng); OnDestroy (trả bảng gốc); Apply (nhân bản bảng và ghi đè); Restore (trả bảng gốc); Override (tĩnh, ghi đè hoặc tạo một cue).
 */
using System.Collections.Generic;
using UnityEngine;

namespace TheReturn
{
    /// <summary>Bảng ô kéo thả âm thanh của scene; chạy sớm hơn mọi bộ phát âm của map để nhạc nền nhận đúng clip.</summary>
    [DefaultExecutionOrder(-1000)]
    [DisallowMultipleComponent]
    public sealed class GameAudioSlots : MonoBehaviour
    {
        [Header("ÂM DÙNG CHUNG (cả game) — bấm vào ô này để mở và chỉnh")]
        [Tooltip("Asset chứa âm giao diện (bấm/rê chuột/kéo thanh trượt) dùng cho cả 3 scene và tiếng bước chân/nhảy dùng cho Sanh_Cho + MapSchoolF. Chỉnh ở đó một lần là mọi scene đổi theo; object này chỉ giữ âm RIÊNG của map.")]
        public SharedAudioProfile sharedProfile;

        [Header("RIÊNG MAP NÀY — NHẠC NỀN VÀ KHÔNG KHÍ")]
        [Tooltip("NHẠC NỀN khi chưa làm nhiệm vụ (đang ở sân hoặc hành lang). Nên dùng clip có thể lặp liền mạch. Cue: music.yard")]
        public AudioClip musicYard;
        [Tooltip("NHẠC NỀN khi đang làm một nhiệm vụ (căng thẳng hơn). Tự chuyển mượt khi vào và ra nhiệm vụ. Cue: music.mission")]
        public AudioClip musicMission;
        [Tooltip("ÂM MÔI TRƯỜNG nền của trường học, lặp liên tục và nhỏ hơn nhạc. Cue: amb.school")]
        public AudioClip ambienceSchool;

        [Header("NHIỆM VỤ VÀ THẮNG GAME")]
        [Tooltip("Âm khi BẮT ĐẦU một nhiệm vụ (cả đội bước vào khu nhiệm vụ). Cue: area.start")]
        public AudioClip missionStart;
        [Tooltip("Âm khi HOÀN THÀNH MỘT NHIỆM VỤ (mỗi khu xong một lần). Cue: area.done")]
        public AudioClip missionComplete;
        [Tooltip("Âm khi nhiệm vụ THẤT BẠI hoặc bị bỏ dở. Cue: area.fail")]
        public AudioClip missionFail;
        [Tooltip("Âm THẮNG GAME, phát khi xong đủ 4 nhiệm vụ và màn hình HOÀN THÀNH hiện ra. Cue: game.win")]
        public AudioClip gameWin;

        [Header("TƯƠNG TÁC VẬT PHẨM: Phòng 1 - Điểm danh")]
        [Tooltip("Âm khi NGỒI xuống ghế. Cue: p1.sit")]
        public AudioClip seatSit;
        [Tooltip("Âm khi ĐỨNG dậy khỏi ghế. Cue: p1.stand")]
        public AudioClip seatStand;

        [Header("TƯƠNG TÁC VẬT PHẨM: Phòng 5 - Cầu dao")]
        [Tooltip("Âm khi BẬT một cầu dao. Cue: p5.switch_on")]
        public AudioClip switchOn;
        [Tooltip("Âm khi TẮT một cầu dao. Cue: p5.switch_off")]
        public AudioClip switchOff;
        [Tooltip("Âm QUÁ TẢI của bảng cầu dao. Cue: p5.overload")]
        public AudioClip overload;

        [Header("TƯƠNG TÁC VẬT PHẨM: Phòng 6 - Hợp xướng")]
        [Tooltip("Nốt NHẠC thứ nhất. Cue: p6.note1")]
        public AudioClip note1;
        [Tooltip("Nốt NHẠC thứ hai. Cue: p6.note2")]
        public AudioClip note2;
        [Tooltip("Nốt NHẠC thứ ba. Cue: p6.note3")]
        public AudioClip note3;
        [Tooltip("Nốt NHẠC thứ tư. Cue: p6.note4")]
        public AudioClip note4;
        [Tooltip("Âm khi hát SAI nốt. Cue: p6.wrong")]
        public AudioClip noteWrong;

        [Header("TƯƠNG TÁC VẬT PHẨM: Hành lang 6 - Ván gỗ và chìa khóa")]
        [Tooltip("Âm khi ĐẶT ván gỗ. Cue: hl6.plank")]
        public AudioClip plankPlace;
        [Tooltip("Âm khi QUẸT thẻ (dùng chìa khóa). Cue: hl6.card")]
        public AudioClip cardSwipe;

        [Header("CỬA VÀ CỔNG (giao diện đã chuyển sang âm dùng chung)")]
        [Tooltip("Âm khi cổng chuẩn bị MỞ. Cue: gate.open")]
        public AudioClip gateOpen;
        [Tooltip("Âm khi cổng KHÓA lại. Cue: gate.lock")]
        public AudioClip gateLock;
        [Tooltip("Âm MỞ cửa gỗ. Cue: door.open")]
        public AudioClip doorOpen;
        [Tooltip("Âm ĐÓNG cửa gỗ. Cue: door.close")]
        public AudioClip doorClose;

        AudioCueBank original;
        AudioCueBank runtimeBank;

        /// <summary>Không nhận tham số; áp dụng các clip đã kéo vào ngay khi scene nạp.</summary>
        void Awake() { Apply(); }

        /// <summary>Không nhận tham số; rời scene thì trả bảng cue gốc để scene khác không bị ảnh hưởng.</summary>
        void OnDestroy() { Restore(); }

        /// <summary>Không nhận tham số; nhân bản bảng cue gốc rồi ghi đè các cue có clip, sau đó gán bảng mới cho AudioService. Gọi lại an toàn (luôn làm từ bảng gốc).</summary>
        [ContextMenu("Áp dụng lại các ô âm thanh")]
        public void Apply()
        {
            var svc = AudioService.Instance;
            if (svc == null) { Debug.LogWarning("GameAudioSlots: chưa có AudioService nên chưa áp dụng được clip.", this); return; }
            if (original == null) original = svc.bank;
            if (original == null) { Debug.LogWarning("GameAudioSlots: không thấy bảng cue gốc (Resources/AudioCueBank_Main).", this); return; }
            if (runtimeBank != null) Destroy(runtimeBank);
            runtimeBank = Instantiate(original);
            var list = new List<AudioCueBank.Entry>(runtimeBank.entries ?? new AudioCueBank.Entry[0]);

            Override(list, AudioCues.MusicYard, musicYard);
            Override(list, AudioCues.MusicMission, musicMission);
            Override(list, AudioCues.AmbSchool, ambienceSchool);
            Override(list, AudioCues.AreaStart, missionStart);
            Override(list, AudioCues.AreaDone, missionComplete);
            Override(list, AudioCues.AreaFail, missionFail);
            Override(list, AudioCues.Win, gameWin);
            Override(list, AudioCues.SeatSit, seatSit);
            Override(list, AudioCues.SeatStand, seatStand);
            Override(list, AudioCues.SwitchOn, switchOn);
            Override(list, AudioCues.SwitchOff, switchOff);
            Override(list, AudioCues.Overload, overload);
            Override(list, AudioCues.Note1, note1);
            Override(list, AudioCues.Note2, note2);
            Override(list, AudioCues.Note3, note3);
            Override(list, AudioCues.Note4, note4);
            Override(list, AudioCues.NoteWrong, noteWrong);
            Override(list, AudioCues.PlankPlace, plankPlace);
            Override(list, AudioCues.CardSwipe, cardSwipe);
            Override(list, AudioCues.GateOpen, gateOpen);
            Override(list, AudioCues.GateLock, gateLock);
            Override(list, AudioCues.DoorOpen, doorOpen);
            Override(list, AudioCues.DoorClose, doorClose);

            runtimeBank.entries = list.ToArray();
            runtimeBank.Rebuild();
            svc.bank = runtimeBank;
        }

        /// <summary>Không nhận tham số; nếu AudioService còn đang dùng bảng đã nhân bản thì trả lại bảng gốc và hủy bảng nhân bản.</summary>
        void Restore()
        {
            var svc = AudioService.Instance;
            if (svc != null && runtimeBank != null && svc.bank == runtimeBank) svc.bank = original;
            if (runtimeBank != null) Destroy(runtimeBank);
            runtimeBank = null;
        }

        /// <summary>Nhận danh sách entry, mã cue, một clip, thời gian nghỉ cho cue mới và âm lượng (số nhỏ hơn 0 = giữ nguyên); clip null thì bỏ qua (giữ âm mặc định).</summary>
        internal static void Override(List<AudioCueBank.Entry> list, string cue, AudioClip clip, float cooldownIfNew = 0.05f, float volume = -1f, AudioBus newBus = AudioBus.Sfx)
        {
            if (clip == null) return;
            Override(list, cue, new[] { clip }, cooldownIfNew, volume, newBus);
        }

        /// <summary>Nhận danh sách entry, mã cue, các clip, thời gian nghỉ cho cue mới và âm lượng; bỏ clip null, nếu còn clip thì thay danh sách clip (và âm lượng nếu có) của cue (tạo cue mới nếu bảng chưa có).</summary>
        internal static void Override(List<AudioCueBank.Entry> list, string cue, AudioClip[] clips, float cooldownIfNew = 0.05f, float volume = -1f, AudioBus newBus = AudioBus.Sfx)
        {
            if (clips == null) return;
            var valid = new List<AudioClip>();
            foreach (var c in clips) if (c != null) valid.Add(c);
            if (valid.Count == 0) return;
            AudioCueBank.Entry e = null;
            foreach (var x in list) if (x != null && x.cue == cue) { e = x; break; }
            if (e == null) { e = new AudioCueBank.Entry { cue = cue, cooldown = cooldownIfNew, bus = newBus }; list.Add(e); }
            e.clips = valid.ToArray();
            if (volume >= 0f) e.volume = Mathf.Clamp01(volume);
        }
    }
}
