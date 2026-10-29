# KẾ HOẠCH T-020: Chuyển 6 root nhiệm vụ mới thành prefab   (file: .ai/PLANS/T-020-mission-prefabs.md)

Trạng thái: XONG (chờ chủ dự án xem) | AI giữ: Claude Sonnet 5.5 (phiên #8) | Cập nhật: 2026-10-29 04:29 UTC

## 1. Mục tiêu và nghiệm thu
- Chuyển 6 root *_LogicOnly của nhiệm vụ mới (Breaker, Choir, GlassBridge, PlankAndKey, SharedLantern, TeacherSays) trong scene SchoolFloorPrototype thành prefab (PF_*), vào Features/<Tên>/Prefabs. Hành vi KHÔNG đổi. Tiếp T-012 (T-012 chỉ làm tài liệu, ghi rõ «chưa chuyển vật nào thành prefab»).
- Nghiệm thu (mức 3-4): dump thuộc tính 6 root TRƯỚC = SAU (trừ chỉ báo prefab); tham chiếu từ ngoài vào trong còn nguyên; ref party -> LocalParty còn trên instance; Console 0 lỗi; EditMode test pass như trước (256/256); scene lưu xong và không dirty.

## 2. Phạm vi
- Được đụng: 6 root trên trong scene SchoolFloorPrototype; tạo 6 file PF_*.prefab (+ thư mục Prefabs của 6 feature); PREFAB_GUIDE.md và README 6 feature (mô tả cách dùng); .ai/*.
- KHÔNG đụng: code C#, bố cục/blockout/cổng/khu chuẩn bị của chủ dự án, 5 root nhiệm vụ cũ (QuietCorridor, GradeRepair, NavigationExam, DontLookBack, TeacherTruth), builder/migration cũ.
- Cần snapshot scene? CÓ: BeforeMissionPrefabs_yyyyMMdd_HHmmss.unity cạnh các snapshot cũ. Chủ dự án cho phép sửa scene 2026-10-29 (OWNER_UPDATES). Scene lock đã bỏ.

## 3. Ngữ cảnh đã đọc
| File/thư mục (+ngày đọc) | Kết luận 1 dòng |
|---|---|
| Scene, 6 root (2026-10-29) | Mỗi root chỉ có ĐÚNG 1 tham chiếu ra ngoài: *Prototype.party -> LocalParty; không có prefab lồng bên trong. Số node 17/45/31/74/18/21 (Choir/GlassBridge/Breaker/SharedLantern/TeacherSays/PlankAndKey). |
| CONVENTIONS.md, PREFAB_GUIDE (2026-10-29) | Prefab tiền tố PF_, chỉ vật có tương tác/logic nhiệm vụ; mỗi prefab cần mô tả cách dùng. Prefab cũ nằm ở Features/<X>/Prefabs. |

## 4. Các subtask
| ID | Việc | File sẽ đụng | Cách kiểm | Trạng thái | Bằng chứng |
|---|---|---|---|---|---|
| S1 | Snapshot scene + dump trước (thuộc tính 6 root, tham chiếu vào) | Scenes/BeforeMissionPrefabs_20261029_043005.unity | file tồn tại; dump có số liệu | DONE | snapshot lưu; dump: 1574/1016/1887/941/4137/659 dòng; 6 tham chiếu vào (SchoolFloorFlow -> 6 Prototype) |
| S2 | Tạo 6 prefab bằng SaveAsPrefabAssetAndConnect | Features/*/Prefabs/PF_*.prefab, scene | 6 instance IsPartOfPrefabInstance | DONE | 6 file PF_*.prefab; tên object trong scene giữ nguyên; mỗi instance chỉ có 1 override (party) |
| S3 | Dump sau, so với trước; test EditMode; Console | - | diff = 0; test pass; 0 lỗi | DONE | 6/6 root KHÁC=0 dòng; tham chiếu vào y hệt; EditMode 256/256; Console 0 lỗi/cảnh báo |
| S4 | Lưu scene; kiểm không dirty | scene | isDirty=false | DONE | SaveScene=true, dirty=False |
| S5 | Tài liệu: PREFAB_GUIDE + README 6 feature + TASKS/LOG | tài liệu | đọc lại | DONE | mục «Prefab nhiệm vụ mới» + 6 README sửa dòng «CHƯA là prefab» |

## 5. Bản đồ lỗi
| Triệu chứng có thể gặp | File > hàm / object | Cách kiểm rẻ nhất | Liên quan |
|---|---|---|---|
| Nhiệm vụ không chạy / NullReference sau khi chuyển | *Prototype.party (mất ref LocalParty) | Đọc ô Party trên instance (phải là LocalParty) | S2 |
| Adapter/Flow không tìm thấy Prototype | MissionRegistryFactory + *MissionAdapter (FindObjectOfType theo kiểu hay theo tên root?) | grep tên root trong Maps/SchoolFloor/Runtime/Missions | S3 |
| Scene báo dirty hoặc mất override | root instance | so dump trước/sau | S3 |

## 6. Rủi ro và câu hỏi cho chủ dự án
- Prefab chứa cả toàn bộ cảnh nhiệm vụ của 1 khu (bục, bảng, HUD logic) nên sửa prefab sẽ ảnh hưởng instance duy nhất đó; vị trí đặt trong phòng là override ở instance (đúng ý).
- Chưa làm 5 root nhiệm vụ cũ (QuietCorridor, GradeRepair, NavigationExam, DontLookBack, TeacherTruth): chúng đã gồm nhiều prefab lồng; hỏi chủ dự án nếu muốn làm tiếp.

## 7. Tiến độ
- 2026-10-29 04:29 UTC: lập kế hoạch, chưa sửa scene.
- 2026-10-29 04:35 UTC: S1-S5 xong. PlayMode: 10/11 test của T-005 xanh; EveryGate_OfEveryMissionArea_StartsItsArea FAIL ở 2 cổng «Hanh lang 1_Vao/Ra» (khu Hanh_Lang_6): CÓ SẴN trước T-020 (tháo prefab root PlankAndKey, lưu, chạy lại: vẫn fail y hệt; đã nối lại prefab, dump = trước). Chưa chẩn đoán nguyên nhân (việc của T-005).
