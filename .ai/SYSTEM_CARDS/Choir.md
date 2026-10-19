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
