# GradeRepair — Sửa bảng điểm

**Vị trí:** Phong_2. Nội thất theo năm cụm tại root `Archives_SpatialPass01`.

| File code | Vai trò |
|---|---|
| Runtime/Domain/GradeRepairRound.cs | Sinh mã hồ sơ, điểm, chứng từ và quyền đọc theo đội/seed. |
| Runtime/Domain/GradeRepairState.cs | Đánh dấu đã đọc, áp dụng phiếu, chuyển điểm, hoàn tác và nộp. |
| Runtime/Data/GradeRepairCatalog.cs | Mẫu văn bản chứng từ và thông báo kết quả. |
| Runtime/Presentation/GradeRepairPrototype.cs | Xác thực vai/tầm đọc, mở UI, gọi luật, retry và cửa. |
| Runtime/Presentation/GradeRepairTerminal.cs | ID tài liệu hoặc bảng nộp, nhãn hiển thị. |

**Dữ liệu:** `Data/GradeRepair_VI.asset`. Bốn tài liệu: sổ mã, hai phiếu bổ sung, biên bản nhập nhầm. Nội dung dùng các token `{code}`, `{source}`, `{target}`; giữ chúng khi đổi câu chữ.

**Prefab:** `Prefabs/PF_GradeTerminal.prefab`, dùng cho cả tài liệu và bảng nộp. `document=-1` là bảng, `0..3` là tài liệu. Giữ thứ tự mảng `documents`, component, collider và Label. Thay hình ảnh trong `Visual_Replaceable/VisualRoot`; parent giữ collider, renderer cũ tắt. Parent có scale, nên kiểm tra lại tỷ lệ khi nhập model.

Chỉ terminal có logic là prefab nhiệm vụ. Tủ/bàn/ghế xung quanh là trang trí; không tự biến chúng thành tài liệu đọc được. Cửa nhỏ cũ đang ẩn nhưng vẫn là reference runtime; ba lối thật do hệ AccessController khóa. Test nhanh nhiệm vụ 2. Chưa triển khai random vị trí tài liệu hoặc sổ tay mới; đừng nhầm với biến thể text/seed đã có.
