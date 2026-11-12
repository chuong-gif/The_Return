# PROTOCOL — quy trình làm việc nhiều AI trên The Return

Mục tiêu: AI mới vào dự án hiểu việc cần làm mà KHÔNG đọc cả dự án; biết ai đã làm gì; báo cáo đúng chỗ; nhận ra việc dở của AI trước; biết chủ dự án vừa đổi gì. Không có khóa kỹ thuật thật — mọi thứ dựa trên quy ước dưới đây, nên phải tuân thủ nghiêm.

## 1. File nào dùng để làm gì
| File | Ai ghi | Nội dung | Cách ghi |
|---|---|---|---|
| `AGENTS.md` | chủ dự án/AI (hiếm) | quy tắc cứng + thứ tự đọc | sửa khi quy tắc đổi |
| `.ai/CURRENT.md` | AI cuối cùng làm việc | trạng thái tổng, cảnh báo — ≤ 30 dòng | GHI ĐÈ |
| `.ai/TASKS.md` | AI nhận/hoàn thành task | bảng task, trạng thái, ai giữ, bằng chứng | SỬA ĐÚNG DÒNG của task |
| `.ai/LOG.md` | MỌI AI | nhật ký vận hành gần đây, tối đa 8 mục/8 KB | ghi ngắn, gộp/bỏ mục cũ khi vượt giới hạn |
| `.ai/OWNER_UPDATES.md` | chủ dự án (hoặc AI ghi hộ) | thay đổi do chủ dự án tự làm ngoài AI | CHỈ THÊM |
| `.ai/PLANS/`, `DECISIONS/`, `BUGS/`, `MAPS/` | AI | kế hoạch, quyết định + lý do, lỗi + nguyên nhân gốc, bản đồ | file riêng theo chủ đề |
| `PROJECT_STATE.md` | chủ dự án + AI | trạng thái triển khai chi tiết (nguồn dài, đọc khi cần) | theo quy ước của file đó |
| `.ai/QUEUE.md` | AI | các bước đang chờ của map/online 4 nhiệm vụ | cập nhật đúng thẻ |

Nguyên tắc nguồn sự thật: code + Unity thực tế > LOG/OWNER_UPDATES > TASKS/CURRENT > PLANS/MAPS. Khi mâu thuẫn, tin cái ở trái và sửa cái ở phải.

## 2. Bắt đầu phiên
1. Đọc `START_HERE.md`, `CURRENT.md` và tối đa 8 mục trong `LOG.md`; trạng thái mới luôn ở CURRENT/TASKS/QUEUE.
2. Mở đúng dòng task trong `TASKS.md`; với task trong hàng đợi online, chỉ mở thẻ kế tiếp trong `QUEUE.md`.
3. Chỉ đọc plan, system card, owner update hoặc bản đồ khi task/điều kiện sửa scene yêu cầu. Tìm đoạn liên quan, không mở cả thư mục/tài liệu lớn.
4. Làm PREFLIGHT (mục 3). Không đọc `Design/`, `Backups/`, Git history hay `ThirdParty` nếu task không cần.

## 3. PREFLIGHT trước khi sửa bất cứ thứ gì
a. Task đã có người giữ? Xem trạng thái ở TASKS + mục LOG gần nhất của task đó.
b. Việc của bạn có thể đã được làm: kiểm bằng CODE/Unity thực tế (không tin mô tả). Nếu đã xong -> ghi LOG `ALREADY-DONE`, cập nhật TASKS, dừng, không làm trùng.
c. Kiểm tra thay đổi chưa ghi bằng `git status --short` hoặc `.ai/tools/changed_since.snippet.cs` nếu có. Nếu file định sửa có thay đổi mới, đọc/tích hợp; không ghi đè hoặc hoàn tác khi chưa rõ nguồn gốc.
d. Kiểm tra Console không có lỗi compile; Unity không ở Play Mode.
e. Trước khi sửa `.unity`/`.prefab`: xác nhận quyền sửa trong owner update liên quan, tạo snapshot, kiểm tra scene dirty; nếu đã dirty thì hỏi/đối chiếu trước khi lưu.

