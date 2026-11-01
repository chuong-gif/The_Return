# KẾ HOẠCH T-033: Sửa 4 bug chủ dự án báo khi chơi thật   (file: .ai/PLANS/T-033-owner-bugs-1029.md)

Trạng thái: XONG (chờ chủ dự án chơi thật) | AI giữ: Claude Sonnet 5.5 (phiên #8) | Cập nhật: 2026-10-31 15:34 UTC

## 1. Bug và NGUYÊN NHÂN GỐC (đã chẩn đoán)
- Bug 1 (phòng 1-6) và 3 (hành lang) CÙNG nguyên nhân: MissionFlowController.TryStartFromGates yêu cầu CẢ ĐỘI đứng trong hộp preparationZone bên trong (CountInside == PartySize). Hộp đó chỉ sâu 4 m (z từ 2,5 đến 6,5 m sau cửa) trong khi phòng/hành lang sâu 47-65 m. Người chơi thật đi sâu và tản ra nên không bao giờ đủ cả đội trong hộp; sau 3 giây không ai ở cả hai vùng thì cửa tự đóng (disarm) và nhiệm vụ không bắt đầu. Test cũ (dịch chuyển vào TÂM hộp) không bắt được. Mọi khu dùng chung luật này nên mọi phòng/hành lang đều bị.
- Hướng sửa (đã viết, CHƯA xác minh): SchoolPreparationGate.IsEntered/CountEntered tính «đã vào khu» = trong hộp HOẶC phía trong cửa trong tầm enteredDepth=30 m, enteredSideMargin=14 m; MissionFlowController dùng CountEntered khi bắt đầu khu/giữ armed.
- Bug 2 (Hanh lang 1 không kết thúc): NGUYÊN NHÂN GỐC: QuietCorridorPrototype.AllAcross yêu cầu MỌI người đứng trong hộp goalZone, nhưng người giữ nút B phải đứng sát nút B (≤ 2,8 m) mà vị trí tự nhiên của họ (phía cửa) nằm NGOÀI hộp goalZone (ví dụ lệch 0,3 m) nên cả đội không bao giờ đủ khi B còn được giữ. Test đi bộ + giữ nút bằng TryToggleStation tái hiện (đỏ, 2 và 4 người). Sửa: người đang giữ nút B (State.Owner(1)) được tính là đã tới đích.
- Bug 4 (Phong_6 không có chữ «bấm E»): AreaHudProviders của 6 khu mới không điền Prompt/Controls (OnGUI dự phòng bị tắt khi dùng Canvas). Sửa: mỗi provider điền Prompt (dòng nhắc khi nhìn vào vật) và Controls (dòng phím).
- Phát hiện thêm (phòng ngừa, mọi nhiệm vụ có nút): các tia tương tác/nhìn thấy nút chỉ bỏ qua thân của chính người điều khiển nên đồng đội đứng chắn làm mất tương tác. Sửa: PrototypePartyController.IsPartyBody bỏ qua thân của cả đội; dùng ở TryGetInteractionHit, QuietCorridor, GradeRepair, NavigationExam, TeacherTruth và trạm DontLookBack (KHÔNG đổi tia «nhìn thấy lưng» của DontLookBack vì đó là luật chơi).

## 2. Subtask
| ID | Việc | Trạng thái | Bằng chứng |
|---|---|---|---|
| S1 | Chẩn đoán bug 1+3 | DONE | đọc MissionFlowController/SchoolPreparationGate/MapProgress/MissionRegistry; vùng không chồng nhau (30 cổng); log Editor không có exception |
| S2 | Sửa bug 1+3 (CountEntered) + test đi bộ SchoolFloorWalkPlayTests | DONE (đã chạy xanh) | file: SchoolPreparationGate.cs, MissionFlowController.cs, SchoolFloorCanvasPresenter.cs (sửa 17:40 giờ máy), Tests/PlayMode/SchoolFloorWalkPlayTests.cs |
| S3 | Chẩn đoán + sửa bug 2 | DONE | Corridor_RealisticHandover_WalkAndHold (2 và 4 người) đỏ rồi xanh; QuietCorridorPrototype.AllAcross + IsPartyBody |
| S4 | Chẩn đoán + sửa bug 4 | DONE | AreaHudProviders điền Prompt/Controls; FillHud kiểm 6 khu: Phong_6 «Nhìn vào bục rồi bấm E», Phong_5, Hanh_Lang_4/6 có gợi ý; Hanh_Lang_2/5 không có tương tác E nên chỉ dòng phím |
| S5 | EditMode + PlayMode xanh, Console 0 lỗi, tài liệu, REVIEW | DONE | EditMode 279/279 (có 6 test mới PrototypePartyControllerInteractionTests); PlayMode SchoolFloorFlowPlayTests 11/11; Corridor realistic 2/2; Console 0 lỗi |

## 3. Tiến độ
- 2026-10-31 15:34 UTC: lần làm trước hết token giữa chừng sau khi đã sửa code S2 mà chưa ghi LOG. Phiên này tìm lại, ghi kế hoạch, sẽ chạy test xác minh.
- 2026-10-31 15:45 UTC: S2 XANH: EveryGate_TeamWalksDeeperIntoArea(4 người, mọi cổng) 6 phút 15 giây; AfterPhong1_WalkingTeam(2 và 4); Corridor_HandoverThenTeamAtGoal(2 và 4). Bug 4: provider HUD 6 nhiệm vụ mới không điền Prompt/Controls (OnGUI dự phòng có chữ [E] nhưng bị tắt khi dùng Canvas). Bug 2: logic hoàn thành đúng khi đã bàn giao qua State.Hold; còn nghi TryToggleStation (khoảng cách 2,8 m + tia chắn) chặn giữ nút B ở lúc chơi thật.
- 2026-10-31 20:41 UTC: S3-S5 xong. Không chạy lại EveryGate_TeamWalksDeeperIntoArea (6 phút) vì lần này không đụng luồng cổng. Backup file cũ: Temp/T033_backup/*.bak.
- 2026-10-31 21:05 UTC: bug 2 lần 2 (chủ dự án đứng ở PreparationZone cổng ra vẫn không xong): mở rộng Team_Goal trong PF_QuietCorridor; test Corridor_TeamStandsInExitGatePreparationZone_Completes xanh (2 và 4 người). Chờ chơi thật.
