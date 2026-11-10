/*
 * Mục đích: «Nhiệm vụ giả» để THỬ bộ hợp đồng (T-039, M1): mỗi người chơi phải «nhận» đúng ô của mình, đủ mọi ô thì thắng;
 * mỗi ô có một chuỗi bí mật riêng; đồng hồ 60 giây thì hết giờ (thua). Có thể cài từng LỖI cố ý (FakeFlaw) để chứng minh
 * hợp đồng BẮT được adapter hỏng. Không dùng trong game. Đây cũng là ví dụ mẫu cho adapter thật (Attendance, BreakerPanel...).
 * Hàm: FakeCommand; FakePublic; FakePrivate; FakeRules (Validate, Apply, BuildPublicState, BuildPrivateState, Tick, ForceTimeout);
 * FakeGame (TryNextSolveMove, TryIllegalMove, ForceLoss, Tick); FakeMissionAdapter (codec, NewGame, SecretsOf, IsSolvedPublic).
 */
using System;
using System.Globalization;
using TheReturn.Network.Framework;

namespace TheReturn.Network.Contract.Tests
{
    /// <summary>Lỗi cố ý cài vào nhiệm vụ giả.</summary>
    public enum FakeFlaw
    {
        None,
        /// <summary>Đưa bí mật ô 0 vào trạng thái chung (mong C5 bắt).</summary>
        LeakSecret,
        /// <summary>Giải mã trạng thái chung ném lỗi với chuỗi rác (mong C2 bắt).</summary>
        DecodeThrows,
        /// <summary>Chuỗi trạng thái chung dài 2500 ký tự (mong C3 bắt).</summary>
        TooLong,
        /// <summary>Giải mã làm mất bit cao của mặt nạ (mong C1 bắt).</summary>
        CodecLossy,
        /// <summary>Luật sửa trạng thái rồi mới từ chối lệnh (mong C4 bắt).</summary>
        RejectChangesState,
        /// <summary>Bí mật chứa số ngẫu nhiên không theo seed (mong C9 bắt).</summary>
        NonDeterministic,
        /// <summary>Bot gửi nước sai nên không thắng nổi (mong C6 bắt).</summary>
        BotFails,
        /// <summary>BuildPublicState trả MỘT đối tượng dùng chung rồi sửa tiếp (gói đã phát bị đổi; mong N1 bắt).</summary>
        AliasedState
    }

    /// <summary>Lệnh giả: «nhận ô Target».</summary>
    public sealed class FakeCommand
    {
        public readonly int Target;

        public FakeCommand(int target) { Target = target; }
    }

    /// <summary>Trạng thái chung giả.</summary>
    public sealed class FakePublic
    {
        public readonly int PlayerCount;
        public int ClaimedMask;
        public bool TimedOut;
        public bool Solved;
        public readonly string Leak;

        public FakePublic(int playerCount, int claimedMask, bool timedOut, bool solved, string leak)
        {
            PlayerCount = playerCount;
            ClaimedMask = claimedMask;
            TimedOut = timedOut;
            Solved = solved;
            Leak = leak;
        }
    }

    /// <summary>Thông tin riêng giả: một chuỗi bí mật.</summary>
    public readonly struct FakePrivate
    {
        public readonly string Secret;

        public FakePrivate(string secret) { Secret = secret; }
    }

    /// <summary>Luật host giả.</summary>
    public sealed class FakeRules : IHostRules<FakeCommand, FakePublic, FakePrivate>
    {
        readonly int n;
        readonly FakeFlaw flaw;
        readonly string[] secrets;
        int claimed;
        float elapsed;
        FakePublic live;

        public bool TimedOut { get; private set; }
        public bool Solved { get { return !TimedOut && claimed == (1 << n) - 1; } }
        public int Claimed { get { return claimed; } }
        public string SecretOf(int slot) { return secrets[slot]; }

        public FakeRules(int n, int seed, FakeFlaw flaw)
        {
            this.n = n;
            this.flaw = flaw;
            secrets = new string[n];
            for (int i = 0; i < n; i++)
            {
                secrets[i] = "SEC-" + i + "-" + seed;
                if (flaw == FakeFlaw.NonDeterministic) secrets[i] += "-" + Guid.NewGuid().ToString("N").Substring(0, 6);
            }
        }

        public RuleVerdict Validate(int senderSlot, FakeCommand command)
        {
            if (command.Target < 0 || command.Target >= n) return RuleVerdict.Reject("Ô đích ngoài đội.");
            if (TimedOut) return RuleVerdict.Reject("Hết giờ.");
            if (command.Target != senderSlot)
            {
                if (flaw == FakeFlaw.RejectChangesState) claimed |= 1 << command.Target;
                return RuleVerdict.Reject("Không phải ô của bạn.");
            }
            if ((claimed >> command.Target & 1) == 1) return RuleVerdict.Reject("Đã nhận rồi.");
            return RuleVerdict.Accept();
        }

        public void Apply(int senderSlot, FakeCommand command)
        {
            claimed |= 1 << command.Target;
        }

        public FakePublic BuildPublicState()
        {
            if (flaw == FakeFlaw.AliasedState)
            {
                if (live == null) live = new FakePublic(n, claimed, TimedOut, Solved, null);
                live.ClaimedMask = claimed;
                live.TimedOut = TimedOut;
                live.Solved = Solved;
                return live;
            }
            return new FakePublic(n, claimed, TimedOut, Solved, flaw == FakeFlaw.LeakSecret ? secrets[0] : null);
        }

        public FakePrivate BuildPrivateState(int slot)
        {
            return new FakePrivate(secrets[slot]);
        }

