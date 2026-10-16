# KẾ HOẠCH T-006a: BreakerPanel - S1 Domain + test   (file: .ai/PLANS/T-006a-breaker-panel.md)

Trạng thái: ĐANG LÀM | AI giữ: Claude Sonnet 5.5 (phiên #3) | Cập nhật: 2026-10-16 00:16 UTC | Duyệt: chủ dự án duyệt thiết kế 2026-10-16 (DECISIONS/2026-10-16-t006-mission-designs-approved.md)

## 1. Mục tiêu và nghiệm thu
- Viết phần LUẬT của nhiệm vụ Phong_5 (Bảng cầu dao) bằng C# thuần: sinh đề theo seed, trạng thái công tắc/đèn, quá tải, giữ khớp 3 giây. CHƯA Presentation, CHƯA prefab, CHƯA scene.
- Nghiệm thu mức 3: NUnit EditMode xanh (đề luôn có đúng 1 nghiệm trong giới hạn quá tải với 2/3/4 người; luật từng thao tác); Console 0 lỗi.

## 2. Phạm vi
- TẠO MỚI: Assets/TheReturn/Features/BreakerPanel/Runtime/Domain/{TheReturn.BreakerPanel.Domain.asmdef, BreakerRound.cs, BreakerGenerator.cs, BreakerState.cs}; Assets/TheReturn/Features/BreakerPanel/Tests/EditMode/{TheReturn.BreakerPanel.Domain.Tests.asmdef, BreakerPanelTests.cs}.
- KHÔNG đụng: scene, prefab, code cũ, MissionRegistryFactory (đăng ký adapter khi có Presentation).
- Scene lock: không cần.

## 3. Ngữ cảnh đã đọc
| File | Kết luận |
|---|---|
| PLANS/T-006-mission-designs.md mục 2 (Phong_5) | S = số người + 2 công tắc và đèn; mỗi công tắc đảo 2-3 đèn; chỉ chủ bấm và xem công tắc đảo đèn nào; quá tải > ceil(S/2) công tắc BẬT thì nhảy; thắng khi khớp đích giữ 3 giây |
| Kiểm chứng 2026-10-16 | Ma trận khả nghịch GF(2) cho S=4,5,6 có đúng 1 nghiệm, nghiệm trong giới hạn quá tải |
| Domain asmdef mẫu (Attendance.Domain) | { name, noEngineReferences: true } |
| T-002/T-003 | Khuôn test NUnit: asmdef Editor + nunit.framework.dll + UNITY_INCLUDE_TESTS |

## 4. Subtask
| ID | Việc | File | Cách kiểm | Trạng thái | Bằng chứng |
|---|---|---|---|---|---|
| S1 | asmdef + BreakerRound/BreakerGenerator (khả nghịch, nghiệm <= giới hạn, >= 2 chủ khác nhau) | Runtime/Domain | Console 0 lỗi | DONE | Compile sạch (mức 2) |
| S2 | BreakerState: Toggle(owner, switch), quá tải, Inspect (chỉ chủ), Matches, Tick giữ 3s, Reset | Runtime/Domain | Console 0 lỗi | DONE | Compile sạch (mức 2) |
| S3 | Test NUnit | Tests/EditMode | Test Runner xanh | DONE | run_tests EditMode assembly TheReturn.BreakerPanel.Domain.Tests: 16/16 passed (mức 3) |
| S4 | Ghi nhận: thẻ hệ thống mới, TASKS, LOG | .ai/** | Đọc lại | TODO | |

## 5. Bản đồ lỗi
| Triệu chứng | Nơi xem | Cách kiểm |
|---|---|---|
| Đề không giải được / nhiều nghiệm | BreakerGenerator > Generate (kiểm khả nghịch) | Test `Generator_AlwaysHasUniqueSolution` |
| Công tắc người khác bấm được | BreakerState > Toggle (OwnerOf) | Test `Toggle_RejectsNonOwner` |
| Nhảy cầu dao sai ngưỡng | BreakerState > Toggle (OverloadLimit = ceil(S/2)) | Test `Overload_TripsAndResetsSwitches` |
| Không bao giờ thắng | BreakerState > Tick (giữ khớp liên tục HoldSeconds) | Test `Tick_SolvesAfterHold` |

## 6. Rủi ro / câu hỏi
- Presentation, prefab PF_BreakerSwitch/PF_BreakerLamp, cổng và đăng ký adapter là task kế tiếp (cần hỏi chủ dự án về scene).

## 7. Tiến độ
- 2026-10-16 00:16 UTC: kế hoạch viết xong, bắt đầu S1.
- 2026-10-16 00:20 UTC: S1-S3 xong (16/16 test xanh; đề có đúng 1 nghiệm với 150 seed x 2/3/4 người). Còn S4 (ghi nhận). Presentation/prefab/scene là task kế tiếp, cần hỏi chủ dự án.
