# DontLookBack — Không được quay đầu

**Vị trí:** Hanh_Lang_4. Hướng cấm được tính theo forward của root nhiệm vụ, không theo trục world.

| File code | Vai trò |
|---|---|
| Runtime/Domain/DontLookBackState.cs | Ký hiệu thẻ, ghi nhận quan sát, chọn đáp án, kiểm tra khóa và reset. |
| Runtime/Data/DontLookBackCatalog.cs | Nội dung luật, ký hiệu và phản hồi. |
| Runtime/Presentation/DontLookBackPrototype.cs | Theo dõi hướng nhìn, đọc thẻ, checkpoint, bảng khóa, vùng đích và cửa. |

**Dữ liệu:** `Data/DontLookBack_VI.asset`.

**Prefab:** `PF_BackCard.prefab` là thẻ gắn vào nhân vật; `PF_BackLockTerminal.prefab` là bảng nhập khóa. Chúng có thể được controller tham chiếu qua Transform/TextMesh, không nhất thiết có script riêng trên từng root. Vẫn là prefab gameplay, không được xóa vì thấy ít component.

Giữ `badges`, `backLabels`, `terminals`, checkpoints đầu/sau và ba trigger `entryZone`, `checkpointZone`, `exitZone`. Khi xoay/bố trí hành lang, các mốc phải đi cùng root. Model thẻ phải ở sau lưng đúng hướng, không che camera. Hai cửa chuẩn bị ngoài hành lang do AccessController điều khiển; cửa cũ giữ ẩn. Test nhanh nhiệm vụ 4; test riêng việc quay đầu và quay lại checkpoint.
