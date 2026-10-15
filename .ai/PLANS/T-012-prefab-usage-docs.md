# KẾ HOẠCH T-012: Tài liệu cách dùng cho từng prefab tương tác   (file: .ai/PLANS/T-012-prefab-usage-docs.md)

Trạng thái: XONG (chờ chủ dự án xem) | AI giữ: Claude Sonnet 5.5 | Cập nhật: 2026-10-15 07:34 UTC

## 1. Mục tiêu và nghiệm thu
- Mỗi prefab có chức năng/tương tác có mô tả: nơi dùng, số instance, script gốc, mốc bắt buộc, collider, phần thay được, lưu ý — để chủ dự án thay asset 3D không hỏng logic.
- Nghiệm thu: mọi prefab gameplay trong `Assets/TheReturn` có mục trong `Assets/TheReturn/PREFAB_GUIDE.md`; số liệu lấy từ prefab và scene thật (mức 4 cho cấu trúc, mức 2 cho hành vi runtime).

## 2. Phạm vi
- Được đụng: `Assets/TheReturn/PREFAB_GUIDE.md` (mới), 1 dòng con trỏ cuối `Assets/TheReturn/README.md`, `.ai/` (PLANS, TASKS, LOG, CURRENT, CONVENTIONS).
- KHÔNG đụng: scene, prefab, code C#, bố cục/gameplay của chủ dự án, mọi builder/migration.
- Scene lock: không cần. Snapshot scene: không cần (không sửa scene).

## 3. Ngữ cảnh đã đọc
| File/thư mục (ngày đọc) | Kết luận 1 dòng |
|---|---|
| AGENTS.md, CLAUDE.md, PROTOCOL.md, CURRENT.md, TASKS.md, LOG.md, OWNER_UPDATES.md (2026-10-15) | Quy trình, an toàn, T-012 TODO chưa ai giữ; scene không dirty, không Play Mode. |
| 14 prefab trong Assets/TheReturn (nạp bộ nhớ tạm, không lưu) | Cấu trúc con, collider, field tuần tự hóa đã ghi trong PREFAB_GUIDE. |
| README 6 nhiệm vụ + Assets/TheReturn/README.md | Đã có đoạn Prefab nhưng rời rạc; thiếu phụ thuộc tên `FrontNumber`, scale thừa kế, tầm/pivot. |
| Code 6 *Prototype + 8 script Presentation/Shared | Cách nhận diện tương tác (GetComponentInParent/IsChildOf), tầm 2.6–4 m, ray chắn tầm bằng RaycastAll. |
| Scene SchoolFloorPrototype (chỉ đọc) | Số instance thật: 24 ghế, 5 terminal, 1 sa bàn, 2 trạm giữ, 1 loa, 4 kênh GV, 4 trạm xác nhận, 4 thẻ, 2 bảng khóa, 7 cửa trượt, 22 cổng chuẩn bị. |

## 4. Các subtask
| ID | Việc | File sẽ đụng | Cách kiểm | Trạng thái | Bằng chứng |
|---|---|---|---|---|---|
| S1 | Khảo sát prefab + scene + code tương tác (chỉ đọc) | - | đối chiếu field/mảng trên controller | DONE | mức 4 (đọc từ Editor) |
| S2 | Viết PREFAB_GUIDE.md | Assets/TheReturn/PREFAB_GUIDE.md | mở lại file, kiểm UTF-8 | DONE | file tạo, đọc lại |
| S3 | Con trỏ trong README + CONVENTIONS; cập nhật TASKS/LOG/CURRENT | README.md, .ai/* | đọc lại các dòng | DONE | mức 1-2 |

## 5. Bản đồ lỗi
| Triệu chứng có thể gặp | File > hàm / object | Cách kiểm rẻ nhất | Liên quan |
|---|---|---|---|
| Sau khi thay model, không bấm E được terminal/trạm | `*Prototype.CanReach` (GradeRepair, NavigationExam, DontLookBack, QuietCorridor, TeacherTruth) | Collider nào ngoài cây root chắn tia mắt (+1.5 m) -> pivot root; pivot có nhìn thấy không | PREFAB_GUIDE mục 1 |
| Thẻ lưng (BackCard) lỗi NullReference | `DontLookBackPrototype.RefreshBadges` dòng ~197 | Child `FrontNumber` còn là con trực tiếp của root không | PREFAB_GUIDE `PF_BackCard` |
| Loa không phát/NullReference | `QuietSpeakerFeedback.Update` (`session` trống) | Gán `session` trên instance | PREFAB_GUIDE `PF_QuietSpeaker` |
| Đèn trạng thái ghế/trạm không đổi màu | `AttendanceSeat.Show`, `QuietHoldStation.Show` | `indicator` còn gán? shader có `_BaseColor`? | PREFAB_GUIDE |
| Model terminal/cửa bị méo | `PF_GradeTerminal/Visual_Replaceable`, `PF_GB_SlidingDoor/Panel` (scale cha) | Xem scale cha của object chứa model | PREFAB_GUIDE mục 1 |

## 6. Rủi ro và câu hỏi cho chủ dự án
- Tài liệu mô tả prefab nguồn; instance trong scene có thể có override chưa đối chiếu từng cái.
- Có muốn thêm con trỏ tới PREFAB_GUIDE vào README từng nhiệm vụ không? (hiện chỉ thêm ở Assets/TheReturn/README.md).

## 7. Tiến độ
- 2026-10-15 07:34 UTC: S1-S3 xong; chưa chạy Play Mode (không cần cho task tài liệu).
