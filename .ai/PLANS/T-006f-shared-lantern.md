# KẾ HOẠCH T-006f: Hanh_Lang_4 «Đèn lồng chung» (SharedLantern)   (file: .ai/PLANS/T-006f-shared-lantern.md)

Trạng thái: S1, S1b, S2 XONG (Domain + phạt + Presentation + adapter, KHÔNG scene); S3 dựng cảnh CHỜ Scene lock | AI giữ: Claude Sonnet 5.5 (phiên #5) | Cập nhật: 2026-10-17 02:55 UTC

## 1. Mục tiêu và nghiệm thu
- Thiết kế đã duyệt (PLANS/T-006-mission-designs.md, Hanh_Lang_4): hành lang tối; ĐÚNG 1 đèn lồng, chỉ soi sáng bán kính nhỏ quanh người cầm. 4 biển ký hiệu trên tường chỉ đọc được khi nằm trong vùng sáng. Mỗi người có sổ riêng chỉ ghi biển CHÍNH MÌNH đã đọc. Trao đèn khi đứng gần (≤ 2 m). Ai ở ngoài vùng sáng quá 5 giây thì về điểm đầu hành lang. Cuối hành lang là bảng nhập 4 ký hiệu theo thứ tự biển 1→4; đúng và cả đội tới cổng thì hoàn thành. Số biển cố định 4; ít người hơn thì mỗi người đọc nhiều biển hơn.
- Phạm vi S1 (task này): luật Domain thuần C# + test NUnit. Nghiệm thu: test xanh qua Test Runner, Console 0 lỗi (mức 3).

## 2. Phạm vi
- Được đụng: file MỚI trong Features/SharedLantern/{Runtime/Domain, Tests/EditMode} (2 asmdef, 3 file code, 1 file test), tài liệu .ai.
- KHÔNG đụng: scene, prefab, MissionRegistry/factory, code nhiệm vụ cũ. Chưa có cổng/khu chuẩn bị cho Hanh_Lang_4 trong scene (cần chủ dự án cho phép sửa scene + Scene lock).

## 3. Ngữ cảnh đã đọc
| File | Kết luận |
|---|---|
| PLANS/T-006-mission-designs.md (mục Hanh_Lang_4) | Luật trên; điểm gần giống DontLookBack: có ký hiệu bí mật nhưng ở đây ký hiệu nằm trên tường và cần ánh sáng |
| Features/BreakerPanel/Runtime/Domain, Tests/EditMode | Mẫu: Round + Generator + State, asmdef noEngineReferences, namespace TheReturn, test ở TheReturn.Tests, comment đầu file + summary tiếng Việt |

## 4. Quyết định thiết kế (chủ dự án trả lời 2026-10-17: GIỮ sổ riêng khi về đầu; nhập sai CÓ phạt; AI tự tạo cổng/khu chuẩn bị Hanh_Lang_4)
- 6 ký hiệu, mỗi đề chọn 4 ký hiệu KHÁC NHAU gán cho biển 1-4 (bằng bộ số ngẫu nhiên tự cài, xác định theo seed, để đồng bộ mạng sau này).
- Thời gian, ai trong vùng sáng, khoảng cách trao đèn và biển nào trong vùng sáng do Presentation đo rồi truyền vào Domain (Tick/TryPass/Read) để test và đồng bộ mạng được.
- Người cầm đèn luôn coi là trong vùng sáng. Đứng ngoài vùng sáng quá 5 giây (nghiêm ngặt: hơn 5.0) thì bị báo về điểm đầu và đồng hồ chạy lại từ đầu. Về điểm đầu KHÔNG xóa sổ riêng (giữ thông tin đã đọc).
- Nhập sai bảng cuối bị PHẠT (AI chọn cụ thể): xóa cả 4 ô và khóa bảng 10 giây (SubmitLockSeconds, chỉnh trong asset). Nhập thiếu ô không phạt. Đã nhập đúng thì khóa bảng; hoàn thành khi TryComplete(true) (cả đội ở cổng).
- Chế độ phát triển một máy: `onlyActivePlayerTimed` (mặc định bật) chỉ tính giờ ngoài vùng sáng cho vai đang điều khiển (phím 1-4), vì các vai khác đứng yên; tắt khi online.
- Vùng sáng = bán kính 4 m quanh người cầm đèn (mặt phẳng ngang); trao đèn bằng F cho người gần nhất trong 2 m; E đọc biển hoặc bấm phím bảng; bảng nhập dùng chữ A-F để chắc chắn hiển thị.
- Không giới hạn ai đọc biển nào: ít người thì một người đọc được cả 4 biển (đúng yêu cầu thang độ khó).

## 5. Subtask
| ID | Việc | File | Cách kiểm | Trạng thái | Bằng chứng |
|---|---|---|---|---|---|
| S1 | Domain + test | LanternRound/Generator/State.cs, SharedLanternTests.cs, 2 asmdef | run_tests EditMode assembly TheReturn.SharedLantern.Domain.Tests | DONE | 33/33 xanh, Console 0 lỗi (mức 3) |
| S1b | Phạt nhập sai (xóa ô + khóa 10s) | LanternState.cs, SharedLanternTests.cs | run_tests | DONE | 37/37 xanh (đọc TestResults.xml) |
| S2 | Presentation + adapter + asset settings + nối flow/factory/asmdef (không scene) | SharedLanternPrototype.cs, LanternSign/PanelKey/Symbols, SharedLanternSettings + SharedLantern_VI.asset, SharedLanternMissionAdapter.cs; SchoolFloorFlow (+field), MissionRegistryFactory (+đăng ký), TheReturn.SchoolFloor.asmdef | compile | DONE | Console 0 lỗi (mức 2); chưa Play Mode vì chưa có object trong scene |
| S3 | Dựng cảnh Hanh_Lang_4: 2 cổng Vao/Ra + khu chuẩn bị, SharedLantern_LogicOnly (4 biển, bảng nhập 6+2 phím, 4 ô, đèn lồng + Light, checkpoint, vùng cổng cuối), gán flow.sharedLantern, cập nhật bảng 22 cổng của MissionFlowController, kiểm Play Mode | scene SchoolFloorPrototype | Play Mode | CHỜ Scene lock (phiên #4 giữ từ 02:46, im lặng ~9,5h); chủ dự án đã cho phép sửa scene cho khu này | |
| S4 | Viết bộ kiểm tích hợp NUnit/Play Mode cho nhiệm vụ | Tests | | TODO | |

## 6. Bản đồ lỗi
| Triệu chứng có thể gặp | File > hàm | Cách kiểm rẻ nhất |
|---|---|---|
| Đọc biển mà báo không đọc được | LanternState.Read (NotInLight/AlreadyRead/InvalidSign) | Đọc LanternReadResult trả về; kiểm kết quả đo vùng sáng ở Presentation |
| Bị đưa về đầu quá sớm/muộn | LanternState.Tick (ngưỡng OutsideLimit, hơn 5.0 giây, người cầm đèn được miễn) | Gọi Tick với dãy thời gian cố định (xem test Tick_*) |
| Không trao được đèn | LanternState.TryPass (NotHolder/TooFar: tầm 2 m, khoảng cách NaN cũng là TooFar) | Đọc LanternPassResult; kiểm đơn vị khoảng cách |
| Nhập đúng mà không hoàn thành | Submit -> CodeAccepted; TryComplete cần allAtGate | Đọc CodeAccepted rồi gọi TryComplete(true) |
| Cùng seed ra đề khác nhau | LanternGenerator.Generate (xorshift tự cài) | Test Generator_IsDeterministicPerSeed |

## 7. Rủi ro và câu hỏi cho chủ dự án
- Về điểm đầu có nên xóa sổ riêng (khó hơn) hay giữ (đang làm)? Có tính vào phạt khác không?
- Bảng cuối: nhập sai có nên có hậu quả (khóa vài giây, đưa về)? Đang là không phạt.
- Hanh_Lang_4 chưa có cổng/khu chuẩn bị trong scene; cần chủ dự án quyết định bố trí hoặc cho phép tạo (đúng quy tắc: khu chuẩn bị trước mỗi hành lang nhiệm vụ).

## 8. Tiến độ
- 2026-10-17 02:55: S1 xong, 33 test xanh.
