# T-006d2 - Hợp xướng (Phong_6): Presentation + scene

## Đã làm
- Data: ChoirSettings (+ Choir_VI.asset): stepWindowSeconds, switchExtraSeconds, fixedSeed, chữ hiển thị; preset Dev (5s/+1.5s) và Online (2s/+0).
- Presentation: ChoirPad (số 1-4, đèn), ChoirPrototype (E bấm bục, R làm lại, phím 1-4 đổi vai + ExtendForSwitch, HUD thẻ nhạc vai đang chọn, bảng nhạc 6 ký hiệu A-D xoay về camera).
- Adapter: ChoirMissionAdapter (Maps/SchoolFloor/Runtime/Missions); đăng ký ở MissionRegistryFactory (ChoirArea=Phong_6) qua SchoolFloorFlow.choir.
- Scene: Choir_LogicOnly trong Phong_6, 4 bục cách nhau 3,2 m quanh (46,11.2,121.5).

## Chưa làm
- Kiểm Play Mode (mức 4); test cho ChoirPrototype/adapter; ký hiệu đẹp thay chữ A-D; đồng bộ mạng.

## Snapshot
- Scenes/BeforeChoir_20261017_023150.unity
