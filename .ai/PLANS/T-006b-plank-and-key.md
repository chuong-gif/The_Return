# KẾ HOẠCH T-006b: PlankAndKey (Hanh_Lang_6) - S1 Domain + test   (file: .ai/PLANS/T-006b-plank-and-key.md)
Trạng thái: XONG phần Domain (S1-S4); Presentation/scene = T-006b2 (BLOCKED) | AI giữ: Claude Sonnet 5.5 (phiên MCP #2) | Cập nhật: 2026-10-16 11:00 UTC | Duyệt: chủ dự án duyệt thiết kế 2026-10-16 (DECISIONS/2026-10-16-t006-mission-designs-approved.md, mục 1 và 3)
## 1. Mục tiêu và nghiệm thu
- Viết phần LUẬT của nhiệm vụ Hanh_Lang_6 (Cầu ván và thẻ từ) bằng C# thuần: sinh đề theo seed, bộ giải tối ưu, trạng thái qua cầu (ván chứa tối đa 2 người, thẻ phải đi cùng nhóm qua cầu, mỗi lượt tốn thời gian của người chậm nhất, hạn tổng). CHƯA Presentation, CHƯA prefab (PF_PlankLift, PF_KeyCard), CHƯA scene.
- Nghiệm thu mức 3: NUnit EditMode xanh, gồm: bộ giải khớp số liệu đã duyệt (min 8/17/17/11 cho [1,2,5], [1,2,5,10], [2,3,4,6], [1,2,3,4]) và khớp một bộ kiểm vét cạn độc lập trong test; phát lại kế hoạch tối ưu qua PlankState thì THẮNG đúng hạn; Console 0 lỗi.
## 2. Phạm vi
- TẠO MỚI (không có file cũ bị sửa): Assets/TheReturn/Features/PlankAndKey/Runtime/Domain/{TheReturn.PlankAndKey.Domain.asmdef, PlankSolver.cs, PlankRound.cs, PlankGenerator.cs, PlankState.cs}; Assets/TheReturn/Features/PlankAndKey/Tests/EditMode/{TheReturn.PlankAndKey.Domain.Tests.asmdef, PlankAndKeyTests.cs}.
- KHÔNG đụng: scene, prefab, code cũ, MissionRegistryFactory (đăng ký adapter khi có Presentation), file của T-006a/T-006d (phiên #3).
- Scene lock: không cần.
## 3. Quyết định thiết kế trong phạm vi Domain
- Đơn vị thời gian: "đơn vị" (units), KHÔNG gọi là giây vì thiết kế chỉ nêu số (min 8/17...) và thời lượng nhiệm vụ 1-1,5 phút; hệ số đổi ra giây thật thuộc Data/Presentation (chưa chốt).
- Đồng hồ tổng = tổng thời gian các lượt qua cầu, KHÔNG tính thời gian suy nghĩ. Lý do: hạn = min + 20-25% (thiết kế mục 3) chỉ đạt được nếu chỉ tính thời gian qua cầu. Cần chủ dự án xác nhận khi có Presentation/playtest.
- Hạn = min + ceil(min * slack%), slack mặc định 25% và là THAM SỐ (không hardcode, giống tinh thần Choir). Lượt làm vượt hạn thì thất bại và KHÔNG được thực hiện.
- Đội 2 người: giữ bản nhẹ (decision mục 3): cả hai qua cùng lúc, min = người chậm hơn.
- Bộ tốc độ (catalog theo số người, gán cho người chơi bằng xáo theo seed): 2 người {2,5} {3,6} {2,4}; 3 người {1,2,5} {1,3,6} {2,3,6}; 4 người {1,2,5,10} {2,3,4,6} {1,2,3,4}. Chỉ các bộ đã có trong thiết kế (8/17/17/11) được đối chiếu với số liệu duyệt; các bộ còn lại do bộ giải tính, chưa duyệt về độ khó (cần playtest).
- Mọi luật nằm trong Domain (host quyết định); Presentation sau chỉ hiển thị.
## 4. Subtask
| ID | Việc | File | Cách kiểm | Trạng thái | Bằng chứng |
|---|---|---|---|---|---|
| S1 | asmdef + PlankSolver (Dijkstra trạng thái người-ở-bờ x thẻ-ở-bờ) + PlankRound + PlankGenerator | Runtime/Domain | Compile, Console 0 lỗi | DONE | Compile sạch, assembly TheReturn.PlankAndKey.Domain nạp (mức 2) |
| S2 | PlankState: TryCross(a,b), kiểm bờ/thẻ/hạn, thắng/thất bại, Reset | Runtime/Domain | Compile, Console 0 lỗi | DONE | Compile sạch (mức 2) |
| S3 | Test NUnit (bộ giải, sinh đề, luật qua cầu, phát lại kế hoạch tối ưu) | Tests/EditMode | Test Runner xanh | DONE | run_tests EditMode assembly TheReturn.PlankAndKey.Domain.Tests: 25/25 passed, 0.71 s (mức 3) |
| S4 | Ghi nhận: thẻ hệ thống, TASKS, LOG | .ai/** | Đọc lại | DONE | Thẻ SYSTEM_CARDS/PlankAndKey.md + INDEX + TASKS + LOG |
## 5. Bản đồ lỗi
| Triệu chứng | Nơi xem | Cách kiểm |
|---|---|---|
| Hạn quá chặt / không thể thắng | PlankRound (OptimalUnits, DeadlineUnits), PlankSolver.Solve | Test bộ giải khớp số liệu duyệt và test phát lại kế hoạch tối ưu |
| Qua cầu mà không có thẻ | PlankState.TryCross (CardNotWithGroup) | Test từ chối khi thẻ ở bờ kia |
| Người ở bờ kia vẫn qua được | PlankState.TryCross (WrongBank) | Test từ chối khi khác bờ |
| Thắng sớm/muộn sai | PlankState.TryCross (điều kiện mọi người ở bờ xa) | Test thắng đúng ở lượt cuối |
| Cùng seed ra đề khác | PlankGenerator.Generate | Test tất định theo seed |
## 6. Rủi ro / câu hỏi cho chủ dự án
- Hệ số đổi "đơn vị" ra giây thật và việc đồng hồ chỉ tính thời gian qua cầu (mục 3): cần xác nhận khi có Presentation.
- Presentation, prefab PF_PlankLift/PF_KeyCard, cổng + khu chuẩn bị cho Hanh_Lang_6 và đăng ký adapter là task kế tiếp; mọi bước sửa scene phải hỏi lại chủ dự án (DECISIONS mục 4).
- Compile script khi scene đang dirty (chưa lưu): Unity giữ scene trong bộ nhớ qua domain reload; không lưu scene và không dừng/điều khiển Play Mode khi chủ dự án đang thử.
## 7. Tiến độ
- 2026-10-16 01:05 UTC: kế hoạch viết xong, bắt đầu S1.
- 2026-10-16 11:00 UTC: S1-S4 xong. 25/25 test xanh (mức 3), Console 0 lỗi/cảnh báo. Bộ giải khớp số liệu duyệt (8/17/17/11 với mọi hoán vị) và khớp bộ kiểm vét cạn độc lập trên toàn danh mục. CHƯA chạy trong scene (mức 4 chưa có).
## 8. Hạn chế đã biết và việc tiếp theo (T-006b2)
- PlankGenerator dùng System.Random(seed): cùng seed có thể cho đề KHÁC giữa các máy/phiên bản runtime. Choir dùng xorshift riêng vì lý do này. Nên đổi sang bộ sinh số tự viết khi Presentation/mạng cần cùng đề trên nhiều máy (đổi nhỏ, thêm test ghim đề với seed cố định). Chưa làm vì chưa cần và để tránh compile thêm khi các phiên khác đang làm việc.
- Chưa chốt: hệ số đổi «đơn vị» ra giây thật; đồng hồ chỉ tính thời gian qua cầu (mục 3); độ khó/độ vui (cần playtest). Với 2 người nhiệm vụ gần như một lượt qua (bản nhẹ đã được chấp nhận mặc định).
- T-006b2 (BLOCKED): Presentation (ván, thẻ, đồng hồ, HUD), prefab PF_PlankLift/PF_KeyCard, PlankMissionAdapter + đăng ký MissionRegistryFactory, cổng + khu chuẩn bị Hanh_Lang_6. Cần chủ dự án cho phép sửa scene cho Hanh_Lang_6 (DECISIONS 2026-10-16 mục 4; hiện chỉ Phong_6 được cho phép), kèm Scene lock + snapshot.
