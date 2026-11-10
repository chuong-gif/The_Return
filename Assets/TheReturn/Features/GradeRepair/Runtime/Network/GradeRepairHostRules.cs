/*
 * Mục đích: LUẬT HOST của «Sửa bảng điểm» khi chơi online (T-036): bọc GradeRepairRound + GradeRepairState (Domain có sẵn, KHÔNG đổi) theo
 * giao diện IHostRules của khung T-024. Host kiểm QUYỀN của từng lệnh (đúng ô, đúng vai phụ trách tài liệu, đứng gần bàn/bảng bằng IGradeRepairWorld,
 * chưa thắng) rồi để Domain chấm; kết quả Domain (Ok, OutOfRange, WrongRecord...) là PHẢN HỒI chung cho cả đội chứ không phải lệnh bị từ chối.
 * Trạng thái chung (điểm, phiếu, đã đọc) gửi mọi người; sự kiện tài liệu CHỈ gửi RIÊNG vai phụ trách và chỉ sau khi họ đã đọc. Seed, mã hồ sơ,
 * đáp án không bao giờ rời host. Không có đồng hồ nên không cần Tick. Theo mẫu PLANS/T-027-plank-network-pattern.md. Thuần C#, không phụ thuộc Unity.
 * Hàm: constructor (hai dạng); FactsOf; Validate; Apply; BuildPublicState; BuildPrivateState; thuộc tính State, Round, PlayerCount, ResetCount, Solved.
 */
using System;
using TheReturn.Network.Framework;

namespace TheReturn.GradeRepairNet
{
    /// <summary>Luật host của nhiệm vụ sửa bảng điểm.</summary>
    public sealed class GradeRepairHostRules : IHostRules<GradeRepairCommand, GradeRepairPublicState, GradeRepairPrivateState>
    {
        readonly GradeRepairState state;
        readonly IGradeRepairWorld world;
        readonly int supplementVariants;
        readonly int transferVariants;
        int resetCount;
        GradeRepairEvent lastEvent;
        GradeResult lastResult;
        int lastActor = -1;

        /// <summary>Phiên luật bên dưới (chỉ đọc; đổi qua lệnh).</summary>
        public GradeRepairState State { get { return state; } }

        /// <summary>Đề đang chơi (chỉ host/test được đọc; KHÔNG gửi cho máy khách).</summary>
        public GradeRepairRound Round { get { return state.Round; } }

        /// <summary>Số người của đề.</summary>
        public int PlayerCount { get { return state.Round.PlayerCount; } }

        /// <summary>Số lần đã đặt lại bảng.</summary>
        public int ResetCount { get { return resetCount; } }

        /// <summary>True sau khi nộp bảng đúng.</summary>
        public bool Solved { get { return state.Solved; } }

        /// <summary>
        /// Nhận số người (2-4), seed, nguồn vật lý của host, số biến thể câu chữ của phiếu bổ sung và của biên bản chuyển điểm trong catalog
        /// (mặc định 2 và 2). Sinh đề theo seed. Ném lỗi nếu tham số sai.
        /// </summary>
        public GradeRepairHostRules(int playerCount, int seed, IGradeRepairWorld world, int supplementVariants = 2, int transferVariants = 2)
            : this(new GradeRepairRound(playerCount, seed), world, supplementVariants, transferVariants)
        {
        }

        /// <summary>Nhận đề có sẵn (cho test), nguồn vật lý của host và số biến thể câu chữ (>= 1). Ném lỗi nếu tham số sai.</summary>
        public GradeRepairHostRules(GradeRepairRound round, IGradeRepairWorld world, int supplementVariants = 2, int transferVariants = 2)
        {
            if (round == null) throw new ArgumentNullException("round");
            if (world == null) throw new ArgumentNullException("world");
            if (supplementVariants < 1) throw new ArgumentOutOfRangeException("supplementVariants");
            if (transferVariants < 1) throw new ArgumentOutOfRangeException("transferVariants");
            state = new GradeRepairState(round);
            this.world = world;
            this.supplementVariants = supplementVariants;
            this.transferVariants = transferVariants;
        }

        /// <summary>
        /// Nhận đề, tài liệu 0-3 và số biến thể câu chữ; trả SỰ KIỆN của tài liệu dạng ASCII ngắn. Tài liệu 0 (sổ đối chiếu): «C.» + mã 4 hồ sơ A-D nối bằng «_». Chỉ dùng ký tự không cần thoát ký tự (chữ, số, «-», «.», «_»).
        /// Tài liệu 1-2 (phiếu bổ sung): «V&lt;biến thể&gt;.» + mã hồ sơ được cộng. Tài liệu 3 (biên bản chuyển điểm): «T&lt;biến thể&gt;.» + mã nguồn + «_» + mã đích.
        /// Biến thể = seed % số biến thể, đúng công thức của GradeRepairCatalog.DocumentText. Trả chuỗi rỗng nếu sai tài liệu.
        /// </summary>
        public static string FactsOf(GradeRepairRound round, int document, int supplementVariants, int transferVariants)
        {
            if (document == 0) return "C." + string.Join("_", round.Codes);
            int voucher = document - 1;
            if (voucher < 0 || voucher > 2) return string.Empty;
            string target = round.Codes[round.CorrectTarget(voucher)];
            if (voucher == 2)
            {
                int tv = (int)((uint)round.Seed % (uint)Math.Max(1, transferVariants));
                return "T" + tv + "." + round.Codes[round.CorrectSource(voucher)] + "_" + target;
            }
            int sv = (int)((uint)round.Seed % (uint)Math.Max(1, supplementVariants));
            return "V" + sv + "." + target;
        }

