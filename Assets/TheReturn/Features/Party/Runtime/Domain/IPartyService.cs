/*
 * Mục đích: Hợp đồng giữa giao diện nhóm chơi và nơi giữ luật (T-008). Hiện chỉ có LocalPartyService (một máy); khi chọn công nghệ mạng
 * chỉ cần viết một lớp khác cài IPartyService, giao diện không đổi. Thuần C#.
 * Mọi lệnh trả PartyResult; trạng thái nhóm hiện tại ở Current (null khi chưa ở trong nhóm); Changed bắn sau mỗi thay đổi (null = đã rời nhóm).
 */
using System;

namespace TheReturn
{
    /// <summary>Dịch vụ nhóm chơi: tạo, vào, sẵn sàng, rời, bắt đầu.</summary>
    public interface IPartyService
    {
        string LocalPlayerId { get; }
        PartySnapshot Current { get; }
        event Action<PartySnapshot> Changed;

        /// <summary>Nhận tên hiển thị; tạo nhóm mới và người chơi trên máy này làm chủ phòng.</summary>
        PartyResult CreateParty(string displayName);

        /// <summary>Nhận mã phòng và tên hiển thị; vào nhóm đã có.</summary>
        PartyResult JoinParty(string code, string displayName);

        /// <summary>Nhận trạng thái; đặt sẵn sàng cho người chơi trên máy này.</summary>
        PartyResult SetReady(bool ready);

        /// <summary>Không nhận tham số; rời nhóm hiện tại.</summary>
        PartyResult Leave();

        /// <summary>Không nhận tham số; chủ phòng yêu cầu bắt đầu trò chơi.</summary>
        PartyResult StartGame();
    }
}
