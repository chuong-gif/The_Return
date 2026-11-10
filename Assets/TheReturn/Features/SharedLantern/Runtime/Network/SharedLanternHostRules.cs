/*
 * Mục đích: LUẬT HOST của «Đèn lồng chung» khi chơi online (T-037): bọc LanternState + LanternGenerator (Domain có sẵn, KHÔNG đổi) theo giao diện
 * IHostRules của khung T-024. Host nhận lệnh trao đèn, đọc biển, nhập/xóa/nộp bảng ký hiệu và làm lại; kiểm điều kiện vật lý bằng ISharedLanternWorld
 * (người cầm đèn, người nhận trong tầm, đứng gần biển, biển trong vùng sáng, đứng gần bảng) rồi chạy luật Domain. Đồng hồ «ngoài vùng sáng quá 5 giây»
 * và việc cả đội tới cổng được host kiểm mỗi Tick. Trạng thái chung (ai cầm đèn, ai ngoài sáng, bảng nhập, tiến độ) gửi mọi người; SỔ ĐỌC BIỂN
 * (ký hiệu từng biển) CHỈ gửi RIÊNG cho người đã tự đọc. Seed và đáp án không bao giờ rời host. Theo mẫu BreakerHostRules / ChoirHostRules.
 * Thuần C#, không phụ thuộc Unity.
 * Hàm: constructor (hai dạng); Validate; Apply; BuildPublicState; BuildPrivateState; Tick; TakeSentBack; thuộc tính State, Round, PlayerCount, ResetCount, Solved.
 */
using System;
using TheReturn.Network.Framework;

namespace TheReturn.LanternNet
{
    /// <summary>Luật host của nhiệm vụ Đèn lồng chung (lệnh SharedLanternCommand, trạng thái chung SharedLanternPublicState, riêng SharedLanternPrivateState).</summary>
    public sealed class SharedLanternHostRules : IHostRules<SharedLanternCommand, SharedLanternPublicState, SharedLanternPrivateState>
    {
        readonly LanternState state;
        readonly ISharedLanternWorld world;
        readonly bool[] lastInLight;
        double clock;
        int resetCount;
        int outsideMask;
        int lastSentBackMask;
        int pendingSentBack;
        SharedLanternMessage lastMessage;
        int lastActor = -1;
        int lastIndex = -1;

        /// <summary>Phiên luật bên dưới (chỉ đọc; đổi qua lệnh và Tick).</summary>
        public LanternState State { get { return state; } }

        /// <summary>Đề đang chơi (chỉ host/test được đọc; KHÔNG gửi cho máy khách).</summary>
        public LanternRound Round { get { return state.Round; } }

        /// <summary>Số người của đề.</summary>
        public int PlayerCount { get { return state.Round.PlayerCount; } }

        /// <summary>Số lần đã làm lại.</summary>
        public int ResetCount { get { return resetCount; } }

        /// <summary>True sau khi nhập đúng bảng và cả đội đã tới cổng.</summary>
        public bool Solved { get { return state.Solved; } }

        /// <summary>
        /// Nhận số người (2-4), seed, nguồn vật lý của host, giây tối đa ngoài vùng sáng (mặc định 5), tầm trao đèn mét (mặc định 2) và giây khóa bảng
        /// sau khi nhập sai (mặc định 10). Sinh đề theo seed. Ném lỗi nếu tham số sai.
        /// </summary>
        public SharedLanternHostRules(int playerCount, int seed, ISharedLanternWorld world, double outsideLimit = 5.0, double passRange = 2.0, double submitLockSeconds = 10.0)
            : this(LanternGenerator.Generate(playerCount, seed), world, outsideLimit, passRange, submitLockSeconds)
        {
        }

        /// <summary>Nhận đề có sẵn (cho test), nguồn vật lý của host và ba thông số luật như trên. Ném lỗi nếu tham số sai.</summary>
        public SharedLanternHostRules(LanternRound round, ISharedLanternWorld world, double outsideLimit = 5.0, double passRange = 2.0, double submitLockSeconds = 10.0)
        {
            if (round == null) throw new ArgumentNullException("round");
            if (world == null) throw new ArgumentNullException("world");
            state = new LanternState(round, outsideLimit, passRange, submitLockSeconds);
            this.world = world;
            lastInLight = new bool[round.PlayerCount];
            for (int i = 0; i < lastInLight.Length; i++) lastInLight[i] = true;
        }