        /// <summary>Cho giờ chạy thêm; trả true nếu vừa hết giờ.</summary>
        public bool Tick(float deltaSeconds)
        {
            if (Solved || TimedOut) return false;
            elapsed += deltaSeconds;
            if (elapsed < 60f) return false;
            TimedOut = true;
            return true;
        }

        /// <summary>Ép hết giờ ngay.</summary>
        public void ForceTimeout()
        {
            for (int i = 0; i < 10 && !TimedOut; i++) Tick(10f);
        }
    }

    /// <summary>Ván giả cho hợp đồng.</summary>
    public sealed class FakeGame : IContractGame<FakeCommand, FakePublic, FakePrivate>
    {
        readonly FakeRules rules;
        readonly FakeFlaw flaw;
        readonly int n;

        public FakeGame(int n, int seed, FakeFlaw flaw)
        {
            this.n = n;
            this.flaw = flaw;
            rules = new FakeRules(n, seed, flaw);
        }

        public IHostRules<FakeCommand, FakePublic, FakePrivate> Rules { get { return rules; } }
        public bool IsSolved { get { return rules.Solved; } }
        public FakeRules FakeRulesRef { get { return rules; } }

        public bool TryNextSolveMove(out int slot, out FakeCommand command)
        {
            for (int s = 0; s < n; s++)
            {
                if ((rules.Claimed >> s & 1) == 1) continue;
                slot = s;
                command = new FakeCommand(flaw == FakeFlaw.BotFails ? (s + 1) % n : s);
                return true;
            }
            slot = -1;
            command = null;
            return false;
        }

        public bool TryIllegalMove(out int slot, out FakeCommand command)
        {
            slot = 0;
            command = new FakeCommand(1);
            return true;
        }

        public bool ForceLoss()
        {
            rules.ForceTimeout();
            return rules.TimedOut;
        }

        public bool Tick(float deltaSeconds)
        {
            return rules.Tick(deltaSeconds);
        }
    }

    /// <summary>Adapter của nhiệm vụ giả (có thể cài lỗi cố ý).</summary>
    public sealed class FakeMissionAdapter : MissionContractAdapter<FakeCommand, FakePublic, FakePrivate>
    {
        readonly FakeFlaw flaw;

        public FakeMissionAdapter(FakeFlaw flaw) { this.flaw = flaw; }

        public override string MissionName { get { return "FakeMission(" + flaw + ")"; } }

        public override IContractGame<FakeCommand, FakePublic, FakePrivate> NewGame(int teamSize, int seed)
        {
            return new FakeGame(teamSize, seed, flaw);
        }

        public override string[] SecretsOf(IContractGame<FakeCommand, FakePublic, FakePrivate> game, int slot)
        {
            return new[] { ((FakeGame)game).FakeRulesRef.SecretOf(slot) };
        }

        public override bool IsSolvedPublic(FakePublic state) { return state.Solved; }

        // ------------------------------------------------------------- codec

        /// <summary>Đọc số không dấu; chế độ DecodeThrows dùng int.Parse nên ném lỗi với chuỗi rác.</summary>
        bool ParseInt(string text, out int value)
        {
            if (flaw == FakeFlaw.DecodeThrows)
            {
                value = int.Parse(text, CultureInfo.InvariantCulture);
                return true;
            }
            return int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out value);
        }

        public override string EncodeCommand(FakeCommand command)
        {
            return "c:" + command.Target;
        }

        public override bool TryDecodeCommand(string text, out FakeCommand command)
        {
            command = null;
            if (text == null || !text.StartsWith("c:", StringComparison.Ordinal)) return false;
            int target;
            if (!int.TryParse(text.Substring(2), NumberStyles.None, CultureInfo.InvariantCulture, out target) || target > 9) return false;
            command = new FakeCommand(target);
            return true;
        }

        public override string EncodePublic(FakePublic s)
        {
            string text = "p:" + s.PlayerCount + "|" + s.ClaimedMask + "|" + (s.TimedOut ? 1 : 0) + "|" + (s.Solved ? 1 : 0);
            if (s.Leak != null) text += "|" + s.Leak;
            else if (flaw == FakeFlaw.TooLong) text += "|" + new string('x', 2500);
            return text;
        }

        public override bool TryDecodePublic(string text, out FakePublic state)
        {
            state = null;
            if (text == null || !text.StartsWith("p:", StringComparison.Ordinal)) return false;
            string[] parts = text.Substring(2).Split('|');
            if (parts.Length < 4 || parts.Length > 5) return false;
            int n, mask, timedOut, solved;
            if (!ParseInt(parts[0], out n) || !ParseInt(parts[1], out mask) || !ParseInt(parts[2], out timedOut) || !ParseInt(parts[3], out solved)) return false;
            if (n < 2 || n > 4 || mask < 0 || mask >= (1 << n) || timedOut > 1 || solved > 1) return false;
            if (flaw == FakeFlaw.CodecLossy) mask &= 7;
            string leak = parts.Length == 5 && flaw != FakeFlaw.TooLong ? parts[4] : null;
            state = new FakePublic(n, mask, timedOut == 1, solved == 1, leak);
            return true;
        }

        public override string EncodePrivate(FakePrivate s)
        {
            return "s:" + (s.Secret ?? string.Empty);
        }

        public override bool TryDecodePrivate(string text, out FakePrivate state)
        {
            state = default(FakePrivate);
            if (text == null || !text.StartsWith("s:", StringComparison.Ordinal)) return false;
            state = new FakePrivate(text.Substring(2));
            return true;
        }
    }
}
