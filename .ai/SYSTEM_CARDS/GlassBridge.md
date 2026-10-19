# CARD: GlassBridge (Cầu kính)

Khu / vai trò: Hanh_Lang_2 (nhiệm vụ MỚI, T-006e/T-006e2). Cập nhật: 2026-10-17 12:33 UTC. Trạng thái: Domain + Presentation + adapter + cảnh đã có; kiểm Play Mode bằng CharacterController giả lập, CHƯA có người chơi thật.

## A. Bản đồ
- Domain (Features/GlassBridge/Runtime/Domain, asmdef TheReturn.GlassBridge.Domain): GlassBridgeRound (đề: dãy ô an toàn), GlassBridgeGenerator (sinh theo seed), GlassBridgeState (vị trí -1 đài đầu / 0..Rows-1 / Rows đài cuối; Step, Disembark, Retreat, TryGetDiagram, IsBroken, Reset). Test: Features/GlassBridge/Tests/EditMode/GlassBridgeTests.cs (27 test).
- Presentation (Features/GlassBridge/Runtime, asmdef TheReturn.GlassBridge): Presentation/GlassBridgePrototype.cs (theo dõi vị trí người chơi -> gọi State, vỡ ô, đưa người về đài đầu, bảng sơ đồ, HUD), Presentation/GlassTile.cs (hình + va chạm một ô, rào khi vỡ).
- Adapter: Maps/SchoolFloor/Runtime/Missions/GlassBridgeMissionAdapter.cs; đăng ký ở MissionRegistryFactory (GlassBridgeArea = Hanh_Lang_2); SchoolFloorFlow.glassBridge; RestartMap gọi glassBridge.StopSession().
- Scene (SchoolFloorPrototype): object gốc `GlassBridge_LogicOnly` (laneOrigin = chính nó; z+ từ đài đầu sang đài cuối, x+ là bên PHẢI người đi; LaneFloor, Rail_Left/Right, StartPlatform/EndPlatform trigger, 12 ô Tile_R1..6_L/R, Board_Start/End). Hai cổng `Hanh lang 3_Vao` / `Hanh lang 3_Ra` (nhóm Hanh_Lang_3_Preparation; TÊN CỔNG HÀNH LANG DÙNG SỐ CŨ: 'Hanh lang 3' = Hanh_Lang_2) có trong AccessController.gates và MissionFlow_Controller.bindings (Vao từ Phong_2, Ra từ Phong_3).
- Thiết kế + 6 giả định đã được chủ dự án xác nhận: PLANS/T-006e-glass-bridge.md.

## B. Triệu chứng -> nơi xem
| Triệu chứng | Nơi xem | Cách kiểm |
|---|---|---|
| Bước đúng mà bị té / bước sai mà không té | GlassBridgePrototype.Locate (hàng = round((z - firstRowZ)/rowSpacing), ô = dấu của x trong hệ laneOrigin); GlassBridgeRound.SafeTile | Đọc State.Round.CopySafeTiles(), vị trí laneOrigin.InverseTransformPoint(người) |
| Đứng trên cầu mà vẫn thấy sơ đồ / đứng ở đài mà không thấy | GlassBridgeState.CanSeeDiagram (OnBridge), GlassBridgePrototype.RefreshBoards (chỉ xét vai ĐANG điều khiển) | Đổi vai bằng phím 1-4 và đọc Board_Start.text |
| Đi vòng qua cầu bằng rãnh hai bên | Rail_Left/Rail_Right (rào chắn trong 2 rãnh sâu 0,4 m của hành lang, x từ 2,8 tới 8,2) | Đi CharacterController dọc rãnh phải bị chặn ở z≈-14,8 (đã kiểm) |
| Ô vỡ vẫn đứng lên được | GlassTile.SetBroken (tắt walkable, bật barrier) | Kiểm walkable.enabled / barrier.enabled |
| Nhiệm vụ không bắt đầu khi cả đội đứng ở cổng | MissionFlowController (bindings của `Hanh lang 3_Vao/Ra`), Registry khu Hanh_Lang_2 | Xem SYSTEM_CARDS/SchoolFloor.md mục B |
| Chơi lại mà ô còn vỡ | SchoolFloorFlow.RestartMap -> glassBridge.StopSession(true); GlassBridgePrototype.Begin luôn ResetTiles | Gọi flow.RestartMap(), đếm tile.IsBroken |

## C. Ghi chú đã xác minh
- 2026-10-17 12:33 UTC: Play Mode (teleport đội vào vùng Vao, đi thật bằng CharacterController 3,5 m/s): cả đội ở cổng thì Hanh_Lang_2 bắt đầu (entry Phong_2); đi đúng 6 hàng + lên đài cuối tính đúng RowOf; ô sai: vỡ, người đó về đài đầu, ô vỡ có rào chặn; rào hai bên chặn đi vòng; sơ đồ ẩn khi trên cầu; đủ 4 người qua thì Solved, khu done, 2 cổng mở. EditMode 185/185 xanh. Chưa người chơi thật.
- Hành lang có 2 rãnh bên thấp hơn lối giữa 0,4 m (CharacterController stepOffset 0,25): ai bước xuống rãnh ở khu đài sẽ KHÔNG leo lại được (vốn có sẵn từ thiết kế hành lang, chưa xử lý).

## D. Task liên quan
- T-006e DONE (Domain), T-006e2 REVIEW (Presentation + adapter + cảnh). Việc có thể làm tiếp: thay hình tạm (khối primitive + material M_GB_Surface/SchoolSage) bằng asset 3D thật; chặn rãnh bên ở khu đài; kiểm bằng người chơi thật.
