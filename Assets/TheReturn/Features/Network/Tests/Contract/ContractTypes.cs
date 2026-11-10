/*
 * Mục đích: Kiểu nền của bộ test HỢP ĐỒNG mạng dùng chung cho mọi nhiệm vụ online (T-039, M1).
 * Mỗi nhiệm vụ chỉ viết MỘT adapter (kế thừa MissionContractAdapter) + một «ván» (IContractGame); bộ hợp đồng C1-C9
 * (ContractRunner / MissionContractTests) tự chạy cho đội 2/3/4 và nhiều seed. Thuần C#, chỉ phụ thuộc khung mạng.
 * Hàm: ContractFailure (constructor, ToString); IContractGame (Rules, IsSolved, TryNextSolveMove, TryIllegalMove, ForceLoss, Tick);
 * MissionContractAdapter (MissionName, MaxChars, NewGame, Encode/TryDecode cho lệnh-chung-riêng, IsSolvedPublic, SecretsOf);
 * delegate TryDecode.
 */
using System;
using TheReturn.Network.Framework;

namespace TheReturn.Network.Contract
{
    /// <summary>Mẫu hàm giải mã kiểu TryParse: nhận chuỗi, trả true và giá trị nếu hợp lệ.</summary>
    public delegate bool TryDecode<T>(string text, out T value);

    /// <summary>Một lỗi hợp đồng: ca nào (C1..C9), nhiệm vụ nào, đội mấy người, seed nào, thông điệp ngắn. Đủ để tái hiện.</summary>
    public sealed class ContractFailure
    {
        public readonly string Case;
        public readonly string Mission;
        public readonly int TeamSize;
        public readonly int Seed;
        public readonly string Message;

        /// <summary>Nhận mã ca, tên nhiệm vụ, số người, seed và thông điệp.</summary>
        public ContractFailure(string caseCode, string mission, int teamSize, int seed, string message)
        {
            Case = caseCode;
            Mission = mission;
            TeamSize = teamSize;
            Seed = seed;
            Message = message;
        }

        /// <summary>Trả một dòng dạng «[Nhiệm vụ] C4 n=3 seed=7: thông điệp».</summary>
        public override string ToString()
        {
            return string.Format("[{0}] {1} n={2} seed={3}: {4}", Mission, Case, TeamSize, Seed, Message);
        }
    }

    /// <summary>
    /// Một ván do adapter dựng sẵn cho hợp đồng điều khiển: luật host + «bot» tự giải từ đáp án Domain (không đoán mò).
    /// </summary>
    public interface IContractGame<TCmd, TPub, TPriv>
    {
        /// <summary>Luật host của ván (đã có thế giới giả bên trong). Hợp đồng tự bọc HostAuthority quanh nó.</summary>
        IHostRules<TCmd, TPub, TPriv> Rules { get; }

        /// <summary>True khi luật/Domain báo đã thắng (nguồn sự thật, không phải bản sao máy khách).</summary>
        bool IsSolved { get; }

        /// <summary>
        /// Bot: trả nước HỢP LỆ kế tiếp để tiến tới thắng (ô gửi + lệnh) tính từ trạng thái hiện tại; KHÔNG tự áp dụng.
        /// Trả false khi không còn nước nào (đang chờ giờ hoặc đã thắng).
        /// </summary>
        bool TryNextSolveMove(out int slot, out TCmd command);

        /// <summary>Trả một lệnh mà luật CHẮC CHẮN từ chối (sai chủ, sai ô...). Trả false nếu nhiệm vụ không dựng được.</summary>
        bool TryIllegalMove(out int slot, out TCmd command);

        /// <summary>Đẩy ván tới trạng thái THUA/HẾT GIỜ (không qua host). Trả false nếu nhiệm vụ không có thua (ca C7 bỏ qua).</summary>
        bool ForceLoss();

        /// <summary>Cho đồng hồ host chạy thêm deltaSeconds giây. Trả true nếu trạng thái đã đổi (hợp đồng sẽ gọi MarkChanged).</summary>
        bool Tick(float deltaSeconds);
    }

    /// <summary>
    /// Adapter của MỘT nhiệm vụ: dựng ván + khai codec + khai bí mật. Đây là phần DUY NHẤT nhiệm vụ phải viết.
    /// </summary>
    public abstract class MissionContractAdapter<TCmd, TPub, TPriv>
    {
        /// <summary>Tên nhiệm vụ (xuất hiện trong mọi thông điệp lỗi để quy về người giữ nhiệm vụ).</summary>
        public abstract string MissionName { get; }

        /// <summary>Độ dài tối đa cho phép của mọi chuỗi mã hóa (mặc định 2000, khớp Codec hiện có).</summary>
        public virtual int MaxChars { get { return 2000; } }

        /// <summary>Nhận số người (2-4) và seed; dựng ván mới có thế giới giả. Không được trả null.</summary>
        public abstract IContractGame<TCmd, TPub, TPriv> NewGame(int teamSize, int seed);

        /// <summary>Mã hóa lệnh thành chuỗi.</summary>
        public abstract string EncodeCommand(TCmd command);

        /// <summary>Giải mã lệnh; trả false (không ném lỗi) với chuỗi xấu.</summary>
        public abstract bool TryDecodeCommand(string text, out TCmd command);

        /// <summary>Mã hóa trạng thái chung thành chuỗi.</summary>
        public abstract string EncodePublic(TPub state);

        /// <summary>Giải mã trạng thái chung; trả false (không ném lỗi) với chuỗi xấu.</summary>
        public abstract bool TryDecodePublic(string text, out TPub state);

        /// <summary>Mã hóa thông tin riêng thành chuỗi.</summary>
        public abstract string EncodePrivate(TPriv state);

        /// <summary>Giải mã thông tin riêng; trả false (không ném lỗi) với chuỗi xấu.</summary>
        public abstract bool TryDecodePrivate(string text, out TPriv state);

        /// <summary>True nếu trạng thái CHUNG (bản máy khách thấy) báo đã thắng.</summary>
        public abstract bool IsSolvedPublic(TPub state);

        /// <summary>
        /// Khai các chuỗi BÍ MẬT chỉ ô slot được biết (đáp án, mặt nạ, manh mối...). Chọn chuỗi đủ đặc trưng, không phải
        /// đoạn ngắn như «1». Hợp đồng kiểm chúng không lọt vào trạng thái chung hay thông tin riêng của ô khác.
        /// Mặc định rỗng = nhiệm vụ không có thông tin riêng (C5 chỉ kiểm khi có khai).
        /// </summary>
        public virtual string[] SecretsOf(IContractGame<TCmd, TPub, TPriv> game, int slot)
        {
            return new string[0];
        }
    }
}