## 4. Vòng đời task (trạng thái trong TASKS.md)
Task có thẻ QUEUE dùng ngưỡng INTERRUPTED 6 giờ (mục 14). Ngưỡng STALE 24 giờ chỉ còn cho task không có thẻ.
`TODO` -> `CLAIMED` -> `IN-PROGRESS` -> `REVIEW` (xong, chờ chủ dự án xem) -> `DONE`; nhánh: `PAUSED` (dừng giữa chừng có chủ ý), `BLOCKED` (cần thông tin/quyết định/quyền), `STALE` (bị bỏ dở).
- Nhận task: sửa dòng task (trạng thái, tên AI/model, giờ UTC) và ghi LOG `START` TRƯỚC khi sửa file đầu tiên, liệt kê file định đụng.
- Đang làm: sau mỗi nhóm thay đổi có ý nghĩa, ghi LOG `PROGRESS` ngắn (đã làm gì, mức xác minh, file). Trước khi thêm, nếu LOG vượt 8 mục hoặc 8 KB thì gộp/bỏ mục cũ đã được ghi ở nơi chuẩn.
- Nếu phiên bị ngắt đột ngột, START/PROGRESS cuối cùng chỉ là dấu vết; CURRENT/TASKS/QUEUE/plan là nguồn trạng thái.
- Một task không có mục LOG nào trong 24 giờ mà đang `CLAIMED/IN-PROGRESS` = ứng viên `STALE`; người sau được tiếp quản sau khi làm mục 6.

## 5. Kết thúc hoặc tạm dừng (bắt buộc, kể cả khi chưa xong)
1. Sửa dòng task trong TASKS.md: trạng thái mới + bằng chứng + bước kế tiếp.
2. Thêm mục LOG `DONE`/`PAUSED`/`BLOCKED` ngắn: kết quả, mức xác minh (M1 file / M2 nội dung / M3 compile-test / M4 runtime / M5 chủ dự án), việc dở và bước kế tiếp.
3. Cập nhật CURRENT.md nếu trọng tâm hoặc cảnh báo đổi.
4. Có tri thức lâu dài -> `DECISIONS/` (quyết định + lý do) hoặc `BUGS/` (lỗi + nguyên nhân gốc + cách sửa). Đổi cấu trúc/hệ thống -> sửa `ARCHITECTURE.md`/`SYSTEMS.md` và bản đồ liên quan; chạy lại index nếu thêm/xóa/đổi tên script (`.ai/tools/rebuild_index.snippet.cs`).
5. Báo cáo cuối trong chat cho chủ dự án (mục 9). Không tạo báo cáo dài cho từng thao tác nhỏ.

## 6. Tiếp quản việc dở của AI khác
Với thẻ QUEUE không cần giấy TAKEOVER; chỉ một dòng LOG `RESUME` (mục 14.4).
1. Đọc các mục LOG của task đó + dòng TASKS. 2. Đối chiếu thực tế: `changed_since`, `git diff`/`git status` nếu có, Console, mở đúng các file được liệt kê. 3. Xác định phần ĐÃ xong và đã xác minh (giữ nguyên) với phần dở/hỏng. 4. Ghi LOG `TAKEOVER` (từ ai, trạng thái thật, kế hoạch). 5. Không làm lại phần đã xong đã xác minh; không xóa/hoàn tác việc của AI trước nếu chưa chắc — hỏi chủ dự án khi có nguy cơ mất dữ liệu.
Ngược lại, nếu BẠN bị ngắt: lần vào lại, đọc LOG xem có `TAKEOVER`/mục của AI khác sau mục cuối của bạn không; nếu có, việc của bạn có thể đã được làm — làm preflight b trước khi tiếp tục.

## 7. Chủ dự án thay đổi dự án
- Chủ dự án ghi vào `.ai/OWNER_UPDATES.md` (mẫu 3 dòng ở đầu file). Cách nhanh nhất: nói với bất kỳ AI nào «ghi vào OWNER_UPDATES: ...» — AI PHẢI ghi vào file đó, không chỉ nhớ.
- AI vào phiên: đọc các mục OWNER_UPDATES mới; với mỗi mục, kiểm tra file/khu vực bị ảnh hưởng và cập nhật các file `.ai/` nào đã lỗi thời (ARCHITECTURE, SYSTEMS, MAPS, PLANS, TASKS). Ghi LOG `OWNER-CHANGE-PROCESSED`.
- Thay đổi chủ dự án làm mà chưa báo (phát hiện qua changed_since/git) -> LOG `UNLOGGED-CHANGE`, không ghi đè; nếu ảnh hưởng task của bạn, hỏi một câu ngắn.

## 8. Quản lý ngữ cảnh / token
HOT = CURRENT + task đang làm + file đang sửa. WARM = plan, ARCHITECTURE/SYSTEMS phần liên quan, README feature. COLD = còn lại. Mỗi task trong TASKS.md có cột `Đọc` liệt kê tối đa vài file/thư mục cần đọc. Tìm symbol bằng IDE/`rg` trong thư mục Core4 đã liệt kê ở `.ai/MAPS/index-scripts.md`; không quét cả Assets. Không đọc lại file chưa đổi; gom thao tác đọc độc lập; giới hạn output công cụ; LOG chỉ ghi kết quả, không dán nhật ký dài.

