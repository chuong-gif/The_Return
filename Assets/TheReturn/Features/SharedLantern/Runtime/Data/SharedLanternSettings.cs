/*
 * Mục đích: Asset chỉnh được trong Inspector cho nhiệm vụ Hanh_Lang_4 «Đèn lồng chung» (T-006f): bán kính vùng sáng, tầm trao đèn,
 * số giây được ở ngoài vùng sáng, thời gian phạt khi nhập sai, chế độ phát triển một máy và chữ hiển thị.
 * SharedLanternPrototype đọc lại asset này mỗi lần cần, nên chỉnh được khi đang chơi thử.
 * Thành viên: lightRadius, passRange, outsideSeconds, submitLockSeconds, onlyActivePlayerTimed, fixedSeed, các chuỗi chữ.
 */
using UnityEngine;

namespace TheReturn
{
    [CreateAssetMenu(menuName = "The Return/Shared Lantern Settings", fileName = "SharedLanternSettings")]
    public sealed class SharedLanternSettings : ScriptableObject
    {
        [Header("Ánh sáng và trao đèn")]
        [Tooltip("Bán kính vùng sáng quanh người cầm đèn (mét, tính trên mặt phẳng ngang). Người và biển trong bán kính này mới đọc được.")]
        [Min(0.5f)] public float lightRadius = 4f;
        [Tooltip("Khoảng cách tối đa để trao đèn cho người khác (mét).")]
        [Min(0.5f)] public float passRange = 2f;

        [Header("Phạt")]
        [Tooltip("Số giây tối đa được đứng ngoài vùng sáng; quá thì bị đưa về điểm đầu hành lang (sổ riêng được giữ).")]
        [Min(0.5f)] public float outsideSeconds = 5f;
        [Tooltip("Số giây bảng nhập bị khóa sau mỗi lần nhập sai (đồng thời xóa 4 ô đã nhập).")]
        [Min(0f)] public float submitLockSeconds = 10f;

        [Header("Chế độ phát triển một máy")]
        [Tooltip("Bật: chỉ người đang điều khiển (phím 1-4) bị tính giờ ngoài vùng sáng, vì các vai còn lại đứng yên. Tắt khi chơi online.")]
        public bool onlyActivePlayerTimed = true;

        [Header("Đề")]
        [Tooltip("0 = đề ngẫu nhiên mỗi lần bắt đầu; khác 0 = luôn cùng một đề (để test).")]
        public int fixedSeed;

        [Header("Chữ hiển thị")]
        [TextArea] public string instructions = "Hành lang tối, chỉ có một đèn lồng. Biển trên tường chỉ đọc được khi nằm trong vùng sáng (bấm E). Trao đèn cho người gần bạn bằng F. Cuối hành lang nhập 4 ký hiệu theo thứ tự biển 1-4.";
        public string readOk = "Đã ghi biển vào sổ của bạn.";
        public string readNotInLight = "Biển nằm ngoài vùng sáng, chưa đọc được.";
        public string readAlready = "Bạn đã đọc biển này rồi.";
        public string passed = "Đã trao đèn lồng.";
        public string passTooFar = "Người nhận ở quá xa để trao đèn.";
        public string passNotHolder = "Chỉ người đang cầm đèn mới trao được.";
        public string sentBack = "Ở ngoài vùng sáng quá lâu - bị đưa về điểm đầu hành lang.";
        public string wrongCode = "Sai mã - bảng bị khóa và các ô bị xóa.";
        public string locked = "Bảng đang bị khóa.";
        public string incomplete = "Chưa nhập đủ 4 ký hiệu.";
        public string accepted = "Đúng mã! Cả đội ra cổng cuối hành lang.";
        public string success = "Hành lang hoàn thành!";
    }
}
