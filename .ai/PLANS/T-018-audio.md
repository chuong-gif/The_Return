# T-018 — Âm thanh (KẾ HOẠCH, chờ chủ dự án duyệt)

Đề xuất bởi chủ dự án 2026-10-22. Soạn: phiên #3 (2026-10-22 08:22 UTC). Chưa sửa code/scene.

## 1. Hiện trạng (đã đo)
- Chỉ có 2 clip trong dự án (Assets/ThirdParty/Free Wood Door Pack/Audio, tiếng cửa gỗ). Chỉ QuietSpeakerFeedback.cs dùng AudioClip; GameSettingsRuntime.cs có phần âm lượng. Chưa có AudioManager, mixer, nhạc, bước chân, tiếng giao diện.
- Cơ chế game có nhiều chỗ NÊN dựa vào âm thanh: Hanh_Lang_1 (loa rè), Phong_4 (giáo viên nghe kênh), Phong_6 (hợp xướng: mỗi bục một nốt), Hanh_Lang_5 (loa «Cô nói» — chỉ một người nghe tiền tố), Phong_5 (công tắc/cầu dao).

## 2. Kiến trúc đề xuất
- Domain (thuần, có test): `AudioCueId` (enum/chuỗi ổn định) + `AudioCueRules` (không phát chồng quá N lần/giây, cooldown theo cue).
- Data: `AudioCueBank` (ScriptableObject): cue -> danh sách clip, âm lượng, pitch ngẫu nhiên, bus (Master/Music/SFX/Voice/Ambience), 3D hay 2D.
- Presentation: `AudioService` (MonoBehaviour, một object trong scene): `Play(cue, vị trí?)`, pool AudioSource, tiếng nền theo khu. Các Prototype gọi qua interface `IAudioPlayer` (không gọi AudioSource trực tiếp) để test và để sau này đồng bộ mạng.
- AudioMixer: Master/Music/SFX/Voice/Ambience; GameSettingsRuntime điều chỉnh bus (nối với Settings của T-008).
- Online (T-009): cue có chủ thể (vai) để người khác nghe được; không phát lặp ở máy khác.

## 3. Danh sách cue
- Chung: bước chân (sàn hành lang/phòng), cửa mở/đóng (đã có 2 clip), cổng khóa/mở, bắt đầu khu, khu xong (chime), thất bại/cảnh cáo, nút giao diện, đổi vai, màn thắng, nhạc nền sân + nhạc/nền riêng khu.
- Theo khu: P1 ngồi/đứng ghế, giấy; HL1 loa rè, nút giữ; P2 lật hồ sơ, đóng dấu; P3 đóng dấu sa bàn, ô bị khóa; HL3 tiếng quay đầu sai; P4 giọng giáo viên 4 kênh; P5 công tắc, cầu dao, quá tải; P6 4 nốt hợp xướng + sai nốt; HL2 kính nứt/vỡ; HL4 đèn, nhập sai; HL5 «Cô nói»/lệnh bước/đứng + còi cảnh cáo; HL6 ván, thẻ quẹt.

## 4. Các bước (kiểm mức 3 trước, mức 4 Play Mode sau)
S1 Domain cue + test. S2 AudioCueBank + AudioService + mixer + IAudioPlayer. S3 nối cue chung (cửa, cổng, xong khu, thắng, UI). S4 nối cue từng khu (6 khu mới trước). S5 nền + nhạc. S6 nối Settings âm lượng. S7 kiểm Play Mode + chủ dự án nghe thật.
Scene: cần AudioService + nguồn nền theo khu -> Scene lock + snapshot + phép chủ dự án.

## 5. Câu hỏi cho chủ dự án
1. Nguồn âm thanh: dùng công cụ sinh âm (generate_audio của Unity MCP) cho bản thử, mua/tải gói miễn phí, hay bạn tự cung cấp?
2. Phong cách (kinh dị trường học nhẹ, tĩnh, hay sáng sủa)?
3. Có nhạc nền không, hay chỉ tiếng môi trường?
4. Giọng nói: lời cô giáo/loa dùng giọng thật, TTS, hay chỉ tiếng «ù» + chữ?
5. Cho phép sửa scene để thêm AudioService và nguồn nền?
