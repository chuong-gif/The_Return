# CARD: BreakerPanel (Bảng cầu dao)

Khu / vai trò: Phong_5 (nhiệm vụ MỚI, T-006a/T-006a2). Cập nhật: 2026-10-18 09:35 UTC. Trạng thái: Domain + Presentation + adapter + scene XONG (mức 3); CHƯA Play Mode thật.

## A. Bản đồ
- Domain (asmdef TheReturn.BreakerPanel.Domain): BreakerRound (ma trận công tắc->đèn, chủ i % số người), BreakerGenerator (khả nghịch GF(2), nghiệm duy nhất, <= giới hạn quá tải), BreakerState (Toggle, Inspect chỉ chủ, quá tải, Tick giữ khớp). Test: Features/BreakerPanel/Tests/EditMode/BreakerPanelTests.cs (16 test).
- Runtime (asmdef TheReturn.BreakerPanel): Data/BreakerSettings.cs + Data/BreakerPanel_VI.asset (holdSeconds 3, fixedSeed 0, chữ); Presentation/BreakerSwitch.cs, BreakerLamp.cs (bóng + chấm đích), BreakerPanelPrototype.cs (E bấm công tắc, R làm lại, phím 1-4 đổi vai, HUD công tắc của vai đang chọn + đèn nó đảo, quá tải, % giữ khớp). File thừa rỗng: Presentation/BreakerParts.cs (xóa tay được).
- Adapter: Maps/SchoolFloor/Runtime/Missions/BreakerPanelMissionAdapter.cs; đăng ký MissionRegistryFactory (BreakerPanelArea=Phong_5) khi SchoolFloorFlow.breakerPanel != null; RestartMap gọi breakerPanel.StopSession().
- Scene: Breaker_LogicOnly tại (110,11.1,23.5): 6 BreakerSwitch (z 17..30) + 6 BreakerLamp, quay mặt về phía tây. Snapshot: Scenes/BeforeBreaker_20261017_125153.unity.
- Thiết kế: PLANS/T-006-mission-designs.md mục 2; PLANS/T-006a-breaker-panel.md.

## B. Triệu chứng -> nơi xem
| Triệu chứng | Nơi xem | Cách kiểm |
|---|---|---|
| Công tắc người khác bấm được | BreakerState.Toggle (OwnerOf) | Test Toggle_RejectsNonOwner |
| Cầu dao nhảy sai ngưỡng | OverloadLimit = ceil(S/2) trong BreakerRound | Test Overload_* |
| Không thắng được | BreakerState.Tick (HoldSeconds liên tục); holdSeconds trong asset | Test Tick_SolvesAfterHold |
| Đề không giải được | BreakerGenerator (khả nghịch) | Test Generator_AlwaysHasUniqueSolution |
| Phong_5 không bắt đầu | SchoolFloorFlow.breakerPanel null? factory đăng ký? cổng Phong 5_Cua 1/2/3 | Console, Registry |
| Công tắc thừa/thiếu | BreakerPanelPrototype.RefreshParts ẩn công tắc/đèn vượt số người + 2 | Đổi số người trong Play |

## C. Ghi chú đã xác minh
- 2026-10-17: compile sạch; EditMode 83/83 (BreakerPanel+Choir+SchoolFloor Domain); registry dựng được với flow thật. Chưa chơi thật trong Play Mode.

## D. Task liên quan
- T-006a DONE (Domain); T-006a2 REVIEW (chờ chủ dự án chơi thử).
