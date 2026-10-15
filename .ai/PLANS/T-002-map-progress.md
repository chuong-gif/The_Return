# KẾ HOẠCH T-002: MapProgress (luật chọn khu + điều kiện thắng, C# thuần)   (file: .ai/PLANS/T-002-map-progress.md)

Trạng thái: XONG | AI giữ: - | Cập nhật: 2026-10-15 07:52 UTC | Duyệt: chủ dự án đồng ý trong chat 2026-10-15

## 1. Mục tiêu và nghiệm thu
- Tạo `MapProgress` thuần C# mô tả luật luồng tự chọn (nonlinear): khu nào đang làm, khu nào đã xong, ai được bắt đầu khu nào, khi nào thắng. Chưa nối vào scene/gameplay.
- Nghiệm thu (mức 3): NUnit EditMode test chạy XANH qua Test Runner (ghi tên + số test) và Console 0 lỗi CS.

## 2. Phạm vi
- Được đụng (chỉ TẠO MỚI): `Assets/TheReturn/Maps/SchoolFloor/Runtime/Domain/` (asmdef + MapProgress.cs), `Assets/TheReturn/Maps/SchoolFloor/Tests/EditMode/` (asmdef + test), file `.ai/`.
- KHÔNG đụng: scene, prefab, 6 nhiệm vụ (Domain/Presentation), SchoolFloorFlow, SchoolFloorAccessController, mọi builder/MenuItem, 5 bộ *Checks cũ.
- Scene lock: không cần. Snapshot scene: không cần (không sửa scene).
- Asmdef Domain mới nằm trong thư mục con của `TheReturn.SchoolFloor` runtime nên tách khỏi assembly đó; chưa assembly nào tham chiếu nó (S2/S3 của kế hoạch lớn mới nối).

## 3. Ngữ cảnh đã đọc
| File (đọc 2026-10-15) | Kết luận 1 dòng |
|---|---|
| PLANS/nonlinear-mission-flow.md | Thiết kế MapProgress: danh sách khu, tập đã xong, tối đa 1 khu đang làm, hành lang chỉ vào từ 2 phòng kề, thắng khi mọi khu xong; Serializable cho mạng |
| DECISIONS/2026-10-14-nonlinear-mission-selection.md | Luật đã chốt; thắng = mọi khu ĐÃ ĐĂNG KÝ nhiệm vụ xong, khu chưa có nhiệm vụ liệt kê là THIẾU và tự tính vào điều kiện thắng khi đăng ký |
| MAPS/area-adjacency.md | Vòng tròn Phong_1..6 / Hanh_Lang_1..6; Hanh_Lang_k nối Phong_k và Phong_(k+1) (Hanh_Lang_6 nối Phong_6, Phong_1); tên object là chuẩn |
| MISSIONS_TODO.md | Có nhiệm vụ: Phong_1..4, Hanh_Lang_1, Hanh_Lang_3; chưa có: Phong_5, Phong_6, Hanh_Lang_2, 4, 5, 6 |
| CONVENTIONS.md, AGENTS.md | Domain thuần C#, comment đầu file + summary tiếng Việt từng hàm, namespace `TheReturn` |
| Domain asmdef mẫu (Attendance.Domain) | `{ name, noEngineReferences: true }`; TeacherTruthState.cs là mẫu phong cách code |
| TESTING.md | Dự án chưa có NUnit; Test Framework 1.6.0 đã cài (Library/PackageCache) nên dùng được |

