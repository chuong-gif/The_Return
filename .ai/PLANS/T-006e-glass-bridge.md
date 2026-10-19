# T-006e — Hanh_Lang_2 «Cầu kính» (GlassBridge): phần Domain

Trạng thái: Domain + test XONG (2026-10-17 02:42 UTC). Presentation và dựng cảnh: CHƯA làm (T-006e2, chờ chủ dự án cho thêm cổng/khu chuẩn bị cho hành lang trong scene). AI giữ: -

## Thiết kế gốc (đã duyệt ở PLANS/T-006-mission-designs.md, mục Cầu kính)
Cầu 6 hàng x 2 ô; mỗi hàng chỉ 1 ô chịu được. Hai đài (đầu và cuối) có bảng sơ đồ ô an toàn; người đang trên cầu không thấy. Một người đi, đồng đội ở đài đọc bảng và dẫn. Bước sai: ô vỡ, chỉ người đó về đầu cầu. Người dẫn lượt đầu cuối cùng cũng phải qua, đồng đội ở đài cuối dẫn lại. 2 người: một đi một dẫn rồi đổi chiều. Đề sinh theo seed.

## Đã làm
- `Features/GlassBridge/Runtime/Domain`: GlassBridgeRound (đề), GlassBridgeGenerator (sinh theo seed, cả hai bên đều có và không quá 3 hàng liền cùng bên), GlassBridgeState (vị trí -1 đài đầu, 0..Rows-1 trên cầu, Rows đài cuối; Step, Disembark, Retreat, TryGetDiagram, IsBroken, FallsOf, AcrossCount, Reset). asmdef riêng `TheReturn.GlassBridge.Domain` (noEngineReferences).
- `Features/GlassBridge/Tests/EditMode`: 27 test NUnit xanh (sinh đề, giới hạn tham số, luật bước/té/vỡ, quyền xem sơ đồ, lùi/xuống đài, thắng, Reset, kịch bản 2 người dẫn nhau và 4 người có té).

## Giả định tôi tự chọn (chủ dự án chưa nói, đổi được dễ)
1. Ô vỡ vỡ CHO CẢ ĐỘI và không bước lại được (nên một lần té làm lộ đáp án hàng đó cho người sau). Ô đúng KHÔNG để lại dấu vết/không sáng lên, để người dẫn vẫn cần thiết.
2. Người đi được lùi một hàng (`Retreat`), không bị té.
3. Hàng cuối phải gọi `Disembark` để lên đài cuối (hàng cuối vẫn là ô kính).
4. Sơ đồ ở đài đầu và đài cuối giống hệt nhau; quyền xem chỉ phụ thuộc đang đứng ở đài hay trên cầu.
5. Không có đồng hồ/giới hạn thời gian. `Reset` giữ cùng đề và giữ số lần té (thống kê).
6. Số hàng mặc định 6, chỉnh được 3..10 qua `Generate(players, seed, rows)`.

## Chưa làm (T-006e2)
- Adapter `GlassBridgeMissionAdapter` + đăng ký trong MissionRegistryFactory (khu `Hanh_Lang_2`) — sửa file dùng chung, làm cùng lúc với Presentation khi có người giữ lock.
- Presentation: GlassBridgePrototype, bảng sơ đồ ở hai đài, ô kính vỡ, cổng/khu chuẩn bị cho Hanh_Lang_2 trong scene (cần chủ dự án cho phép thêm).
- Thẻ SYSTEM_CARDS/GlassBridge.md (tạo khi có Presentation).

## Kết quả T-006e2 (2026-10-17 12:33 UTC)
- Chủ dự án xác nhận 6 giả định và cho phép làm (thêm cổng/khu chuẩn bị Hanh_Lang_2). Đã làm: GlassBridgePrototype + GlassTile + adapter + đăng ký registry + cảnh + 2 cổng mới (xem SYSTEM_CARDS/GlassBridge.md mục A).
- Số liệu cảnh: 6 hàng x 2 ô, ô 2,8 x 3,0 m cách nhau 4,5 m; làn rộng 5,6 m; đài là hai đầu hành lang (z -26..-14,5 và 14,5..26); rào chặn hai rãnh bên. Tất cả là hình tạm (khối primitive).
- Kiểm: Play Mode giả lập bằng CharacterController (mức 4 một phần), EditMode 185/185. Chưa người chơi thật.
- Để lại: thay hình thật; xử lý rãnh bên ở khu đài (người bước xuống không leo lại được); luật cửa mới đóng-từ-đầu do T-016.
