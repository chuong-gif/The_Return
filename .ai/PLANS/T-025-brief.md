# PHIẾU T-025: Nhân vật online (mỗi người một nhân vật cố định)
- Mục tiêu (1 câu): chế độ online: mỗi người sở hữu đúng 1 nhân vật CỐ ĐỊNH cả ván (không đổi vai); số nhân vật = số người trong đội (2-4); spawn + đồng bộ di chuyển/CharacterController. Chế độ 1 máy (phím 1-4) giữ nguyên.
- CHỦ DỰ ÁN ĐÃ QUYẾT: không đổi vai; đội 2-4 người cố định lúc lập (DECISIONS/2026-10-29-t009-owner-answers.md). Không còn bị chặn bởi T-022.
- Được đụng: Core/Runtime/Players/PrototypePartyController.cs (thêm chế độ, không phá chế độ cũ), Features/Network/*.
- KHÔNG được đụng: scene map; prefab nhân vật chỉ khi đã báo T-010 (phiên đang làm Player.fbx thay capsule) qua LOG. Scene map: không sửa nếu OWNER_UPDATES chưa cho phép; scene thử riêng trong Features/Network thì tự do.
- Cách kiểm xong: Play Mode 2 bản (rồi 3, 4 nếu máy cho phép): mỗi người điều khiển 1 nhân vật, thấy nhau; test cũ vẫn xanh (EditMode + SchoolFloor PlayTests).
- Đọc trước (tối đa 3): PrototypePartyController.cs; Features/Party/Runtime; Features/Network/Runtime/NetworkTestPlayer.cs + OwnerNetworkTransform.cs
- Việc kế tiếp sau khi xong: T-027 (cùng T-024).
