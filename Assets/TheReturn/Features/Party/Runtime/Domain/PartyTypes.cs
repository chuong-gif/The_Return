/*
 * Mục đích: Các kiểu dữ liệu dùng chung của hệ thống nhóm chơi (T-008): mã kết quả, thông tin thành viên và ảnh chụp bất biến của nhóm.
 * Thuần C#, không phụ thuộc Unity; giao diện và (sau này) mạng chỉ đọc PartySnapshot, không sửa trực tiếp luật.
 * Kiểu: PartyResult (enum), PartyMemberInfo, PartySnapshot.
 */
using System;
using System.Collections.Generic;

namespace TheReturn
{
    /// <summary>Kết quả một thao tác trên nhóm chơi.</summary>
    public enum PartyResult
    {
        Ok,
        NoParty,
        AlreadyInParty,
        NotFound,
        Full,
        Started,
        InvalidName,
        InvalidCode,
        NotHost,
        NotEnoughPlayers,
        NotAllReady
    }

    /// <summary>Thông tin bất biến của một thành viên trong nhóm.</summary>
    public sealed class PartyMemberInfo
    {
        public string Id { get; }
        public string Name { get; }
        public int Slot { get; }
        public bool IsHost { get; }
        public bool Ready { get; }
        public bool IsLocal { get; }

        /// <summary>Nhận mã người, tên, vị trí 0-3, cờ chủ phòng, cờ sẵn sàng và cờ là người chơi trên máy này.</summary>
        public PartyMemberInfo(string id, string name, int slot, bool isHost, bool ready, bool isLocal)
        {
            Id = id;
            Name = name;
            Slot = slot;
            IsHost = isHost;
            Ready = ready;
            IsLocal = isLocal;
        }
    }

    /// <summary>Ảnh chụp bất biến của nhóm tại một thời điểm (thành viên đã sắp theo vị trí 0-3).</summary>
    public sealed class PartySnapshot
    {
        readonly PartyMemberInfo[] members;

        public string Code { get; }
        public bool Started { get; }
        public bool CanStart { get; }
        public IReadOnlyList<PartyMemberInfo> Members { get { return members; } }
        public int Count { get { return members.Length; } }

        /// <summary>Nhận mã phòng, danh sách thành viên đã sắp, cờ đã bắt đầu và cờ đủ điều kiện bắt đầu.</summary>
        public PartySnapshot(string code, PartyMemberInfo[] members, bool started, bool canStart)
        {
            if (members == null) throw new ArgumentNullException(nameof(members));
            Code = code;
            this.members = (PartyMemberInfo[])members.Clone();
            Started = started;
            CanStart = canStart;
        }

        /// <summary>Vị trí (0-3) của người chơi trên máy này; -1 nếu không có.</summary>
        public int LocalSlot
        {
            get
            {
                foreach (PartyMemberInfo m in members) if (m.IsLocal) return m.Slot;
                return -1;
            }
        }

        /// <summary>True nếu người chơi trên máy này là chủ phòng.</summary>
        public bool IsLocalHost
        {
            get
            {
                foreach (PartyMemberInfo m in members) if (m.IsLocal && m.IsHost) return true;
                return false;
            }
        }

        /// <summary>Nhận vị trí 0-3; trả thành viên ở vị trí đó hoặc null nếu trống.</summary>
        public PartyMemberInfo MemberAtSlot(int slot)
        {
            foreach (PartyMemberInfo m in members) if (m.Slot == slot) return m;
            return null;
        }
    }
}
