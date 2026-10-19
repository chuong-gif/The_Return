# TeacherSays — Hanh_Lang_5 «Cô nói»

Trạng thái: REVIEW (2026-10-18 15:56 UTC).

## A. Luật
5 đoạn hành lang. Loa đọc lệnh Bước/Đứng; có lệnh thật thì chỉ người nghe được tiền tố «Cô nói» thấy rõ; không có tiền tố thì làm NGƯỢC. Bước: cả đội vào vùng sáng trong 20s (+3s mỗi lần đổi vai). Đứng: cả đội đứng yên 5s (di chuyển sau 1s ân hạn = cảnh cáo). 3 cảnh cáo -> về điểm xuất phát. Chỉnh trong Data/TeacherSays_VI.asset (live tuning). Online preset 8s/0 có sẵn.

## B. Code
Features/TeacherSays/Runtime (Domain: Rules/Round/Generator/State; Data: Settings; Presentation: TeacherSaysPrototype). Adapter: Maps/SchoolFloor/Runtime/Missions/TeacherSaysMissionAdapter.cs, area id Hanh_Lang_5. Test: TeacherSaysTests (18).

## C. Scene
TeacherSays_LogicOnly (SegmentZone_1..5 y=11.5 size (10.5,4,5), StartPoint_1..4, SpeakerBoard). Cổng: nhóm Hanh_Lang_6_Preparation ('Hanh lang 6_Vao/Ra' do nhãn cũ). Snapshot BeforeTeacherSays_20261018_155142.unity.

## D. Còn thiếu
Người chơi thật; thử nhánh cảnh cáo/SentBack/RestartMap bằng chạy thật; thiết kế mô hình 3D thay hộp.
