# T-043 — tinh gọn tài liệu AI theo Core4

Trạng thái: REVIEW | AI giữ: Copilot | Cập nhật: 2026-11-13

## Mục tiêu và ranh giới
- Chỉ giữ tri thức phục vụ PlankAndKey, Attendance, BreakerPanel, Choir, MapSchoolF và online chung.
- Giữ nguyên tiến độ T-042/P-014, các quyết định cần cho Q-013 và mọi thay đổi Assets/scene/code hiện có.
- Không commit, xóa Assets, chạy builder hoặc thay Build Settings.

## Việc đã làm
| Nhóm | Kết quả |
|---|---|
| START/CURRENT/TASKS/QUEUE/LOG/PROTOCOL | Gọn trạng thái; LOG có giới hạn 8 mục/8 KB; Q-023/P-014 rõ |
| NETWORK/TESTING/GAUNTLET/registry/failures | Chỉ giữ 4 nhiệm vụ + shared; số liệu cũ gắn nhãn lịch sử |
| PROJECT/ARCHITECTURE/SYSTEMS/OWNER_UPDATES | Viết lại thành điều hướng Core4, bỏ mô tả lỗi thời |
| Plans/cards/decisions/bugs/sessions | Xóa lịch sử không còn phục vụ; giữ T-040 deletion dependency và T-042 working plan/survey |
| Maps/tools | Index tập trung Core4; script index và changed_since giới hạn vào Core4/shared |

## Nghiệm thu đã kiểm
- Rà tham chiếu: không còn liên kết tới tài liệu đã xóa; ngoại lệ chỉ là dependency xóa Q-013 và cảnh báo builder cũ.
- `.ai`: 49 file, khoảng 95 KB; 7 plan. `LOG.md`: 4 mục, 25 dòng, dưới 2 KB (giới hạn 8 mục/8 KB).
- `git diff --check -- .ai AGENTS.md CLAUDE.md`: sạch; không chạy Unity test vì không sửa gameplay/code runtime. Snippet index chưa chạy trong Editor.
- TASKS/CURRENT/LOG đã cập nhật; không đụng Assets, scene hoặc Build Settings.

## Nghiệm thu
- T-042/P-014, điều kiện Q-013 và các bất biến online không bị mất.
- Đọc khởi động chỉ cần START_HERE + CURRENT + tối đa 8 mục LOG + một dòng TASK.
- Không còn hồ sơ task dài hoặc thẻ vận hành cho nhiệm vụ ngoài phạm vi (ngoại trừ danh sách xóa/cảnh báo an toàn).
