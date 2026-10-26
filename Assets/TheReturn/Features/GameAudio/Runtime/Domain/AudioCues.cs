/*
 * Mục đích: Danh sách mã cue âm thanh ổn định (T-018) và giao diện phát để mọi hệ thống gọi mà không phụ thuộc Unity.
 * Hàm: AudioCues (hằng chuỗi); AudioBus (kênh); IAudioPlayer.Play/PlayAt/PlayMusic/StopMusic/PlayAmbience; AudioHub (điểm truy cập tĩnh); NullAudioPlayer (không làm gì, dùng khi chưa có AudioService/test).
 * Quy ước mã: «nhóm.tên». Thêm cue mới = thêm hằng ở đây + một dòng trong AudioCueBank; thay clip chỉ sửa bank, không sửa code.
 */
namespace TheReturn
{
    /// <summary>Kênh âm lượng: chỉnh riêng trong Settings.</summary>
    public enum AudioBus { Sfx, Ui, Music, Voice, Ambience }

    /// <summary>Mã cue dùng chung (chuỗi cố định để lưu, mạng hóa và thay clip).</summary>
    public static class AudioCues
    {
        // Giao diện và luồng game
        public const string UiClick = "ui.click";
        public const string UiSwitchPlayer = "ui.switch_player";
        public const string AreaStart = "area.start";
        public const string AreaDone = "area.done";
        public const string AreaFail = "area.fail";
        public const string Warning = "area.warning";
        public const string Win = "game.win";
        // Cửa và cổng
        public const string DoorOpen = "door.open";
        public const string DoorClose = "door.close";
        public const string GateOpen = "gate.open";
        public const string GateLock = "gate.lock";
        // Di chuyển
        public const string Step = "step.floor";
        // Nhạc và nền
        public const string MusicYard = "music.yard";
        public const string MusicMission = "music.mission";
        public const string AmbSchool = "amb.school";
        // Theo khu
        public const string SeatSit = "p1.sit";
        public const string SeatStand = "p1.stand";
        public const string SpeakerBuzz = "hl1.buzz";
        public const string ButtonHold = "hl1.hold";
        public const string Paper = "p2.paper";
        public const string Stamp = "p3.stamp";
        public const string WrongTurn = "hl3.wrong_turn";
        public const string TeacherVoice = "p4.voice";
        public const string SwitchOn = "p5.switch_on";
        public const string SwitchOff = "p5.switch_off";
        public const string Overload = "p5.overload";
        public const string Note1 = "p6.note1";
        public const string Note2 = "p6.note2";
        public const string Note3 = "p6.note3";
        public const string Note4 = "p6.note4";
        public const string NoteWrong = "p6.wrong";
        public const string GlassStep = "hl2.glass_step";
        public const string GlassCrack = "hl2.glass_crack";
        public const string LanternPass = "hl4.pass";
        public const string CodeWrong = "hl4.code_wrong";
        public const string TeacherSays = "hl5.teacher_says";
        public const string StepCommand = "hl5.step";
        public const string StandCommand = "hl5.stand";
        public const string PlankPlace = "hl6.plank";
        public const string CardSwipe = "hl6.card";
    }

    /// <summary>Giao diện phát âm thanh; Presentation gọi qua đây để test được và sau này đồng bộ mạng.</summary>
    public interface IAudioPlayer
    {
        /// <summary>Nhận mã cue; phát 2D (không theo vị trí).</summary>
        void Play(string cue);
        /// <summary>Nhận mã cue và tọa độ thế giới; phát 3D tại vị trí đó.</summary>
        void PlayAt(string cue, float x, float y, float z);
        /// <summary>Nhận mã cue nhạc; chuyển nhạc nền mượt (rỗng = tắt dần).</summary>
        void PlayMusic(string cue);
        /// <summary>Không nhận tham số; tắt nhạc nền dần.</summary>
        void StopMusic();
        /// <summary>Nhận mã cue nền môi trường (lặp); rỗng = tắt dần.</summary>
        void PlayAmbience(string cue);
    }

    /// <summary>Bộ phát rỗng, không làm gì (khi chưa có AudioService hoặc trong test).</summary>
    public sealed class NullAudioPlayer : IAudioPlayer
    {
        public static readonly NullAudioPlayer Instance = new NullAudioPlayer();
        public void Play(string cue) { }
        public void PlayAt(string cue, float x, float y, float z) { }
        public void PlayMusic(string cue) { }
        public void StopMusic() { }
        public void PlayAmbience(string cue) { }
    }

    /// <summary>Điểm truy cập tĩnh tới bộ phát hiện tại (mặc định rỗng). Mọi hệ thống gọi AudioHub.Player.Play(...) mà không cần tham chiếu AudioService.</summary>
    public static class AudioHub
    {
        static IAudioPlayer player;

        /// <summary>Bộ phát hiện tại; không bao giờ null.</summary>
        public static IAudioPlayer Player { get { return player ?? NullAudioPlayer.Instance; } }

        /// <summary>Nhận bộ phát; đặt làm bộ phát hiện tại.</summary>
        public static void Set(IAudioPlayer p) { player = p; }

        /// <summary>Nhận bộ phát; chỉ gỡ nếu đó đúng là bộ phát hiện tại.</summary>
        public static void Clear(IAudioPlayer p) { if (ReferenceEquals(player, p)) player = null; }
    }
}
