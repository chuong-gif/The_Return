# KẾ HOẠCH T-006d: Choir (Phong_6) - S1 Domain + test   (file: .ai/PLANS/T-006d-choir.md)

Trạng thái: ĐANG LÀM | AI giữ: Claude Sonnet 5.5 (phiên #3) | Cập nhật: 2026-10-16 00:16 UTC | Duyệt: chủ dự án duyệt 2026-10-16 và yêu cầu thời gian giữa hai nốt CHỈNH ĐƯỢC

## 1. Mục tiêu và nghiệm thu
- Viết phần LUẬT của nhiệm vụ Phong_6 (Hợp xướng) bằng C# thuần: sinh đề theo seed, chia thẻ nhạc theo số người, bấm bục theo dãy với hạn chót chỉnh được. CHƯA Presentation/prefab/scene.
- Chỉnh được (theo chủ dự án): `ChoirRules.StepWindowSeconds` (0 = không giới hạn) và `SwitchExtraSeconds` (cộng thêm mỗi lần đổi vai khi đang dở dãy). Mặc định giai đoạn phát triển 5 giây và 1,5 giây.
- Nghiệm thu mức 3: NUnit EditMode xanh; Console 0 lỗi.

## 2. Phạm vi
- TẠO MỚI: Assets/TheReturn/Features/Choir/Runtime/Domain/{TheReturn.Choir.Domain.asmdef, ChoirRules.cs, ChoirRound.cs, ChoirGenerator.cs, ChoirState.cs}; Assets/TheReturn/Features/Choir/Tests/EditMode/{TheReturn.Choir.Domain.Tests.asmdef, ChoirTests.cs}.
- KHÔNG đụng scene, prefab, code cũ. Scene lock: không cần.

## 3. Ngữ cảnh đã đọc
| File | Kết luận |
|---|---|
| PLANS/T-006-mission-designs.md mục 2 (Phong_6) | 4 bục số 1-4 + 4 ký hiệu; dãy 6 nốt; thẻ nhạc chia theo số người (owner = thẻ % số người); sai bục/quá giờ thì dãy về đầu, thẻ giữ nguyên |
| OWNER_UPDATES 2026-10-16 | Khoảng thời gian giữa hai nốt phải chỉnh được vì đang đổi vai bằng phím 1-4 trên một máy |
| T-006a | Cùng khuôn asmdef/test |

## 4. Subtask
| ID | Việc | File | Cách kiểm | Trạng thái | Bằng chứng |
|---|---|---|---|---|---|
| S1 | asmdef + ChoirRules + ChoirRound + ChoirGenerator (6 nốt, hai nốt liền khác bục, >= 3 bục khác nhau, hoán vị ký hiệu theo seed) | Runtime/Domain | Console 0 lỗi | TODO | |
| S2 | ChoirState: Press(pad, now), Tick(now), ExtendDeadline khi đổi vai, Solved, CardInfo(player) | Runtime/Domain | Console 0 lỗi | TODO | |
| S3 | Test NUnit (gồm hạn chót chỉnh được, 0 = không giới hạn, cộng giờ khi đổi vai) | Tests/EditMode | Test Runner xanh | TODO | |
| S4 | Ghi nhận: TASKS, LOG | .ai/** | Đọc lại | TODO | |

## 5. Bản đồ lỗi
| Triệu chứng | Nơi xem | Cách kiểm |
|---|---|---|
| Không kịp bấm khi đổi vai | ChoirRules (StepWindowSeconds, SwitchExtraSeconds) -> sau này asset Data | Tăng giá trị; 0 = tắt giới hạn |
| Dãy reset sai lúc | ChoirState > Tick/Press (deadline) | Test `Window_*`, `SwitchExtra_*` |
| Dịch bản nhạc sai | ChoirRound (PadOfSymbol) / ChoirState > CardInfo | Test `Round_CardsCoverAllPads` |

## 6. Rủi ro / câu hỏi
- Trình sinh chưa biết khoảng cách bục trong scene: ràng buộc «hai nốt liền nhau không buộc một người chạy quá xa» thuộc Presentation/scene (cần quyết định khi có bục thật).

## 7. Tiến độ
- 2026-10-16 00:16 UTC: kế hoạch viết xong, bắt đầu S1.
