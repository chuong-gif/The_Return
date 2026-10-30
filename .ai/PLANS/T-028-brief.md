# PHIẾU T-028: Luồng map SchoolFloor qua mạng
- Mục tiêu (1 câu): bắt đầu/thoát nhiệm vụ, cổng, khu chuẩn bị, MapProgress, màn thắng chạy trên host; người vào muộn nhận đủ trạng thái.
- Được đụng: Maps/SchoolFloor/Runtime/Missions/MissionFlowController.cs, SchoolFloorAccessController.cs (thêm lớp mạng, giữ chế độ 1 máy), Features/Network/*.
- KHÔNG được đụng: Domain MapProgress nếu test cũ đỏ; scene map khi chưa có phép + snapshot.
- Phép chủ dự án: scene map cần mục OWNER_UPDATES + snapshot trước khi sửa.
- Cách kiểm xong: 2 bản đi hết 1 vòng khu chuẩn bị -> nhiệm vụ -> cổng mở; SchoolFloor PlayTests (T-005) xanh như trước.
- Đọc trước (tối đa 3): Maps/SchoolFloor/Runtime/Domain/MapProgress.cs; MissionFlowController.cs; PLANS/T-002-map-progress.md
- Chờ: T-027. Việc kế tiếp sau khi xong: T-030, T-031.
