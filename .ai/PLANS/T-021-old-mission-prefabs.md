# KẾ HOẠCH T-021: Chuyển 5 root nhiệm vụ cũ thành prefab   (file: .ai/PLANS/T-021-old-mission-prefabs.md)

Trạng thái: XONG (chờ chủ dự án xem) | AI giữ: Claude Sonnet 5.5 (phiên #8) | Cập nhật: 2026-10-29 04:41 UTC

## 1. Mục tiêu và nghiệm thu
- Tiếp T-020: chuyển 5 root nhiệm vụ cũ (QuietCorridor, GradeRepair, NavigationExam, DontLookBack, TeacherTruth) trong SchoolFloorPrototype thành prefab PF_* ở Features/<Tên>/Prefabs. Hành vi KHÔNG đổi.
- Nghiệm thu (mức 3-4): dump thuộc tính từng root trước = sau (kể cả prefab lồng); tham chiếu từ ngoài vào (6, từ SchoolFloorFlow) và từ trong ra ngoài (LocalParty; riêng DontLookBack thêm badges -> BackCard_1..4, backLabels -> BackSymbol) y hệt; EditMode 256/256; Console 0 lỗi; PlayMode không xấu hơn T-020 (10/11, lỗi Hanh lang 1 có sẵn); scene lưu và không dirty.

## 2. Phạm vi
- Được đụng: 5 root trên trong scene SchoolFloorPrototype; 5 file PF_*.prefab mới; PREFAB_GUIDE.md và README 5 feature; .ai/*.
- KHÔNG đụng: code C#, bố cục/blockout/cổng của chủ dự án, 6 root nhiệm vụ mới (xong ở T-020), Environment_LogicOnly, TmpPoint, Puzzle_Attendance.
- Snapshot scene: CÓ (BeforeOldMissionPrefabs_yyyyMMdd_HHmmss.unity). Chủ dự án cho phép sửa scene 2026-10-29 và bảo «làm tiếp» sau T-020. Không có Scene lock.

## 3. Ngữ cảnh đã đọc
| File/thư mục (+ngày đọc) | Kết luận 1 dòng |
|---|---|
| Scene, 5 root (2026-10-29) | 206-237 node/root, 21-41 prefab lồng mỗi root (PF_GB_Block, PF_GB_SlidingDoor, PF_* của nhiệm vụ, Props_*, Table01...), 0 script thiếu. Ref ra ngoài: 1 (party -> LocalParty) trừ DontLookBack có 9 (party, badges x4 -> BackCard_n, backLabels x4 -> BackSymbol). Ref vào từ ngoài: 6 (SchoolFloorFlow). |
| Ánh sáng (2026-10-29) | Không có lightmap, không ContributeGI, đèn đều Realtime: chuyển prefab không mất ánh sáng nướng. |
| PLANS/T-020 (2026-10-29) | Cách làm đã chạy: SaveAsPrefabAssetAndConnect + dump trước/sau + ref vào/ra; đã chứng minh hành vi không đổi. |

## 4. Các subtask
| ID | Việc | File sẽ đụng | Cách kiểm | Trạng thái | Bằng chứng |
|---|---|---|---|---|---|
| S1 | Snapshot + dump trước + ref vào/ra | Scenes/BeforeOldMissionPrefabs_20261029_044208.unity, Temp/T021_before_* | file tồn tại | DONE | snapshot lưu; dump 12011/12041/11116/11023/12415 dòng; inbound 6; ref ra ngoài 1/1/1/9/1 |
| S2 | Tạo 5 prefab | Features/*/Prefabs/PF_*.prefab, scene | 5 instance | DONE | 5 file PF_*.prefab; mỗi instance 1 override (Prototype: party, DontLookBack thêm badges/backLabels) |
| S3 | Dump sau = trước; ref; EditMode; PlayMode; Console | - | diff 0; 256/256; Play không xấu hơn | DONE | 5/5 root KHÁC=0 dòng; inbound y hệt; EditMode 256/256; PlayMode 10/11 (cùng lỗi Hanh lang 1 như trước); Console 0 lỗi |
| S4 | Lưu scene; không dirty | scene | isDirty=false | DONE | SaveScene=true, dirty=False |
| S5 | Tài liệu + TASKS/LOG | PREFAB_GUIDE, .ai | đọc lại | DONE | mục «5 nhiệm vụ cũ (T-021)» trong PREFAB_GUIDE |

## 5. Bản đồ lỗi
| Triệu chứng có thể gặp | File > hàm / object | Cách kiểm rẻ nhất | Liên quan |
|---|---|---|---|
| DontLookBack mất thẻ lưng/nhãn | DontLookBackPrototype.badges / backLabels | đọc mảng trên instance | S2 |
| Cửa/trạm không bấm được | PF_* lồng bên trong | so dump | S3 |
| Scene dirty / mất override prefab lồng | root instance | so dump | S3 |

## 6. Rủi ro
- Prefab rất lớn (200+ node, nhiều prefab lồng): mở/sửa chậm hơn; prefab lồng giữ override trong prefab ngoài.
- Ref sang BackCard_n/BackSymbol là override trên instance; instance thêm mới phải gán lại.

## 7. Tiến độ
- 2026-10-29 04:41 UTC: phân tích và lập kế hoạch, chưa sửa scene.
- 2026-10-29 04:44 UTC: S1-S5 xong. PlayMode vẫn 10/11, đúng lỗi EveryGate (Hanh lang 1_Vao/Ra) có sẵn từ trước T-020.
