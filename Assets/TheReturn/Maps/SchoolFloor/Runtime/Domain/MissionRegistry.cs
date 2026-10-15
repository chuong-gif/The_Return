/*
 * Mục đích: Nối MapProgress (luật chọn khu) với các IMissionAdapter (nhiệm vụ thật): bắt đầu khu, tự hoàn thành
 * khi nhiệm vụ xong, bỏ dở mà giữ tiến độ khu khác (T-003). Thuần C#, chưa gắn vào scene.
 * Hàm: constructor; Register; Find; ActiveAdapter; TryBegin; Tick; Abandon; Reset; Won.
 */
using System;
using System.Collections.Generic;

namespace TheReturn
{
    /// <summary>Danh bạ nhiệm vụ theo khu + điều phối vòng đời qua MapProgress.</summary>
    public sealed class MissionRegistry
    {
        readonly Dictionary<string, IMissionAdapter> adapters = new Dictionary<string, IMissionAdapter>();

        public MapProgress Progress { get; }

        /// <summary>Nhận MapProgress (không null); tạo registry rỗng.</summary>
        public MissionRegistry(MapProgress progress)
        {
            if (progress == null) throw new ArgumentNullException(nameof(progress));
            Progress = progress;
        }

        /// <summary>
        /// Nhận adapter; đăng ký nhiệm vụ cho khu của nó và đánh dấu khu có nhiệm vụ trong MapProgress (tự vào điều kiện thắng).
        /// Ném ArgumentException nếu khu không có trong MapProgress, InvalidOperationException nếu trùng khu hoặc khu đang làm/đã xong.
        /// </summary>
        public void Register(IMissionAdapter adapter)
        {
            if (adapter == null) throw new ArgumentNullException(nameof(adapter));
            if (Progress.Find(adapter.AreaId) == null)
                throw new ArgumentException("Khu không có trong MapProgress: " + adapter.AreaId, nameof(adapter));
            if (adapters.ContainsKey(adapter.AreaId))
                throw new InvalidOperationException("Khu đã có nhiệm vụ: " + adapter.AreaId);
            if (!Progress.SetHasMission(adapter.AreaId, true))
                throw new InvalidOperationException("Không đăng ký được khi khu đang làm hoặc đã xong: " + adapter.AreaId);
            adapters[adapter.AreaId] = adapter;
        }

        /// <summary>Nhận ID khu; trả adapter hoặc null nếu chưa đăng ký.</summary>
        public IMissionAdapter Find(string areaId)
        {
            IMissionAdapter a;
            return areaId != null && adapters.TryGetValue(areaId, out a) ? a : null;
        }

        /// <summary>Trả adapter của khu đang làm, hoặc null nếu không có khu nào đang làm.</summary>
        public IMissionAdapter ActiveAdapter => Find(Progress.ActiveAreaId);

        /// <summary>Trả true khi mọi khu đã đăng ký nhiệm vụ đều xong (xem MapProgress.Won).</summary>
        public bool Won => Progress.Won;

        /// <summary>
        /// Nhận ID khu, khu chuẩn bị đội đang đứng, cờ «đội đủ»; kiểm luật bằng MapProgress rồi bắt đầu nhiệm vụ thật.
        /// Khu có cờ nhiệm vụ nhưng chưa có adapter trả NoMission (không bắt đầu). Trả kết quả; Started nghĩa là adapter.Begin() đã được gọi.
        /// </summary>
        public BeginResult TryBegin(string areaId, string entryFromId, bool teamReady)
        {
            BeginResult check = Progress.CheckBegin(areaId, entryFromId, teamReady);
            if (check != BeginResult.Started) return check;
            IMissionAdapter adapter = Find(areaId);
            if (adapter == null) return BeginResult.NoMission;
            Progress.TryBegin(areaId, entryFromId, teamReady);
            adapter.Begin();
            return BeginResult.Started;
        }

        /// <summary>
        /// Không nhận tham số; gọi mỗi frame. Nếu khu đang làm đã giải xong và an toàn bàn giao thì dừng phiên (giữ trạng thái xong)
        /// và đánh dấu hoàn thành. Trả true khi vừa hoàn thành một khu.
        /// </summary>
        public bool Tick()
        {
            string id = Progress.ActiveAreaId;
            IMissionAdapter adapter = Find(id);
            if (adapter == null || !adapter.Solved || !adapter.CanHandOff) return false;
            adapter.Stop(false);
            return Progress.Complete(id);
        }

        /// <summary>Không nhận tham số; bỏ dở khu đang làm: dừng nhiệm vụ có reset, nhả khu, GIỮ tiến độ khu khác. Trả true nếu có khu đang làm.</summary>
        public bool Abandon()
        {
            string id = Progress.ActiveAreaId;
            if (id == null) return false;
            IMissionAdapter adapter = Find(id);
            if (adapter != null) adapter.Stop(true);
            return Progress.Abandon(id);
        }

        /// <summary>Không nhận tham số; dừng khu đang làm (có reset) rồi xóa mọi tiến độ. Danh sách adapter và cờ nhiệm vụ giữ nguyên.</summary>
        public void Reset()
        {
            Abandon();
            Progress.Reset();
        }
    }
}
