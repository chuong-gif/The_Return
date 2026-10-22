/*
 * Mục đích: Dữ liệu cài đặt của người chơi (T-008): âm lượng, độ nhạy chuột, toàn màn hình, cùng nơi lưu (ISettingsStore) và mô hình giữ
 * cài đặt hiện tại (GameSettingsModel) để giao diện đọc/ghi. Thuần C#; việc áp dụng vào Unity nằm ở lớp Runtime.
 * Luật: âm lượng 0..1 (mặc định 0,8); độ nhạy chuột là HỆ SỐ NHÂN 0,2..3,0 (mặc định 1,0 = độ nhạy hiện tại của game); giá trị ngoài khoảng
 * bị cắt, NaN/vô cực thành mặc định. GameSettingsData bất biến.
 * Kiểu: GameSettingsData, ISettingsStore, InMemorySettingsStore, GameSettingsModel.
 * Hàm GameSettingsModel: constructor; Current; SetVolume; SetMouseSensitivity; SetFullscreen; ResetToDefaults; Save; Commit.
 */
using System;

namespace TheReturn
{
    /// <summary>Cài đặt bất biến của người chơi.</summary>
    public sealed class GameSettingsData : IEquatable<GameSettingsData>
    {
        public const float DefaultVolume = 0.8f;
        public const float MinSensitivity = 0.2f;
        public const float MaxSensitivity = 3.0f;
        public const float DefaultSensitivity = 1.0f;
        public const bool DefaultFullscreen = true;

        public float Volume { get; }
        public float MouseSensitivity { get; }
        public bool Fullscreen { get; }

        /// <summary>Nhận âm lượng, độ nhạy chuột và cờ toàn màn hình; cắt vào khoảng hợp lệ, giá trị không phải số thành mặc định.</summary>
        public GameSettingsData(float volume, float mouseSensitivity, bool fullscreen)
        {
            Volume = Clamp(volume, 0f, 1f, DefaultVolume);
            MouseSensitivity = Clamp(mouseSensitivity, MinSensitivity, MaxSensitivity, DefaultSensitivity);
            Fullscreen = fullscreen;
        }

        /// <summary>Không nhận tham số; trả cài đặt mặc định.</summary>
        public static GameSettingsData Defaults() { return new GameSettingsData(DefaultVolume, DefaultSensitivity, DefaultFullscreen); }

        /// <summary>Âm lượng làm tròn thành phần trăm 0..100 để hiển thị.</summary>
        public int VolumePercent { get { return (int)Math.Round(Volume * 100f); } }

        /// <summary>Nhận âm lượng mới; trả bản sao đã đổi.</summary>
        public GameSettingsData WithVolume(float volume) { return new GameSettingsData(volume, MouseSensitivity, Fullscreen); }

        /// <summary>Nhận độ nhạy mới; trả bản sao đã đổi.</summary>
        public GameSettingsData WithMouseSensitivity(float sensitivity) { return new GameSettingsData(Volume, sensitivity, Fullscreen); }

        /// <summary>Nhận cờ toàn màn hình; trả bản sao đã đổi.</summary>
        public GameSettingsData WithFullscreen(bool fullscreen) { return new GameSettingsData(Volume, MouseSensitivity, fullscreen); }

        /// <summary>Nhận cài đặt khác; trả true nếu cả ba giá trị bằng nhau.</summary>
        public bool Equals(GameSettingsData other)
        {
            return other != null && Volume == other.Volume && MouseSensitivity == other.MouseSensitivity && Fullscreen == other.Fullscreen;
        }

        /// <summary>Nhận đối tượng; so sánh như Equals(GameSettingsData).</summary>
        public override bool Equals(object obj) { return Equals(obj as GameSettingsData); }

        /// <summary>Không nhận tham số; trả mã băm theo ba giá trị.</summary>
        public override int GetHashCode() { return (Volume.GetHashCode() * 397 ^ MouseSensitivity.GetHashCode()) * 397 ^ Fullscreen.GetHashCode(); }

        /// <summary>Nhận giá trị, khoảng và giá trị dự phòng; trả giá trị đã cắt, hoặc dự phòng nếu không phải số hữu hạn.</summary>
        static float Clamp(float v, float min, float max, float fallback)
        {
            if (float.IsNaN(v) || float.IsInfinity(v)) return fallback;
            return v < min ? min : (v > max ? max : v);
        }
    }

    /// <summary>Nơi lưu cài đặt (PlayerPrefs trong game, bộ nhớ trong test).</summary>
    public interface ISettingsStore
    {
        /// <summary>Không nhận tham số; trả cài đặt đã lưu hoặc mặc định nếu chưa có.</summary>
        GameSettingsData Load();

        /// <summary>Nhận cài đặt (không null); ghi vào nơi lưu.</summary>
        void Save(GameSettingsData data);
    }

    /// <summary>Nơi lưu trong bộ nhớ (dùng cho test và khi chưa có lưu trữ thật).</summary>
    public sealed class InMemorySettingsStore : ISettingsStore
    {
        GameSettingsData saved;

        public int SaveCount { get; private set; }

        /// <summary>Không nhận tham số; trả bản đã lưu hoặc mặc định.</summary>
        public GameSettingsData Load() { return saved ?? GameSettingsData.Defaults(); }

        /// <summary>Nhận cài đặt (không null); ghi vào bộ nhớ và tăng số lần lưu.</summary>
        public void Save(GameSettingsData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            saved = data;
            SaveCount++;
        }
    }

    /// <summary>Mô hình cài đặt hiện tại: giao diện gọi Set..., lớp Runtime lắng nghe Changed để áp dụng vào Unity.</summary>
    public sealed class GameSettingsModel
    {
        readonly ISettingsStore store;

        public GameSettingsData Current { get; private set; }
        public event Action<GameSettingsData> Changed;

        /// <summary>Nhận nơi lưu (không null); nạp cài đặt đã lưu làm giá trị hiện tại.</summary>
        public GameSettingsModel(ISettingsStore store)
        {
            if (store == null) throw new ArgumentNullException(nameof(store));
            this.store = store;
            Current = store.Load();
        }

        /// <summary>Nhận âm lượng; đổi và lưu ngay nếu khác giá trị hiện tại.</summary>
        public void SetVolume(float volume) { Commit(Current.WithVolume(volume)); }

        /// <summary>Nhận độ nhạy chuột; đổi và lưu ngay nếu khác giá trị hiện tại.</summary>
        public void SetMouseSensitivity(float sensitivity) { Commit(Current.WithMouseSensitivity(sensitivity)); }

        /// <summary>Nhận cờ toàn màn hình; đổi và lưu ngay nếu khác giá trị hiện tại.</summary>
        public void SetFullscreen(bool fullscreen) { Commit(Current.WithFullscreen(fullscreen)); }

        /// <summary>Không nhận tham số; đưa cả ba cài đặt về mặc định và lưu.</summary>
        public void ResetToDefaults() { Commit(GameSettingsData.Defaults()); }

        /// <summary>Không nhận tham số; ghi giá trị hiện tại vào nơi lưu (dùng khi cần lưu lại chắc chắn).</summary>
        public void Save() { store.Save(Current); }

        /// <summary>Nhận cài đặt mới; nếu khác hiện tại thì thay, lưu và bắn Changed.</summary>
        void Commit(GameSettingsData next)
        {
            if (next.Equals(Current)) return;
            Current = next;
            store.Save(next);
            if (Changed != null) Changed(next);
        }
    }
}
