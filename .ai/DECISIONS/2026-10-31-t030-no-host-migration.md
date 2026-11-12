# Quyết định: không chuyển host

Chủ dự án chốt 2026-10-31: host thoát/mất kết nối thì cả nhóm rời phòng và quay về menu; không migrate host.

Không làm host election, state handoff, rejoin/restore slot trong nhiệm vụ. Disconnect flow phải hiển thị thông báo phù hợp. Hành vi hiện hành/kiểm chứng xem code `RoomService` và `.ai/NETWORK.md`.
