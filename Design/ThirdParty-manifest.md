# Third-party asset manifest

Các package dưới đây được cài cục bộ trong `Assets/ThirdParty` và không được đưa lên kho GitHub công khai. Mã và scene của dự án luôn giữ lớp graybox để gameplay vẫn hoạt động khi chưa có các package này.

| Package | Thư mục cục bộ | Mục đích |
| --- | --- | --- |
| Backrooms Like Asset Pack | `Assets/ThirdParty/BackroomsLikeAsset` | Locker, bàn ghế, đạo cụ và module kiến trúc trường học |
| Fantasy Skybox FREE | `Assets/ThirdParty/Fantasy Skybox FREE` | Skybox đêm cho Lobby và SchoolFloor |
| Fog Particles | `Assets/ThirdParty/Fog Particles` | Sương ở portal và vực trung tâm |
| Yughues Free Concrete Materials | `Assets/ThirdParty/YughuesFreeConcreteMaterials` | Lớp bề mặt bê tông trang trí |

## Thiết lập sau khi clone

1. Nhận từng package bằng Unity ID sở hữu dự án từ Unity Asset Store.
2. Import package vào Unity 6 URP.
3. Di chuyển nguyên thư mục package vào đúng đường dẫn trong bảng, giữ các file `.meta`.
4. Với Yughues, import file `YughuesFreeConcreteMaterials_URP.unitypackage` nằm trong package; không import biến thể HDRP.
5. Chạy `The Return/Lobby/Rebuild Monumental Lobby`.
6. Chạy `The Return/School Floor/Build Monumental Atrium`.
7. Kiểm tra Console và thử luồng Lobby → SchoolFloor.

Các builder kiểm tra asset theo đường dẫn. Nếu package thiếu, builder bỏ qua lớp trang trí ThirdParty và giữ map graybox có thể chơi được.