        /// <summary>Kiểm quyền của lệnh, không đổi trạng thái. Nhận ô gửi và lệnh; trả chấp nhận hoặc từ chối kèm lý do tiếng Việt.</summary>
        public RuleVerdict Validate(int senderSlot, GradeRepairCommand command)
        {
            if (senderSlot < 0 || senderSlot >= PlayerCount) return RuleVerdict.Reject("Ô này không tham gia nhiệm vụ.");
            if (state.Solved) return RuleVerdict.Reject("Bảng điểm đã được khôi phục, không cần thao tác nữa.");
            switch (command.Kind)
            {
                case GradeRepairCommandKind.Read:
                    return ValidateRead(senderSlot, command.Index);
                case GradeRepairCommandKind.Apply:
                case GradeRepairCommandKind.Undo:
                case GradeRepairCommandKind.Submit:
                    return world.IsNearBoard(senderSlot) ? RuleVerdict.Accept() : RuleVerdict.Reject("Bạn đứng quá xa bảng sửa điểm.");
                case GradeRepairCommandKind.Reset:
                    return RuleVerdict.Accept();
                default:
                    return RuleVerdict.Reject("Lệnh không hợp lệ.");
            }
        }

        RuleVerdict ValidateRead(int slot, int document)
        {
            if (document < 0 || document >= GradeRepairPublicState.Documents) return RuleVerdict.Reject("Tài liệu này không tồn tại.");
            int owner = state.Round.OwnerOf(document);
            if (owner != slot) return RuleVerdict.Reject("Tài liệu này dành cho vai " + (owner + 1) + ". Hãy nhờ đồng đội đọc và đối chiếu.");
            if (state.Read(document)) return RuleVerdict.Reject("Bạn đã đọc tài liệu này rồi.");
            if (!world.IsNearDocument(slot, document)) return RuleVerdict.Reject("Bạn đứng quá xa bàn tài liệu này.");
            return RuleVerdict.Accept();
        }

        /// <summary>Chạy lệnh vừa được Validate chấp nhận (Validate phải được gọi ngay trước, như HostAuthority làm). Nhận ô gửi và lệnh.</summary>
        public void Apply(int senderSlot, GradeRepairCommand command)
        {
            switch (command.Kind)
            {
                case GradeRepairCommandKind.Read:
                    if (!state.MarkRead(command.Index, senderSlot))
                        throw new InvalidOperationException("Domain từ chối lệnh đọc đã được Validate.");
                    lastEvent = GradeRepairEvent.Read;
                    lastResult = GradeResult.Ok;
                    break;
                case GradeRepairCommandKind.Apply:
                    lastEvent = GradeRepairEvent.Apply;
                    lastResult = state.Apply(command.Index, command.Source, command.Target);
                    break;
                case GradeRepairCommandKind.Undo:
                    lastEvent = GradeRepairEvent.Undo;
                    lastResult = state.Undo();
                    break;
                case GradeRepairCommandKind.Submit:
                    lastEvent = GradeRepairEvent.Submit;
                    lastResult = state.Submit();
                    break;
                case GradeRepairCommandKind.Reset:
                    state.Reset();
                    resetCount++;
                    lastEvent = GradeRepairEvent.Reset;
                    lastResult = GradeResult.Ok;
                    break;
            }
            lastActor = senderSlot;
        }

        /// <summary>Chụp trạng thái chung (bản mới, bất biến). Không chứa seed, mã hồ sơ hay đáp án.</summary>
        public GradeRepairPublicState BuildPublicState()
        {
            int[] scores = new int[GradeRepairPublicState.Rows];
            for (int i = 0; i < scores.Length; i++) scores[i] = state.Score(i);
            int used = 0;
            for (int v = 0; v < GradeRepairPublicState.Vouchers; v++) if (state.Used(v)) used |= 1 << v;
            int read = 0;
            for (int d = 0; d < GradeRepairPublicState.Documents; d++) if (state.Read(d)) read |= 1 << d;
            return new GradeRepairPublicState(PlayerCount, scores, used, read, state.Solved, resetCount, lastEvent, lastResult, lastActor);
        }

        /// <summary>
        /// Nhận ô; trả sự kiện của các tài liệu ô đó phụ trách VÀ đã đọc (rỗng nếu ô không tham gia hoặc chưa đọc gì).
        /// Tuyệt đối không chứa tài liệu của người khác, và không chứa tài liệu chưa đọc.
        /// </summary>
        public GradeRepairPrivateState BuildPrivateState(int slot)
        {
            if (slot < 0 || slot >= PlayerCount) return new GradeRepairPrivateState(null, null);
            int[] ids = new int[GradeRepairPublicState.Documents];
            string[] facts = new string[GradeRepairPublicState.Documents];
            int count = 0;
            for (int d = 0; d < GradeRepairPublicState.Documents; d++)
            {
                if (state.Round.OwnerOf(d) != slot || !state.Read(d)) continue;
                ids[count] = d;
                facts[count] = FactsOf(state.Round, d, supplementVariants, transferVariants);
                count++;
            }
            int[] ownedIds = new int[count];
            string[] ownedFacts = new string[count];
            Array.Copy(ids, ownedIds, count);
            Array.Copy(facts, ownedFacts, count);
            return new GradeRepairPrivateState(ownedIds, ownedFacts);
        }
    }
}
