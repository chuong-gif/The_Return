# Player.fbx: 6 clip (Idle, Walking, Walking Backwards, Running, Jump, Sitting) không có chuyển động

- Phát hiện: 2026-10-30, phiên #6, khi test T-010 (PlayerModelPlayTests: Idle_*/Walking_* báo Inconclusive).
- Bằng chứng: đọc thẳng file FBX nhị phân (8316 đường cong KeyTime), MỌI đường cong chỉ có 2 key (đầu và cuối) cùng giá trị. Sau import, Idle/Walking/Running/Jump/Sitting/Walking Backwards có cùng một tư thế tĩnh (vd LeftArm euler 7.9,0,-1.3; LeftUpLeg -1.3,-0.1,176.0), chỉ T-Pose khác. Tắt nén/resample vẫn y vậy => dữ liệu keyframe không có trong file, không phải lỗi import của Unity.
- Hậu quả: Animator đổi state đúng (Idle/Walk/WalkBack/Run/Jump/Sit) nhưng nhân vật đứng yên một tư thế.
- Cách khắc phục (phía Blender, chủ dự án làm): xuất lại FBX với Include > Animation bật, Bake Animation bật (Key All Bones, Force Start/End Keying), tắt NLA Strips nếu action không nằm trong NLA, chọn All Actions; kiểm từng action có keyframe trong Dope Sheet. Thay Assets/My_Asset/Player.fbx bằng file mới (giữ .meta để giữ cấu hình). Unity đã sẵn Avatar, loop clip, AC_Player, PlayerAnimationDriver; chỉ cần chạy lại PlayMode, 2 test Inconclusive sẽ tự chuyển thành chạy thật.
- Chưa có phím nhảy trong game; clip Jump chỉ phát khi người chơi rời mặt đất > 0.25 s.
