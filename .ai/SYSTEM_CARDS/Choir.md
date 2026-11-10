# CARD: Choir (Hợp xướng)

Khu / vai trò: Phong_6 (nhiệm vụ MỚI, T-006d/T-006d2). Cập nhật: 2026-10-18 09:35 UTC. Trạng thái: Domain + Presentation + adapter + scene XONG (mức 3); CHƯA Play Mode thật.

## A. Bản đồ
- Domain (asmdef TheReturn.Choir.Domain): ChoirRules, ChoirRound, ChoirGenerator (xorshift), ChoirState. Test: Features/Choir/Tests/EditMode/ChoirTests.cs (24 test).
- Runtime (asmdef TheReturn.Choir): Data/ChoirSettings.cs + Data/Choir_VI.asset (stepWindowSeconds 5, switchExtraSeconds 1.5, fixedSeed, chữ); Presentation/ChoirPad.cs (bục + số 1-4, nháy đúng/sai), Presentation/ChoirPrototype.cs (E bấm bục, R làm lại, phím 1-4 đổi vai -> ExtendForSwitch, HUD thẻ nhạc vai đang chọn, bảng nhạc xoay về camera, OnGUI).
- Adapter: Maps/SchoolFloor/Runtime/Missions/ChoirMissionAdapter.cs; đăng ký MissionRegistryFactory (ChoirArea=Phong_6) khi SchoolFloorFlow.choir != null; RestartMap gọi choir.StopSession() (bỏ đề cũ).
- Scene: object gốc Choir_LogicOnly (4 ChoirPad_1..4 quanh (46,11.2,121.5), ScoreBoard + 6 Note_, StatusText, ChoirPrototype). Snapshot trước khi dựng: Scenes/BeforeChoir_20261017_023150.unity.
- Thiết kế: PLANS/T-006-mission-designs.md mục 2; PLANS/T-006d-choir.md; PLANS/T-006d2-choir-presentation.md.

## B. Triệu chứng -> nơi xem
| Triệu chứng | Nơi xem | Cách kiểm |
|---|---|---|
| Không kịp bấm nốt kế khi đổi vai bằng phím | Choir_VI.asset (stepWindowSeconds, switchExtraSeconds; 0 = không giới hạn); ChoirPrototype.Update gọi ExtendForSwitch khi ActivePlayer đổi | Chỉnh asset ngay khi Play (áp lại mỗi frame) |
| Dãy tự reset | ChoirState.Tick/Press | Timeouts, RemainingSeconds |
| Bấm đúng mà báo sai | ChoirRound.SolutionPad; bục hiện số = chỉ số + 1 | Test Sequence_* |
| Người chơi đọc thẻ người khác | ChoirState.TryReadCard (symbol % số người) | Test Cards_OnlyOwnerCanRead; HUD chỉ hiện thẻ của vai đang chọn |
| Phong_6 không bắt đầu | SchoolFloorFlow.choir null? MissionRegistryFactory đăng ký? cổng Phong 6_Cua 1/2/3 trong MissionFlowController | Registry.IsRegistered / Console |
| Sau Chơi lại map vẫn dở dãy | SchoolFloorFlow.RestartMap -> choir.StopSession(true) | Mở lại khu, đề phải mới |
| Bảng nhạc/bục bị che hoặc xa nhau | Vị trí con của Choir_LogicOnly (chỉnh Inspector) | 4 bục cách nhau 3,2 m |

## C. Ghi chú đã xác minh
- 2026-10-17: compile sạch; EditMode 67/67 (Choir+SchoolFloor Domain). Chưa chơi thật trong Play Mode (mức 4 chưa có). Ký hiệu hiện là chữ A-D (TextMesh), có thể đổi sang biểu tượng.

## D. Task liên quan
- T-006d DONE (Domain); T-006d2 REVIEW (chờ chủ dự án chơi thử).

## E. Mạng (T-037 GĐ1, phiên #14, 2026-11-07; mức M3)
- Lớp thuần C# (asmdef TheReturn.Choir.Network, namespace TheReturn.ChoirNet, noEngine): Runtime/Network/{ChoirCommand (Press/Retry), IChoirWorld (IsNearPad), ChoirPublicState (+ChoirPrivateState), ChoirHostRules (IHostRules), ChoirCodec}.cs. Domain KHÔNG đổi; cửa sổ giữa hai nốt = ChoirRules.OnlineFor(N) = 3 / 2,5 / 2 giây (P-003).
- Chung: bản nhạc, nốt tới, sai/quá giờ, cửa sổ, thông báo. RIÊNG: thẻ nhạc «ký hiệu=bục» của mình. Đồng hồ KHÔNG nằm trong trạng thái chung (bất biến theo Version, bài học F-001 ở BreakerPanel); client tự đếm từ lúc nhận gói có Progress mới.
- Host: Press kiểm ô hợp lệ, bục 0-3, IsNearPad (vị trí host thấy, không tin client), chưa thắng; Tick(delta) quá cửa sổ thì dãy về đầu (trả true để Online host gọi MarkChanged). Quyết định P-004: bấm E gần bục.
- Test: Tests/EditModeNet (TheReturn.Choir.Network.Tests): ChoirHostRulesTests, ChoirCodecTests, ChoirContractAdapter (bộ hợp đồng C1-C9+N1; C7 bỏ qua vì Choir không có «thua»).
- CÒN (GĐ2, sau Q-007 lớp cơ sở Online): Runtime/Online (IChoirWorld từ OnlineAvatar.transform.position, bán kính «gần bục» đo trong scene, Tick mỗi frame, HUD thẻ riêng + đồng hồ ước lượng, phím E/R), scene thử NetworkChoirTest, LAN 3 tiến trình thắng/thua/quá giờ + ca client ở xa bị bác (M4). Rủi ro: cửa sổ 2-3 giây rất nhạy với độ trễ khi chơi qua Internet (vị trí host thấy trễ hơn); cần thử Relay (M5), không tự nới luật.