## 9. Báo cáo cuối phiên (chat, ≤ 8 dòng)
Làm gì | mức xác minh (1-5) | file đã đổi | việc dở + bước kế | cần chủ dự án quyết định gì | đã cập nhật những file .ai nào.

## 10. Prompt khởi động cho chủ dự án (dán cho AI mới)
> Bạn đang vào dự án Unity The Return tại [đường dẫn]. Đọc AGENTS.md, rồi làm đúng .ai/PROTOCOL.md (mục 2-3). Không quét toàn dự án. Task của bạn: [ID hoặc mô tả]. Trước khi sửa phải ghi START trong .ai/LOG.md và cập nhật .ai/TASKS.md; khi dừng hoặc xong phải ghi LOG và báo cáo theo mục 9.

## 12. Lập kế hoạch cho một task (bắt buộc với task đổi >1 file, đổi scene/prefab, hoặc đổi hành vi)
1. Chọn task `TODO` chưa ai giữ và không `BLOCKED` (ưu tiên theo ID/thứ tự plan). Preflight (mục 3), nhận task, ghi `START`.
2. ĐỌC CÓ GIỚI HẠN: (a) thẻ hệ thống liên quan (`.ai/SYSTEM_CARDS/INDEX.md` -> thẻ), (b) README/kế hoạch cũ liên quan, (c) chỉ các file/hàm cần — tìm bằng `.ai/MAPS/index-scripts.md` rồi đọc đoạn, không đọc cả file/thư mục. Quy tắc: chưa nêu được lý do cụ thể thì không mở file.
3. Viết kế hoạch vào `.ai/PLANS/T-xxx-<slug>.md` theo `.ai/PLANS/_TEMPLATE.md`: mục tiêu + nghiệm thu, phạm vi, NGỮ CẢNH ĐÃ ĐỌC (mỗi file 1 dòng kết luận), subtask nhỏ có cách kiểm, BẢN ĐỒ LỖI (triệu chứng -> nơi xem -> cách kiểm).
4. Duyệt: task đổi gameplay/kiến trúc/scene hoặc >5 subtask -> đặt `CHỜ DUYỆT`, dừng và hỏi chủ dự án trước khi viết code. Task nhỏ tự duyệt, vẫn phải có kế hoạch ngắn.
5. Làm TỪNG subtask: làm -> kiểm -> sửa dòng subtask trong bảng (trạng thái + bằng chứng) -> LOG 1-3 dòng -> báo chat 1-3 dòng: «S2 xong (mức 3): <kết quả>. Tiếp S3.» Không viết báo cáo dài.
6. Kế hoạch là tài liệu sống: hiểu thêm thì sửa, ghi lý do. Xong task: cập nhật system card nếu có tri thức tái sử dụng, TASKS/QUEUE và LOG. Khi task đóng, bỏ plan khỏi PLANS nếu không còn bước hoặc chi tiết cần tra; giữ kết quả cần thiết ở TASKS_DONE/decision/system card.

## 13. Xử lý lỗi (bug triage) — ít token nhất
1. Ghi 1 dòng mô tả lỗi + cách tái hiện/log Console. Chưa mở code.
2. Xác định khu/hệ thống: `.ai/SYSTEM_CARDS/INDEX.md` (theo khu hoặc từ khóa) -> mở ĐÚNG 1 thẻ.
3. Đọc bảng «Triệu chứng -> nơi xem» của thẻ; nếu task gần đây có kế hoạch (`PLANS/`), đọc mục 5 (bản đồ lỗi) và các subtask đã `DONE` + LOG/`git log` để biết gần đây đổi gì (nghi phạm đầu tiên).
4. Mở ĐÚNG file/hàm được chỉ (tìm symbol bằng index, đọc đoạn). Chưa thấy thì mở rộng từng bước, có bằng chứng.
5. Tối đa 3 giả thuyết; chọn phép kiểm rẻ nhất phân biệt được chúng (Console, đo giá trị runtime, test hẹp). Không sửa mò.
6. Sửa nhỏ nhất -> compile -> kiểm lại đúng phép kiểm -> mức xác minh. Giới hạn 3 vòng sửa; sau đó `BLOCKED` + báo.
7. GHI LẠI để người sau tốn ít token hơn: thêm/sửa 1 dòng ở bảng B của thẻ (triệu chứng -> nơi xem -> cách kiểm); nếu bài học lâu dài, tạo `BUGS/<slug>.md` (triệu chứng, nguyên nhân gốc, cách sửa, mức xác minh); sửa mục 5 của kế hoạch nếu lỗi thuộc một task; LOG.
8. Không có thẻ/kế hoạch phù hợp: lần theo symbol bằng index, rồi tạo/bổ sung thẻ sau khi sửa xong.

