/*
 * Mục đích: Sửa bảng điểm bằng chứng từ hữu hạn; hoàn tác trả lại cả điểm và quyền dùng phiếu.
 * Hàm: constructor tạo lượt; Score / Used / Read đọc trạng thái;
 * MarkRead xác nhận đúng người đọc; Apply kiểm tra giới hạn và ghi thao tác;
 * Undo khôi phục thao tác cuối; Reset trả bảng về đầu; Submit đối chiếu chứng từ và tổng điểm.
 */
using System;
using System.Collections.Generic;
namespace TheReturn
{
    public enum GradeResult { Ok, Invalid, AlreadyUsed, OutOfRange, MissingEvidence, MissingVouchers, WrongRecord, Requirements, Solved, NothingToUndo }

    public sealed class GradeRepairState
    {
        public GradeRepairRound Round { get; }
        public bool Solved { get; private set; }
        public int Revision { get; private set; }
        public int UndoCount => history.Count;
        readonly int[] scores;
        readonly bool[] read = new bool[4];
        readonly bool[] used = new bool[3];
        readonly int[] targets = { -1, -1, -1 };
        readonly int[] sources = { -1, -1, -1 };
        readonly Stack<int> history = new Stack<int>();

        /// <summary>Nhận đề không null; sao chép điểm ban đầu để thao tác không sửa dữ liệu đề.</summary>
        public GradeRepairState(GradeRepairRound round)
        {
            Round = round ?? throw new ArgumentNullException(nameof(round));
            scores = (int[])round.InitialScores.Clone();
        }

        /// <summary>Nhận hồ sơ 0–3; trả điểm hiện tại, -1 nếu sai ID.</summary>
        public int Score(int row) => row >= 0 && row < 4 ? scores[row] : -1;

        /// <summary>Nhận phiếu 0–2; trả true nếu phiếu đã được dùng.</summary>
        public bool Used(int voucher) => voucher >= 0 && voucher < 3 && used[voucher];

        /// <summary>Nhận tài liệu 0–3; trả true nếu vai phụ trách đã đọc.</summary>
        public bool Read(int document) => document >= 0 && document < 4 && read[document];

        /// <summary>Nhận tài liệu và vai; chỉ đánh dấu đọc khi đúng người, trả true nếu hợp lệ.</summary>
        public bool MarkRead(int document, int player)
        {
            if (document < 0 || document >= 4 || player < 0 || player >= Round.PlayerCount ||
                Round.OwnerOf(document) != player) return false;
            read[document] = true;
            return true;
        }

        /// <summary>Nhận phiếu, nguồn và đích; áp thao tác trong 0–10, ghi lịch sử; chưa tiết lộ đáp án đúng/sai.</summary>
        public GradeResult Apply(int voucher, int source, int target)
        {
            if (Solved) return GradeResult.Solved;
            if (voucher < 0 || voucher >= 3 || target < 0 || target >= 4) return GradeResult.Invalid;
            if (used[voucher]) return GradeResult.AlreadyUsed;
            if (voucher == 2)
            {
                if (source < 0 || source >= 4 || source == target) return GradeResult.Invalid;
                if (scores[source] < 1 || scores[target] > 9) return GradeResult.OutOfRange;
                scores[source]--;
                scores[target]++;
            }
            else
            {
                if (source != -1) return GradeResult.Invalid;
                if (scores[target] > 8) return GradeResult.OutOfRange;
                scores[target] += 2;
            }
            used[voucher] = true;
            targets[voucher] = target;
            sources[voucher] = source;
            history.Push(voucher);
            Revision++;
            return GradeResult.Ok;
        }

        /// <summary>Không nhận tham số; đảo thao tác cuối và trả phiếu về chưa dùng, trả mã kết quả.</summary>
        public GradeResult Undo()
        {
            if (Solved) return GradeResult.Solved;
            if (history.Count == 0) return GradeResult.NothingToUndo;
            int voucher = history.Pop();
            if (voucher == 2)
            {
                scores[sources[voucher]]++;
                scores[targets[voucher]]--;
            }
            else scores[targets[voucher]] -= 2;
            used[voucher] = false;
            sources[voucher] = targets[voucher] = -1;
            Revision++;
            return GradeResult.Ok;
        }

        /// <summary>Không nhận tham số; reset điểm/phiếu/lịch sử, giữ hồ sơ đã đọc để không xóa thông tin đội có.</summary>
        public void Reset()
        {
            Array.Copy(Round.InitialScores, scores, scores.Length);
            Array.Clear(used, 0, used.Length);
            for (int i = 0; i < 3; i++) sources[i] = targets[i] = -1;
            history.Clear();
            Solved = false;
            Revision++;
        }

        /// <summary>Không nhận tham số; kiểm tra đủ chứng từ, đúng hồ sơ, tổng 32 và từng điểm 6–10 rồi khóa kết quả.</summary>
        public GradeResult Submit()
        {
            if (Solved) return GradeResult.Solved;
            foreach (bool value in read) if (!value) return GradeResult.MissingEvidence;
            foreach (bool value in used) if (!value) return GradeResult.MissingVouchers;
            for (int i = 0; i < 3; i++)
                if (targets[i] != Round.CorrectTarget(i) || sources[i] != Round.CorrectSource(i)) return GradeResult.WrongRecord;
            int total = 0;
            foreach (int value in scores)
            {
                if (value < 6 || value > 10) return GradeResult.Requirements;
                total += value;
            }
            if (total != 32) return GradeResult.Requirements;
            Solved = true;
            Revision++;
            return GradeResult.Solved;
        }
    }
}
