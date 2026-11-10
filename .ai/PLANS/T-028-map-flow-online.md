# T-028: Luồng map qua mạng

Cập nhật: 2026-11-03 08:18 UTC (phiên #9). Code + test XONG; nối vào scene thật và chạy 2 máy CHƯA làm (chờ quyết định ở mục 4).

## 1. Thiết kế (đã cài)
- HOST chạy luồng map như cũ (MissionFlowController: tập hợp ngoài cửa -> cổng mở -> cả đội vào -> Registry.TryBegin -> adapter.Begin -> Registry.Tick tới khi xong). MÁY KHÁCH không chạy luật, chỉ khôi phục trạng thái host gửi.
- Vai: MissionFlowController.NetRole = Offline (MẶC ĐỊNH, luồng một máy cũ KHÔNG đổi) / Host / Client. Online không gọi FreeRoam (di chuyển do OnlinePartyLocalDriver). EnterOnline(vai, adapter) dựng registry rỗng rồi chỉ đăng ký nhiệm vụ ĐÃ có bản online => khu chưa có bản online cửa đóng hẳn; ExitOnline về registry mặc định.
- MapFlowState (Domain, thuần C#): khu đang làm | khu đã xong | chỉ số cổng đang mở (chỉ số trong bảng bindings, giống nhau mọi máy vì cùng scene). Mã hóa chuỗi, giải mã không tin dữ liệu mạng. Máy khách: MapProgress.RestoreSnapshot => cửa, bản đồ nhỏ, màn thắng dùng lại code cũ.
- RemoteMissionAdapter (Domain): adapter giả trên máy khách (khu có nhiệm vụ nhưng không Active/Solved).
- Features: Maps/SchoolFloor/Online (asmdef TheReturn.SchoolFloor.Online): IOnlineMission (AreaId, CreateHostAdapter, SetClientActive) + MapFlowOnline (dùng MissionNetBridge của T-027; host đẩy trạng thái mỗi khi đổi, client xin cho tới khi nhận gói đầu, mất mạng thì ExitOnline).
- Nhiệm vụ nào có bản online: thêm một component cài IOnlineMission cạnh Host/Client của nó và kéo vào MapFlowOnline.onlineMissions. Đã làm cho Cầu ván: PlankOnlineMission (Hanh_Lang_6; PlankOnlineHost autoStart=false, PlankOnlineClient startActive=false ở map thật). T-031 của các phiên khác làm tương tự (Attendance: #7).
- SchoolPreparationGate.CountOutside/CountInside chịu được ô chưa có người (null).

## 2. Đã làm thêm (2026-11-07 06:26 UTC) và đã kiểm
- Scene thật SchoolFloorPrototype có thêm gốc OnlineLayer (bản sao lưu Assets/TheReturn/Maps/SchoolFloor/Scenes/SchoolFloorBeforeOnlineLayer_20261104_1914.unity): PartySlots (OnlinePartySlots đội 2 người + 4 điểm xuất phát lấy từ 4 PlayerSlot cũ, OnlinePartyLocalDriver), FlowNetBridge + PlankNetBridge (NetworkObject + MissionNetBridge), PlankOnline (Host autoStart=false, Client startActive=false, PlankOnlineMission cho Hanh_Lang_6), MapFlowOnline. Offline: tất cả nằm im.
- KHÔNG đặt NetworkManager vào scene: OnlineMapBootstrap (Features/Network/Online) tạo NetworkRoot lúc chạy từ prefab Resources/OnlineBootstrap/PF_NetworkRoot CHỈ khi có tham số -netauto. OnlinePartyLocalDriver tắt 4 PlayerSlot một máy khi có nhân vật online. OnlinePartySlots đọc -teamsize N.
- Bản build thử: Builds/NetworkMap/NetworkMap.exe (không nằm trong git) + chay_host.bat (host, đội 2 người) + chay_client.bat. CHỈ nhiệm vụ Cầu ván (khu Hanh_Lang_6, các cửa tên «Hanh lang 1_Vao» ở (14.5,10.9,139.2) và «Hanh lang 1_Ra» ở (-37.5,10.9,139.2), cách điểm xuất phát khoảng 115 m) có bản online: các khu khác cửa đóng hẳn.
- EditMode toàn dự án 517/517 (trước khi nối scene). PlayMode SchoolFloor (chế độ một máy) sau khi nối scene: 28/32; các test luồng logic qua hết (kể cả EveryGate_OfEveryMissionArea_StartsItsArea, từng fail trước T-020). 4 test thất bại là SchoolFloorWalkPlayTests.EveryGate_WalkingTeam_StartsItsArea(2/3/4) và EveryGate_TeamWalksDeeperIntoArea_StartsItsArea(4), chạy lại riêng vẫn ra đúng 4 lỗi, TẤT CẢ ở một cổng «Phong 2_Cua 1» (khu Phong_2): cổng bị armed lại là «Hanh lang 2_Ra», outside=0 inside=0, cửa vẫn đóng. Các cổng khác đều qua. Chưa chứng minh nguyên nhân (nghi hình học vùng của hai cổng này; code luồng một máy tôi sửa không đổi hành vi: Update vai Offline y nguyên, SchoolPreparationGate chỉ thêm kiểm null). Chưa làm A/B bằng cách khôi phục scene Before...: tốn khoảng 20 phút Play Mode.

## 3. Chưa làm
- Xử lý MẤT KẾT NỐI giữa ván (host rời hoặc ai đó rời => cả đội về MainMenu kèm thông báo). Hiện MapFlowOnline chỉ trả luồng về một máy (ExitOnline) khi mất mạng.
- Chạy 2 máy thật cho luồng map đầy đủ (chủ dự án thử bản NetworkMap), 3-4 người, người vào lại giữ ô cũ (T-030).
- Đường «phòng -> map» (tách task riêng); thông số nhiệm vụ khác (T-031).
- Chẩn đoán 4 test đi bộ thất bại ở «Phong 2_Cua 1».

## 4. Quyết định đã chốt (chủ dự án «làm theo lời khuyên»)
1. Lớp online đặt TRONG SchoolFloorPrototype (đã làm).
2. Đường «phòng -> map» tách thành task riêng; T-028 dùng khởi động bằng tham số -netauto.
