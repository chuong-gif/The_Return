# LOG — nhật ký vận hành gần đây

LOG không phải nguồn trạng thái. Xem `CURRENT.md`, `TASKS.md`, `QUEUE.md` và plan đang làm.

## Quy tắc chống phình
- Giữ tối đa 8 mục gần nhất và tối đa 8 KB; mỗi mục 1–3 dòng, ghi kết quả và mức xác minh.
- Chi tiết bền vững chuyển vào plan/decision/system card; trạng thái task chỉ lưu ở TASKS/QUEUE.
- Khi vượt giới hạn, gộp hoặc bỏ START/PROGRESS/DONE đã được ghi đầy đủ ở nơi chuẩn; không tạo bản sao LOG_FULL cho task/hệ thống ngoài Core4.
- Không chép nguyên lỗi test, danh sách file dài hoặc kết quả lặp. Ghi rõ chưa kiểm; M3 test, M4 runtime/build, M5 chủ dự án xác nhận.

## Mục gần nhất

### 2026-11-14 | T-052 | DONE (M3 + M4 một máy; chưa nhìn/nghe bằng mắt tai)
- Scene Sanh_Cho (đã lưu): 4 `Visual_Replaceable` (khối bầu dục) → `PF_PlayerModel` (cùng prefab MapSchoolF, 7 mesh + AC_Player); CharacterController như MapSchoolF (cao 1.75, bán kính 0.27, tâm 0.9), y=0, `bodies` = null; thêm `Audio_Slots` (cùng 2 clip bước + clip nhảy + âm lượng như MapSchoolF) và `Lobby_Audio_Director` (script mới `Lobby/Runtime/LobbyLocomotionAudio.cs`, bản sao TickSteps/TickJump của SchoolFloorAudioDirector); `TheReturn.Lobby.asmdef` +TheReturn.GameAudio.Domain. Bản gốc: `Backups/Sanh_Cho_BeforePlayerSwap_20261114.unity.txt`, `Backups/TheReturn.Lobby_BeforeT052.asmdef.txt`.
- Kiểm: compile sạch; EditMode toàn bộ 988: 983 đạt, 0 đỏ, 5 bỏ qua (C7 có sẵn). Play Sanh_Cho: 4 người có mô hình + Idle, người điều khiển ẩn thân (ShadowsOnly), camera 1.65; bảng cue có clip bước/nhảy của MapSchoolF; đi bộ → cue bước đúng clip; Space (giả lập qua Input System) → nhảy cao 0.81 m + cue `jump`; Sanh_Cho → MapSchoolF → MainMenu → Sanh_Cho: bảng cue áp/trả đúng, không rò. Console 0 lỗi/cảnh báo.
- Chưa: nhìn/nghe bằng mắt tai, bản build. `The Return/Lobby/Rebuild Monumental Lobby` (LobbySceneBuilder) vẫn tạo khối bầu dục và không có Audio_Slots: chạy nó sẽ mất thay đổi T-052 (chưa sửa builder). Đổi clip bước/nhảy: sửa Audio_Slots ở CẢ MapSchoolF và Sanh_Cho.

### 2026-11-14 | T-053 | START
- Chủ dự án nhờ làm tiếp việc UI dở của phiên chat trước (hết lượt giữa chừng, bước 2/5: menu Cài đặt). Phát hiện file chưa log: ModernUI.cs, ExtraSettings.cs, SettingsMenu.cs, GameSettings.asmdef, SchoolFloor.asmdef (05:08-05:11 UTC). Plan: .ai/PLANS/T-053-ui-professional.md. Sẽ snapshot scene trước khi sửa.
- 05:30 UTC tiếp quản T-053 (phiên trước hết lượt). Việc phiên trước chưa log thêm: MainMenuSkin.cs, MainMenu.asmdef, MainMenu.unity, snapshot BeforeUiRedesign.

- 2026-11-14 11:47 UTC | T-053 | chat #4 tiếp quản phần còn lại (xem PLANS/T-053 §5).

