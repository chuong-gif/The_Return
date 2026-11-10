# T-040 Cắt còn 4 nhiệm vụ + scene/map mới cho 4 nhiệm vụ (KẾ HOẠCH)
Trạng thái: HƯỚNG ĐÃ CHỌN 2026-11-10 08:21 UTC (PA2 dựng mới hoàn toàn; xóa hẳn code 8 nhiệm vụ SAU KHI scene mới chạy; giữ hành lang 5). Chờ: duyệt BẢN THIẾT KẾ sơ đồ (P3a) | AI lập: phiên #13 | Quyết định gốc: DECISIONS/2026-11-10-scope-cut-4-missions.md

## 1. Quyết định của chủ dự án (2026-11-10)
- Từ 12 nhiệm vụ còn 4: PlankAndKey (Hanh_Lang_6), Attendance (Phong_1), BreakerPanel (Phong_5), Choir (Phong_6). Lý do: tiến độ quá lâu, ưu tiên cái làm nhanh nhất.
- BỎ 8: TeacherSays (HL5), GlassBridge (HL2), SharedLantern (HL4), QuietCorridor (HL1), GradeRepair (P2), NavigationExam (P3), DontLookBack (HL3), TeacherTruth (P4).
- Yêu cầu: tạo scene MỚI và thiết kế lại map phù hợp với 4 nhiệm vụ.

## 2. Hiện trạng (đo 2026-11-10, từ TASKS/MISSIONS_NET/LOG; con số Gauntlet 2026-11-08)
| Nhiệm vụ | Khu | Logic + mạng | Online/scene thử | Còn thiếu |
|---|---|---|---|---|
| PlankAndKey | HL6 | xong (T-027) | nghiệm thu 2 máy thật 2026-11-01 | chưa có adapter hợp đồng; chủ dự án chơi thật (T-006b2) |
| Attendance | P1 | xong | LAN 3 tiến trình đạt; đội 2 người chưa kết luận | chủ dự án chơi thật |
| BreakerPanel | P5 | xong | scene thử NetworkBreakerTest: host một mình khớp mốc; dùng lớp cơ sở T-039 M5 | thử 2 máy thật; chủ dự án chơi thật (T-006a2) |
| Choir | P6 | xong (ChoirRules.OnlineFor theo số người) | scene thử NetworkChoirTest + Online + adapter hợp đồng; host một máy (T-037 GĐ2, #14 PAUSED) | thử 3 tiến trình/LAN; chủ dự án chơi thật (T-006d2) |
- Hạ tầng sẵn dùng: khung mạng host-authoritative (T-024), nhân vật online (T-025), phòng/đội (T-026), LAN + Relay (đã nghiệm thu Internet), Gauntlet 1 lệnh (T-039).
- Map cũ SchoolFloorPrototype: vòng tròn 12 không gian quanh Sân: P1-HL1-P2-HL2-P3-HL3-P4-HL4-P5-HL5-P6-HL6-P1; 24 cổng; MissionFlowController (tự chọn khu), MapProgress (thắng khi xong hết); Canvas/âm thanh/test gắn 12 khu (MAPS/area-adjacency.md, SYSTEM_CARDS/SchoolFloor.md).
- PHÁT HIỆN: 4 khu giữ lại nằm LIỀN NHAU trên một cung của vòng: P5 - HL5 - P6 - HL6 - P1. HL5 (TeacherSays) nằm giữa nhưng bị bỏ nhiệm vụ.

## 3. Phạm vi và nguyên tắc
- GIỮ: 4 nhiệm vụ + hạ tầng chung (khung mạng, phòng, Relay, UI chung, Gauntlet).
- BỎ có hai mức (mục 9 câu 1). A) ĐÓNG BĂNG [PROVISIONAL mặc định]: 8 nhiệm vụ không vào scene mới, UI, âm thanh, registry, đường build; KHÔNG xóa code/asset (git giữ, hoàn tác được). B) XÓA HẲN code 8 nhiệm vụ: chỉ khi chủ dự án xác nhận riêng, và sau khi scene cũ không còn cần; assembly SchoolFloor (+Editor) tham chiếu cứng 8 assembly nhiệm vụ nên xóa sớm sẽ làm hỏng biên dịch.
- KHÔNG sửa SchoolFloorPrototype (giữ làm bản lưu); mọi thứ mới nằm ở scene mới. Blockout là sản phẩm của chủ dự án: nhân bản có snapshot, không dựng lại bằng builder cũ.
- KHÔNG thêm nhiệm vụ mới, KHÔNG làm lại nhiệm vụ đang chạy tốt, KHÔNG chạy MenuItem builder/migration cũ (AGENTS.md).