        /// <summary>Kiểm lệnh của một ô, không đổi trạng thái. Nhận ô gửi và lệnh; trả chấp nhận hoặc từ chối kèm lý do tiếng Việt.</summary>
        public RuleVerdict Validate(int senderSlot, SharedLanternCommand command)
        {
            if (command == null) return RuleVerdict.Reject("Lệnh không hợp lệ.");
            if (senderSlot < 0 || senderSlot >= PlayerCount) return RuleVerdict.Reject("Ô này không tham gia nhiệm vụ.");
            if (state.Solved) return RuleVerdict.Reject("Đã hoàn thành hành lang, không cần thao tác thêm.");
            switch (command.Kind)
            {
                case SharedLanternCommandKind.Pass:
                    return ValidatePass(senderSlot, command.Index);
                case SharedLanternCommandKind.Read:
                    return ValidateRead(senderSlot, command.Index);
                case SharedLanternCommandKind.SetEntry:
                    return ValidateSetEntry(senderSlot, command.Index, command.Symbol);
                case SharedLanternCommandKind.ClearEntries:
                    if (state.CodeAccepted) return RuleVerdict.Reject("Bảng ký hiệu đã được chấp nhận.");
                    return world.IsNearPanel(senderSlot) ? RuleVerdict.Accept() : RuleVerdict.Reject("Bạn đứng quá xa bảng ký hiệu.");
                case SharedLanternCommandKind.Submit:
                    return ValidateSubmit(senderSlot);
                case SharedLanternCommandKind.Retry:
                    return RuleVerdict.Accept();
                default:
                    return RuleVerdict.Reject("Lệnh không hợp lệ.");
            }
        }

        /// <summary>Nhận người trao và người nhận; trả chấp nhận nếu người trao đang cầm đèn và người nhận đứng trong tầm (theo vị trí host thấy).</summary>
        RuleVerdict ValidatePass(int from, int to)
        {
            if (to < 0 || to >= PlayerCount || to == from) return RuleVerdict.Reject("Người nhận đèn không hợp lệ.");
            if (state.Holder != from) return RuleVerdict.Reject("Bạn không đang cầm đèn lồng.");
            double distance = world.DistanceBetween(from, to);
            if (!(distance <= state.PassRange)) return RuleVerdict.Reject("Người nhận đứng quá xa để trao đèn.");
            return RuleVerdict.Accept();
        }

        /// <summary>Nhận người đọc và biển; trả chấp nhận nếu biển hợp lệ, chưa đọc, người đọc đứng gần và biển nằm trong vùng sáng.</summary>
        RuleVerdict ValidateRead(int slot, int sign)
        {
            if (sign < 0 || sign >= SharedLanternPublicState.SignCount) return RuleVerdict.Reject("Biển này không tồn tại.");
            if (state.HasRead(slot, sign)) return RuleVerdict.Reject("Bạn đã đọc biển này rồi.");
            if (!world.IsNearSign(slot, sign)) return RuleVerdict.Reject("Bạn đứng quá xa biển này.");
            if (!world.IsSignInLight(sign)) return RuleVerdict.Reject("Biển này đang nằm ngoài vùng sáng, chưa đọc được.");
            return RuleVerdict.Accept();
        }

        /// <summary>Nhận người nhập, ô nhập và ký hiệu; trả chấp nhận nếu bảng chưa đúng, giá trị trong khoảng và người nhập đứng gần bảng.</summary>
        RuleVerdict ValidateSetEntry(int slot, int entry, int symbol)
        {
            if (state.CodeAccepted) return RuleVerdict.Reject("Bảng ký hiệu đã được chấp nhận.");
            if (entry < 0 || entry >= SharedLanternPublicState.SignCount) return RuleVerdict.Reject("Ô nhập này không tồn tại.");
            if (symbol < 0 || symbol >= SharedLanternPublicState.SymbolCount) return RuleVerdict.Reject("Ký hiệu này không tồn tại.");
            if (!world.IsNearPanel(slot)) return RuleVerdict.Reject("Bạn đứng quá xa bảng ký hiệu.");
            return RuleVerdict.Accept();
        }

