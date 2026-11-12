# CARD: NetworkFramework — luật host-authoritative

- Vị trí: `Assets/TheReturn/Features/Network/Runtime/Framework`; Domain thuần C#, không phụ thuộc Unity/NGO.
- `HostAuthority` nhận slot + sequence + command; kiểm luật và phát `Version`/snapshot. `ClientReplica` chỉ nhận snapshot mới của đúng slot.
- `IHostRules.Validate` không đổi trạng thái; host tự xác định người gửi từ kết nối, không tin slot/vị trí trong command.
- Snapshot public phải bất biến trong cùng Version. Private state chỉ gửi đúng slot.
- Vào lại: nối lại slot, reset sequence và gửi state đầy đủ; không tự làm migration host.
- Bốn adapters đã nối MapSchoolF trong T-042. Tình trạng room→map/smoke hiện hành ở `QUEUE.md`.
- Test dùng chung: `TheReturn.Network.Framework.Tests`, contract, online, room và `TheReturn.SchoolFloor.Online.Tests`; xem `MISSIONS_NET.md`/`TESTING.md`.
- Lỗi framework mới tái hiện được: ghi `FAILURES.md`, thêm FIX vào `QUEUE.md`; không lưu log lịch sử đã đóng ở đây.
