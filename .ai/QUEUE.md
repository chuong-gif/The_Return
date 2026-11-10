# QUEUE — hàng đợi thẻ việc online (T-039). MỘT nơi, MỘT prompt cố định

Prompt chuẩn chủ dự án dán cho AI bất kỳ:
> Bạn vào dự án Unity The Return tại E:\kho model 3D\Project Unity\The Return. Đọc AGENTS.md rồi .ai/QUEUE.md. Nhận thẻ đầu tiên có trạng thái READY hoặc INTERRUPTED (thẻ FIX trước), làm đúng thẻ đó theo .ai/PROTOCOL.md mục 14. Không khảo sát thêm. Mỗi bước để dự án biên dịch sạch và cập nhật khối RESUME; hết limit giữa chừng là chuyện bình thường.

Trạng thái: READY | IN-PROGRESS | INTERRUPTED | DONE | BLOCKED. IN-PROGRESS không có LOG >= 6 giờ = INTERRUPTED (AI nào cũng tiếp được ngay). Mỗi thẻ nhỏ: làm xong trong MỘT lượt limit. Thẻ FIX luôn nằm TRÊN CÙNG. Thẻ DONE >= 10 dòng thì chuyển sang cuối file (mục ĐÃ XONG) để file ngắn.

## FIX (thẻ sửa lỗi; sinh từ FAILURES.md)
| ID | Task | Việc (1 câu) | File tạo/sửa | XONG KHI | Cần trước | Trạng thái | Cập nhật |
|---|---|---|---|---|---|---|---|
| FIX-F-002 | T-031 | Sửa F-002 (MISSION) trong Attendance: test C6_BotTuGiaiToiThang | file của nhiệm vụ Attendance (xem FAILURES.md F-002) | Chạy lại Gauntlet: test đó xanh | - | DONE (lỗi cố ý của M4) | 2026-11-07 06:06 |
| FIX-F-003 | T-031 | Sửa F-003 (MISSION) trong Attendance: test N1_MangGiaXauVanHoiTu | file của nhiệm vụ Attendance (xem FAILURES.md F-003) | Chạy lại Gauntlet: test đó xanh | - | DONE (lỗi cố ý của M4) | 2026-11-07 06:06 |
| FIX-F-004 | T-039 | Sửa F-004 (KHÁC) trong TheReturn.GradeRepair.Network.Tests: test Private_Malformed_ReturnsFalse_NeverThrows | file của nhiệm vụ TheReturn.GradeRepair.Network.Tests (xem FAILURES.md F-004) | Chạy lại Gauntlet: test đó xanh | - | DONE | 2026-11-08 14:06 |