        /// <summary>Nhận người nộp; trả chấp nhận nếu bảng chưa đúng, người nộp đứng gần, đủ 4 ô và bảng không đang bị khóa.</summary>
        RuleVerdict ValidateSubmit(int slot)
        {
            if (state.CodeAccepted) return RuleVerdict.Reject("Bảng ký hiệu đã đúng.");
            if (!world.IsNearPanel(slot)) return RuleVerdict.Reject("Bạn đứng quá xa bảng ký hiệu.");
            for (int i = 0; i < SharedLanternPublicState.SignCount; i++)
                if (state.Entry(i) < 0) return RuleVerdict.Reject("Chưa nhập đủ 4 ký hiệu.");
            if (state.LockRemaining(clock) > 0) return RuleVerdict.Reject("Bảng đang bị khóa vì nhập sai, hãy chờ.");
            return RuleVerdict.Accept();
        }

        /// <summary>Chạy lệnh vừa được Validate chấp nhận (Validate phải được gọi ngay trước, như HostAuthority làm). Nhận ô gửi và lệnh.</summary>
        public void Apply(int senderSlot, SharedLanternCommand command)
        {
            switch (command.Kind)
            {
                case SharedLanternCommandKind.Pass:
                    // Khoảng cách đã được kiểm ở Validate ngay trước đó nên truyền 0 để Domain không kiểm lần nữa.
                    LanternPassResult pass = state.TryPass(senderSlot, command.Index, 0.0);
                    if (pass != LanternPassResult.Passed) throw new InvalidOperationException("Domain từ chối lệnh đã được Validate: " + pass);
                    RefreshOutside();
                    Note(SharedLanternMessage.Passed, senderSlot, command.Index);
                    break;
                case SharedLanternCommandKind.Read:
                    LanternReadResult read = state.Read(senderSlot, command.Index, true);
                    if (read != LanternReadResult.Read) throw new InvalidOperationException("Domain từ chối lệnh đã được Validate: " + read);
                    Note(SharedLanternMessage.Read, senderSlot, command.Index);
                    break;
                case SharedLanternCommandKind.SetEntry:
                    if (!state.SetEntry(command.Index, command.Symbol)) throw new InvalidOperationException("Domain từ chối lệnh nhập đã được Validate.");
                    Note(SharedLanternMessage.EntrySet, senderSlot, command.Index);
                    break;
                case SharedLanternCommandKind.ClearEntries:
                    state.ClearEntries();
                    Note(SharedLanternMessage.EntriesCleared, senderSlot, -1);
                    break;
                case SharedLanternCommandKind.Submit:
                    LanternSubmitResult submit = state.Submit(clock);
                    if (submit == LanternSubmitResult.WrongCode) Note(SharedLanternMessage.WrongCode, senderSlot, -1);
                    else if (submit == LanternSubmitResult.Accepted)
                    {
                        Note(SharedLanternMessage.CodeAccepted, senderSlot, -1);
                        if (state.TryComplete(world.AllAtGate())) Note(SharedLanternMessage.Solved, -1, -1);
                    }
                    else throw new InvalidOperationException("Domain từ chối lệnh nộp đã được Validate: " + submit);
                    break;
                case SharedLanternCommandKind.Retry:
                    state.Reset();
                    resetCount++;
                    lastSentBackMask = 0;
                    pendingSentBack = 0;
                    RefreshOutside();
                    Note(SharedLanternMessage.Retry, senderSlot, -1);
                    break;
            }
        }

        /// <summary>Nhận thông báo, ô và chỉ số; ghi lại làm «sự kiện gần nhất» cho trạng thái chung.</summary>
        void Note(SharedLanternMessage message, int actor, int index)
        {
            lastMessage = message;
            lastActor = actor;
            lastIndex = index;
        }