## 11. Giới hạn đã biết
- Thẻ hệ thống có thể cũ; kiểm mã/scene hiện hành trước khi tin bản đồ hoặc kết quả test ghi trong thẻ.
- Không có khóa: chủ dự án chạy AI nối tiếp (hết limit mới chuyển AI khác) nên đã bỏ Scene lock (2026-10-29). Nếu có lúc chạy song song, chủ dự án chỉ định phạm vi file/scene cho từng AI.
- Mốc thời gian dùng UTC (giờ file trên đĩa cũng là UTC khi đọc bằng changed_since).
- Nếu không có terminal/Git, dùng `changed_since` + LOG để phát hiện file mới đổi nhưng không có nội dung diff. Nếu có Git, ưu tiên `status`/`diff`; không suy luận rằng file không đổi chỉ vì chưa có OWNER UPDATE.

## 14. Quy trình task mạng chịu ngắt limit
Pha: L Logic -> G Gauntlet -> F Fix -> O Online -> S Smoke -> H Người thật. Việc đi theo thẻ trong `.ai/QUEUE.md`; lỗi test đi theo `.ai/FAILURES.md` + `.ai/MISSIONS_NET.md`; câu hỏi chờ ở `DECISIONS/PENDING.md`.
1. Bước nguyên tử: mọi file của một bước tạo liền nhau, rồi MỘT lần refresh + compile, MỘT lần chạy test của bước.
2. Luôn để trạng thái xanh: không kết thúc/không bắt đầu bước mới khi Console còn lỗi biên dịch do mình gây ra. Thứ tự tạo: asmdef -> kiểu nền -> kiểu dùng nền -> test.
3. Ghi ý định TRƯỚC, kết quả SAU: dòng NEXT trong khối RESUME của plan (1 dòng: việc + danh sách file định tạo) cập nhật trước khi bắt đầu bước, và cập nhật lại khi xong.
4. Vào lại sau khi bị ngắt: đọc NEXT; kiểm đủ file + compile + test của bước; thiếu/hỏng thì làm lại bước từ đầu (chỉ file MỚI nên ghi đè an toàn); đủ và xanh thì đánh dấu xong. Chỉ một dòng LOG `RESUME`, không TAKEOVER.
5. Thẻ IN-PROGRESS không có LOG >= 6 giờ = INTERRUPTED (không phải bị bỏ rơi); AI nào cũng tiếp được ngay.
6. Ghi file phải idempotent: tạo-mới hoặc ghi-đè-toàn-bộ. LOG có giới hạn 8 mục/8 KB: giữ việc đang làm và kết quả cần tiếp nối; gộp tiến độ lặp, bỏ lịch sử không còn liên quan, không tạo LOG_FULL.
7. Cập nhật bảng TASKS + LOG + QUEUE gộp trong MỘT lần gọi ở cuối bước để không lệch nhau.
8. Trả Editor về như lúc nhận (scene gốc, không Play, không dirty) trước khi kết thúc thẻ.
9. Sắp hết limit (hoặc nghi ngờ): viết RESUME ngay, ngắn, rồi mới làm tiếp.
10. Câu hỏi có phương án PROVISIONAL: ghi lý do/cách đổi và tiếp tục phần không bị chặn. Nếu PENDING ghi CHỜ DUYỆT hoặc task yêu cầu chủ dự án cho phép, dừng đúng tại ranh giới đó; không tự làm (hiện tại P-014).
11. Lỗi chéo (Network/Framework/Contract/lớp cơ sở hoặc nhiều nhiệm vụ cùng nguyên nhân) -> thẻ FIX dùng chung trong QUEUE. Lỗi Domain chưa rõ -> không sửa mò; ghi bằng chứng và báo chủ dự án.
12. Gauntlet: một lệnh kiểm mọi nhiệm vụ: `execute_menu_item` đường dẫn `TheReturn/Gauntlet/Chạy Gauntlet mạng` rồi đọc `.ai/GAUNTLET.md` khi dòng đầu hết «ĐANG CHẠY» (không Thread.Sleep/chờ trong execute_code: chặn luồng chính của Unity, test không chạy được). Lỗi tự vào FAILURES.md + thẻ FIX trong QUEUE.md theo nhiệm vụ/người viết (MISSIONS_NET.md); test xanh lại thì dòng F thành XANH-LẠI, người giữ xác nhận rồi mới FIXED. Nhiệm vụ mạng mới PHẢI có dòng trong MISSIONS_NET.md (Gauntlet cảnh báo nếu thiếu).
