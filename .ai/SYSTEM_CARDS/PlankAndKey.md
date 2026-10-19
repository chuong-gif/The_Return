# CARD: PlankAndKey (Cầu ván và thẻ từ)

Khu / vai trò: Hanh_Lang_6 (nhiệm vụ MỚI, T-006b). Cập nhật: 2026-10-16 11:00 UTC. Trạng thái: Domain + PlankSession (thời gian thực) + PlankSettings + test; chưa Presentation/prefab/scene/adapter.

## A. Bản đồ
- Assets/TheReturn/Features/PlankAndKey/Runtime/Domain (asmdef TheReturn.PlankAndKey.Domain, noEngineReferences): PlankSolver (Dijkstra tìm thời gian tối thiểu + kế hoạch), PlankRound (đề bất biến: thời gian qua cầu từng người, tối ưu, hạn), PlankGenerator (danh mục bộ tốc độ theo số người, xáo theo seed, slack % là tham số), PlankState (TryCross, thẻ, hạn, thắng/thất bại, Reset), PlankSession (bọc PlankState + thời gian thực: TryStartCrossing, Tick, đồng hồ HUD, Solved chỉ sau khi nhóm cuối đến nơi).
- Runtime (asmdef TheReturn.PlankAndKey: Domain + Core + Shared + Unity.InputSystem): Data/PlankSettings.cs (giây mỗi đơn vị mặc định 3, slack 25%, câu chữ tiếng Việt); asset Features/PlankAndKey/Data/PlankAndKey_VI.asset. Chưa có Presentation.
- Test: Features/PlankAndKey/Tests/EditMode/{PlankAndKeyTests.cs (25 test), PlankSessionTests.cs (13 test)} (asmdef TheReturn.PlankAndKey.Domain.Tests).
- Thiết kế: PLANS/T-006-mission-designs.md mục 3 (Hanh_Lang_6); kế hoạch: PLANS/T-006b-plank-and-key.md (mục 3 quyết định, mục 8 hạn chế).

## B. Triệu chứng -> nơi xem
| Triệu chứng | Nơi xem | Cách kiểm |
|---|---|---|
| Hạn quá chặt hoặc không thể thắng | PlankRound (DeadlineUnits = OptimalUnits + ceil(OptimalUnits x SlackPercent / 100)); PlankSolver.Solve | Test Round_DeadlineIsOptimalPlusCeilingOfSlack, Generator_DeadlineNeverBelowOptimalAndAlwaysWinnable |
| Qua cầu mà không có thẻ, hoặc thẻ kẹt ở bờ kia | PlankState.TryCross (CardNotWithGroup; thẻ đi cùng nhóm) | Test State_RejectsCrossingWithoutCard, State_ReturnTripBringsCardBack |
| Người ở hai bờ qua cùng nhau | PlankState.TryCross (WrongBank) | Test State_RejectsGroupAcrossBanks |
| Thua dù còn giờ, hoặc thắng sai lúc | PlankState.TryCross (UsedUnits + cost > DeadlineUnits; AllFar) | Test State_WinsOnLastCrossingExactlyAtDeadline, State_OverDeadlineFailsAndMoveIsNotApplied |
| Hai máy ra hai đề khác nhau | PlankGenerator.Generate dùng System.Random(seed) (chưa đảm bảo giống nhau giữa máy/runtime) | Test Generator_IsDeterministicPerSeed chỉ kiểm trong cùng một máy; đổi sang bộ sinh số tự viết khi cần (PLANS/T-006b mục 8) |
| Qua cầu mất sai thời gian | PlankSession.TryStartCrossing (chi phí đơn vị x SecondsPerUnit) | Test Session_CrossingDurationFollowsSlowestPlayer |
| Thắng khi nhóm cuối chưa đến nơi | PlankSession.Solved (đợi Tick đưa Phase về Idle) | Test Session_SolvedOnlyAfterFinalArrival |
| Đồng hồ HUD nhảy hoặc âm | PlankSession.DisplayUsedSeconds | Test Session_DisplayClockProgressesDuringCrossing |
| Bấm lượt khi đang qua cầu | PlankSession.TryStartCrossing (Busy) | Test Session_RejectsNewCrossingWhileBusy |

## C. Ghi chú đã xác minh
- 2026-10-17 02:20 UTC: 38/38 test xanh (mức 3) gồm 13 test PlankSession; asset PlankAndKey_VI.asset tạo được. Vẫn chưa chạy trong scene (mức 4 chưa có).
- 2026-10-16 11:00 UTC: 25/25 test xanh (mức 3); bộ giải khớp số liệu duyệt 8/17/17/11 và bộ kiểm vét cạn độc lập. Chưa chạy trong scene (mức 4 chưa có).
- Đơn vị thời gian là «đơn vị», chưa phải giây; đồng hồ chỉ tính thời gian các lượt qua cầu (chưa xác nhận với chủ dự án).

## D. Task liên quan
- T-006b DONE (Domain). T-006b2 IN-PROGRESS: S2-S3 xong (PlankSession, PlankSettings, asset); S4-S6 (Presentation, adapter, đăng ký, dựng scene, Play Mode) chờ Scene lock của phiên #3.
