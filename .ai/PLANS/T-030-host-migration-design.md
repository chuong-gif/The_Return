> ĐÃ HỦY 2026-10-31: chủ dự án bỏ chuyển host (host off thì cả phòng rời). KHÔNG thực hiện tài liệu này; giữ để tham khảo. Xem DECISIONS/2026-10-31-t030-no-host-migration.md.

# T-030 bước 1: rớt mạng, vào lại, chuyển host — TÀI LIỆU THIẾT KẾ   (file: .ai/PLANS/T-030-host-migration-design.md)

Trạng thái: ĐỀ XUẤT, CHƯA CODE, CHỜ CHỦ DỰ ÁN DUYỆT HƯỚNG (mục 8) | Tác giả: Claude Sonnet 5.5 (phiên #6) | Viết: 2026-10-31 UTC | Phạm vi: chỉ tài liệu; không sửa code/scene/Domain.
Nền: DECISIONS/2026-10-29-t009-owner-answers.md câu 3: chủ dự án chọn CÓ chuyển host; phiếu PLANS/T-030-brief.md; kiến trúc host-authoritative ở SYSTEM_CARDS/NetworkFramework.md.
Nhãn: [ĐÃ TRA] = tra tài liệu Unity ngày 2026-10-31 (nguồn ở mục 9); [ĐỌC CODE] = đọc code trong repo ngày 2026-10-31; [CHƯA XÁC MINH] = phải thử thật (mục 7, bước S0); [ĐỀ XUẤT] = AI đề xuất, chưa duyệt.

## 0. Tóm tắt
- Netcode for GameObjects (NGO) KHÔNG có chuyển host sẵn [ĐÃ TRA]. Cách chính thức của Unity là đổi sang Distributed Authority (không còn máy chủ, quyền chia cho các máy, dịch vụ đám mây). Cách đó giải quyết kết nối nhưng KHÔNG tự chuyển trạng thái nhiệm vụ nằm trong các lớp C# thuần của mình (xem 3.2).
- Đề xuất (C): tự làm trên NGO + Sessions hiện có. Ván được mô tả bằng NHẬT KÝ SỰ KIỆN (hạt giống + lệnh đã được host chấp nhận + sự kiện luồng map) do host ghi và phát cho mọi máy. Khi host mất, một máy được chọn theo quy tắc cố định tạo lại kết nối, nạp nhật ký (phát lại lệnh) và đội chơi tiếp. Không cần sửa Domain nhiệm vụ nếu Domain xác định (cùng đầu vào cùng kết quả, cần kiểm).
- Việc quyết định hướng đi nằm ở một thử nghiệm ngắn (S0, mục 7): khi host chết, phòng (session) và Relay còn sống thế nào. Nên làm S0 trước khi viết bất kỳ code sản phẩm nào.
- Công sức ước lượng (chưa đo): 9-16 phiên AI + vài buổi thử thật 2-3 máy của chủ dự án. Làm SAU T-028 (luồng map qua mạng) cho phần map; phần khung có thể làm trước.
- Cái sẽ MẤT khi chuyển host (mục 5): đồng hồ bước đang chạy, lệnh đang bay, vài giây đến vài chục giây gián đoạn (ước lượng, chưa đo), và nếu đội chỉ còn 1 người thì phải chờ người kia vào lại.

## 1. Sự thật đã tra [ĐÃ TRA]
1. NGO: không có chuyển host; bản thay thế là Distributed Authority (NGO 2.0+). Trong DA, đối tượng thuộc SessionOwner tự chuyển cho chủ phiên mới khi chủ phiên đổi. Cần Unity Cloud, gói Multiplayer Services; docs hiện có gói miễn phí giới hạn băng thông/giờ kết nối, trang cũ từng nói sẽ tính phí khi ra bản chính thức: PHẢI xem lại trang giá trước khi chọn.
2. Multiplayer Services (Sessions): (a) bầu host: đổi Host của session rồi SavePropertiesAsync; KHÔNG chuyển dữ liệu mạng; (b) chuyển dữ liệu: WithHostMigration + IMigrationDataHandler (hai hàm Generate/Apply trả/nhận byte[]), mặc định chụp mỗi 3 giây, hạn tải lên 5 giây, lưu qua Lobby, tự gọi khi phát hiện chuyển; bản cài sẵn chỉ cho Netcode for Entities; với NGO docs bảo dùng Distributed Authority. Dùng IMigrationDataHandler tự viết với NGO có chạy được hay không: CHƯA XÁC MINH.
3. Relay: allocation và join code chỉ sống khi host còn kết nối; host mất hoặc timeout thì cả hai hết hiệu lực; không cách nào hủy chủ động; Relay không chuyển host, docs gợi ý đưa người còn lại vào Lobby và chọn host mới. Host mới phải tạo allocation + mã mới; chọn vùng tự động có thể thêm tới ~500 ms; sau chuyển phải lặp lại toàn bộ chuỗi tạo/vào.
4. Lobby/Sessions: không biết từ docs tra được khi host chết đột ngột thì phòng còn bao lâu và có tự đổi host không: CHƯA XÁC MINH (S0).

## 2. Hiện trạng trong repo [ĐỌC CODE]
- NetworkSessionService dùng Sessions API (CreateSessionAsync + WithRelayNetwork, WSS mặc định, T-023). Mã phòng hiển thị là mã SESSION, không phải join code Relay.
- RoomService.OnClientDisconnect: khi client mất host chỉ báo «Mất kết nối với chủ phòng» rồi LeaveSession. Chưa có vào lại, chưa có chuyển host.
- SlotAllocator: ô theo mã client NGO (ulong); người rời trả ô, ô khác không dồn. Hệ quả: người vào lại nhận ô nhỏ nhất còn trống, KHÔNG đảm bảo ô cũ (nếu hai người rớt cùng lúc có thể đổi nhân vật cho nhau).
- HostAuthority/ClientReplica (T-024): đã có ConnectSlot/DisconnectSlot, BuildStateFor + ClientReplica.Reset cho vào lại; test EndToEnd_DisconnectedPlayerMissesUpdates_ReconnectCatchesUp. Lệnh có số thứ tự, Version tăng mỗi lệnh chấp nhận, HostLog chỉ để chẩn đoán (không đủ để phát lại).
- IHostRules (Validate/Apply/BuildPublicState/BuildPrivateState): KHÔNG có hàm lưu/nạp trạng thái và KHÔNG nhận thời gian; đồng hồ nằm trong Domain/đối tượng luật và chỉ báo ra ngoài bằng MarkChanged.
- Chưa có: luồng map qua mạng (T-028), nhiệm vụ nào chạy qua HostAuthority ngoài mẫu (T-027 đang làm PlankAndKey), nên thiết kế này bám giao diện hiện tại và chỉ đề xuất bổ sung nhỏ.

## 3. Kịch bản cần xử lý và hành vi mong muốn [ĐỀ XUẤT]
| # | Kịch bản | Hành vi mong muốn | Hiện tại |
|---|---|---|---|
| S1 | Người thường rớt mạng ngắn (dưới ~10 giây) | Tự nối lại, nhận lại đúng ô và trạng thái; người khác không bị gián đoạn | Bị đá ra phòng, không vào lại |
| S2 | Người thường rớt lâu / thoát | Ô được giữ; nhiệm vụ đang chạy tạm dừng; chờ vào lại | SlotAllocator.Release trả ô ngay; chưa có luật tạm dừng nhiệm vụ khi thiếu người [ĐỌC CODE; hành vi thật chưa thử] |
| S3 | Host rớt mạng / treo máy | Phát hiện, bầu host mới, đội chơi tiếp từ trạng thái gần nhất | Mọi client văng ra |
| S4 | Host thoát có chủ ý | Như S3 nhưng nhanh hơn (host báo trước) | Như S3 |
| S5 | Host cùng người khác rớt | Người còn lại làm host nếu đủ điều kiện; chờ những người kia vào lại | Như S3 |
| S6 | Tất cả rớt | Phòng kết thúc; ván không lưu (xem Q5) | Như S3 |

## 4. Các phương án
| Phương án | Ý chính | Công sức | Rủi ro chính | Ghi chú |
|---|---|---|---|---|
| A. Không chuyển host | Host mất thì ván hết | không | không đáp ứng yêu cầu | Chủ dự án đã chọn CÓ chuyển host: loại |
| B. Distributed Authority | Đổi topology, quyền chia cho các máy | Lớn: đổi kiến trúc (T-024/T-025/T-027 dựng cho host-authoritative) | Dịch vụ đám mây + giá chưa rõ; trạng thái Domain (C# thuần) vẫn phải đưa vào NetworkVariable/đối tượng mạng mới chuyển được; luật thông tin riêng khó giữ khi không có máy chủ | KHÔNG khuyến nghị lúc này |
| C. Tự làm trên NGO + Sessions, nhật ký sự kiện | Mục 4.1-4.5 | Vừa-lớn (9-16 phiên) | Phụ thuộc kết quả S0; Domain phải xác định | KHUYẾN NGHỊ |
| C'. Dùng WithHostMigration + IMigrationDataHandler tự viết | SDK lo chụp/nạp/relay, ta cung cấp byte[] (chính là nhật ký của C) | Nhỏ hơn C nếu chạy được với NGO | Docs không hỗ trợ NGO; có thể tồn tại lỗi/hạn chế | Thử ở S0; nếu chạy được thì thay phần 4.4 |

### 4.1 Nhật ký sự kiện
- Host ghi theo thứ tự: tiêu đề (số người N, hạt giống ván, hạt giống từng nhiệm vụ), sau đó mỗi sự kiện: (a) LỆNH ĐÃ CHẤP NHẬN: ô, số thứ tự, lệnh tuần tự hóa, thời điểm TƯƠNG ĐỐI so với lúc nhiệm vụ bắt đầu; (b) sự kiện đồng hồ làm đổi trạng thái (MarkChanged) kèm thời điểm; (c) sự kiện luồng map: vào/thoát nhiệm vụ, hoàn thành, mở/đóng cổng.
- Dùng thời gian tương đối vì đồng hồ các máy khác nhau.
- Phát gia tăng cho mọi máy cùng gói trạng thái; mỗi máy giữ bản sao nhật ký và Version cuối. Kích thước ước tính vài trăm byte đến vài KB mỗi ván (CHƯA đo).
- Bổ sung đề xuất cho khung: HostAuthority ghi một JournalEntry cho mỗi lệnh chấp nhận (hiện chỉ ghi HostLog); lớp luật mỗi nhiệm vụ nhận thêm tham số thời gian hoặc có cách phát lại có thời gian. Chưa làm.
### 4.2 Nạp lại (host mới)
- Dựng lại HostAuthority + luật bằng hạt giống rồi phát lại sự kiện theo thứ tự. Cần mỗi Domain xác định (cùng đầu vào cùng kết quả, không đọc Random/Time bên trong): Generate đã được thiết kế như vậy; Apply từng nhiệm vụ phải kiểm lại (phần việc chung với T-031).
- Kiểm khớp: mỗi client gửi Version và băm trạng thái chung của bản cuối nó nhận; host mới so với kết quả phát lại. Lệch thì dùng bản có Version cao nhất của người còn lại, nếu vẫn không khớp thì làm lại nhiệm vụ đó từ đầu (Q3).
### 4.3 Bầu host
- Quy tắc cố định, không cần bỏ phiếu: ô nhỏ nhất còn kết nối (trừ host cũ). Mọi máy tự tính cùng kết quả. Công bố bằng thuộc tính session kèm số thế hệ (generation) để chặn hai máy cùng tự nhận (split-brain). Nếu S0 cho thấy Sessions tự bầu host khi host mất thì dùng kết quả đó thay vì tự bầu.
### 4.4 Dựng lại mạng [phụ thuộc S0]
- Trường hợp (a): session còn sống sau khi host mất -> đổi Host của session, máy mới tạo lại mạng Relay trên CÙNG mã phòng; người khác vào lại bằng mã cũ.
- Trường hợp (b): session chết -> máy mới tạo session mới (mã mới); người còn lại nhận mã mới qua một kênh phụ (thuộc tính Lobby, hoặc kênh Vivox T-029 nếu còn sống, hoặc nhắn tay); chọn ở S0.
- Cả hai trường hợp: tắt NGO cũ, dựng lại NetworkManager trong cùng tiến trình (cần kiểm đối tượng giữ lại giữa chừng: RoomRoster, các NetworkObject đã spawn, đối tượng DontDestroyOnLoad), tạo allocation mới, nạp nhật ký, mở lại phát trạng thái.
### 4.5 Trong lúc chuyển host
- Màn hình báo «Mất kết nối với chủ phòng, đang chuyển host...» kèm đếm giây; nhiệm vụ đóng băng, đồng hồ nhiệm vụ tạm dừng; nhân vật vẫn đi lại cục bộ (vị trí do chính máy sở hữu nên không phụ thuộc host) [ĐỀ XUẤT].
### 4.6 Giữ ô theo người, vào lại
- Mở rộng SlotAllocator: gắn ô với PlayerId (Authentication) thay vì chỉ mã client; khi rời không trả ô ngay mà bảo lưu trong thời gian chờ (Q2). Vào lại: nếu Sessions hỗ trợ ReconnectToSession thì dùng (CHƯA XÁC MINH), sau đó host gọi ConnectSlot + BuildStateFor, client gọi ClientReplica.Reset (đã có và có test ở mức khung).
- Giọng nói: Vivox tách khỏi NGO; kiểm ở S0 xem kênh còn khi chuyển, nếu gắn mã session thì vào lại kênh mới.

## 5. Cái sẽ MẤT hoặc BỊ HẠN CHẾ khi chuyển host
- Gián đoạn: ước vài giây đến vài chục giây (phát hiện mất host + tạo allocation mới + vào lại); CHƯA đo, S0 sẽ đo.
- Đồng hồ bước đang chạy (Choir cửa sổ giữa hai nốt, TeacherSays đoạn đang đếm, TeacherTruth cửa sổ xác nhận, SharedLantern đếm 5 giây, QuietCorridor ồn): đề xuất giữ giá trị trạng thái nhưng KHỞI ĐỘNG LẠI hạn của bước đang dang dở, và cho thêm vài giây ân hạn.
- Lệnh đã gửi nhưng host cũ chưa chấp nhận/chưa phát đi: mất; người chơi phải làm lại thao tác.
- Vị trí nhân vật: giữ (mỗi máy tự sở hữu vị trí của mình).
- Đội chỉ còn 1 người (đội 2 người, host rớt): nhiệm vụ cần cả đội nên chờ người rớt vào lại; hết thời gian chờ thì chủ phòng chọn kết thúc ván (Q2).
- Thông tin riêng: người nắm nhật ký + hạt giống có thể (về mặt kỹ thuật) tự tính ra thông tin riêng của người khác. Đề xuất chấp nhận vì là game bạn bè (Q4); nếu không chấp nhận, nhật ký phải mã hóa và việc chuyển host phức tạp hơn nhiều.

## 6. Rủi ro
| # | Rủi ro | Mức | Cách giảm |
|---|---|---|---|
| R1 | Sessions không cho đổi host / tạo lại Relay cho NGO sau khi host chết | Cao | S0 trước mọi thứ; chuẩn bị đường (b) mã mới |
| R2 | Domain không xác định -> phát lại ra trạng thái khác | Trung bình | Test phát lại cho từng nhiệm vụ (gộp vào T-031); lệch thì làm lại nhiệm vụ |
| R3 | Hai máy cùng tự nhận làm host (split-brain) | Trung bình | Số thế hệ trong thuộc tính session; chỉ ô nhỏ nhất được phép thử trước |
| R4 | Phòng (Lobby) hết hạn khi không có heartbeat từ host đã chết | Trung bình | S0 đo; đường (b) |
| R5 | Dựng lại NetworkManager trong cùng tiến trình để lại đối tượng cũ | Trung bình | S0 + dọn đối tượng mạng có kiểm soát |
| R6 | Chênh lệch giờ giữa các máy | Thấp | Dùng thời gian tương đối |
| R7 | Relay vùng khác -> trễ | Thấp | Giữ cùng vùng (docs có tham số bảo lưu vùng khi chuyển) |
| R8 | Chi phí/hạn mức dịch vụ (Lobby, Relay) khi thử nhiều | Thấp | Theo dõi dashboard |
| R9 | Trải nghiệm: người chơi tưởng treo | Thấp | Màn hình thông báo + đếm giây |

## 7. Kế hoạch bước (chưa làm gì cả; chỉ sau khi chủ dự án duyệt mục 8)
| Bước | Việc | Cỡ ước lượng | Ghi chú |
|---|---|---|---|
| S0 | Thử nghiệm nhỏ (scene thử riêng trong Features/Network, không động code sản phẩm): U1 kill host, xem session/Relay/Lobby còn không, Sessions có tự đổi host không; U2 đổi Host + tạo lại Relay trên cùng session; U3 thử WithHostMigration + IMigrationDataHandler với NGO; U4 Vivox còn kênh không; đo thời gian | 1-2 phiên AI + 1 buổi chủ dự án thử 2 máy (LAN rồi Internet) | Quyết định chọn C hay C' và đường (a)/(b) |
| S1 | Khung: nhật ký lệnh trong HostAuthority + phát lại + test EditMode mô phỏng host chết | 1-2 phiên | Làm được ngay sau duyệt, không cần S0 |
| S2 | Kiểm tính xác định từng nhiệm vụ + ghi sự kiện đồng hồ/luồng map | 2-3 phiên | Chung việc với T-031/T-028 |
| S3 | Phát hiện mất host + bầu host + dựng lại mạng + nạp nhật ký | 2-4 phiên | Phần bất định nhất |
| S4 | Giữ ô theo PlayerId, vào lại tự động (S1/S2 mục 3) | 1-2 phiên | |
| S5 | Giao diện thông báo, đếm giây, nút vào lại | 1 phiên | |
| S6 | Thử thật: rút mạng host giữa chừng, 2 máy còn lại chơi tiếp (mức 4) | 1-2 phiên + chủ dự án | Cách kiểm xong theo phiếu |

## 8. Câu hỏi cho chủ dự án (mặc định đề xuất nếu không đổi ý)
1. Duyệt hướng C (tự làm, nhật ký sự kiện) và cho làm S0 trước? Mặc định: CÓ. (S0 cần chủ dự án chạy 2 bản game hoặc 2 Editor để thử rút mạng.)
2. Khi một người rớt mạng (không phải host): ô được giữ và nhiệm vụ tạm dừng bao lâu trước khi chủ phòng chọn «tiếp tục chờ» hoặc «kết thúc ván»? Mặc định: 5 phút.
3. Nhiệm vụ đang dở lúc chuyển host: tiếp tục từ bước đã ghi (mặc định) hay luôn làm lại nhiệm vụ đó từ đầu (dễ và chắc hơn)?
4. Chấp nhận việc người rành kỹ thuật có thể xem thông tin riêng của người khác qua nhật ký/hạt giống (game bạn bè)? Mặc định: chấp nhận.
5. Có cần LƯU tiến độ để chơi tiếp sau khi tất cả thoát? Hiện không có; mặc định: không (ngoài phạm vi T-030).
6. Người rớt mạng vào lại bằng cách nào? Mặc định: game tự thử lại 60 giây, sau đó hiện nút «Vào lại» và ô nhập mã (mã có thể đã đổi nếu host chuyển).
7. Host mới là ô nhỏ nhất còn kết nối (mặc định) hay máy có ping tốt nhất (phức tạp hơn, đo thêm)?

## 9. Nguồn (tra ngày 2026-10-31)
- docs.unity.com/en-us/mps-sdk/session-host-migration (Migrate session host: host election, WithHostMigration, IMigrationDataHandler, mặc định 3 s/5 s, lưu qua Lobby, chỉ cài sẵn cho Netcode for Entities, NGO dùng Distributed Authority).
- docs.unity.com/relay/join-codes và docs.unity.com/relay/disconnection (allocation/mã hết hiệu lực khi host mất; Relay không chuyển host).
- docs.unity3d.com Netcode for Entities host-migration/lobby-relay-integration (Relay: tạo lại allocation + mã mới sau chuyển, QoS thêm ~500 ms).
- docs.unity3d.com NGO 2.x ownership + distributed authority quickstart/WebGL (SessionOwner tự chuyển; gói miễn phí hiện tại).
- discussions.unity.com (Unity staff): NGO không có chuyển host, tương đương là Distributed Authority.
- Repo: NetworkSessionService.cs, RoomService.cs, RoomRoster.cs, Framework/SlotAllocator.cs, HostAuthority.cs, IHostRules.cs, SYSTEM_CARDS/NetworkFramework.md, PLANS/T-009 mục 8.
- Chưa làm trong phiên này: không chạy thử mạng thật, không đo thời gian, không kiểm tính xác định của Domain.

## 10. Tiến độ
- 2026-10-31: bản đề xuất viết xong; chờ chủ dự án trả lời mục 8 (nhất là câu 1).
