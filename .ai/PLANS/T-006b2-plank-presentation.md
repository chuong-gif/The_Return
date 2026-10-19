# KẾ HOẠCH T-006b2: PlankAndKey (Hanh_Lang_6) - Presentation + scene   (file: .ai/PLANS/T-006b2-plank-presentation.md)
Trạng thái: ĐANG LÀM (S1-S3 XONG; S4-S6 chờ Scene lock của phiên #3) | AI giữ: Claude Sonnet 5.5 (phiên MCP #2) | Cập nhật: 2026-10-17 02:20 UTC | Duyệt: chủ dự án xác nhận 2026-10-16 (DECISIONS/2026-10-16-t006b2-plank-owner-confirm.md)
## 1. Mục tiêu và nghiệm thu
- Làm nhiệm vụ Hanh_Lang_6 chơi được trong game: ván + thẻ từ + đồng hồ + HUD, nối vào luồng nhiệm vụ (adapter + MissionRegistryFactory), dựng cổng + khu chuẩn bị ở Hanh_Lang_6. Nghiệm thu: compile + test (mức 3) và chạy thật trong Play Mode (mức 4) trước khi báo xong.
## 2. Phạm vi
- Không cần scene (làm ngay): PLANS, DECISIONS, OWNER_UPDATES; Features/PlankAndKey/Runtime/Domain/PlankSession.cs (MỚI) + test; Features/PlankAndKey/Runtime/{TheReturn.PlankAndKey.asmdef, Data/PlankSettings.cs} (MỚI) + asset PlankAndKey_VI.asset (MỚI).
- Cần Scene lock + snapshot (chờ khóa của phiên #3 trả): Features/PlankAndKey/Runtime/Presentation/* (MỚI), Maps/SchoolFloor/Runtime/Missions/PlankMissionAdapter.cs (MỚI), SỬA MissionRegistryFactory (thêm 1 đăng ký Hanh_Lang_6) và asmdef TheReturn.SchoolFloor (thêm reference), đối tượng MỚI trong scene tại Hanh_Lang_6 (cổng, khu chuẩn bị, ván, thẻ, HUD), prefab PF_PlankLift/PF_KeyCard.
- KHÔNG đụng: bố cục/nội thất/cổng có sẵn của chủ dự án; các khu khác; file của T-006a/c/d/d2/e (phiên khác). Mọi file dùng chung (MissionRegistryFactory, asmdef SchoolFloor, SchoolFloorFlow) sửa kiểu đọc-lại-rồi-ghi ngay, sửa tối thiểu, kiểm tra không ai vừa sửa.
## 3. Quyết định thiết kế
- PlankSession (thuần C#, trong Domain để test được): bọc PlankState + thời gian thực. Gọi PlankState.TryCross khi BẮT ĐẦU lượt (kiểm luật, trừ hạn), rồi chạy bộ đếm = chi phí x giây-mỗi-đơn-vị; trong lúc qua cầu không nhận lượt khác (Busy); khi hết bộ đếm thì «đến nơi». Solved chỉ đúng SAU khi lượt cuối đã đến nơi (nhóm không bị cắt giữa chừng); CanHandOff = Solved.
- Đồng hồ HUD chạy liên tục theo tiến độ lượt qua cầu (đã dùng = đơn vị đã trừ x giây - phần còn lại của lượt hiện tại). Đồng hồ KHÔNG chạy khi không có lượt qua cầu (quyết định 3 của chủ dự án).
- Thiết lập chỉnh được (PlankSettings): giây mỗi đơn vị (mặc định 3), phần dư % (mặc định 25) và các câu chữ tiếng Việt (theo mẫu QuietCorridorSettings).
- Presentation (S4): tương tác bằng phím E qua PrototypePartyController như các hành lang khác; chi tiết viết sau khi đọc scene thực tế và PrototypePartyController (chưa khảo sát đủ nên chưa chốt ở đây).
## 4. Subtask
| ID | Việc | File | Cách kiểm | Trạng thái | Bằng chứng |
|---|---|---|---|---|---|
| S1 | Ghi quyết định chủ dự án + kế hoạch | DECISIONS, OWNER_UPDATES, PLANS | Đọc lại | DONE | Đã ghi 2026-10-16 11:08 UTC |
| S2 | PlankSession (Domain) + test thời gian thực | Runtime/Domain, Tests/EditMode | run_tests xanh | DONE | run_tests EditMode: 38/38 passed (25 test T-006b + 13 test PlankSession), mức 3 |
| S3 | asmdef Runtime + PlankSettings + asset PlankAndKey_VI | Runtime, Runtime/Data | Compile + Console 0 lỗi | DONE | Compile sạch (mức 2); asset Features/PlankAndKey/Data/PlankAndKey_VI.asset tạo và tải lại đúng giá trị (3 giây/đơn vị, 25%) |
| S4 | Presentation + adapter + đăng ký (CẦN Scene lock) | Runtime/Presentation, Maps/SchoolFloor/Runtime/Missions | Compile + test | TODO | |
| S5 | Dựng scene Hanh_Lang_6: cổng, khu chuẩn bị, ván, thẻ, HUD (CẦN Scene lock + snapshot) | scene, prefab | Đọc scene + screenshot được phép | TODO | |
| S6 | Kiểm Play Mode (mức 4): vòng thắng, hết giờ thử lại, đổi vai, đội 2-4 người (chủ dự án đang chơi thử: xin phép trước) | - | Chơi thật | TODO | |
| S7 | README feature, thẻ hệ thống, TASKS, LOG | .ai/**, Features/PlankAndKey | Đọc lại | TODO | |
## 5. Bản đồ lỗi
| Triệu chứng | Nơi xem | Cách kiểm |
|---|---|---|
| Qua cầu mất sai thời gian | PlankSession.TryStartCrossing (chi phí x giây-mỗi-đơn-vị) | Test Session_CrossingDurationFollowsSlowestPlayer |
| Thắng khi nhóm cuối chưa đến nơi | PlankSession.Solved (đợi đến nơi) | Test Session_SolvedOnlyAfterFinalArrival |
| Đồng hồ HUD nhảy/âm | PlankSession.DisplayUsedSeconds | Test Session_DisplayClockProgressesDuringCrossing |
| Bấm lượt khi đang qua cầu | PlankSession.TryStartCrossing (Busy) | Test Session_RejectsNewCrossingWhileBusy |
## 6. Rủi ro / câu hỏi
- Scene lock đang do phiên #3 giữ (T-006d2, từ 10:29 UTC); phiên #4 cũng chờ scene (T-006c/T-006e). Không sửa/lưu scene khi chưa có khóa. Chủ dự án đang tự chơi thử Play Mode: không điều khiển Play Mode, không lưu scene khi chưa hỏi.
- Giả định giây/đơn vị = 3 và đồng hồ chỉ tính lượt qua cầu: cần playtest.
- Sửa file dùng chung cùng lúc với phiên khác: đọc lại ngay trước khi ghi, sửa tối thiểu.
## 7. Tiến độ
- 2026-10-16 11:08 UTC: S1 xong; nhận task; bắt đầu S2-S3.
- 2026-10-17 02:20 UTC: S2-S3 xong. Tạo PlankSession.cs (Domain), PlankSessionTests.cs, TheReturn.PlankAndKey.asmdef (Runtime: Domain + Core + Shared + Unity.InputSystem), Data/PlankSettings.cs, Data/PlankAndKey_VI.asset. 38/38 test xanh, Console 0 lỗi/cảnh báo. Scene không đụng (24 root, không dirty).
- 2026-10-17 02:20 UTC: S4-S6 CHƯA làm: Scene lock vẫn do phiên #3 giữ từ 2026-10-16 10:29 UTC (T-006d2, chưa có tiến độ ghi sau START); chưa quá 24 giờ nên chưa coi là bỏ dở. Lần gọi ghi file đầu tiên của S2-S3 bị treo 4 phút (nghi hộp thoại phê duyệt của Claude Desktop chưa bấm); không file nào được ghi, đã chạy lại thành công.