        /// <summary>Không nhận tham số; tính lại mặt nạ «ngoài vùng sáng» từ lần đo gần nhất và người đang cầm đèn (người cầm đèn luôn trong sáng).</summary>
        void RefreshOutside()
        {
            int mask = 0;
            for (int i = 0; i < lastInLight.Length; i++)
                if (!lastInLight[i] && i != state.Holder) mask |= 1 << i;
            outsideMask = mask;
        }

        /// <summary>Chụp trạng thái chung (bản mới, bất biến). Không chứa seed, ký hiệu biển, đáp án hay đồng hồ đang chạy.</summary>
        public SharedLanternPublicState BuildPublicState()
        {
            int[] entries = new int[SharedLanternPublicState.SignCount];
            for (int i = 0; i < entries.Length; i++) entries[i] = state.Entry(i);
            return new SharedLanternPublicState(PlayerCount, state.Holder, outsideMask, state.SentBackCount, lastSentBackMask, entries,
                state.CodeAccepted, state.Solved, state.WrongSubmits, state.OutsideLimit, state.SubmitLockSeconds, resetCount,
                lastMessage, lastActor, lastIndex);
        }

        /// <summary>Nhận ô; trả sổ riêng của ô đó (các biển đã tự đọc và ký hiệu; rỗng nếu ô không tham gia). Tuyệt đối không chứa sổ người khác.</summary>
        public SharedLanternPrivateState BuildPrivateState(int slot)
        {
            if (slot < 0 || slot >= PlayerCount) return new SharedLanternPrivateState(null, null);
            int[] signs = new int[SharedLanternPublicState.SignCount];
            int[] symbols = new int[SharedLanternPublicState.SignCount];
            int count = 0;
            for (int sign = 0; sign < SharedLanternPublicState.SignCount; sign++)
            {
                int symbol = state.SymbolSeen(slot, sign);
                if (symbol < 0) continue;
                signs[count] = sign;
                symbols[count] = symbol;
                count++;
            }
            int[] readSigns = new int[count];
            int[] readSymbols = new int[count];
            Array.Copy(signs, readSigns, count);
            Array.Copy(symbols, readSymbols, count);
            return new SharedLanternPrivateState(readSigns, readSymbols);
        }

        /// <summary>
        /// Đồng hồ và phép đo vị trí chạy trên host. Nhận số giây đã trôi. Hỏi world ai đang trong vùng sáng, chạy luật «ngoài sáng quá giờ thì bị đưa về»,
        /// rồi kiểm cả đội tới cổng khi bảng đã đúng. Trả true khi trạng thái chung đổi mà không lệnh nào gây ra (đổi mặt nạ ngoài sáng, có người bị đưa về,
        /// vừa thắng) để lớp Unity gọi HostAuthority.MarkChanged(). Người bị đưa về lấy bằng TakeSentBack().
        /// </summary>
        public bool Tick(float deltaSeconds)
        {
            if (float.IsNaN(deltaSeconds) || float.IsInfinity(deltaSeconds) || deltaSeconds < 0f) return false;
            if (state.Solved) return false;
            clock += deltaSeconds;
            int n = PlayerCount;
            bool[] inLight = new bool[n];
            for (int i = 0; i < n; i++) inLight[i] = world.IsInLight(i);
            Array.Copy(inLight, lastInLight, n);
            int before = outsideMask;
            int sent = state.Tick(clock, inLight);
            RefreshOutside();
            bool changed = outsideMask != before;
            if (sent != 0)
            {
                pendingSentBack |= sent;
                lastSentBackMask = sent;
                Note(SharedLanternMessage.SentBack, -1, -1);
                changed = true;
            }
            if (state.TryComplete(world.AllAtGate()))
            {
                Note(SharedLanternMessage.Solved, -1, -1);
                changed = true;
            }
            return changed;
        }

        /// <summary>Không nhận tham số; trả mặt nạ những người bị đưa về kể từ lần gọi trước rồi xóa (lớp Unity trên host dịch chuyển họ về điểm đầu).</summary>
        public int TakeSentBack()
        {
            int mask = pendingSentBack;
            pendingSentBack = 0;
            return mask;
        }
    }
}
