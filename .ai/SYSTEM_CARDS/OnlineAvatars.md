# CARD: OnlineAvatars (nhân vật online, T-025)

Khu / vai trò: mỗi người chơi online có đúng MỘT nhân vật cố định cả ván (không đổi vai), số nhân vật = số người (2-4). Cập nhật: 2026-10-30 08:01 UTC. Trạng thái: code + test xanh; host chạy đúng trong Play Mode; CHƯA xác minh 2 bản thật (chờ chủ dự án chạy client); CHƯA nối vào scene map (T-027).

## A. Bản đồ
- Features/Network/Runtime/Framework/SlotAllocator.cs (thuần C#): chia ô cố định cho client (ô nhỏ nhất còn trống; rời chỉ trả ô của mình, KHÔNG dồn ô; đội đầy trả -1).
- Core/Runtime/Players/PrototypePartyController.cs: thêm chế độ online, MẶC ĐỊNH TẮT (OnlineLocalSlot = -1). EnterOnlineMode(ô, số người), ExitOnlineMode, BindSlot(ô, Transform|null), IsOnline. Online: SwitchRole sang ô khác trả false. Initialize/SetPartySize/ResetPositions/TickInput chịu được ô null.
- Features/Network/Online (asmdef TheReturn.Network.Online): OnlinePartySlots (số người, điểm xuất phát, SlotAllocator trên host), OnlineAvatar (NetworkBehaviour, NetworkVariable ô, host chia ô lúc spawn, owner tự đặt vị trí xuất phát, máy khác tắt CharacterController của người khác), OnlinePartyLocalDriver (gắn nhân vật vào PrototypePartyController, bật chế độ online cho nhân vật của mình, gọi TickInput; Tab khóa/thả chuột).
- Prefab Features/Network/Prefabs/PF_OnlineAvatar.prefab (CharacterController giống PlayerSlot + NetworkObject + OwnerNetworkTransform + OnlineAvatar + PF_PlayerModel lồng bên trong, không sửa PF_PlayerModel); danh sách NetworkPrefabsList_Party.asset.
- Scene thử Features/Network/Scenes/NetworkPartyTest.unity (bản sao NetworkS1Test của #7). Bản build thử: Builds/NetworkParty/NetworkParty.exe + chay_client.bat (không nằm trong git).
- Test: Features/Network/Tests/EditMode (SlotAllocatorTests), Features/Network/Tests/EditModeOnline (PartyControllerOnlineModeTests, asmdef TheReturn.Network.Online.Tests).

## B. Triệu chứng -> nơi xem
| Triệu chứng | Nơi xem | Cách kiểm |
|---|---|---|
| Không điều khiển được nhân vật online | PrototypePartyController.TickInput chỉ chạy khi chuột đang khóa: bấm Tab; OnlinePartyLocalDriver.HandleReady có gọi EnterOnlineMode chưa (controller.IsOnline) | Play Mode: controller.IsOnline, OnlineLocalSlot |
| Hai người cùng một ô / ô nhảy | SlotAllocator chạy trên HOST trong OnlineAvatar.OnNetworkSpawn; OnlinePartySlots phải có trong scene | SlotAllocatorTests |
| Client thứ 5 (hoặc vượt số người) bị ngắt | Cố ý: OnlineAvatar ngắt client khi AssignSlot trả -1 (cảnh báo trong Console host) | SlotAllocatorTests, log [Online] |
| Nhân vật người khác không di chuyển / giật | OwnerNetworkTransform (owner có quyền ghi); CharacterController của người khác phải tắt | Chạy 2 bản, xem log vị trí |
| Camera biến mất khi mình rời | PrototypePartyController.BindSlot(ô, null) thả camera khỏi nhân vật bị gỡ | Party_UnbindLocalSlot_ReleasesCameraAndTickInputIsSafe |

## C. Ghi chú đã xác minh
- 2026-10-30 08:01 UTC (phiên #9): EditMode 361/361 xanh (gồm SlotAllocator 11 test, chế độ online của bộ điều khiển 14 test); PlayMode SchoolFloor 18/18 (2 Inconclusive cũ vì clip Player.fbx không có keyframe) -> chế độ 1 máy không bị phá. Play Mode NetworkPartyTest, Editor làm host một mình: nhân vật spawn ô 0 tại điểm xuất phát đầu, controller.IsOnline = true, camera gắn vào nhân vật, thân của mình bị ẩn, SwitchRole sang ô khác bị chặn, Console 0 lỗi/cảnh báo.
- CHƯA xác minh: 2 bản (host + client) mỗi người một nhân vật và thấy nhau; 3-4 người; vào lại sau khi mất kết nối (T-028: hiện người vào lại bị coi là người mới và lấy ô nhỏ nhất còn trống, có thể KHÁC ô cũ); chạy trong scene SchoolFloor thật (T-027, cần phép sửa scene map).
- Cách chủ dự án thử 2 bản: mở scene NetworkPartyTest, bấm Play, bấm nút Host LAN (localhost) trong bảng thử; rồi bấm đúp Builds/NetworkParty/chay_client.bat ĐÚNG 1 LẦN mỗi client (chạy nhiều lần để thêm người, tối đa 4). Mỗi cửa sổ: bấm Tab để khóa chuột, WASD đi, chuột nhìn quanh.

## D. Task liên quan
- T-025 (REVIEW). Dựa trên T-024 (SlotAllocator dùng TeamRoster) và T-009 S1 (OwnerNetworkTransform, NetworkSessionService). Kế tiếp: T-027 (nối nhiệm vụ mẫu qua mạng, cần phép sửa scene map), T-026 (phòng online quyết định số người), T-028 (vào lại/đồng bộ), T-010 (prefab nhân vật, đã được lồng chứ không sửa).
