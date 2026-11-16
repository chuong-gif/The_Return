# KẾ HOẠCH T-045: Màn thắng + về MainMenu + ô thay âm thanh trong scene   (file: .ai/PLANS/T-045-win-audio-slots.md)

Trạng thái: XONG (chờ chủ dự án lưu scene và thử) | AI giữ: Claude | Cập nhật: 2026-11-13 08:07 UTC

## 1. Mục tiêu và nghiệm thu
- Chủ dự án duyệt trực tiếp trong chat (2026-11-13): (a) hiện màn thắng khi xong 4 nhiệm vụ ở MapSchoolF, có nút về MainMenu; (b) một object trong scene để kéo thả clip âm thanh (bước chân, nhảy, nhạc nền, thắng, tương tác vật phẩm, xong nhiệm vụ) với Tooltip tiếng Việt; (c) tiếng nhảy.
- Nghiệm thu: compile sạch; Play Mode: xong 4 nhiệm vụ -> hiện màn thắng + tiếng game.win; nút Về menu chính nạp MainMenu; gán clip vào Audio_Slots thì nghe clip đó. Mức 3 (compile) rồi 4 (runtime) khi chủ dự án thử.

## 2. Phạm vi
- Được đụng: WinScreenPresenter.cs, SchoolFloorAudioDirector.cs (thêm nhận biết nhảy), AudioCues.cs (thêm hằng Jump), GameAudioSlots.cs (MỚI, assembly TheReturn.GameAudio), scene MapSchoolF (thêm object Audio_Slots, gán missionFlow/flow cho WinScreen).
- KHÔNG đụng: PrototypePartyController (Core không được tham chiếu Feature), AudioCueBank_Main.asset, gameplay nhiệm vụ, builder cũ.
- Snapshot scene: có, chụp ngay trước khi sửa scene. Scene đang DIRTY lúc bắt đầu (2026-11-13) nên chưa lưu đè; chờ chủ dự án lưu hoặc xác nhận.

## 3. Ngữ cảnh đã đọc
| File | Kết luận |
|---|---|
| WinScreenPresenter.cs (2026-11-13) | Update thoát sớm khi missionFlow==null; trong scene WinScreen.missionFlow và flow đều NULL nên không bao giờ hiện |
| AudioCueBank.cs / AudioBootstrap.cs / AudioService.cs | Bank cue->clip nạp từ Resources/AudioCueBank_Main; AudioService tự tạo lúc chạy; svc.bank là field công khai |
| SchoolFloorAudioDirector.cs | Đã phát Step, AreaDone, Win, nhạc sân/nhiệm vụ; do SchoolFloorFlow.Start tự thêm; chưa có tiếng nhảy |
| PrototypePartyController.cs | Nhảy ở TickInput (phím Space); Core không thể gọi AudioHub |
| SceneTransitionService.cs | Chuyển scene mờ dần; Instance là null nếu scene không có object này |

## 4. Subtask
| ID | Việc | File | Cách kiểm | Trạng thái | Bằng chứng |
|---|---|---|---|---|---|
| S1 | Gán missionFlow/flow cho WinScreen (+ tắt showMissingNote) | scene MapSchoolF | đọc lại SerializedObject | XONG (chưa lưu scene) | đọc lại: missionFlow/flow đã gán |
| S2 | Nút Menu chính | WinScreenPresenter.cs | compile + Play | XONG | Play: Show dựng nút, OnMainMenu nạp MainMenu |
| S3 | GameAudioSlots + object Audio_Slots | GameAudioSlots.cs, AudioCues.cs, scene | compile + Play | XONG (chưa lưu scene) | Play: bảng nhân bản, ghi đè step/jump, asset gốc không đổi |
| S4 | Tiếng nhảy trong director | SchoolFloorAudioDirector.cs | compile | XONG (chưa thử bấm Space thật) | compile sạch |

## 5. Bản đồ lỗi
| Triệu chứng | Nơi xem | Cách kiểm rẻ nhất | Liên quan |
|---|---|---|---|
| Không hiện màn thắng | WinScreen (missionFlow/flow) > WinScreenPresenter.Update | Inspector WinScreen, Registry.Won | MissionRegistry |
| Gán clip mà không nghe | GameAudioSlots.Apply, AudioService.bank | Console cảnh báo AudioService | AudioBootstrap |
| Không có tiếng nhảy | SchoolFloorAudioDirector.TickJump, cue player.jump | Gán clip Jump trong Audio_Slots | CharacterController.isGrounded |
| Về menu chính lỗi khi chơi online | WinScreenPresenter.OnMainMenu | Netcode chưa Shutdown | NetworkSessionService |

## 6. Rủi ro
- Online: nút Về menu chính chưa Shutdown NetworkManager (chỉ đúng cho chơi offline).

## 7. Tiến độ
- 2026-11-13: chủ dự án duyệt; bắt đầu S2-S4 (code) trước, S1/S3-scene sau khi scene sạch.
- 2026-11-13: snapshot Scenes/Maps/BeforeAudioWin_20261113_080952.unity (chụp từ file trên đĩa, trước khi sửa scene đang dirty). Đã sửa scene trong bộ nhớ, CHƯA lưu: chờ chủ dự án lưu. Chưa kiểm: xong 4 nhiệm vụ thật để thấy màn thắng và tiếng nhảy bằng Space thật.
