# T-042 S0 Khảo sát (CHỈ ĐỌC) — 2026-11-11 22:27 UTC, phiên #13
Không sửa Assets/scene khi khảo sát. Nguồn: scene MapSchoolF đang mở, mã nguồn, LOG.

## 0. KẾT LUẬN
1. Muốn luồng map đồng bộ một nhiệm vụ qua mạng, mỗi nhiệm vụ cần một thành phần IOnlineMission (namespace TheReturn.SchoolFloorNet, trong MapFlowOnline.cs) đăng ký vào MapFlowOnline.onlineMissions: host tạo IMissionAdapter (Begin/Stop/Active/Solved/CanHandOff); mọi máy gọi SetClientActive(khu đang làm). CHỈ PlankAndKey có (PlankOnlineMission, T-028).
2. Breaker, Choir, Attendance chỉ có online dành cho scene thử: host tự dựng ván ngay khi là host (không có Begin/End/Solved/CanHandOff), client luôn chạy. Phải bổ sung vòng đời (autoStart + BeginMission/EndMission + MissionActive) giống mẫu Plank.
3. Thiết kế đề xuất: thêm vòng đời GENERIC vào MissionOnlineHostBase/ClientBase (Breaker, Choir đang dùng lớp cơ sở) + một thành phần chung OnlineMissionLink (IOnlineMission) dùng cho cả 3 nhiệm vụ; Attendance (host/client kiểu cũ, chưa dùng lớp cơ sở) chỉ THÊM đúng các thành viên vòng đời vào lớp cũ, KHÔNG chuyển sang lớp cơ sở (chưa được phép).
4. Lớp mạng có sẵn trong scene nhưng đứt tham chiếu (S1 sửa): OnlinePartyLocalDriver.controller=NULL; MapFlowOnline.flow=NULL; PlankOnlineHost.prototype=NULL; PlankOnlineClient.prototype=NULL và party=NULL.
5. Luồng cổng offline của MapSchoolF: MissionFlowController.bindings có 30 mục, chỉ 11 mục có tham chiếu (Phong_1 x3, Phong_5 x3, Phong_6 x3, Hanh_Lang_6 x2 qua 'Hanh lang 1_Vao/Ra'); 19 mục rỗng (thừa từ scene cũ). 11 SchoolPreparationGate nằm ở tọa độ layout CŨ (x khoảng 33-70), chưa gắn vào map Blender (SchoolFloor_Blockout ở offset 200,0,0 có Gate_Choir/PlankAndKey/Breaker/Attendance riêng). MissionFlowController.topology=NULL (topology mặc định cũ, chưa dùng Core4TopologyProvider/YardEntry). Theo LOG của Copilot: map Blender 'chưa nối vào luồng gameplay/MissionFlow'.
6. Vào scene: LobbyReadyController.schoolSceneName mặc định 'SchoolFloorPrototype' (Maps/Lobby/Runtime/LobbyReadyController.cs dòng 26; giá trị trong scene Sanh_Cho có thể khác). MapSchoolF KHÔNG có trong Build Settings. Sửa Build Settings cần chủ dự án cho phép.

## 1. Bảng 'cái gì phải nối' mỗi nhiệm vụ
| Nhiệm vụ | Khu | Lớp online có sẵn | Có trong MapSchoolF | Thiếu |
|---|---|---|---|---|
| PlankAndKey | Hanh_Lang_6 | PlankOnlineHost/Client/Mission + PlankNetBridge | CÓ (đứt prototype/party) | S1: nối lại tham chiếu; kiểm Play |
| BreakerPanel | Phong_5 | BreakerOnlineHost/Client (lớp cơ sở) | KHÔNG | NetworkObject + MissionNetBridge + host/client + nối switches/lamps/settings/party + OnlineMissionLink + vòng đời |
| Choir | Phong_6 | ChoirOnlineHost/Client (lớp cơ sở) | KHÔNG | như Breaker (4 pad) |
| Attendance | Phong_1 | AttendanceOnlineHost/Client (kiểu cũ) | KHÔNG | như Breaker + thêm vòng đời vào lớp cũ + HUD/24 ghế |

## 2. Các bước đã điều chỉnh (thay mục 2 của plan)
S0 khảo sát (xong) | S1 snapshot + sửa tham chiếu đứt + Plank host một mình | S2 vòng đời generic (host/client base + OnlineMissionLink + test) | S3 Breaker | S4 Choir | S5 Attendance | S6 MapFlowOnline đủ 4 nhiệm vụ + spawn khớp | S7 vào scene (LobbyReadyController, Build Settings) | S8 kiểm tổng + chuẩn bị smoke.

## 3. Câu hỏi cho chủ dự án (PENDING P-012, P-013)
- P-012: luồng cổng/khu chuẩn bị trên map Blender chưa nối. T-042 chỉ nối MẠNG; việc gắn SchoolPreparationGate/bindings vào map Blender là việc luồng offline (T-040 P3b còn dở). Mặc định: S1-S5 làm trên các đối tượng hiện có (không phụ thuộc tọa độ); S6 cần luồng chạy được nên dùng layout hiện có; ai gắn cổng vào map Blender: chủ dự án/Copilot hay #13 sau T-042?
- P-013: cho phép đổi LobbyReadyController.schoolSceneName/scene Sanh_Cho sang MapSchoolF và thêm MapSchoolF vào Build Settings (ở S7)?

## 4. Rủi ro / chưa biết
- MapSchoolF là scene làm việc của chủ dự án và Copilot: mỗi bước phải snapshot trước và chỉ thêm/nối tham chiếu.
- Chưa kiểm luồng offline của MapSchoolF có chạy được khi Play không (ví dụ Play có lỗi nào do tham chiếu rỗng).
- Chưa kiểm AccessController.gates có phần tử null (chỉ biết 30 mục).
- Chưa kiểm HUD của 3 nhiệm vụ khi nhiều nhiệm vụ cùng online trong một scene (OnGUI/phím có thể xung đột nếu client không tắt khi không hoạt động; vòng đời MissionActive xử lý).