## HÀNG ĐỢI
| ID | Task | Việc (1 câu) | File tạo/sửa | XONG KHI (lệnh + kết quả phải có) | Cần trước | Trạng thái | Cập nhật |
|---|---|---|---|---|---|---|---|
| Q-003 | T-039 M1 | Bộ hợp đồng C1-C9 + interface adapter, chạy thử với adapter giả | Features/Network/Tests/Contract/* (asmdef + file MỚI) | run_tests EditMode assembly_names=TheReturn.Network.Contract.Tests: tất cả xanh; Console 0 lỗi | S0.2/S0.3 | DONE | 2026-11-06 06:26 |
| Q-004 | T-039 M2 | SimNet trong bộ nhớ + tiêm lỗi có seed (trễ, trùng, đảo, mất rồi gửi lại) | Features/Network/Tests/Sim/* (MỚI) | Test hội tụ xanh; ca cố tình hỏng bị bắt | Q-003 | DONE | 2026-11-06 06:26 |
| Q-005 | T-039 M3 | Adapter THỬ Attendance + BreakerPanel; đo số dòng adapter + thời gian chạy; điền PLANS/T-039 mục 8 | Features/{Attendance,BreakerPanel}/Tests/EditModeNet/*Adapter* | Xanh; số liệu ghi vào mục 8 | Q-003, Q-004 | DONE | 2026-11-07 05:42 |
| Q-006 | T-039 M4 | Gauntlet: một lệnh chạy mọi assembly trong MISSIONS_NET, ghi GAUNTLET.md + FAILURES.md | .ai/tools/gauntlet.snippet.cs + .ai/*.md | Một lệnh -> bảng; cố tình làm hỏng 1 adapter -> FAILURES có dòng đúng nhiệm vụ | Q-005 | DONE | 2026-11-07 05:55 |
| Q-007 | T-039 M5 | Lớp cơ sở Online dùng chung (host/client/autorun), CHỈ THÊM; chuyển BreakerPanel làm thử | Features/Network/Online/* (MỚI), Features/BreakerPanel/Runtime/Online | Compile; test cũ xanh; Play host một mình đúng như 2026-11-05 | Q-006 | DONE | 2026-11-08 13:39 |
| Q-008 | T-039 M6 | Scaffolder nhiệm vụ mới + sửa NETWORK.md mục 4-5 + chuyển bảng/brief T-036/37/38 sang pha L-trước | .ai/tools/new_mission_net.snippet.cs; NETWORK.md; TASKS.md; briefs | Tạo nhiệm vụ giả -> compile xanh | Q-007 | CANCELLED (T-040: không còn nhiệm vụ mới để sinh khung) | 2026-11-10 08:16 |
| Q-009 | T-039 M7 | Pha S dùng chung: MỘT build + MỘT .bat cho mọi nhiệm vụ đã qua O | Builds/ | Log mọi tiến trình, thắng + thua | Q-007 | BLOCKED (gộp vào T-040 P5) | 2026-11-10 08:16 |
| Q-010 | T-040 P1 | Khảo sát CHỈ ĐỌC: (a) điểm phụ thuộc 12 khu/24 cổng + phần dùng chung được; (b) footprint 4 nhiệm vụ giữ; (c) DANH SÁCH XÓA an toàn 8 nhiệm vụ | .ai/PLANS/T-040-dependencies.md (MỚI) | Có 3 bảng (a)(b)(c) | - | DONE | 2026-11-10 19:19 |
| Q-011 | T-040 P3a | Thiết kế layout map mới: sơ đồ + kích thước theo footprint, cổng, spawn; để chủ dự án DUYỆT | .ai/PLANS/T-040-map-design.md (MỚI) | Có sơ đồ + chủ dự án duyệt (P-010) | Q-010 | DONE (đề xuất v1; chờ chủ dự án duyệt P-010) | 2026-11-10 19:21 |
| Q-012 | T-040 P3b | Dựng scene mới đúng thiết kế đã duyệt (blockout, luật cửa, co UI/âm thanh/registry, Build Settings) | Scene mới + code nối tối thiểu | Play được, chỉ 4 nhiệm vụ, Gauntlet xanh, scene cũ nguyên | Q-011 + duyệt sơ đồ | BLOCKED (chờ duyệt sơ đồ) | 2026-11-10 08:21 |
| Q-013 | T-040 P7 | XÓA HẲN code 8 nhiệm vụ + scene cũ theo DANH SÁCH XÓA (P1); mỗi lượt compile sạch + Gauntlet xanh | Features/<8 nhiệm vụ>, scene cũ, builder, *Checks | Không còn tên 8 nhiệm vụ trong Assets; compile sạch; Gauntlet xanh | Chủ dự án xác nhận scene mới chạy + đã commit/tag git | BLOCKED (chờ chủ dự án) | 2026-11-10 08:21 |
| Q-014 | T-040 P3c/P4 | Hệ thống dùng chung cho scene mới: (1) Core4Layout + kiểm layout, (2) cầu nối topology cho MissionFlowController/Registry, (3) chuyển người phòng->map online, (4) âm thanh 4 khu | Maps/Core4/**, Features/Network/**, cầu nối nhỏ ở SchoolFloor/Runtime/Missions | Test EditMode xanh; Gauntlet xanh; không đổi hành vi scene cũ | - | IN-PROGRESS (#14) | 2026-11-10 09:10 |

## ĐÃ XONG
| Q-001 | T-039 S0.2 | Tạo QUEUE/MISSIONS_NET/FAILURES/GAUNTLET | .ai/*.md | Mở file thấy đủ | - | DONE | 2026-11-06 06:26 |
| Q-002 | T-039 S0.3 | PROTOCOL mục 14 + START_HERE prompt + DECISIONS/PENDING.md | PROTOCOL.md, START_HERE.md | Đọc lại | - | DONE | 2026-11-06 06:26 |
