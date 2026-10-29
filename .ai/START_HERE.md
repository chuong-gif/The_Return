# START_HERE — đọc file này TRƯỚC; thường là ĐỦ để bắt đầu (cập nhật 2026-10-28 17:20 UTC)
Dự án Unity «The Return» (co-op 2–4 người, tầng trường học; 12 khu = Phong_1..6 + Hanh_Lang_1..6). Gốc: E:\kho model 3D\Project Unity\The Return. Chủ dự án nói tiếng Việt.

## NGÂN SÁCH ĐỌC (bắt buộc — nguyên nhân tốn token trước đây)
- Khởi động chỉ đọc: file này + `.ai/CURRENT.md` + 8 mục cuối `.ai/LOG.md`. Tổng ≲ 6.000 token.
- KHÔNG mở cả file: LOG_FULL, PLANS/*, SYSTEM_CARDS/*, TOOLING.md, PROTOCOL.md, MAPS/*, PROJECT_STATE.md — trừ khi task BẠN nhận trỏ đúng file đó; khi mở thì tìm bằng Grep/find_in_file rồi đọc đoạn (offset/limit).
- Mỗi tool call gửi lại toàn bộ ngữ cảnh: ít lượt hơn = rẻ hơn. Gom nhiều thao tác đọc vào MỘT execute_code và chỉ `return` chuỗi ngắn (≤ 40 dòng). Không gọi hierarchy/scene dump/read_console không giới hạn (luôn đặt count).
- «Tìm việc rảnh» = chỉ đọc mục FREE dưới đây, chọn, trả lời chủ dự án ngay (≤ 3 tin), rồi mới mở PLANS của task đó. Không khảo sát cả dự án.
- Viết LOG ≤ 5 dòng/≤ 700 ký tự mỗi mục; chi tiết dài → PLANS/ hoặc SYSTEM_CARDS/. Báo chat cuối ≤ 8 dòng.

## LUẬT CỨNG (bản rút gọn của AGENTS.md)
- Không commit/push/xóa nếu chưa được yêu cầu. Không chạy lại builder/migration cũ (menu The Return/*). Không ghi API key/mật khẩu.
- Sửa scene: chỉ khi chủ dự án cho phép (xem OWNER_UPDATES, 5 mục cuối) + snapshot `BeforeXxx_yyyyMMdd_HHmmss.unity` (SaveScene(scene, path, true)) trước khi sửa. KHÔNG còn Scene lock (chủ dự án bỏ 2026-10-29: AI chạy nối tiếp). Scene dirty lúc bắt đầu = thay đổi chưa lưu của phiên trước/chủ dự án: kiểm trước khi lưu, đừng ghi đè.
- Trước khi làm: ghi START vào LOG + đổi dòng task trong TASKS. Sau khi làm: LOG DONE/PAUSED + TASKS + CURRENT. Task DONE chuyển sang `.ai/TASKS_DONE.md`.
- Mức xác minh: 1 tạo file / 2 compile / 3 test xanh / 4 Play Mode / 5 chưa xác nhận. Không nói «xong» chỉ vì compile.
- Test: run_tests EditMode cần `assembly_names` (vd TheReturn.SchoolFloor.Domain.Tests). Play Mode: Cursor.lockState=Locked mới thấy HUD (editor coi là pause).
- AI chạy NỐI TIẾP, không song song (chủ dự án xác nhận 2026-10-29). Phiên trước có LOG START mà chưa DONE thường là đã hết limit: đọc LOG rồi làm tiếp, không cần chờ.

## BOARD (TỰ SINH từ TASKS.md bằng `.ai/tools/make_board.snippet.cs`; DONE nằm ở TASKS_DONE.md)
<!--BOARD-->
- T-005 IN-PROGRESS | Claude Sonnet 5.5 (phiên #6,… | Test Play Mode NUnit cho luồng cổng/chọn khu/RestartMap: 11 [UnityTest] đã commit (81b6926…
- T-006a2 REVIEW | Claude Sonnet 5.5 (phiên #3) | Phong_5 Cầu dao: code + scene xong. CÒN: chủ dự án chơi thật.
- T-006b2 REVIEW | Claude Sonnet 5.5 (phiên MCP… | Hanh_Lang_6 PlankAndKey: Domain + Session + Presentation + adapter + đăng ký + s…
- T-006c REVIEW | Claude Sonnet 5.5 (phiên #3,… | Hanh_Lang_5 Cô nói: code + scene xong, Play Mode giả lập qua. CÒN: chơi thật, nhánh cảnh c…
- T-006d2 REVIEW | Claude Sonnet 5.5 (phiên #3) | Phong_6 Choir: code + scene xong. CÒN: chủ dự án chơi thật.
- T-006e2 REVIEW | Claude Sonnet 5.5 (phiên #4,… | Hanh_Lang_2 GlassBridge: code + scene + 2 cổng xong. CÒN: chủ dự án chơi thật.
- T-006f REVIEW | Claude Sonnet 5.5 (phiên #5;… | Hanh_Lang_4 SharedLantern: code + scene xong, Play Mode giả lập qua. CÒN: chơi thật, test …
- T-008 REVIEW | Claude Sonnet 5.5 (phiên #9,… | MainMenu + nhóm chơi (IPartyService một máy) + Settings xong; Play Mode mô phỏng cả luồng …
- T-009 IN-PROGRESS | Claude Sonnet 5.5 (phiên #7,… | Online co-op 2-4 người (gói NGO đã có trong manifest từ 23/10, chờ chủ dự án xác nhận S1):…
- T-010 TODO | - | Nhân vật + animation. Cần chủ dự án cho asset/hướng.
- T-011 REVIEW | Claude Sonnet 5.5 (phiên #3) | UI theo khu XONG S1-S5 (HUD chung, bản đồ nhỏ sân, phím N sổ tay, 12 khu dùng provider). C…
- T-012 REVIEW | Claude Sonnet 5.5 (phiên #5) | Tài liệu prefab/vật tương tác: PREFAB_GUIDE (14 prefab cũ, phiên trước) + 6 README mới cho…
- T-017 REVIEW | Claude Sonnet 5.5 (phiên #3) | Không mặc định vào Phong_1: UI tự chọn phòng + F8 test 12 khu + nhãn cổng. CÒN: chơi thật.
- T-018 REVIEW | Claude Sonnet 5.5 (phiên #3) | Âm thanh bản thử: AudioService + bank (41 clip tạm) + tiếng chung + tiếng riêng 12 khu (Mi…
- T-019 REVIEW | Claude Sonnet 5.5 (phiên #3) | Bug Phong_1 (chủ dự án 2026-10-24): N không mở sổ, ghế không tương tác, không có tên nhân …
- T-020 REVIEW | Claude Sonnet 5.5 (phiên #8) | 6 root nhiệm vụ mới (Breaker, Choir, GlassBridge, PlankAndKey, SharedLantern, TeacherSays)…
- T-021 REVIEW | Claude Sonnet 5.5 (phiên #8) | 5 root nhiệm vụ cũ (QuietCorridor, GradeRepair, NavigationExam, DontLookBack, TeacherTruth…
<!--/BOARD-->

## FREE — việc AI có thể nhận NGAY (cập nhật 2026-10-28; nguồn sự thật là BOARD ở trên)
Code 12 khu, HUD theo khu, MainMenu/Party/Settings và âm thanh bản thử đều đã có. KHÔNG còn task code nào hoàn toàn rảnh.
1. Đang có người giữ (chỉ tiếp quản khi >24h không LOG hoặc chủ dự án cho phép): T-005 (phiên #6), T-009 (phiên #7).
2. Chờ chủ dự án quyết/brief: T-009 S1 (cho phép cài gói NGO/Relay), T-010 nhân vật/animation (cần asset/hướng).
3. REVIEW chờ chủ dự án CHƠI THẬT (AI không làm thêm được): T-006a2/b2/c/d2/e2/f, T-008, T-011, T-017, T-018, T-019, T-020, T-021. T-012 xong phần tài liệu; T-020 (6 root nhiệm vụ mới) và T-021 (5 root nhiệm vụ cũ) đã chuyển thành prefab.
4. Việc chủ dự án tự làm (AI không xóa được): xóa thư mục Features/Audio (trùng GameAudio) và ảnh thừa Assets/Screenshots/*.png.
→ Khi chủ dự án bảo «tìm việc»: trả lời NGAY từ mục này (≤ 6 dòng, 0 tool call nếu START_HERE vừa đọc xong), đừng mở thêm file. Nếu không có việc rảnh thì nói thẳng và hỏi chủ dự án muốn nhận gì.

## MẸO GIẢM TOKEN CHO CHỦ DỰ ÁN
- Giao việc CỤ THỂ theo mã (vd «làm T-006b2 S6») thay vì «tìm việc» mở; mỗi việc một cuộc chat mới (chat dài = mỗi lượt gửi lại cả lịch sử).
- Khi AI báo xong, đóng chat; việc sau mở chat mới (AI đọc START_HERE ~2k token là đủ).

## CÔNG CỤ GỌN (dùng thay vì tự đọc)
- Preflight MỘT lệnh: dán `.ai/tools/status.snippet.cs` vào execute_code (trả ≤ 15 dòng: Play/compile/scene dirty/3 mục LOG cuối/file .cs đổi sau LOG).
- Sau khi sửa TASKS: dán `.ai/tools/make_board.snippet.cs` để sinh lại BOARD.
- Giao việc: dùng `.ai/PLANS/_BRIEF_TEMPLATE.md` (phiếu ≤ 15 dòng). Nhận phiếu thì làm luôn.
- Đọc đoạn file: find_in_file/Grep rồi offset+limit; mẫu execute_code gọn: `var l=System.IO.File.ReadAllLines(p); string r=""; for(int i=a;i<b;i++) r+=l[i].Substring(0,System.Math.Min(160,l[i].Length))+"\n"; return r;`
- 3 tầng: (1) luôn đọc START_HERE + CURRENT; (2) khi nhận task: PLANS/T-xxx của nó; (3) tra khi cần: SYSTEM_CARDS/TOOLING/PROTOCOL/SESSIONS bằng tìm kiếm.
- LOG mỗi mục ≤ 3 dòng/≤ 500 ký tự (mẫu ở đầu LOG.md). TASKS cột Task ≤ 140 ký tự.
