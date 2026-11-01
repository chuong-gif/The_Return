/*
 * Mục đích: Nhật ký nhỏ của host trong khung mạng (T-024): ghi các lệnh bị từ chối và sự kiện kết nối, giữ tối đa N dòng gần nhất
 * để test và công cụ gỡ lỗi đọc lại. Lớp Unity bên ngoài nối Sink vào Debug.Log; phần thuần C# này không biết Unity.
 * Hàm: constructor; Info; Warning; CountOf; Clear; chỉ mục theo vị trí; Sink.
 */
using System;
using System.Collections.Generic;

namespace TheReturn.Network.Framework
{
    /// <summary>Mức của một dòng nhật ký.</summary>
    public enum HostLogLevel { Info, Warning }

    /// <summary>Một dòng nhật ký.</summary>
    public readonly struct HostLogEntry
    {
        /// <summary>Mức của dòng.</summary>
        public HostLogLevel Level { get; }

        /// <summary>Nội dung.</summary>
        public string Message { get; }

        /// <summary>Nhận mức và nội dung.</summary>
        public HostLogEntry(HostLogLevel level, string message)
        {
            Level = level;
            Message = message;
        }
    }

    /// <summary>Nhật ký vòng: quá sức chứa thì bỏ dòng cũ nhất.</summary>
    public sealed class HostLog
    {
        readonly List<HostLogEntry> entries = new List<HostLogEntry>();
        readonly int capacity;

        /// <summary>Hàm được gọi cho MỖI dòng mới (vd đẩy sang Debug.Log); có thể để null.</summary>
        public Action<HostLogEntry> Sink { get; set; }

        /// <summary>Số dòng đang giữ.</summary>
        public int Count { get { return entries.Count; } }

        /// <summary>Dòng ở vị trí index (0 là cũ nhất).</summary>
        public HostLogEntry this[int index] { get { return entries[index]; } }

        /// <summary>Nhận sức chứa tối đa (từ 1 trở lên, mặc định 200). Ném lỗi nếu nhỏ hơn 1.</summary>
        public HostLog(int capacity = 200)
        {
            if (capacity < 1)
                throw new ArgumentOutOfRangeException("capacity", capacity, "Sức chứa nhật ký phải từ 1 trở lên.");
            this.capacity = capacity;
        }

        /// <summary>Ghi một dòng thông tin.</summary>
        public void Info(string message)
        {
            Add(HostLogLevel.Info, message);
        }

        /// <summary>Ghi một dòng cảnh báo (vd lệnh bị từ chối).</summary>
        public void Warning(string message)
        {
            Add(HostLogLevel.Warning, message);
        }

        /// <summary>Đếm số dòng đang giữ ở một mức.</summary>
        public int CountOf(HostLogLevel level)
        {
            int count = 0;
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].Level == level) count++;
            }
            return count;
        }

        /// <summary>Xóa hết dòng đang giữ.</summary>
        public void Clear()
        {
            entries.Clear();
        }

        void Add(HostLogLevel level, string message)
        {
            HostLogEntry entry = new HostLogEntry(level, message ?? string.Empty);
            if (entries.Count >= capacity) entries.RemoveAt(0);
            entries.Add(entry);
            Action<HostLogEntry> sink = Sink;
            if (sink != null) sink(entry);
        }
    }
}