### 2026-11-14 | T-053 phần lẻ (chat #2) + TIẾP QUẢN T-054
- T-053 (chat #2, trước khi chuyển): MainMenu Panel_Main/Title 124→110 + tắt Best Fit (nguyên nhân «to hơn 10%»: Best Fit giữ max=fontSize, bản cũ bị kẹt ở max ban đầu ~110); MapSchoolF PauseMenu: Retry rộng đủ hàng, Note dev đổi thành «Chơi một máy: phím 1–4 đổi vai.»; SchoolFloorCanvasHud.cs minimap kiểu ModernUI (có thể đã bị AI sau thay bằng SchoolFloorMiniMapView — chưa đối chiếu). Snapshot `Backups/*_BeforeT053Fix_20261114.unity.txt`. EditMode 988: 983/0/5 sau bake.
- Chủ dự án bảo dừng và làm T-054 (AI giữ trước hết lượt). Tiếp tục từ S4: thoại âm lượng từng người, rồi S5 LobbyOnlineController, S6 panel Online MainMenu, S7 UI dev, S8.

### 2026-11-14 | T-054 | chat #5 TIẾP QUẢN (chủ dự án: dừng việc khác, tập trung T-054)
- Ghi sổ việc chat #4 (T-053) chưa log: `Shared/Runtime/UI/ModernGui.cs` (kiểu IMGUI ModernUI: Begin/End, Skin, Title/Dim/Primary/Danger/Pill), `RoomHud.cs` + `VoiceHud.cs` + `OnlineSessionGuard.cs` (OnGUI dùng ModernGui), asmdef Room/Voice +TheReturn.Shared; bản gốc `Backups/*_BeforeT053Gui.*.txt`. Sau đó AI khác còn sửa RoomHud/VoiceHud (17:13/18:10) nên cần đối chiếu nếu thấy lệch.
- T-054: code S4–S6 đã có (xem PLANS/T-054 §tiến độ), thiếu object scene. Bắt đầu dựng Canvas Sanh_Cho và panel MainMenu bằng script Editor.

### 2026-11-15 | T-054 tạm dừng + T-055 START (chat #2)
- T-054: chat #5 đã dựng xong Lobby_OnlineCanvas (Sanh_Cho, host 1 máy chạy: mã phòng, mic/âm lượng, nút) và Panel_Online (MainMenu, đã nối; mình lưu lại MainMenu dirty). Khi thử Play MainMenu→Chơi online màn hình đen (editor rất chậm ~5fps, chưa xác định lỗi). Còn lại: S7 UI dev EditorOnly, S8 Build Settings 3 scene + smoke.
- T-055 (chủ dự án giao): tên người chơi nhập ở panel Chơi online thay Minh/An/Bình/Chi trong map. Sẽ đụng: PrototypePartyController (Core), OnlinePartySlots/RoomMapHandoff (Network), AttendanceOnlineHost, SchoolFloorCanvasHud; bản gốc `Backups/*_BeforeT055.txt`.

### 2026-11-16 | T-054 | TẠM DỪNG giữa chừng (chat #5)
- Xong S5+S6 (xem PLANS/T-054 cuối file), scene đã lưu, compile sạch. Còn S7, S8, kiểm 2 tiến trình. Chủ dự án chuyển sang sửa nhiệm vụ Cầu dao (MapSchoolF).

### 2026-11-16 | T-056 | START
- Chủ dự án giao: sửa nhiệm vụ Cầu dao cho dễ hiểu/dễ giải (T-054 tạm dừng sau S5+S6). Plan: .ai/PLANS/T-056-breaker-easier.md. Sẽ đụng các file ở §4 của plan; bản gốc Backups/*_BeforeT056.*.txt.

### 2026-11-16 | T-055 | DONE → REVIEW (M3 + một máy; chưa thử 2 máy = M5)
- Luồng tên: panel Chơi online (tên nhập) → RoomRoster (đã chốt tên duy nhất) → khi bắt đầu, `RoomMapHandoff` ghi `OnlineNames.SetSynced(tên theo ô)` trên MỌI máy (cả `BeginForTest`) → `AttendanceOnlineHost.TryBuild` dùng `OnlineNames.Resolve(...)` thay `roleNames` (thiếu tên/trùng thì tự bù/đánh số) → tên đi vào manh mối, ghế, «Bạn là …» của mọi client qua public state có sẵn.
- Mới/sửa: `Network/Online/OnlineNames.cs` (mới), `OnlineNameTag.cs` (mới, biển «số • tên» trên đầu nhân vật người khác, OnlineAvatar tự gắn), `OnlineAvatar.cs`, `RoomMapHandoff.cs`, `PrototypePartyController.cs` (SlotNameProvider/SlotName), `AttendanceOnlineHost.cs`, `SchoolFloorCanvasHud.cs` (NameOfSlot + sửa lỗi MeshRenderer trùng của NameTag), test `OnlineNamesTests.cs` (+asmdef). Bản gốc `Backups/*_BeforeT055.*`.
- Kiểm: compile sạch; 6 test mới đạt; EditMode 1015: 1014 đạt, 1 đỏ `BreakerPanelTests.Generator_AlwaysHasUniqueSolution…` (seed 1: «mỗi công tắc đảo 2-3 đèn») — không liên quan T-055, do `BreakerGenerator/Settings` bị AI khác sửa 11-16 11:03, cần AI đó cập nhật test hoặc luật. Play một máy: roster nhận đúng tên có dấu «Hùng Test», `PrototypePartyController.SlotName` trả đúng; HUD offline `VAI n / ĐỘI` không chạy ở chế độ online (HUD online riêng), tên hiện qua Điểm danh + biển tên. Chưa thử: 2 người thật, nhìn biển tên bằng mắt, Điểm danh online thật.

### 2026-11-17 | T-058 | START
- Chia toàn bộ thay đổi đang có thành commit theo nhóm; không chỉnh sửa code, scene hoặc asset. Kiểm danh sách đường dẫn và diff trước khi stage.

### 2026-11-17 | T-058 | DONE (M2)
- Đã tạo 8 commit cho docs, BreakerPanel, network, audio, UI/map, mission dependency, URP settings và recovery/folder metadata. Không sửa nội dung code/scene/asset và không chạy test.
- `git status --short` sạch; các cảnh báo `diff --check` chỉ do giá trị YAML/meta Unity để trống, không chỉnh sửa serialization.
