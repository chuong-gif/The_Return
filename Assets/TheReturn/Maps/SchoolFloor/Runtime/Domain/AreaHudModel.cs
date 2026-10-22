/*
 * Mục đích: Dữ liệu HUD chung của một khu (T-011): tên, tiến độ, mục tiêu, cảnh báo ngắn, nhắc phím. Presenter chỉ vẽ.
 * Hàm: AddAlert/ExpireAlerts quản lý cảnh báo có hạn; Alerts trả cảnh báo còn hiệu lực theo thời gian do caller truyền.
 */
using System.Collections.Generic;

namespace TheReturn
{
    /// <summary>Nhà cung cấp HUD của một khu (adapter tùy chọn implement); khu không implement dùng đường text cũ.</summary>
    public interface IAreaHudProvider
    {
        /// <summary>Nhận chỉ số vai đang xem và thời điểm hiện tại; điền model HUD của khu.</summary>
        void FillHud(AreaHudModel model, int player, double now);
    }

    /// <summary>Mô hình HUD của khu đang chơi.</summary>
    public sealed class AreaHudModel
    {
        public string Title = "";
        public string Progress = "";
        public string Objective = "";
        public string Prompt = "";
        public string Notebook = "";
        public string Controls = "";

        sealed class Alert { public string Text; public double Until; }
        readonly List<Alert> alerts = new List<Alert>();

        /// <summary>Nhận chữ và hạn (giây từ lúc hiện); bỏ qua chữ rỗng. Cảnh báo trùng chữ chỉ gia hạn.</summary>
        public void AddAlert(string text, double now, double seconds)
        {
            if (string.IsNullOrEmpty(text)) return;
            foreach (var a in alerts)
                if (a.Text == text) { a.Until = now + seconds; return; }
            alerts.Add(new Alert { Text = text, Until = now + seconds });
        }

        /// <summary>Nhận thời điểm; xóa cảnh báo đã hết hạn.</summary>
        public void ExpireAlerts(double now) { alerts.RemoveAll(a => a.Until <= now); }

        /// <summary>Nhận thời điểm; trả chữ các cảnh báo còn hiệu lực theo thứ tự thêm.</summary>
        public List<string> ActiveAlerts(double now)
        {
            var r = new List<string>();
            foreach (var a in alerts) if (a.Until > now) r.Add(a.Text);
            return r;
        }

        /// <summary>Xóa mọi trường chữ và cảnh báo (khi đổi khu).</summary>
        public void Clear()
        {
            Title = Progress = Objective = Prompt = Notebook = Controls = "";
            alerts.Clear();
        }
    }
}
