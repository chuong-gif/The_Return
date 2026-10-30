# Player.fbx: 6 clip (Idle, Walking, Walking Backwards, Running, Jump, Sitting) không có chuyển động

- Phát hiện: 2026-10-30, phiên #6, khi test T-010 (PlayerModelPlayTests: Idle_*/Walking_* báo Inconclusive).
- Bằng chứng: đọc thẳng file FBX nhị phân (8316 đường cong KeyTime), MỌI đường cong chỉ có 2 key (đầu và cuối) cùng giá trị. Sau import, Idle/Walking/Running/Jump/Sitting/Walking Backwards có cùng một tư thế tĩnh (vd LeftArm euler 7.9,0,-1.3; LeftUpLeg -1.3,-0.1,176.0), chỉ T-Pose khác. Tắt nén/resample vẫn y vậy => dữ liệu keyframe không có trong file, không phải lỗi import của Unity.
- Hậu quả: Animator đổi state đúng (Idle/Walk/WalkBack/Run/Jump/Sit) nhưng nhân vật đứng yên một tư thế.
- Cách khắc phục (phía Blender, chủ dự án làm): xuất lại FBX với Include > Animation bật, Bake Animation bật (Key All Bones, Force Start/End Keying), tắt NLA Strips nếu action không nằm trong NLA, chọn All Actions; kiểm từng action có keyframe trong Dope Sheet. Thay Assets/My_Asset/Player.fbx bằng file mới (giữ .meta để giữ cấu hình). Unity đã sẵn Avatar, loop clip, AC_Player, PlayerAnimationDriver; chỉ cần chạy lại PlayMode, 2 test Inconclusive sẽ tự chuyển thành chạy thật.
- Chưa có phím nhảy trong game; clip Jump chỉ phát khi người chơi rời mặt đất > 0.25 s.

## ĐÃ SỬA 2026-10-30 (phiên #6, kết nối Blender MCP)
- Trong Player.blend các action CÓ keyframe (Idle 251 frame, Walking 32, Walking Backwards 30, Running 22, Jump 24, Sitting 36), mỗi action nằm trong một track NLA của armature Player. Lỗi nằm ở xuất FBX: khi bake bằng NLA, exporter của Blender 5.2 để track Idle ở trạng thái solo nên mọi clip khác ra tư thế tĩnh; khi bake All Actions mà NLA còn bật thì cũng chỉ Idle chuyển động.
- Cách xuất đúng: tạm đặt armature.animation_data.use_nla=False và bỏ solo mọi track NLA, rồi export_scene.fbx với bake_anim, bake_anim_use_all_actions=True, bake_anim_use_nla_strips=False, bake_anim_force_startend_keying, bake_anim_simplify_factor=0, add_leaf_bones=False, axis -Z/Y, FBX_SCALE_NONE, không nhúng texture; sau đó khôi phục use_nla/solo/action/frame. Không lưu file .blend.
- Kết quả: Player.fbx được thay bằng bản mới (giữ nguyên .meta; bản cũ còn trong git). Clip có chuyển động thật: Jump 160 đường cong, Running 181, Walking 158, Walking Backwards 85, Sitting 64, Idle 59 (thở nhẹ). Importer: Generic + Avatar từ model, loop Idle/Walking/Walking Backwards/Running, đổi tên clip bỏ tiền tố "Player|". Phân cấp mới: armature scale 1, mesh Ban Tay.004 scale 100/rot 270 (bù đơn vị, mesh 0.0166 m) - tương đương bản cũ.
- PlayMode 20/20 (2 test xương động Idle/Walking hết Inconclusive, test đế giày/chiều cao vẫn qua).
- Còn thừa Assets/My_Asset/Player_Animated.fbx (bản nháp trùng nội dung với Player.fbx) - chủ dự án xóa tay.
