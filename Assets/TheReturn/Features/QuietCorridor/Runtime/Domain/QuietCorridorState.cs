/*
 * Mục đích: Luật giữ trật tự độc lập scene; nhận thời gian và ý định đã kiểm tra.
 * Hàm: constructor tạo lượt 2–4 người; Hold giữ nút; Release nhả nút;
 * Tick cập nhật ân hạn/tiếng ồn; TryComplete kiểm tra cả đội và bàn giao;
 * Reset xóa lượt hiện tại; Owner đọc người giữ nút.
 */
using System;

namespace TheReturn
{
    public sealed class QuietCorridorState
    {
        public int PlayerCount { get; private set; }
        public float Noise { get; private set; }
        public float Grace { get; private set; }
        public bool Failed { get; private set; }
        public bool Solved { get; private set; }
        public bool HandedOver { get; private set; }
        public bool Held => owners[0] >= 0 || owners[1] >= 0;
        public bool SpeakerOn => !Solved && !Held && Grace <= 0;
        public bool GateRequested => Solved || Held || Grace > 0;
        readonly int[] owners = { -1, -1 };
        readonly float graceSeconds, sprintRate, speakerRate, recoveryRate;

        /// <summary>Nhận số người và tốc độ luật; tạo trạng thái trống, báo lỗi nếu cấu hình không hợp lệ.</summary>
        public QuietCorridorState(int count, float grace = 3, float sprint = 20, float speaker = 15, float recovery = 15)
        {
            if (count < 2 || count > 4 || grace < 0 || sprint < 0 || speaker < 0 || recovery < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            PlayerCount = count;
            graceSeconds = grace; sprintRate = sprint; speakerRate = speaker; recoveryRate = recovery;
        }

        /// <summary>Nhận chỉ số nút; trả ID người giữ, hoặc -1 nếu nút trống/không hợp lệ.</summary>
        public int Owner(int station) => station >= 0 && station < 2 ? owners[station] : -1;

        /// <summary>Nhận nút và ID đã xác thực khoảng cách; trả true nếu giữ được, ghi nhận bàn giao hai người.</summary>
        public bool Hold(int station, int player)
        {
            if (Failed || Solved || station < 0 || station > 1 || player < 0 || player >= PlayerCount ||
                (owners[station] >= 0 && owners[station] != player) || owners[1 - station] == player) return false;
            owners[station] = player;
            Grace = graceSeconds;
            if (owners[0] >= 0 && owners[1] >= 0 && owners[0] != owners[1]) HandedOver = true;
            return true;
        }

        /// <summary>Nhận ID người; nhả mọi nút người đó giữ, giữ nguyên thời gian ân hạn còn lại.</summary>
        public void Release(int player)
        {
            for (int i = 0; i < owners.Length; i++) if (owners[i] == player) owners[i] = -1;
        }

        /// <summary>Nhận delta giây và số người đang chạy; cập nhật ồn chính xác cả frame đi qua mốc ân hạn.</summary>
        public void Tick(float seconds, int runners)
        {
            if (seconds < 0 || float.IsNaN(seconds) || float.IsInfinity(seconds)) throw new ArgumentOutOfRangeException(nameof(seconds));
            if (Solved || Failed) return;
            runners = Math.Max(0, Math.Min(PlayerCount, runners));
            float silentTime = Held ? seconds : Math.Min(Grace, seconds);
            if (Held) Grace = graceSeconds;
            else Grace = Math.Max(0, Grace - seconds);
            float loudTime = seconds - silentTime;
            float silentRate = runners > 0 ? runners * sprintRate : -recoveryRate;
            Noise = Math.Max(0, Math.Min(100, Noise + silentRate * silentTime));
            Noise = Math.Max(0, Math.Min(100, Noise + (runners * sprintRate + speakerRate) * loudTime));
            if (Noise >= 100) Failed = true;
        }

        /// <summary>Nhận cờ tất cả người ở vùng đích; trả true khi có bàn giao thật và không thất bại.</summary>
        public bool TryComplete(bool allAcross)
        {
            if (!Failed && HandedOver && allAcross) Solved = true;
            return Solved;
        }

        /// <summary>Không nhận tham số; xóa ồn, nút giữ, bàn giao và kết quả, không tác động nhiệm vụ khác.</summary>
        public void Reset()
        {
            Noise = 0; Grace = 0; Failed = false; Solved = false; HandedOver = false;
            owners[0] = owners[1] = -1;
        }
    }
}
