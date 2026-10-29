# CURRENT (HOT — ≤ 12 dòng, GHI ĐÈ khi đổi)

Cập nhật: 2026-10-29 04:44 UTC bởi Claude Sonnet 5.5 (phiên #8)
Scene lock: đã BỎ (chủ dự án 2026-10-29: AI chạy nối tiếp). Vẫn chụp snapshot trước khi sửa scene. Editor lúc 2026-10-29 04:44 UTC: play=False compiling=False scene=SchoolFloorPrototype dirty=False.
Đang chạy: #8 xong T-020 + T-021 (REVIEW). #6 (T-005) và #7 (T-009) có START 28/10 chưa DONE. LOG gần nhất của phiên khác: #9 DONE T-008 (2026-10-29 04:22 UTC).
Trạng thái: code 12 khu + HUD theo khu + MainMenu/Party/Settings + âm thanh bản thử; 11 root nhiệm vụ đều là prefab (T-020, T-021). EditMode 256/256 (2026-10-29). PlayMode T-005: 10/11; lỗi EveryGate_OfEveryMissionArea_StartsItsArea ở cổng «Hanh lang 1_Vao/Ra» (khu Hanh_Lang_6), CÓ SẴN trước T-020, chưa chẩn đoán.
Cảnh báo:
- Chạy Play Mode: đặt Cursor.lockState=Locked mới thấy HUD. Không chạy builder/migration cũ (menu The Return/*). Không commit/push/xóa khi chưa được yêu cầu.
- Features/Audio TRÙNG kiểu AudioCueRules với Features/GameAudio (có thể lỗi ambiguous): chủ dự án nên XÓA thư mục Features/Audio (AI không xóa được).
- Chờ chủ dự án CHƠI THẬT: T-006a2/b2/c/d2/e2/f, T-008, T-011, T-017..T-021. File thừa chủ dự án xóa tay: Assets/Screenshots/*.png, BreakerParts.cs (rỗng). Snapshot mới: Scenes/BeforeMissionPrefabs_20261029_043005.unity, BeforeOldMissionPrefabs_20261029_044208.unity.
Chi tiết cũ: SESSIONS/CURRENT_FULL_20261022.md. Chủ dự án báo thay đổi ở OWNER_UPDATES.md (đọc 3 mục cuối).
