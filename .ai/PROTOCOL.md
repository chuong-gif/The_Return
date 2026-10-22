# PROTOCOL — quy trình làm việc nhiều AI trên The Return

Mục tiêu: AI mới vào dự án hiểu việc cần làm mà KHÔNG đọc cả dự án; biết ai đã làm gì; báo cáo đúng chỗ; nhận ra việc dở của AI trước; biết chủ dự án vừa đổi gì. Không có khóa kỹ thuật thật — mọi thứ dựa trên quy ước dưới đây, nên phải tuân thủ nghiêm.

## 1. File nào dùng để làm gì
| File | Ai ghi | Nội dung | Cách ghi |
|---|---|---|---|
| `AGENTS.md` | chủ dự án/AI (hiếm) | quy tắc cứng + thứ tự đọc | sửa khi quy tắc đổi |
| `.ai/CURRENT.md` | AI cuối cùng làm việc | trạng thái tổng, khóa scene, cảnh báo — ≤ 30 dòng | GHI ĐÈ |
| `.ai/TASKS.md` | AI nhận/hoàn thành task | bảng task, trạng thái, ai giữ, bằng chứng | SỬA ĐÚNG DÒNG của task |
| `.ai/LOG.md` | MỌI AI | nhật ký sự kiện, mới nhất ở CUỐI file | CHỈ THÊM, không sửa/xóa mục cũ |
| `.ai/OWNER_UPDATES.md` | chủ dự án (hoặc AI ghi hộ) | thay đổi do chủ dự án tự làm ngoài AI | CHỈ THÊM |
| `.ai/PLANS/`, `DECISIONS/`, `BUGS/`, `MAPS/` | AI | kế hoạch, quyết định + lý do, lỗi + nguyên nhân gốc, bản đồ | file riêng theo chủ đề |
| `PROJECT_STATE.md` | chủ dự án + AI | trạng thái triển khai chi tiết (nguồn dài, đọc khi cần) | theo quy ước của file đó |
| `.ai/MISSIONS_TODO.md` | AI | nhiệm vụ gameplay chưa thiết kế | sửa bảng |

Nguyên tắc nguồn sự thật: code + Unity thực tế > LOG/OWNER_UPDATES > TASKS/CURRENT > PLANS/MAPS. Khi mâu thuẫn, tin cái ở trái và sửa cái ở phải.

## 2. Bắt đầu phiên (≈ 5 file, không quét dự án)
**CẬP NHẬT 2026-10-21 (giảm token): đọc `.ai/START_HERE.md` + CURRENT + 8 mục LOG cuối; bỏ qua danh sách dưới trừ khi cần. TASKS đầy đủ chỉ mở khi sửa dòng task; DONE ở TASKS_DONE.md; LOG cũ ở SESSIONS/.**
1. `AGENTS.md`  2. `.ai/CURRENT.md`  3. `.ai/LOG.md` — 15 mục cuối  4. `.ai/OWNER_UPDATES.md` — các mục mới hơn mục LOG cuối cùng  5. `.ai/TASKS.md` — dòng task của bạn (hoặc task chủ dự án giao).
Rồi làm PREFLIGHT (mục 3). Chỉ sau đó mới đọc ngữ cảnh WARM mà brief task chỉ ra. Không đọc `Design/`, `Backups/`, Git history, ThirdParty nếu task không cần.

## 3. PREFLIGHT trước khi sửa bất cứ thứ gì
a. Task đã có người giữ? Xem trạng thái ở TASKS + mục LOG gần nhất của task đó.
b. Việc của bạn có thể đã được làm: kiểm bằng CODE/Unity thực tế (không tin mô tả). Nếu đã xong -> ghi LOG `ALREADY-DONE`, cập nhật TASKS, dừng, không làm trùng.
c. Phát hiện thay đổi chưa ghi: chạy `.ai/tools/changed_since.snippet.cs` (đổi mốc `since` = thời điểm UTC của mục LOG cuối mà bạn đã đọc). Có terminal thì thêm `git status --short` và `git log --oneline -10`. File bạn sắp sửa có mtime mới hơn mục LOG nhắc tới nó = ai đó (chủ dự án hoặc AI khác) đã đổi mà chưa ghi: đọc nội dung hiện tại, ghi LOG `UNLOGGED-CHANGE` (file, giờ, nghi ngờ do ai), KHÔNG ghi đè.
d. Kiểm tra Console không có lỗi compile; Unity không ở Play Mode.
e. Khóa scene: CURRENT.md có dòng `Scene lock:`. Nếu AI khác đang giữ -> không sửa `.unity`/`.prefab`; chọn việc khác hoặc hỏi chủ dự án.

## 4. Vòng đời task (trạng thái trong TASKS.md)
`TODO` -> `CLAIMED` -> `IN-PROGRESS` -> `REVIEW` (xong, chờ chủ dự án xem) -> `DONE`; nhánh: `PAUSED` (dừng giữa chừng có chủ ý), `BLOCKED` (cần thông tin/quyết định/quyền), `STALE` (bị bỏ dở).
- Nhận task: sửa dòng task (trạng thái, tên AI/model, giờ UTC) và ghi LOG `START` TRƯỚC khi sửa file đầu tiên, liệt kê file định đụng.
- Đang làm: sau mỗi nhóm thay đổi có ý nghĩa, ghi LOG `PROGRESS` ngắn (đã làm gì, mức xác minh, file). Nếu phiên bị ngắt đột ngột, mục START/PROGRESS cuối cùng là dấu vết cho người sau.
- Một task không có mục LOG nào trong 24 giờ mà đang `CLAIMED/IN-PROGRESS` = ứng viên `STALE`; người sau được tiếp quản sau khi làm mục 6.

