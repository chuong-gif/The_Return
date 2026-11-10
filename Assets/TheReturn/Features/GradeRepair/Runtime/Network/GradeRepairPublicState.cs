/*
 * Mục đích: Trạng thái CHUNG (cả đội cùng thấy) và thông tin RIÊNG (chỉ một người) của «Sửa bảng điểm» online (T-036).
 * Chung: điểm 4 hồ sơ, phiếu nào đã dùng, tài liệu nào đã được đọc, đã thắng chưa, kết quả thao tác gần nhất. KHÔNG chứa seed, mã hồ sơ, đáp án.
 * Riêng: SỰ KIỆN của các tài liệu MÌNH phụ trách và ĐÃ ĐỌC (chuỗi ASCII ngắn như «V1.HS-452»; máy khách tự dựng câu chữ từ catalog,
 * nên không phải gửi tiếng Việt qua mạng và không vượt giới hạn 2000 ký tự). Bất biến; mảng được sao khi tạo và khi đọc. Thuần C#.
 * Hàm: GradeRepairPublicState (constructor, ScoresCopy, Score, Used, Read, Total); GradeRepairPrivateState (constructor, DocumentsCopy, FactsCopy,
 * TryGetFact, OwnedCount); GradeRepairEvent.
 */
using System;

namespace TheReturn.GradeRepairNet
{
    /// <summary>Thao tác gần nhất để HUD hiển thị (mã enum; chữ tiếng Việt do lớp hiển thị chọn).</summary>
    public enum GradeRepairEvent
    {
        None,
        Read,
        Apply,
        Undo,
        Submit,
        Reset
    }

    /// <summary>Thông tin riêng của một ô: sự kiện của các tài liệu mình đã đọc.</summary>
    public readonly struct GradeRepairPrivateState
    {
        /// <summary>Độ dài tối đa của một sự kiện (ký tự ASCII).</summary>
        public const int MaxFactChars = 100;

        readonly int[] documents;
        readonly string[] facts;

        /// <summary>
        /// Nhận danh sách tài liệu (tăng dần, 0-3, không trùng) và sự kiện tương ứng (ASCII 32..126, dài tối đa 100). null coi là rỗng.
        /// Ném lỗi nếu sai hình dạng.
        /// </summary>
        public GradeRepairPrivateState(int[] documentIds, string[] documentFacts)
        {
            int a = documentIds == null ? 0 : documentIds.Length;
            int b = documentFacts == null ? 0 : documentFacts.Length;
            if (a != b) throw new ArgumentException("Số tài liệu và số sự kiện phải bằng nhau.");
            if (a > 4) throw new ArgumentException("Tối đa 4 tài liệu.");
            documents = new int[a];
            facts = new string[a];
            for (int i = 0; i < a; i++)
            {
                if (documentIds[i] < 0 || documentIds[i] > 3) throw new ArgumentOutOfRangeException("documentIds");
                if (i > 0 && documentIds[i] <= documentIds[i - 1]) throw new ArgumentException("Tài liệu phải tăng dần và không trùng.");
                if (!IsValidFact(documentFacts[i])) throw new ArgumentException("Sự kiện phải là ASCII 32..126, 1-100 ký tự.");
                documents[i] = documentIds[i];
                facts[i] = documentFacts[i];
            }
        }

        /// <summary>Nhận chuỗi; trả true nếu là sự kiện hợp lệ (không null, 1-100 ký tự, toàn ASCII in được).</summary>
        public static bool IsValidFact(string text)
        {
            if (string.IsNullOrEmpty(text) || text.Length > MaxFactChars) return false;
            for (int i = 0; i < text.Length; i++) if (text[i] < 32 || text[i] > 126) return false;
            return true;
        }

        /// <summary>Số tài liệu đã đọc của ô này.</summary>
        public int OwnedCount { get { return documents == null ? 0 : documents.Length; } }

        /// <summary>Trả bản sao danh sách tài liệu đã đọc.</summary>
        public int[] DocumentsCopy()
        {
            return documents == null ? new int[0] : (int[])documents.Clone();
        }

        /// <summary>Trả bản sao danh sách sự kiện, cùng thứ tự với DocumentsCopy.</summary>
        public string[] FactsCopy()
        {
            return facts == null ? new string[0] : (string[])facts.Clone();
        }

        /// <summary>Nhận tài liệu; trả true và gán sự kiện nếu ô này đã đọc tài liệu đó, ngược lại false (fact = null).</summary>
        public bool TryGetFact(int document, out string fact)
        {
            fact = null;
            if (documents == null) return false;
            for (int i = 0; i < documents.Length; i++)
            {
                if (documents[i] != document) continue;
                fact = facts[i];
                return true;
            }
            return false;
        }
    }

    /// <summary>Trạng thái chung của nhiệm vụ sửa bảng điểm.</summary>
    public sealed class GradeRepairPublicState
    {
        /// <summary>Số hồ sơ trên bảng.</summary>
        public const int Rows = 4;

