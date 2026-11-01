/*
 * Mục đích: Danh sách đội CỐ ĐỊNH 2-4 người cho khung mạng host-authoritative (T-024). Mỗi người giữ một ô (slot) 0..Size-1.
 * Host dùng danh sách này để biết ai đang kết nối: chỉ người đang kết nối mới được gửi lệnh và được nhận trạng thái.
 * Số người chốt ngay lúc lập đội (quyết định chủ dự án 2026-10-29), nên Size không đổi sau khi tạo. Thuần C#, không phụ thuộc Unity.
 * Hàm: constructor; IsValidSlot; IsConnected; SetConnected; ConnectedCount; ConnectedSlots.
 */
using System;
using System.Collections.Generic;

namespace TheReturn.Network.Framework
{
    /// <summary>Đội 2-4 người cố định; theo dõi ai đang kết nối.</summary>
    public sealed class TeamRoster
    {
        /// <summary>Số người tối thiểu của một đội.</summary>
        public const int MinSize = 2;
        /// <summary>Số người tối đa của một đội.</summary>
        public const int MaxSize = 4;

        readonly bool[] connected;

        /// <summary>Số người của đội (2..4), cố định.</summary>
        public int Size { get; private set; }

        /// <summary>Nhận số người (2..4) và cờ "mọi người đang kết nối" (mặc định có). Ném lỗi nếu số người ngoài 2..4.</summary>
        public TeamRoster(int size, bool allConnected = true)
        {
            if (size < MinSize || size > MaxSize)
                throw new ArgumentOutOfRangeException("size", size, "Đội phải có từ 2 đến 4 người.");
            Size = size;
            connected = new bool[size];
            if (allConnected)
            {
                for (int i = 0; i < size; i++) connected[i] = true;
            }
        }

        /// <summary>True nếu ô nằm trong 0..Size-1.</summary>
        public bool IsValidSlot(int slot)
        {
            return slot >= 0 && slot < Size;
        }

        /// <summary>True nếu ô hợp lệ và người đó đang kết nối.</summary>
        public bool IsConnected(int slot)
        {
            return IsValidSlot(slot) && connected[slot];
        }

        /// <summary>Đánh dấu một người kết nối hoặc mất kết nối. Ném lỗi nếu ô không hợp lệ.</summary>
        public void SetConnected(int slot, bool isConnected)
        {
            if (!IsValidSlot(slot))
                throw new ArgumentOutOfRangeException("slot", slot, "Ô người chơi không hợp lệ.");
            connected[slot] = isConnected;
        }

        /// <summary>Số người đang kết nối.</summary>
        public int ConnectedCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < Size; i++)
                {
                    if (connected[i]) count++;
                }
                return count;
            }
        }

        /// <summary>Bản sao danh sách ô đang kết nối, theo thứ tự tăng dần.</summary>
        public int[] ConnectedSlots()
        {
            List<int> slots = new List<int>();
            for (int i = 0; i < Size; i++)
            {
                if (connected[i]) slots.Add(i);
            }
            return slots.ToArray();
        }
    }
}
