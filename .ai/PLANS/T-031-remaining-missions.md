# T-031: nối nhiệm vụ qua mạng — CHỈ MỤC (cập nhật 2026-11-04, phiên #7)
T-031 giờ là task TIÊN PHONG gồm 2 nhiệm vụ làm đầu tiên để tạo mẫu. 9 nhiệm vụ còn lại đã tách thành 3 task lớn (xem mục 2). Quyết định và lý do: DECISIONS/2026-11-04-t031-split.md. Mẫu kỹ thuật: PLANS/T-027-plank-network-pattern.md + NETWORK.md.

## 1. Hai nhiệm vụ tiên phong (mỗi AI sửa đúng dòng của mình)
Trạng thái: TODO | CLAIMED | IN-PROGRESS | REVIEW | DONE.
| Nhiệm vụ (thư mục Features/) | Trạng thái | AI giữ | Cập nhật | Ghi chú / bằng chứng |
|---|---|---|---|---|
| Attendance | REVIEW | Claude Sonnet 5.5 (phiên #7) | 2026-11-02 16:55 | Lớp mạng thuần C# + 44 test xanh; lớp Unity (AttendanceOnlineHost/Client/AutoRun) + scene NetworkAttendanceTest (24 ghế). LAN qua tiến trình thật: đội 3 thắng, tranh ghế bị từ chối đúng, đội 2 ngồi sai -> báo sai -> đứng dậy xóa cờ. CÒN: người thật chơi 2 máy (phím E/R, HUD, đọc manh mối riêng), đội 2 thắng qua mạng (chay_lai_R1.bat), đội 4 thật, nối cửa ra/luồng map (T-028) |
| BreakerPanel | REVIEW | Claude Sonnet 5.5 (phiên #10) | 2026-11-05 07:49 | XONG CODE + SCENE THỬ. Lớp mạng thuần C# (58 test xanh, đội 2/3/4) + lớp Online `Runtime/Online` (BreakerOnlineHost/Client/AutoRun, asmdef TheReturn.BreakerPanel.Online) + scene thử `Features/Network/Scenes/NetworkBreakerTest.unity` (nhân bản NetworkAttendanceTest, thay phòng bằng PF_BreakerPanel). ĐÃ tự kiểm Play-mode 1 tiến trình (2026-11-06 05:02): host LAN + 1 khách đội 2, lệnh sai bị host từ chối, bật/tắt/làm lại đúng; CHƯA chạy 2 tiến trình để thắng ván (cần build player với -brkauto solve -brkteam N -brksolfile). CHƯA nối vào luồng map (T-028), KHÔNG đụng scene map/Build Settings |

## 2. Chín nhiệm vụ còn lại: đã chuyển sang task lớn
| Nhiệm vụ | Task lớn | Lý do nhóm | Kế hoạch |
|---|---|---|---|
| GradeRepair | T-036 (Nhóm A) | thông tin riêng + thao tác rời rạc | PLANS/T-036-private-info-missions.md |
| NavigationExam | T-036 (Nhóm A) | thông tin riêng + lượt đi | PLANS/T-036-private-info-missions.md |
| TeacherTruth | T-036 (Nhóm A) | phụ đề riêng + xác nhận ở trạm | PLANS/T-036-private-info-missions.md |
| Choir | T-037 (Nhóm B) | đứng trên bục theo đồng hồ; đổi cửa sổ theo số người | PLANS/T-037-timing-zone-missions.md |
| TeacherSays | T-037 (Nhóm B) | vùng + hạn 8/5 giây + đi hay đứng yên | PLANS/T-037-timing-zone-missions.md |
| SharedLantern | T-037 (Nhóm B) | vùng sáng + 5 giây ngoài vùng + trao đèn | PLANS/T-037-timing-zone-missions.md |
| GlassBridge | T-038 (Nhóm C) | ô cầu theo vị trí + đài quan sát | PLANS/T-038-movement-observation-missions.md |
| QuietCorridor | T-038 (Nhóm C) | ai đang chạy + giữ nút | PLANS/T-038-movement-observation-missions.md |
| DontLookBack | T-038 (Nhóm C) | ai thấy lưng ai (vị trí + hướng nhìn) | PLANS/T-038-movement-observation-missions.md |

Bảng nhận việc từng nhiệm vụ nằm TRONG kế hoạch của task lớn tương ứng (mục 7). Đừng ghi trạng thái ở đây.