        /// <summary>Số tài liệu.</summary>
        public const int Documents = 4;

        /// <summary>Số phiếu (hai phiếu bổ sung + một biên bản chuyển điểm).</summary>
        public const int Vouchers = 3;

        /// <summary>Điểm tối đa của một hồ sơ.</summary>
        public const int MaxScore = 10;

        readonly int[] scores;

        /// <summary>Số người của đội (2-4).</summary>
        public int PlayerCount { get; }

        /// <summary>Mặt nạ phiếu đã dùng (bit i = phiếu i).</summary>
        public int UsedMask { get; }

        /// <summary>Mặt nạ tài liệu đã được vai phụ trách đọc (bit i = tài liệu i).</summary>
        public int ReadMask { get; }

        /// <summary>True khi đã nộp bảng đúng (cửa mở).</summary>
        public bool Solved { get; }

        /// <summary>Số lần đã đặt lại bảng.</summary>
        public int ResetCount { get; }

        /// <summary>Thao tác gần nhất.</summary>
        public GradeRepairEvent LastEvent { get; }

        /// <summary>Kết quả Domain của thao tác gần nhất (Ok, OutOfRange, WrongRecord...).</summary>
        public GradeResult LastResult { get; }

        /// <summary>Ô người thao tác gần nhất, -1 nếu chưa có.</summary>
        public int LastActor { get; }

        /// <summary>
        /// Nhận số người (2-4), điểm 4 hồ sơ (0-10), mặt nạ phiếu đã dùng (3 bit), mặt nạ tài liệu đã đọc (4 bit), cờ thắng, số lần đặt lại, thao tác và kết quả
        /// gần nhất, ô thao tác gần nhất. Ném lỗi nếu sai hình dạng (đã thắng mà chưa đọc đủ/chưa dùng đủ phiếu...).
        /// </summary>
        public GradeRepairPublicState(int playerCount, int[] scoreList, int usedMask, int readMask, bool solved, int resetCount,
            GradeRepairEvent lastEvent, GradeResult lastResult, int lastActor)
        {
            if (playerCount < 2 || playerCount > 4) throw new ArgumentOutOfRangeException("playerCount");
            if (scoreList == null || scoreList.Length != Rows) throw new ArgumentException("Cần đúng 4 điểm.", "scoreList");
            for (int i = 0; i < Rows; i++) if (scoreList[i] < 0 || scoreList[i] > MaxScore) throw new ArgumentOutOfRangeException("scoreList");
            if (usedMask < 0 || usedMask >= (1 << Vouchers)) throw new ArgumentOutOfRangeException("usedMask");
            if (readMask < 0 || readMask >= (1 << Documents)) throw new ArgumentOutOfRangeException("readMask");
            if (resetCount < 0) throw new ArgumentOutOfRangeException("resetCount");
            if (lastActor < -1 || lastActor >= playerCount) throw new ArgumentOutOfRangeException("lastActor");
            if (!Enum.IsDefined(typeof(GradeRepairEvent), lastEvent)) throw new ArgumentOutOfRangeException("lastEvent");
            if (!Enum.IsDefined(typeof(GradeResult), lastResult)) throw new ArgumentOutOfRangeException("lastResult");
            if (solved && (usedMask != (1 << Vouchers) - 1 || readMask != (1 << Documents) - 1)) throw new ArgumentException("Đã thắng thì phải đọc đủ và dùng đủ phiếu.", "solved");
            PlayerCount = playerCount;
            scores = (int[])scoreList.Clone();
            UsedMask = usedMask;
            ReadMask = readMask;
            Solved = solved;
            ResetCount = resetCount;
            LastEvent = lastEvent;
            LastResult = lastResult;
            LastActor = lastActor;
        }

        /// <summary>Trả bản sao điểm 4 hồ sơ.</summary>
        public int[] ScoresCopy()
        {
            return (int[])scores.Clone();
        }

        /// <summary>Nhận hồ sơ 0-3; trả điểm, -1 nếu sai chỉ số.</summary>
        public int Score(int row)
        {
            return row >= 0 && row < Rows ? scores[row] : -1;
        }

        /// <summary>Nhận phiếu 0-2; trả true nếu phiếu đã dùng.</summary>
        public bool Used(int voucher)
        {
            return voucher >= 0 && voucher < Vouchers && (UsedMask >> voucher & 1) == 1;
        }

        /// <summary>Nhận tài liệu 0-3; trả true nếu vai phụ trách đã đọc.</summary>
        public bool Read(int document)
        {
            return document >= 0 && document < Documents && (ReadMask >> document & 1) == 1;
        }

        /// <summary>Trả tổng điểm 4 hồ sơ.</summary>
        public int Total()
        {
            return scores[0] + scores[1] + scores[2] + scores[3];
        }
    }
}
