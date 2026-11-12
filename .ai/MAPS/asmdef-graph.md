# Assembly boundaries — Core4/online

Điều hướng để giữ ranh giới; kiểm file asmdef hiện hành trước khi sửa.

- Bốn Domain (Attendance, BreakerPanel, Choir, PlankAndKey): C# thuần, không tham chiếu Unity.
- Mỗi mission Network/Online ở trên Domain tương ứng; Online/Unity layer dùng Network Framework và NGO.
- `Features/Network/Runtime/Framework`: C# thuần, không tham chiếu NGO/Unity.
- `Maps/SchoolFloor`: Domain map + runtime adapters/flow; Online map nối `MapFlowOnline` với các mission online.
- `Core`/`Shared` không tham chiếu ngược Feature; Party/Settings là hạ tầng chung.

Không liệt kê assembly ngoài phạm vi hiện hành. Xem `.ai/NETWORK.md` và `.ai/MISSIONS_NET.md` cho khung online/test.
