# CARD: NetworkFramework (khung mạng host-authoritative, T-024)

Khu / vai trò: lớp thuần C# dưới mạng thật, dùng cho mọi nhiệm vụ online. Cập nhật: 2026-10-30 02:54 UTC. Trạng thái: code + 67 test EditMode xanh (mức 3); CHƯA nối NGO, CHƯA nhiệm vụ nào dùng (đó là T-025/T-027).

## A. Bản đồ
- Assets/TheReturn/Features/Network/Runtime/Framework (asmdef TheReturn.Network.Framework, noEngineReferences, KHÔNG phụ thuộc TheReturn.Network/NGO):
  - TeamRoster: đội CỐ ĐỊNH 2-4 người, ô 0..Size-1, ai đang kết nối.
  - IHostRules<TCommand,TPublic,TPrivate>: mỗi nhiệm vụ viết MỘT lớp luật: Validate (không đổi trạng thái), Apply, BuildPublicState (bản bất biến/sao), BuildPrivateState(slot).
  - HostAuthority<,,>: Submit(ô, số thứ tự, lệnh) -> CommandResult; BuildBroadcast (mỗi người đang kết nối một StateMessage); BuildStateFor(ô) (vào muộn/vào lại); ConnectSlot/DisconnectSlot; MarkChanged (đổi do đồng hồ, không do lệnh); Version tăng 1 mỗi lệnh được chấp nhận.
  - ClientReplica<,>: NextSequence (1,2,3...), Receive (bỏ gói ô khác/cũ/trùng/rỗng), Reset (khi kết nối lại).
  - CommandResult / RejectReason / RuleVerdict (CommandResult.cs), StateMessage + NoPrivate, HostLog (nhật ký vòng, Sink để nối Debug.Log).
- Test: Features/Network/Tests/EditMode (asmdef TheReturn.Network.Framework.Tests): TeamRosterTests, HostAuthorityTests, ClientReplicaTests, HostLogTests, NetworkFrameworkEndToEndTests; luật giả ở NetworkTestRules.cs (ClaimRules, EchoRules). NetworkFrameworkTestKit.cs là file thừa (chỉ còn 2 dòng chú thích), xóa tay được.
- Kế hoạch/phiếu: PLANS/T-024-brief.md; kiến trúc: PLANS/T-009-online-coop.md mục 2 và 8.

## B. Triệu chứng -> nơi xem
| Triệu chứng | Nơi xem | Cách kiểm |
|---|---|---|
| Lệnh bị bỏ không rõ lý do | HostAuthority.Submit (thứ tự kiểm: ô hợp lệ, còn kết nối, lệnh không null, số thứ tự, luật); host.Log (mỗi lệnh bị từ chối ghi 1 dòng Warning kèm RejectReason) | Test Submit_*, Log_RejectionWrittenAsWarningWithSlotAndReason |
| Lệnh bị coi là trùng (StaleSequence) sau khi vào lại | Máy khách chưa Reset hoặc host chưa ConnectSlot (host đặt lại số thứ tự của ô khi ConnectSlot) | Test ConnectSlot_ResetsSequenceSoRejoinedClientStartsAtOne |
| Người chơi thấy thông tin riêng của người khác | BuildBroadcast/BuildStateFor chỉ gọi BuildPrivateState đúng ô nhận; kiểm luật nhiệm vụ không nhét dữ liệu người khác vào TPublic | Test Broadcast_*, EndToEnd_PrivateInfoNeverLeaksToOtherPlayers |
| Máy khách không cập nhật | ClientReplica.Receive trả false: gói của ô khác, version không mới hơn, hoặc null | Test Replica_* |
| Người mất kết nối không nhận gói | Cố ý: BuildBroadcast bỏ ô không kết nối; dùng BuildStateFor + ClientReplica.Reset khi họ vào lại | Test EndToEnd_DisconnectedPlayerMissesUpdates_ReconnectCatchesUp |
| Luật ném lỗi | Submit bắt lỗi -> RuleError; nếu lỗi ở Apply, Version vẫn tăng vì trạng thái có thể đã đổi một phần | Test Submit_ValidateThrows_*, Submit_ApplyThrows* |

## C. Ghi chú đã xác minh
- 2026-10-30 02:54 UTC (phiên #9): 67/67 test EditMode xanh (mức 3), compile sạch; đúng với đội 2/3/4. Chưa nối NGO nên chưa có mức 4.
- Chưa làm (cố ý, ngoài phạm vi T-024): tuần tự hóa lệnh/gói qua NGO, RPC, kiểm tra thời gian thực (Tick), đồng bộ thời gian, chuyển host (T-030).
- Quy ước cho T-025/T-027: người gửi LUÔN lấy từ kết nối (ô), không đọc từ nội dung lệnh; luật nhiệm vụ nhận N từ TeamRoster.Size (không đổi vai, nhân vật = số người).

## D. Task liên quan
- T-024 (khung, REVIEW). Dùng bởi: T-025 (tách PrototypePartyController), T-027 (nối nhiệm vụ mẫu PlankAndKey). Liên quan: T-009 (NGO/LAN), T-030 (chuyển host dùng BuildStateFor).
