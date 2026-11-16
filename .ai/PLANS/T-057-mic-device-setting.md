# T-057: Chọn micro thu âm trong Cài đặt → Âm thanh

Trạng thái: REVIEW | Claude (chat #6, Unity MCP) | 2026-11-16 (đánh số lại từ T-055 vì trùng mã với task tên người chơi của AI khác)

## 1. Yêu cầu của chủ dự án
Mic tai nghe bị hỏng nên muốn chọn mic có sẵn của máy ngay trong menu Cài đặt, mục Âm thanh.

## 2. Thiết kế
- `Core/Runtime/MicDeviceSettings.cs`: lưu tên mic ưa thích (PlayerPrefs `TheReturn.Set.MicDevice`, rỗng = mặc định hệ thống), danh sách mic (ưu tiên danh sách Vivox khi đã vào kênh thoại, chưa vào thì `Microphone.devices`), so khớp tên mềm (Unity cắt tên dài, Vivox tên đầy đủ), sự kiện `Changed`. Không phụ thuộc Vivox nên GameSettings và Voice cùng dùng.
- Menu: prefab `PF_SettingsMenu` trang Âm thanh thêm Header «MICRO THU ÂM» + dòng chọn `Row_Micro thu âm` (id `audio.mic`, nhân bản từ dòng chọn của trang Đồ họa, nới rộng) + ghi chú. `SettingsMenu.RefreshSelectors` đổ danh sách (mục đầu «Mặc định của hệ thống», bỏ «Default…» và «No Device» của Vivox, tên dài cắt 60 ký tự). «Khôi phục âm thanh» (`ExtraSettings.ResetAudio`) cũng về mic mặc định.
- Thoại: `VoiceChatService.ApplyPreferredMicAsync` đặt `SetActiveInputDeviceAsync` khi vào kênh, khi người chơi đổi mic, khi Vivox báo đổi thiết bị; rỗng/không thấy = «Default System Device». Công bố danh sách mic Vivox cho menu; rời kênh thì xóa danh sách.

## 3. Kiểm
- Play: menu hiện «Micro thu âm: Mặc định của hệ thống»; bấm › chọn «Microphone Array (Intel® Smart Sound…)», PlayerPrefs lưu tên đầy đủ (ảnh chụp).
- Play Relay thật (phòng CQNRJC): chọn trước mic Intel → vào kênh thoại thì Vivox Active/Effective = Microphone Array (Intel…); đổi sang Realtek khi đang trong phòng → Active = Realtek ngay; về mặc định → Active = Default System Device (Effective = mic mặc định của máy).
- Test mới `Tests/EditModeOnline/MicDeviceSettingsTests.cs` (8 ca: so khớp tên, lưu, sự kiện Changed, ưu tiên danh sách Vivox). EditMode 1015: 1 đỏ không liên quan (BreakerPanelTests của T-056).

## 4. Chưa kiểm / lưu ý
- Chưa thử trên máy có tai nghe cắm/rút giữa chừng (sự kiện AvailableInputDevicesChanged đã nối nhưng chưa kích hoạt thật); chưa kiểm tiếng thật qua 2 người (chỉ kiểm thiết bị Vivox đang dùng).
- Prefab sao lưu: `Features/GameSettings/Backups/PF_SettingsMenu_BeforeMic_20261116_1756.prefab` (đã chuyển khỏi Resources).
- Chưa có thanh đo mức mic để tự kiểm mic có thu tiếng không (có thể thêm sau).