## 4. Ảnh hưởng tới task hiện có
| Task | Hiện trạng | Đề xuất |
|---|---|---|
| T-036 (GradeRepair, NavigationExam, TeacherTruth) | GradeRepair pha L DONE hôm nay (#10) | ĐÓNG: cả 3 bị cắt, không làm tiếp |
| T-037 (Choir, TeacherSays, SharedLantern) | #14 PAUSED ở Choir GĐ2 | THU HẸP còn Choir; TeacherSays, SharedLantern cắt |
| T-038 (GlassBridge, QuietCorridor, DontLookBack) | TODO, chưa ai nhận | HỦY |
| T-031 | Attendance + BreakerPanel; 9 nhiệm vụ còn lại tách T-036/37/38 | giữ; phần còn lại chỉ còn Choir (T-037) |
| T-006c, T-006e2, T-006f, T-020/T-021, T-012 (phần 8 nhiệm vụ cắt) | REVIEW chờ chơi thật | đánh dấu CẮT; 4 nhiệm vụ giữ vẫn cần chủ dự án chơi thật |
| T-011 (UI theo 12 khu), T-018 (âm thanh 12 khu) | REVIEW | co về 4 khu trong scene mới (bước P3) |
| T-028 (luồng map qua mạng, phần scene chờ) | REVIEW | KHÔNG làm trên SchoolFloorPrototype nữa; gộp vào bước P4 trên scene mới |
| T-032 (build gửi bạn bè) | BLOCKED chờ T-028 | vẫn chờ; build sẽ dùng scene mới |
| T-039 | M1-M5 DONE | M6 (sinh nhiệm vụ mới) HỦY vì không còn nhiệm vụ mới; M7 smoke gộp vào P5 của T-040 |
| T-009 umbrella | IN-PROGRESS | cập nhật phạm vi 4 nhiệm vụ |

## 5. Thiết kế scene/map mới (ĐỀ XUẤT, chờ duyệt)
Mục tiêu: bản đồ gọn, đi trọn 4 nhiệm vụ nhanh, đội 2-4 người, mỗi khu dễ nhận ra, không còn phòng trống.
- PHƯƠNG ÁN 1 [đề xuất, PROVISIONAL]: NHÂN BẢN + CẮT CUNG. Scene mới tên tạm SchoolFloorCore4. Giữ nguyên blockout của chủ dự án ở cung P5-HL5-P6-HL6-P1 cùng Sân; gỡ P2, P3, P4, HL1-HL4 và cổng/cầu thang liên quan. Hai đầu cung (P1 và P5) trước đây nối nhau qua 8 không gian đã gỡ nên phải xử lý: (i) cung hở, mỗi đầu chỉ thông ra Sân; hoặc (ii) thêm một hành lang ngắn P1-P5 để khép vòng. HL5 (hết TeacherSays) thành hành lang đi lại.
- PHƯƠNG ÁN 2: DỰNG MỚI compact: một sảnh/sân và 4 khu như 4 cánh. Sạch, vừa khít, nhưng tốn công nhất và bỏ phần blockout đã làm tay.
- PHƯƠNG ÁN 3 (không đề xuất): giữ scene cũ, chỉ tắt 8 nhiệm vụ. Nhanh nhất nhưng còn phòng trống nên KHÔNG đáp ứng yêu cầu thiết kế lại map.
- Vì sao đề xuất PA1: tái dùng blockout, cổng, khu chuẩn bị và chỗ đã gắn của chính 4 nhiệm vụ nên rủi ro thấp nhất mà vẫn cho ra bản đồ nhỏ.
- Luật tiến trình GIỮ NGUYÊN (chủ dự án chốt 2026-10-18): mọi cửa đóng; cả đội đứng đủ NGOÀI cổng thì mở cổng đó; vào đủ BÊN TRONG thì nhiệm vụ bắt đầu và khóa lối; thắng khi xong cả 4; thứ tự tự chọn (câu 4 mục 9 cho phép đổi).
- Lobby (Sanh_Cho) và MainMenu: KHÔNG đổi, chỉ đổi đích vào scene mới.

## 6. Các bước và điều kiện XONG
| Bước | Việc | XONG KHI | Trạng thái |
|---|---|---|---|
| P0 | Ghi quyết định; thông báo phạm vi mới cho mọi AI (START_HERE, TASKS, QUEUE, MISSIONS_NET, PENDING) | AI mới mở START_HERE thấy ngay 4 nhiệm vụ và T-040 | DONE 2026-11-10 08:16 |
| P1 | KHẢO SÁT (CHỈ ĐỌC) hai việc: (a) điểm phụ thuộc 12 khu/24 cổng: MissionRegistryFactory, MissionFlowController.bindings, SchoolFloorAccessController.gates, MapProgress, MissionTestLauncher, SchoolFloorCanvasPresenter.Phase, MissionAudioDirector, SchoolFloorFlow, *Checks, spawn; phần nào dùng chung được cho scene mới và phần nào dính 8 nhiệm vụ; (b) FOOTPRINT của 4 nhiệm vụ giữ (kích thước/điểm vào/vật tương tác/số người) để thiết kế phòng vừa khít | Bảng 'cái gì phải đổi' + bảng footprint + DANH SÁCH XÓA an toàn cho 8 nhiệm vụ, ghi PLANS/T-040-dependencies.md | DONE 2026-11-10 19:19 UTC: PLANS/T-040-dependencies.md (41 file code dính, 9 scene dính, 8 thư mục = 67 .cs/9447 dòng, footprint 4 nhiệm vụ) |
| P2 | Đóng băng sổ: đánh dấu CẮT ở TASKS/MISSIONS_NET/thẻ hệ thống; KHÔNG xóa code | mọi dòng liên quan có nhãn CẮT (T-040) | một phần (MISSIONS_NET, T-038); phần còn lại cùng P3 |
| P3a | THIẾT KẾ layout map mới (chưa dựng): sơ đồ một sảnh/sân + 4 khu + hành lang đi lại giữa khu Cầu dao và khu Choir; kích thước theo footprint P1; vị trí cổng/khu chuẩn bị, điểm spawn, lối đi, đường nhìn; luật tiến trình giữ nguyên | PLANS/T-040-map-design.md có sơ đồ + chủ dự án DUYỆT sơ đồ | ĐỀ XUẤT v1 xong 2026-11-10 19:21 UTC (PLANS/T-040-map-design.md), CHỜ DUYỆT P-010 |
| P3b | DỰNG scene mới (blockout) đúng thiết kế đã duyệt; nối luật cửa/tiến trình; co UI/âm thanh/registry về 4 khu; đăng ký Build Settings; scene cũ KHÔNG bị sửa | Mở scene mới Play được; chỉ 4 nhiệm vụ; Gauntlet xanh; Console 0 lỗi | BLOCKED chờ duyệt P3a (Q-012) |
| P4 | Nối online trên scene mới: 4 cặp Online host/client + MissionNetBridge, spawn 2-4 người, luồng phòng->map (phần T-028), người vào muộn nhận đủ trạng thái, adapter hợp đồng cho PlankAndKey | Play host một máy qua đủ 4 nhiệm vụ; Gauntlet gồm 4 nhiệm vụ đều có adapter | TODO |
| P5 | Smoke nhiều tiến trình + Relay: MỘT build, MỘT .bat cho cả 4 nhiệm vụ, đội 2/3/4, thắng và thua có log (thay M7 của T-039) | log từng tiến trình khớp; 2 máy thật qua Relay một vòng | TODO |
| P6 | Chủ dự án chơi thật + dọn: cập nhật tài liệu/thẻ hệ thống; quyết định xóa hẳn (mức B) hay giữ | chủ dự án xác nhận; PENDING trống | TODO |
| P7 | XÓA HẲN code 8 nhiệm vụ (mức B) KHI chủ dự án xác nhận scene mới chạy được: theo DANH SÁCH XÓA của P1; gồm scene cũ SchoolFloorPrototype (sẽ hỏng), builder/migration, bộ *Checks, README/thẻ liên quan, tham chiếu assembly. Mỗi lượt xóa: compile sạch + Gauntlet xanh. Nhắc chủ dự án commit/tag git TRƯỚC (AI không chạy git) | Không còn tên 8 nhiệm vụ trong Assets; compile sạch; Gauntlet xanh; scene mới vẫn chơi được | BLOCKED chờ chủ dự án xác nhận (Q-013) |

XONG KHI cả T-040: scene mới chơi trọn 4 nhiệm vụ với 2-4 người (LAN và Relay), thắng khi xong 4, Gauntlet xanh, scene/Build Settings/UI mới không tham chiếu 8 nhiệm vụ bị cắt, chủ dự án chơi thật xác nhận.

## 7. Rủi ro
- Assembly SchoolFloor (+Editor) tham chiếu cứng 8 assembly nhiệm vụ: scene mới dùng chung code sẽ kéo theo; cần cách tách ở P1 (ưu tiên cách ít sửa code chung nhất).
- Nhiều nơi gắn cứng 12 khu/24 cổng (UI, âm thanh, test, spawn): có thể lớn hơn dự tính; P1 đo trước khi hứa.
- Blockout là sản phẩm của chủ dự án; luôn snapshot, không chạy builder cũ.
- Phiên khác đang/vừa làm 8 nhiệm vụ bị cắt (#10, #14): đã có biển báo ở START_HERE; việc dở dừng ở trạng thái xanh.
- 4 nhiệm vụ giữ lại chưa có xác nhận 'chơi thật' của chủ dự án; kế hoạch này không kiểm chất lượng nhiệm vụ.
- Game sẽ ngắn hơn: độ dài ván là quyết định thiết kế của chủ dự án, ngoài phạm vi kế hoạch.

## 8. Việc của T-039 còn lại
M6 HỦY. M7 gộp vào P5. Dọn sổ (TASKS, CURRENT, BOARD). Thêm adapter PlankAndKey (nằm trong P4). Chuyển Attendance/PlankAndKey sang lớp cơ sở Online: chỉ khi chủ dự án cho phép riêng.

## 9. Câu hỏi cho chủ dự án (câu 1-3 ĐÃ TRẢ LỜI 2026-11-10; câu 4-6 mặc định PROVISIONAL, DECISIONS/PENDING.md)
1. Mức bỏ 8 nhiệm vụ: A) đóng băng, giữ code [mặc định] hay B) xóa hẳn code?
2. Hướng map: PA1 nhân bản + cắt cung [mặc định], PA2 dựng mới, hay PA3? Nếu PA1: khép vòng P1-P5 bằng hành lang mới hay để cung hở [mặc định: cung hở]?
3. Hành lang 5 (giữa P5 và P6, hết TeacherSays): giữ làm hành lang đi lại [mặc định] hay bỏ, nối thẳng?
4. Thứ tự chơi: tự chọn như hiện nay [mặc định] hay cố định?
5. Scene cũ SchoolFloorPrototype: giữ file làm bản lưu, bỏ khỏi Build Settings khi scene mới chạy được [mặc định]?
6. Tên scene mới: SchoolFloorCore4 [tạm]. Có tên khác không?

## 10. Quyết định đã nhận 2026-11-10 08:21 UTC
- Hướng map: PA2 DỰNG MỚI HOÀN TOÀN (không nhân bản scene cũ). Mục 5 PA1 chỉ còn là tham khảo.
- 8 nhiệm vụ bị bỏ: ĐÓNG BĂNG cho tới khi scene mới chạy được, rồi XÓA HẲN (mức B, bước P7).
- Hành lang 5: giữ làm hành lang đi lại; trong map mới là một hành lang nối khu Cầu dao và khu Choir.
- Hệ quả: (a) phải có bản thiết kế sơ đồ duyệt trước khi dựng (P3a); (b) xóa code làm scene cũ hỏng nên scene cũ cũng bị gỡ ở P7; (c) code điều phối dùng chung có thể tái dùng nếu tách được khỏi 8 nhiệm vụ (P1 trả lời).
- Điểm cần lưu ý: dựng mới đồng nghĩa bỏ blockout chủ dự án đã làm tay ở scene cũ; P3a sẽ hỏi nếu có phần nào muốn giữ lại ý tưởng.
