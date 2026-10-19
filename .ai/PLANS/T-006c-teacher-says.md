# KẾ HOẠCH T-006c: TeacherSays (Hanh_Lang_5 «Cô nói»)   (file: .ai/PLANS/T-006c-teacher-says.md)

Trạng thái: ĐANG LÀM | AI giữ: Claude Sonnet 5.5 (phiên #4) | Cập nhật: 2026-10-16 10:48 UTC | Duyệt: thiết kế đã duyệt 2026-10-16 (DECISIONS/2026-10-16-t006-mission-designs-approved.md); sửa scene cho Hanh_Lang_5 được phép (mục 7)

## 1. Mục tiêu và nghiệm thu
- Nhiệm vụ hành lang 5 đoạn: loa ra lệnh BƯỚC TỚI/ĐỨNG YÊN; lệnh có «Cô nói» (thật) làm đúng, lệnh không có (giả) làm NGƯỢC; chỉ MỘT người (đổi sau mỗi đoạn) xem được có «Cô nói» hay không; vi phạm = cảnh cáo, 3 cảnh cáo về đầu hành lang; qua đủ 5 đoạn thì thắng.
- Nghiệm thu: (S1) NUnit EditMode xanh, Console 0 lỗi; (S2-S4) compile sạch; (S5) Play Mode mức 4: vào khu chuẩn bị -> bắt đầu -> chơi tới thắng/bị về đầu, cửa theo luật hiện hành.

## 2. Phạm vi
- S1 TẠO MỚI: Features/TeacherSays/Runtime/Domain/{TheReturn.TeacherSays.Domain.asmdef, TeacherSaysRules.cs, TeacherSaysRound.cs, TeacherSaysGenerator.cs, TeacherSaysState.cs}; Tests/EditMode/{TheReturn.TeacherSays.Domain.Tests.asmdef, TeacherSaysTests.cs}.
- Sau S1 (cần khảo sát lại khi bắt tay): Data catalog tiếng Việt, Presentation (đo di chuyển/đứng yên, HUD người nghe), MissionAdapter + đăng ký MissionRegistryFactory, cổng + khu chuẩn bị + 5 vùng đoạn trong scene.
- KHÔNG đụng: 6 nhiệm vụ cũ, Hanh_Lang_2/4/6, bố cục/nội thất của chủ dự án, file đang do T-016/T-006d2 sửa.
- Scene lock: S1-S4 không cần; S5 cần (đang do phiên #3 giữ).

## 3. Luật đã chọn (thực thi thiết kế duyệt; số liệu là mặc định AI, chỉnh được như Choir)
- 5 đoạn; mỗi đoạn có lệnh (Step/Stand), cờ «Cô nói» (thật/giả), người nghe. Thật+Bước = Bước; thật+Đứng = Đứng; giả đảo ngược.
- Ràng buộc đề: 2-3 lệnh thật; có cả hành động Bước và Đứng; người nghe khác người nghe đoạn trước; mỗi người nghe ít nhất 1 lần.
- Bước: cả đội vào vùng đoạn kế trong StepWindowSeconds. Đứng: không ai di chuyển trong StandSeconds rồi đi tự do tới vùng đoạn kế. Vi phạm = cảnh cáo, lệnh lặp lại; đủ MaxWarnings thì về đầu hành lang (đề giữ nguyên).
- Mặc định giai đoạn phát triển một máy (đổi vai bằng phím 1-4): Bước 20 giây, +3 giây mỗi lần đổi vai (tối đa bằng cửa sổ), Đứng 5 giây, 3 cảnh cáo; Online gợi ý 8 giây (không cộng thêm). AI chọn các số này, chủ dự án chỉnh sau.
- Domain thuần C#: thời gian và trạng thái chuyển động do nơi gọi truyền vào (anyMoving, allInNextZone).

## 4. Subtask
| ID | Việc | File | Cách kiểm | Trạng thái | Bằng chứng |
|---|---|---|---|---|---|
| S1 | Domain: Rules, Round, Generator, State | Runtime/Domain | Console 0 lỗi | TODO | |
| S2 | Test NUnit | Tests/EditMode | Test Runner xanh | TODO | |
| S3 | Data catalog + Presentation + adapter + đăng ký | Features/TeacherSays, SchoolFloor/Runtime/Missions | Compile sạch | TODO (chờ #3 xong MissionRegistryFactory) | |
| S4 | Dựng scene Hanh_Lang_5 (cổng Vao/Ra, khu chuẩn bị, 5 vùng đoạn, loa/HUD) | scene | Play Mode mức 4 | TODO (chờ Scene lock) | |
| S5 | Ghi nhận: thẻ hệ thống, TASKS, LOG | .ai/** | Đọc lại | TODO | |

## 5. Bản đồ lỗi
| Triệu chứng | Nơi xem | Cách kiểm |
|---|---|---|
| Đề thiếu lệnh thật/giả hoặc Bước/Đứng | TeacherSaysGenerator > Generate | Test `Generator_*` |
| Người nghe trùng đoạn trước | TeacherSaysGenerator | Test `Generator_ListenerChangesEverySegment` |
| Lệnh giả vẫn bị tính đúng | TeacherSaysRound > RequiredAction | Test `Round_RequiredAction_*` |
| Đứng yên không tính / tính sớm | TeacherSaysState > Tick (Standing) | Test `Stand_*` |
| Hết giờ Bước không cảnh cáo | TeacherSaysState > Tick (Stepping) | Test `Step_*` |
| 3 cảnh cáo không về đầu | TeacherSaysState > Warn | Test `Warnings_*` |

## 6. Rủi ro / câu hỏi
- Giai đoạn một máy: người chơi chỉ điều khiển được một nhân vật mỗi lúc, nên Bước 20 giây + 3 giây/lần đổi vai là ước lượng; cần chủ dự án chơi thử để chỉnh.
- Người nghe «Cô nói» hiển thị trên HUD (T-011 tổ chức lại UI sau); lúc đầu dùng khung HUD chung giống các nhiệm vụ khác.
- Cổng + khu chuẩn bị Hanh_Lang_5: chủ dự án đã cho phép dựng, nhưng vị trí cổng phải khớp luật cửa mới của T-016 (vùng chuẩn bị ngoài cửa).

## 7. Tiến độ
- 2026-10-16 10:48 UTC: kế hoạch viết xong; bắt đầu S1.
