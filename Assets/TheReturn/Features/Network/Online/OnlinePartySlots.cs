/*
 * Mục đích: Cấu hình ô nhân vật online của một ván (T-025): số người của đội (2-4) và điểm xuất phát từng ô. Trên HOST còn giữ bảng
 * chia ô cố định (SlotAllocator): mỗi người một ô cả ván, không đổi vai. Một đối tượng duy nhất trong scene online.
 * Tham số dòng lệnh -teamsize N (2-4) ghi đè số người khi chạy bản build thử.
 * Hàm: SetTeamSize; AssignSlot; ReleaseSlot; SlotOf; TryGetClient; SpawnPositionFor; thuộc tính TeamSize, Roster, Instance.
 */
using TheReturn.Network.Framework;
using UnityEngine;

namespace TheReturn.Network.Online
{
    public sealed class OnlinePartySlots : MonoBehaviour
    {
        [SerializeField] private int teamSize = 4;
        [SerializeField] private Transform[] spawnPoints;

        private SlotAllocator allocator;

        /// <summary>Đối tượng cấu hình đang hoạt động trong scene (null nếu chưa có).</summary>
        public static OnlinePartySlots Instance { get; private set; }

        /// <summary>Số người của đội (2-4).</summary>
        public int TeamSize { get { return teamSize; } }

        private void Awake()
        {
            Instance = this;
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i + 1 < args.Length; i++)
            {
                int size;
                if (args[i] == "-teamsize" && int.TryParse(args[i + 1], out size)) SetTeamSize(size);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>Nhận số người (kẹp về 2-4) và xóa bảng chia ô cũ. Gọi trước khi người chơi vào.</summary>
        public void SetTeamSize(int size)
        {
            teamSize = Mathf.Clamp(size, 2, 4);
            allocator = null;
        }

        /// <summary>Chỉ HOST gọi. Nhận mã client; trả ô cố định của người đó, hoặc -1 nếu đội đã đủ người.</summary>
        public int AssignSlot(ulong clientId)
        {
            if (allocator == null) allocator = new SlotAllocator(Mathf.Clamp(teamSize, 2, 4));
            return allocator.Assign(clientId);
        }

        /// <summary>Chỉ HOST gọi. Nhận mã client; trả ô của người đó khi họ rời (các ô khác không đổi).</summary>
        public void ReleaseSlot(ulong clientId)
        {
            if (allocator != null) allocator.Release(clientId);
        }

        /// <summary>Đội đang chơi (2-4 ô), ô nào đã có người thì ở trạng thái đang kết nối. Dùng chung với HostAuthority trên host.</summary>
        public TeamRoster Roster
        {
            get
            {
                if (allocator == null) allocator = new SlotAllocator(Mathf.Clamp(teamSize, 2, 4));
                return allocator.Roster;
            }
        }

        /// <summary>Chỉ HOST. Nhận mã client; trả ô người đó đang giữ, hoặc -1 nếu chưa có.</summary>
        public int SlotOf(ulong clientId)
        {
            return allocator != null ? allocator.SlotOf(clientId) : -1;
        }

        /// <summary>Chỉ HOST. Nhận ô; trả true và mã client nếu ô đang có người.</summary>
        public bool TryGetClient(int slot, out ulong clientId)
        {
            clientId = 0;
            return allocator != null && allocator.TryGetClient(slot, out clientId);
        }

        /// <summary>Nhận ô; trả vị trí xuất phát (điểm gán sẵn, hoặc hàng ngang mặc định nếu chưa gán).</summary>
        public Vector3 SpawnPositionFor(int slot)
        {
            if (spawnPoints != null && slot >= 0 && slot < spawnPoints.Length && spawnPoints[slot] != null)
                return spawnPoints[slot].position;
            return new Vector3(slot * 1.5f, 0.1f, 0f);
        }
    }
}