## 4. Các subtask
| ID | Việc | File sẽ đụng | Cách kiểm | Trạng thái | Bằng chứng |
|---|---|---|---|---|---|
| S1 | asmdef Domain (noEngineReferences) + kiểu dữ liệu `AreaKind`, `AreaInfo`, `BeginResult` | Runtime/Domain/*.asmdef, MapProgress.cs | Console 0 lỗi CS | DONE | Console 0 lỗi; kiểu nằm trong assembly TheReturn.SchoolFloor.Domain, chỉ tham chiếu netstandard (mức 2) |
| S2 | Luật `MapProgress`: CheckBegin/TryBegin, Complete, Abandon, Reset, Won, MissingMissionAreas, snapshot, `CreateRing` | Runtime/Domain/MapProgress.cs | Console 0 lỗi CS | DONE | Biên dịch sạch cùng S1 (mức 2) |
| S3 | asmdef test (NUnit) + test các luật và vòng 6 phòng | Tests/EditMode/*.asmdef, MapProgressTests.cs | Test Runner xanh | DONE | run_tests EditMode, assembly TheReturn.SchoolFloor.Domain.Tests: 21/21 passed, 0,63 s (mức 3) |
| S4 | Ghi nhận: SYSTEM_CARDS/SchoolFloor.md, MAPS/asmdef-graph.md, TESTING.md, CURRENT, TASKS, LOG | .ai/** | Đọc lại các file đã sửa | DONE | index-scripts.md CHƯA dựng lại (tự sinh) |

## 5. Bản đồ lỗi
| Triệu chứng có thể gặp | File > hàm / object | Cách kiểm rẻ nhất | Liên quan |
|---|---|---|---|
| CS lỗi «UnityEngine does not exist» trong MapProgress | Runtime/Domain/TheReturn.SchoolFloor.Domain.asmdef (noEngineReferences) | Mở MapProgress.cs, xóa using UnityEngine | Domain phải thuần |
| Test không hiện trong Test Runner | Tests/EditMode asmdef (references TestRunner, precompiled nunit.framework.dll, defineConstraints UNITY_INCLUDE_TESTS) | Refresh, mở Window > General > Test Runner > EditMode | Test Framework 1.6.0 |
| Hành lang cho vào từ sai phòng | MapProgress > CheckBegin (so entryFromId với Neighbors) | Test `Corridor_StartsOnlyFromAdjacentRooms` | luật «hành lang chỉ từ 2 phòng kề» |
| Hai khu chạy cùng lúc | MapProgress > CheckBegin (ActiveAreaId != null) | Test `OnlyOneActiveArea` | luật mọi nhiệm vụ cần cả đội |
| Thắng sớm/muộn | MapProgress > Won / MissingMissionAreas | Test `Won_*` | DECISIONS mục 5 |

## 6. Rủi ro và câu hỏi cho chủ dự án
- GIẢ ĐỊNH A1 (cần xác nhận khi nối scene, S3 kế hoạch lớn): phòng bắt đầu được từ sân HOẶC từ hành lang kề (khu chuẩn bị «hành lang -> phòng»); hành lang chỉ từ phòng kề. Nếu sai chỉ sửa 1 điều kiện trong CheckBegin.
- GIẢ ĐỊNH A2: `Abandon` chỉ nhả khu đang làm, KHÔNG đánh dấu xong, KHÔNG xóa tiến độ khu khác (câu hỏi mở của kế hoạch lớn: rời giữa chừng giữ hay reset). Mỗi nhiệm vụ vẫn tự xử lý retry.
- GIẢ ĐỊNH A3: khu chưa có nhiệm vụ không bắt đầu được (`NoMission`) và được tính vào `MissingMissionAreas`; khi được đăng ký nhiệm vụ (`SetHasMission`) tự vào điều kiện thắng (theo DECISIONS mục 5).
- Rủi ro thấp: chỉ tạo file mới, không tham chiếu từ code đang chạy.

## 7. Tiến độ
- 2026-10-15 07:46 UTC: kế hoạch viết xong, bắt đầu S1.
- 2026-10-15 07:52 UTC: S1-S4 xong; 21/21 test xanh. Bước kế: T-003 (adapter + registry, cần kế hoạch riêng).
- 2026-10-15 07:58 UTC: chủ dự án XÁC NHẬN A1-A3 (OWNER_UPDATES). Không còn giả định mở.