## 5. Kết thúc hoặc tạm dừng (bắt buộc, kể cả khi chưa xong)
1. Sửa dòng task trong TASKS.md: trạng thái mới + bằng chứng + bước kế tiếp.
2. Thêm mục LOG `DONE` hoặc `PAUSED` hoặc `BLOCKED` (mẫu ở LOG.md): đã làm gì, file đã đụng, mức xác minh (1 tạo file / 2 compile / 3 test chạy / 4 runtime xác nhận / 5 chưa xác nhận), việc dở, bước kế tiếp CỤ THỂ, câu hỏi cho chủ dự án.
3. Cập nhật CURRENT.md nếu trọng tâm hoặc cảnh báo đổi; nhả `Scene lock`.
4. Có tri thức lâu dài -> `DECISIONS/` (quyết định + lý do) hoặc `BUGS/` (lỗi + nguyên nhân gốc + cách sửa). Đổi cấu trúc/hệ thống -> sửa `ARCHITECTURE.md`/`SYSTEMS.md` và bản đồ liên quan; chạy lại index nếu thêm/xóa/đổi tên script (`.ai/tools/rebuild_index.snippet.cs`).
5. Báo cáo cuối trong chat cho chủ dự án (mục 9). Không tạo báo cáo dài cho từng thao tác nhỏ.

## 6. Tiếp quản việc dở của AI khác
1. Đọc các mục LOG của task đó + dòng TASKS. 2. Đối chiếu thực tế: `changed_since`, `git diff`/`git status` nếu có, Console, mở đúng các file được liệt kê. 3. Xác định phần ĐÃ xong và đã xác minh (giữ nguyên) với phần dở/hỏng. 4. Ghi LOG `TAKEOVER` (từ ai, trạng thái thật, kế hoạch). 5. Không làm lại phần đã xong đã xác minh; không xóa/hoàn tác việc của AI trước nếu chưa chắc — hỏi chủ dự án khi có nguy cơ mất dữ liệu.
Ngược lại, nếu BẠN bị ngắt: lần vào lại, đọc LOG xem có `TAKEOVER`/mục của AI khác sau mục cuối của bạn không; nếu có, việc của bạn có thể đã được làm — làm preflight b trước khi tiếp tục.

## 7. Chủ dự án thay đổi dự án
- Chủ dự án ghi vào `.ai/OWNER_UPDATES.md` (mẫu 3 dòng ở đầu file). Cách nhanh nhất: nói với bất kỳ AI nào «ghi vào OWNER_UPDATES: ...» — AI PHẢI ghi vào file đó, không chỉ nhớ.
- AI vào phiên: đọc các mục OWNER_UPDATES mới; với mỗi mục, kiểm tra file/khu vực bị ảnh hưởng và cập nhật các file `.ai/` nào đã lỗi thời (ARCHITECTURE, SYSTEMS, MAPS, PLANS, TASKS). Ghi LOG `OWNER-CHANGE-PROCESSED`.
- Thay đổi chủ dự án làm mà chưa báo (phát hiện qua changed_since/git) -> LOG `UNLOGGED-CHANGE`, không ghi đè; nếu ảnh hưởng task của bạn, hỏi một câu ngắn.

## 8. Quản lý ngữ cảnh / token
HOT = CURRENT + task đang làm + file đang sửa. WARM = brief task, ARCHITECTURE/SYSTEMS phần liên quan, README feature. COLD = còn lại. Mỗi task trong TASKS.md có cột `Đọc` liệt kê tối đa vài file/thư mục cần đọc. Quy tắc: tìm symbol/đường dẫn bằng `.ai/MAPS/index-scripts.md` trước khi mở file; không đọc lại file chưa đổi; gom nhiều thao tác đọc thành một lần; script cho việc xác định được; giới hạn kích thước output công cụ; LOG chỉ ghi kết quả, không dán nhật ký dài.

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
6. Kế hoạch là tài liệu sống: hiểu thêm thì sửa, ghi lý do. Xong task: hoàn thiện mục 5 (bản đồ lỗi), cập nhật thẻ hệ thống (mục 13), TASKS + LOG `DONE`.
Ước lượng (CHƯA đo): đọc khởi động ≈ AGENTS + CURRENT + 15 mục LOG + 1 dòng task + 1 thẻ + 1 kế hoạch, vài nghìn token; chi phí thực tế phải đo khi có số.

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
- Thẻ hệ thống mới chỉ có phần A (tự sinh) cho hầu hết hệ thống; bảng «Triệu chứng -> nơi xem» chỉ có nội dung cho SchoolFloor và QuietCorridor (từ lỗi đã gặp). Hiệu quả tiết kiệm token phụ thuộc vào việc AI ghi lại sau mỗi lần sửa lỗi.
- Không có khóa thật: hai AI chạy đồng thời vẫn có thể giẫm chân; chủ dự án nên chỉ định phạm vi file/scene cho từng AI chạy song song. Quy ước `Scene lock` chỉ có hiệu lực khi mọi AI tuân thủ.
- Mốc thời gian dùng UTC (giờ file trên đĩa cũng là UTC khi đọc bằng changed_since).
- AI không có terminal/Git chỉ dựa được vào changed_since và LOG; thay đổi chủ dự án làm mà không ghi sẽ chỉ phát hiện qua mtime, không có nội dung diff.
