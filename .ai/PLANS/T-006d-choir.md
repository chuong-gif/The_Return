# KẾ HOẠCH T-006d: Choir (Phong_6) - S1 Domain + test   (file: .ai/PLANS/T-006d-choir.md)

Trạng thái: XONG PHẦN DOMAIN | AI giữ: - | Cập nhật: 2026-10-16 10:26 UTC | Duyệt: chủ dự án duyệt 2026-10-16 và yêu cầu thời gian giữa hai nốt CHỈNH ĐƯỢC

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
| S1 | asmdef + ChoirRules + ChoirRound + ChoirGenerator (6 nốt, hai nốt liền khác ký hiệu và khác bục, >= 3 ký hiệu, hoán vị ký hiệu theo seed, bộ sinh xorshift riêng để đồng bộ mạng) | Runtime/Domain | Console 0 lỗi | DONE | Compile sạch, Console 0 lỗi (mức 2) |
| S2 | ChoirState: Press(pad, now), Tick(now), ExtendForSwitch(now), ApplyRules, RemainingSeconds, TryReadCard(player, symbol), Reset | Runtime/Domain | Console 0 lỗi | DONE | Compile sạch, Console 0 lỗi (mức 2) |
| S3 | Test NUnit (gồm hạn chót chỉnh được, 0 = không giới hạn, cộng giờ khi đổi vai, đổi quy tắc lúc đang chơi) | Tests/EditMode/ChoirTests.cs | Test Runner xanh | DONE | run_tests EditMode TheReturn.Choir.Domain.Tests: 24/24 passed (0,69 s); chạy chung Breaker+Choir+SchoolFloor.Domain: 83/83 passed (mức 3) |
| S4 | Ghi nhận: TASKS, LOG, CURRENT, thẻ Choir, TESTING | .ai/** | Đọc lại | DONE | Xong 2026-10-16 10:26 UTC |

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

## 8. Làm rõ và bước kế tiếp (2026-10-16 10:26 UTC, phiên #3 tiếp quản)
- LÀM RÕ thiết kế: bục chỉ hiện SỐ 1-4; ký hiệu hình chỉ nằm trên bản nhạc (tường) và trên thẻ nhạc. (Đề xuất gốc ghi bục có cả số lẫn ký hiệu: nếu vậy người chơi khớp thẳng ký hiệu với bục và thẻ nhạc vô nghĩa.) Domain đã theo cách này (ChoirRound.PadOfSymbol chỉ lộ qua ChoirState.TryReadCard cho chủ thẻ).
- Thông số chỉnh được: `ChoirRules(StepWindowSeconds, SwitchExtraSeconds)`. Mặc định phát triển 5 giây + 1,5 giây mỗi lần đổi vai; `ChoirRules.Online` = 2 giây, không cộng thêm; 0 = không giới hạn. Tổng cộng thêm cho một nốt không quá StepWindowSeconds. Đổi lúc đang chơi: `ChoirState.ApplyRules` (áp dụng cho nốt sau).
- Presentation gọi `ChoirState.ExtendForSwitch(now)` MỖI LẦN người chơi đổi vai (phím 1-4) khi đang dở dãy; gọi `Tick(now)` mỗi frame; `RemainingSeconds(now)` cho thanh đếm ngược HUD.
- CHƯA làm (task tiếp, ID T-006d2): `ChoirSettings` ScriptableObject (hiện StepWindowSeconds/SwitchExtraSeconds trong Inspector, có ô chọn preset Dev/Online), Presentation (bục PF_ChoirPad, bản nhạc trên tường, HUD thẻ nhạc + đếm ngược), `ChoirMissionAdapter` + đăng ký vào MissionRegistryFactory (khu Phong_6), cảnh trong scene. Cần Scene lock + snapshot; Phong_6 đã có 3 cổng luôn mở nhưng CHƯA có quyết định cho AI sửa scene (DECISIONS 2026-10-16 mục 4) -> hỏi chủ dự án trước.
- Ràng buộc «hai nốt liền nhau không buộc chạy quá xa» (kiểm khoảng cách bục thật) thuộc Presentation/scene, chưa làm; Domain chỉ đảm bảo hai nốt liền khác bục.
