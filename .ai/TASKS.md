# TASKS — việc đang ảnh hưởng tới Core4

Trạng thái hiện hành. Task hoàn tất có giá trị lâu dài được tóm tắt trong `TASKS_DONE.md`; từng bước online nằm ở `QUEUE.md`.

| ID | Trạng thái | Giữ | Cập nhật | Phạm vi còn lại | Đọc | Nghiệm thu |
|---|---|---|---|---|---|---|
| T-040 | IN-PROGRESS | T-042 / chủ dự án | 2026-11-13 | MapSchoolF thay scene blockout Core4 ban đầu; hoàn tất map/luồng dùng chung. Q-013 xóa code 8 nhiệm vụ chỉ sau chủ dự án xác nhận scene mới chạy + commit/tag. | `PLANS/T-040-scope-cut-4-missions.md`; `PLANS/T-040-dependencies.md`; `QUEUE.md` | Map mới chạy đủ 4 nhiệm vụ; không xóa code ngoài phạm vi trước Q-013 |
| T-042 | IN-PROGRESS | Claude #13 | 2026-11-12 | Q-016–Q-022 xong. Q-023 offline/Build Settings xong; phần online phòng→map chờ P-014. Q-024 chờ. | `PLANS/T-042-net-wiring-mapschoolf.md`; `QUEUE.md`; `DECISIONS/PENDING.md` | Online host/client qua 4 nhiệm vụ; Gauntlet xanh; smoke 2 máy do chủ dự án xác nhận |
| T-043 | REVIEW | Copilot | 2026-11-13 | Đã rút gọn `.ai` theo Core4; 49 file/~95 KB, 7 plan; LOG dưới 2 KB. Tham chiếu và diff whitespace đã kiểm. | `PLANS/T-043-ai-docs-core4-prune.md` | Chờ chủ dự án xem; không sửa Assets/code/scene |
