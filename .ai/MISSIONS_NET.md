# MISSIONS_NET — registry nhiệm vụ đã nối mạng (nguồn cho Gauntlet; tên assembly test chứa tên nhiệm vụ)

Pha: L (logic + test) | G (qua Gauntlet) | O (Online + scene thử) | S (smoke LAN) | H (người thật). Gauntlet gần nhất: xem GAUNTLET.md.

| Nhiệm vụ | Khu | Task giữ | Assembly test (EditMode) | VIẾT BỞI (phiên/ngày) | Pha hiện tại | Adapter hợp đồng | Gauntlet gần nhất |
|---|---|---|---|---|---|---|---|
| Attendance | Phong_1 | T-031 | TheReturn.Attendance.Network.Tests | #7 / 2026-11-03 | O (LAN 3 tiến trình; đội 2 chưa kết luận) | có (Q-005; AttendanceContractAdapter) | 2026-11-10 08:33 XANH 53/54 |
| BreakerPanel | Phong_5 | T-031 | TheReturn.BreakerPanel.Network.Tests | #10 / 2026-11-05 | O (host một mình Play) | có (Q-005; BreakerContractAdapter) | 2026-11-10 08:33 XANH 70/71 |
| GradeRepair | Phong_2 | T-036 | TheReturn.GradeRepair.Network.Tests | #10 / 2026-11-07 | L (xong; chưa Online) - CẮT (T-040, 2026-11-10, đóng băng) | có (GradeRepairContractAdapter) | 2026-11-10 08:33 XANH 64/65 |
| PlankAndKey | Hanh_Lang_6 | T-027 | TheReturn.PlankAndKey.Network.Tests | #9 / 2026-11-01 | O (DONE nghiệm thu 2 máy 2026-11-01) | có (T-040 P4, #14 2026-11-10) | 2026-11-10 08:33 XANH 55/55 |
| TeacherSays | Hanh_Lang_5 | T-037 | TheReturn.TeacherSays.Network.Tests | #11 / 2026-11-05 | L (GĐ1 xong) - CẮT (T-040, 2026-11-10, đóng băng) | chưa | 2026-11-10 08:33 XANH 70/70 |
| Choir | Phong_6 | T-037 | TheReturn.Choir.Network.Tests | #14 / 2026-11-07 | L (GĐ1 xong) | có (ChoirContractAdapter) | 2026-11-10 08:33 XANH 65/66 |
| SharedLantern | Hanh_Lang_4 | T-037 | TheReturn.SharedLantern.Network.Tests | #14 / 2026-11-08 | L (GĐ1 xong) - CẮT (T-040, 2026-11-10, đóng băng) | có (SharedLanternContractAdapter) | 2026-11-10 08:33 XANH 83/84 |

Khung dùng chung (lỗi chéo -> thẻ FIX thuộc T-039): TheReturn.Network.Framework.Tests (T-024, #9).
Thêm dòng mới khi nhiệm vụ qua pha L; Gauntlet quét assembly *Network.Tests trong dự án và cảnh báo nếu lệch bảng này.
