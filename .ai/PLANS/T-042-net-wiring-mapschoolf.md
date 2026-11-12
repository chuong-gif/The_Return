# T-042 Nối logic mạng của 4 nhiệm vụ vào scene MapSchoolF (KẾ HOẠCH) — 2026-11-11 22:25 UTC, phiên #13
Yêu cầu chủ dự án (2026-11-11): đã tự tạo scene mới MapSchoolF, đưa logic 4 nhiệm vụ qua và dựng map; AI NỐI logic xử lý mạng của 4 nhiệm vụ vào map, chia từng bước làm lần lượt. Thực hiện T-040 P4 (+ chuẩn bị P5).
Scene: Assets/TheReturn/Scenes/Maps/MapSchoolF.unity. Scene làm việc của chủ dự án + phiên Copilot (Blender blockout): MỖI bước sửa scene phải có SNAPSHOT trước (theo mẫu Before*), chỉ THÊM/NỐI tham chiếu, không xóa/dời object của người khác.

## 1. Hiện trạng đo được (chỉ đọc, 2026-11-11 22:25 UTC)
- Có: 4 logic nhiệm vụ (Puzzle_Attendance 24 ghế + AttendanceHUD; Breaker_LogicOnly Variant 6 công tắc/đèn; Choir_LogicOnly Variant 4 pad; PlankAndKey_LogicOnly Variant), luồng MissionFlow_Controller + SchoolFloor_Flow + SchoolFloorAccessController + 11 SchoolPreparationGate + Canvas/WinScreen, LocalParty (4 người, offline), SchoolLayoutV2 (layout cũ), SchoolFloor_Blockout + Lights_BlenderLayout (map mới từ Blender, offset 200,0,0), Cac Phong (inactive).
- Lớp mạng CÓ SẴN nhưng đứt tham chiếu: OnlineLayer/{PartySlots (OnlinePartySlots teamSize=2, 4 PartySpawn; OnlinePartyLocalDriver.controller=NULL), FlowNetBridge (NetworkObject+MissionNetBridge), MapFlowOnline (flow=NULL, onlineMissions=[1]), PlankNetBridge, PlankOnline (PlankOnlineHost/Client prototype=NULL, party=NULL; PlankOnlineMission areaId Hanh_Lang_6)}.
- THIẾU hoàn toàn trong scene: online của Điểm danh, Cầu dao, Choir (host/client/bridge).
- MissionFlowController.topology=NULL (dùng topology mặc định cũ, chưa dùng Core4TopologyProvider); AccessController.gates và MissionFlowController.bindings có 30 mục (nhiều khả năng còn null/thừa từ scene cũ): thuộc luồng offline, cần kiểm ở S0.
- Chưa rõ vị trí 4 nhiệm vụ so với map Blender mới (SchoolFloor_Blockout ở 200,0,0): kiểm ở S0 (không ảnh hưởng logic mạng nhưng ảnh hưởng PlayerSlot/spawn).

## 2. Các bước (làm lần lượt; mỗi bước có XONG KHI)
| Bước | Việc | XONG KHI | Thẻ |
|---|---|---|---|
| S0 | KHẢO SÁT chỉ đọc (xong 2026-11-11 22:27 UTC): PLANS/T-042-survey.md | Có bảng 'cái gì phải nối' + câu hỏi P-012, P-013 | Q-016 DONE |
| S1 | SNAPSHOT scene; sửa tham chiếu đứt của lớp mạng sẵn có (PartySlots driver, MapFlowOnline.flow, Plank host/client/party); PlankAndKey chạy host một mình trong MapSchoolF | Plank Play host một mình khớp mốc cũ; Console 0 lỗi; scene chỉ đổi phần online | Q-017 |
| S2 | Vòng đời GENERIC: interface + BeginMission/EndMission/MissionActive/Solved/CanHandOff trong MissionOnlineHostBase, MissionActive/startActive trong MissionOnlineClientBase, thành phần chung OnlineMissionLink (IOnlineMission) ở SchoolFloor.Online, thêm test; hành vi scene thử không đổi (autoStart/startActive mặc định true) | Compile sạch; Gauntlet xanh; test mới đạt | Q-018 |
| S3 | BreakerPanel online vào MapSchoolF (NetworkObject + MissionNetBridge + BreakerOnlineHost/Client + OnlineMissionLink, nối switches/lamps/settings/party/bridge) | Play host một mình: bật công tắc đúng ô, từ chối ô sai/xa, Retry; Console 0 lỗi | Q-019 |
| S4 | Choir online vào MapSchoolF | Play host một mình: pad/đồng hồ theo số người, xong/thất bại đồng bộ | Q-020 |
| S5 | Attendance online vào MapSchoolF (thêm vòng đời vào lớp cũ; HUD, 24 ghế, manh mối riêng) | Play host một mình: ngồi/đứng/xác nhận, manh mối riêng đúng ô | Q-021 |
| S6 | MapFlowOnline.onlineMissions đủ 4 nhiệm vụ; PartySlots spawn khớp; (tùy chủ dự án) Core4TopologyProvider + YardEntry | Play host một mình: qua cổng -> nhiệm vụ -> thắng 4 nhiệm vụ trên layout hiện có; người vào muộn nhận đủ trạng thái (mô phỏng) | Q-022 |
| S7 | Vào scene: LobbyReadyController.schoolSceneName + Build Settings (CẦN P-013) | Từ phòng chờ vào được MapSchoolF bằng host + client | Q-023 |
| S8 | Kiểm tổng: Gauntlet xanh + Play host một mình 4 nhiệm vụ; hướng dẫn smoke 2 máy/Relay cho chủ dự án | Gauntlet 0 đỏ; chủ dự án thử 2 máy | Q-024 |

## 3. Nguyên tắc
- Mỗi bước sửa scene: snapshot BeforeNet_<bước>_<giờ>.unity cùng thư mục; chỉ thêm đối tượng dưới OnlineLayer hoặc nối field SerializedObject; không xóa/dời object của người khác; mở scene không lưu thừa (SchoolFloorPrototype từng tự dirty).
- Dùng sẵn các lớp cơ sở Online (MissionOnlineHostBase/ClientBase, T-039 M5) khi nhiệm vụ đã dùng; Attendance/PlankAndKey đang dùng bản cũ: KHÔNG chuyển sang lớp cơ sở nếu chưa được chủ dự án cho phép riêng.
- Không chạy builder/migration cũ. Không Thread.Sleep trong execute_code. Mỗi bước: compile sạch + Gauntlet xanh + Console 0 lỗi trước khi sang bước sau.
- Chỉ kiểm bằng host một mình trong Editor và test tự động. KHÔNG chứng minh 2 máy thật/Internet: cần smoke của chủ dự án (S7).
- Quan hệ T-040: T-042 thực hiện P4 (và chuẩn bị P5). SchoolFloorCore4 (blockout do #13 dựng) bị MapSchoolF thay thế; xóa khi chủ dự án xác nhận (không xóa trong T-042).
