/*
 * Mục đích: ENGINE của bộ hợp đồng mạng (T-039, M1). Với MỘT adapter, chạy chín ca C1-C9 cho từng đội (mặc định 2/3/4) và
 * từng seed, qua đúng đường thật HostAuthority -> StateMessage -> ClientReplica (chưa có mạng thật). Trả danh sách
 * ContractFailure (mỗi ván tối đa một lỗi mỗi ca) để fixture assert và để test «adapter hỏng có bị bắt không».
 *   C1 mã-giải-mã-mã ra đúng chuỗi cũ trên mọi trạng thái/lệnh gặp trong ván   C2 dữ liệu rác không bao giờ ném lỗi
 *   C3 chuỗi mã hóa <= MaxChars                                                C4 lệnh sai bị từ chối, KHÔNG đổi Version/trạng thái
 *   C5 bí mật không lọt (chung / riêng người khác)                              C6 BOT tự giải tới thắng qua host + bản sao
 *   C7 ván thua/hết giờ vẫn nhất quán (bỏ qua nếu adapter không có thua)       C8 Version đơn điệu, vào muộn/vào lại khớp, gói cũ bị bỏ
 *   C9 cùng seed ra cùng ván từng bước
 *   N1 (ContractRunnerSim.cs) chơi qua MẠNG GIẢ có lỗi (SimNet): trễ/đảo/trùng/mất; gói bất biến; hội tụ về host
 * Hàm: constructor; Run; (nội bộ) NewRig, Broadcast, Send, Solve, PubText, ExpectReject, RoundTrip, Reconnect, Trace, Cut, Mutate,
 * DecodeEverything, C1..C9.
 */
using System;
using System.Collections.Generic;
using System.Text;
using TheReturn.Network.Framework;

namespace TheReturn.Network.Contract
{
    /// <summary>Chạy bộ hợp đồng C1-C9 cho một adapter nhiệm vụ.</summary>
    public sealed partial class ContractRunner<TCmd, TPub, TPriv>
    {
        /// <summary>Mã mọi ca, theo thứ tự.</summary>
        public static readonly string[] AllCases = { "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9", "N1" };

        const int MaxMoves = 400;
        const int MaxIdleTicks = 400;
        const float IdleTickSeconds = 0.25f;

        sealed class Rig
        {
            public int N;
            public int Seed;
            public IContractGame<TCmd, TPub, TPriv> Game;
            public HostAuthority<TCmd, TPub, TPriv> Host;
            public ClientReplica<TPub, TPriv>[] Replicas;
        }

        readonly MissionContractAdapter<TCmd, TPub, TPriv> adapter;
        readonly int seedCount;
        readonly int[] teamSizes;
        int skippedGames;

        /// <summary>True nếu lần Run gần nhất bị bỏ qua hoàn toàn (hiện chỉ C7 khi adapter không có thua).</summary>
        public bool Skipped { get; private set; }

        /// <summary>Nhận adapter, số seed (seed chạy 1..seedCount) và danh sách số người (thường 2,3,4).</summary>
        public ContractRunner(MissionContractAdapter<TCmd, TPub, TPriv> adapter, int seedCount, int[] teamSizes)
        {
            if (adapter == null) throw new ArgumentNullException("adapter");
            if (teamSizes == null || teamSizes.Length == 0) throw new ArgumentException("Cần ít nhất một cỡ đội.", "teamSizes");
            this.adapter = adapter;
            this.seedCount = Math.Max(1, seedCount);
            this.teamSizes = teamSizes;
        }

        /// <summary>
        /// Chạy MỘT ca (C1..C9) cho mọi cỡ đội x mọi seed. Trả danh sách lỗi (rỗng = đạt). Lỗi không bắt được trong adapter
        /// cũng thành ContractFailure (không làm sập cả lượt).
        /// </summary>
        public List<ContractFailure> Run(string caseCode)
        {
            if (Array.IndexOf(AllCases, caseCode) < 0) throw new ArgumentException("Mã ca không hợp lệ: " + caseCode, "caseCode");
            var sink = new List<ContractFailure>();
            Skipped = false;
            skippedGames = 0;
            int games = 0;
            foreach (int n in teamSizes)
            {
                for (int seed = 1; seed <= seedCount; seed++)
                {
                    games++;
                    bool failedOnce = false;
                    int nn = n;
                    int ss = seed;
                    Action<string> fail = delegate(string message)
                    {
                        if (failedOnce) return;
                        failedOnce = true;
                        sink.Add(new ContractFailure(caseCode, adapter.MissionName, nn, ss, message));
                    };
                    try
                    {
                        switch (caseCode)
                        {
                            case "C1": C1(n, seed, fail); break;
                            case "C2": C2(n, seed, fail); break;
                            case "C3": C3(n, seed, fail); break;
                            case "C4": C4(n, seed, fail); break;
                            case "C5": C5(n, seed, fail); break;
                            case "C6": C6(n, seed, fail); break;
                            case "C7": C7(n, seed, fail); break;
                            case "C8": C8(n, seed, fail); break;
                            case "C9": C9(n, seed, fail); break;
                            case "N1": N1(n, seed, fail); break;
                        }
                    }
                    catch (Exception ex)
                    {
                        fail("Ném lỗi không bắt được: " + ex.GetType().Name + ": " + ex.Message);
                    }
                }
            }
            Skipped = caseCode == "C7" && games > 0 && skippedGames == games;
            return sink;
        }

        // ------------------------------------------------------------------ nền

        static void NoFail(string message) { }

        /// <summary>Dựng một ván + host + bản sao cho mọi ô.</summary>
        Rig NewRig(int n, int seed)
        {
            var game = adapter.NewGame(n, seed);
            if (game == null || game.Rules == null) throw new InvalidOperationException("Adapter.NewGame trả null (hoặc Rules null).");
            var rig = new Rig();
            rig.N = n;
            rig.Seed = seed;
            rig.Game = game;
            rig.Host = new HostAuthority<TCmd, TPub, TPriv>(new TeamRoster(n), game.Rules, new HostLog(64));
            rig.Replicas = new ClientReplica<TPub, TPriv>[n];
            for (int i = 0; i < n; i++) rig.Replicas[i] = new ClientReplica<TPub, TPriv>(i);
            return rig;
        }

        /// <summary>Host phát trạng thái, mỗi bản sao nhận gói của mình.</summary>
        void Broadcast(Rig r)
        {
            foreach (var message in r.Host.BuildBroadcast()) r.Replicas[message.Slot].Receive(message);
        }

        /// <summary>Ô slot gửi lệnh qua host với số thứ tự kế tiếp.</summary>
        CommandResult Send(Rig r, int slot, TCmd command)
        {
            return r.Host.Submit(slot, r.Replicas[slot].NextSequence(), command);
        }

        /// <summary>Mã hóa trạng thái chung hiện tại của luật (nguồn sự thật phía host).</summary>
        string PubText(Rig r)
        {
            return adapter.EncodePublic(r.Game.Rules.BuildPublicState());
        }

        /// <summary>
        /// Cho bot giải tới thắng: gửi từng nước hợp lệ, phát trạng thái; hết nước thì cho đồng hồ chạy tới khi thắng.
        /// afterMove chạy sau mỗi nước được chấp nhận; afterWait chạy sau mỗi nhịp chờ. Trả true nếu thắng.
        /// </summary>
        bool Solve(Rig r, Action<string> fail, Action<Rig, int, TCmd> afterMove, Action<Rig> afterWait)
        {
            for (int i = 0; i < MaxMoves; i++)
            {
                if (r.Game.IsSolved) break;
                int slot;
                TCmd command;
                if (!r.Game.TryNextSolveMove(out slot, out command)) break;
                var result = Send(r, slot, command);
                if (!result.IsAccepted)
                {
                    fail("Nước của bot bị host từ chối [" + result.Reason + "]: " + result.Detail + " (ô " + slot + ").");
                    return false;
                }
                Broadcast(r);
                if (afterMove != null) afterMove(r, slot, command);
            }
            for (int t = 0; t < MaxIdleTicks && !r.Game.IsSolved; t++)
            {
                if (r.Game.Tick(IdleTickSeconds)) r.Host.MarkChanged();
                Broadcast(r);
                if (afterWait != null) afterWait(r);
            }
            if (!r.Game.IsSolved)
            {
                fail("Bot hết nước và hết giờ chờ mà ván chưa thắng.");
                return false;
            }
            Broadcast(r);
            return true;
        }

        /// <summary>Gửi lệnh qua submit; mong bị từ chối, Version và trạng thái chung KHÔNG đổi.</summary>
        void ExpectReject(Rig r, string label, Func<CommandResult> submit, Action<string> fail, RejectReason expected)
        {
            int v0 = r.Host.Version;
            string before = PubText(r);
            CommandResult result = submit();
            if (result.IsAccepted) { fail(label + ": lệnh sai vẫn được CHẤP NHẬN."); return; }
            if (expected != RejectReason.None && result.Reason != expected)
                fail(label + ": lý do từ chối là " + result.Reason + ", mong " + expected + " (" + result.Detail + ").");
            if (result.Version != v0 || r.Host.Version != v0)
                fail(label + ": Version đổi sau khi từ chối (" + v0 + " -> " + r.Host.Version + ").");
            if (PubText(r) != before)
                fail(label + ": trạng thái chung ĐỔI sau khi từ chối lệnh (luật vừa sửa vừa từ chối).");
        }

        /// <summary>Kiểm giá trị mã hóa rồi giải mã rồi mã hóa lại ra đúng chuỗi cũ.</summary>
        void RoundTrip<T>(string label, Func<T, string> encode, TryDecode<T> decode, T value, Action<string> fail)
        {
            string text = encode(value);
            T back;
            if (!decode(text, out back)) { fail(label + ": giải mã thất bại với chính chuỗi vừa mã hóa " + Cut(text)); return; }
            string again = encode(back);
            if (again != text) fail(label + ": mã-giải-mã-mã không ra lại chuỗi cũ. Trước " + Cut(text) + " sau " + Cut(again));
        }

        /// <summary>Ngắt rồi nối lại ô 0 giữa ván: ô ngắt không nhận gói; vào lại nhận bản chụp đầy đủ khớp host.</summary>
        void Reconnect(Rig r, Action<string> fail)
        {
            r.Host.DisconnectSlot(0);
            if (r.Host.BuildStateFor(0) != null) fail("Ô đã ngắt kết nối vẫn nhận được gói trạng thái đầy đủ.");
            r.Host.ConnectSlot(0);
            r.Replicas[0].Reset();
            var message = r.Host.BuildStateFor(0);
            if (message == null || !r.Replicas[0].Receive(message)) { fail("Ô vào lại không nhận được bản chụp đầy đủ."); return; }
            if (adapter.EncodePublic(r.Replicas[0].Public) != PubText(r)) fail("Bản chụp vào-lại khác trạng thái chung hiện tại của host.");
        }

        /// <summary>Rút gọn chuỗi để in thông điệp lỗi (ký tự điều khiển thành «?»).</summary>
        static string Cut(string s)
        {
            if (s == null) return "<null>";
            var sb = new StringBuilder();
            foreach (char ch in s)
            {
                if (sb.Length >= 70) { sb.Append("..."); break; }
                sb.Append(char.IsControl(ch) ? '?' : ch);
            }
            return "\"" + sb + "\" (len " + s.Length + ")";
        }

        /// <summary>Biến đổi nhẹ một chuỗi hợp lệ để dò codec: cắt, xóa, thay ký tự, nhân đôi, thêm đuôi, số khổng lồ.</summary>
        static string Mutate(string s, Random rnd)
        {
            if (string.IsNullOrEmpty(s)) return "|";
            const string alphabet = "0123456789|,=:-.;abcXYZ é";
            switch (rnd.Next(6))
            {
                case 0: return s.Substring(0, rnd.Next(s.Length));
                case 1: { int i = rnd.Next(s.Length); return s.Remove(i, 1); }
                case 2: { int i = rnd.Next(s.Length); return s.Substring(0, i) + alphabet[rnd.Next(alphabet.Length)] + s.Substring(i + 1); }
                case 3: { int i = rnd.Next(s.Length); return s.Substring(0, i) + s.Substring(i); }
                case 4: return s + "|" + rnd.Next(int.MaxValue);
                default: return s.Replace("1", "99999999999").Replace("0", "x");
            }
        }

        /// <summary>Cho cả ba bộ giải mã ăn một chuỗi; nếu giải mã được thì mã hóa lại. Bất kỳ lỗi ném ra đều là vi phạm C2.</summary>
        void DecodeEverything(string s, Action<string> fail)
        {
            try
            {
                TCmd c;
                if (adapter.TryDecodeCommand(s, out c)) adapter.EncodeCommand(c);
                TPub p;
                if (adapter.TryDecodePublic(s, out p)) adapter.EncodePublic(p);
                TPriv q;
                if (adapter.TryDecodePrivate(s, out q)) adapter.EncodePrivate(q);
            }
            catch (Exception ex)
            {
                fail("Codec ném " + ex.GetType().Name + " với đầu vào " + Cut(s) + ": " + ex.Message);
            }
        }

        /// <summary>Dấu vết ván (mọi chuỗi mã hóa từng bước) dùng so sánh hai lần chạy cùng seed.</summary>
        List<string> Trace(int n, int seed)
        {
            var list = new List<string>();
            var r = NewRig(n, seed);
            Action<Rig> snap = delegate(Rig rr)
            {
                foreach (int slot in rr.Host.Roster.ConnectedSlots())
                {
                    var m = rr.Host.BuildStateFor(slot);
                    list.Add("v" + m.Version + " o" + slot + " " + adapter.EncodePublic(m.Public) + " / " + adapter.EncodePrivate(m.Private));
                }
            };
            Broadcast(r);
            snap(r);
            Solve(r, NoFail, delegate(Rig rr, int s, TCmd c) { list.Add("cmd o" + s + " " + adapter.EncodeCommand(c)); snap(rr); }, snap);
            return list;
        }

        // ------------------------------------------------------------------ các ca

        void C1(int n, int seed, Action<string> fail)
        {
            var r = NewRig(n, seed);
            Action<Rig> check = delegate(Rig rr)
            {
                foreach (int slot in rr.Host.Roster.ConnectedSlots())
                {
                    var m = rr.Host.BuildStateFor(slot);
                    RoundTrip<TPub>("trạng thái chung", adapter.EncodePublic, adapter.TryDecodePublic, m.Public, fail);
                    RoundTrip<TPriv>("thông tin riêng ô " + slot, adapter.EncodePrivate, adapter.TryDecodePrivate, m.Private, fail);
                }
            };
            Broadcast(r);
            check(r);
            Solve(r, NoFail, delegate(Rig rr, int s, TCmd c)
            {
                RoundTrip<TCmd>("lệnh của ô " + s, adapter.EncodeCommand, adapter.TryDecodeCommand, c, fail);
                check(rr);
            }, check);
        }

        void C2(int n, int seed, Action<string> fail)
        {
            var rnd = new Random(seed * 7919 + n);
            var valid = new List<string>();
            var r = NewRig(n, seed);
            Action<Rig> grab = delegate(Rig rr)
            {
                if (valid.Count >= 40) return;
                var m = rr.Host.BuildStateFor(rr.Host.Roster.ConnectedSlots()[0]);
                valid.Add(adapter.EncodePublic(m.Public));
                valid.Add(adapter.EncodePrivate(m.Private));
            };
            Broadcast(r);
            grab(r);
            Solve(r, NoFail, delegate(Rig rr, int s, TCmd c) { valid.Add(adapter.EncodeCommand(c)); grab(rr); }, grab);

            var inputs = new List<string> { null, "", " ", "|", "||||", ":", "p:", "-1", "9999999999999999999", "\u0000", "é|ü|ß", "p:a|b|c|d", "p:2|x|0|0", "c:x", "s:" };
            foreach (string v in valid)
            {
                inputs.Add(v);
                for (int k = 0; k < 6; k++) inputs.Add(Mutate(v, rnd));
            }
            const string chars = "0123456789|,=:;-.abcXYZ é\t";
            for (int k = 0; k < 100; k++)
            {
                int len = rnd.Next(0, 200);
                var sb = new StringBuilder();
                for (int j = 0; j < len; j++) sb.Append(chars[rnd.Next(chars.Length)]);
                inputs.Add(sb.ToString());
            }
            foreach (string s in inputs) DecodeEverything(s, fail);
        }

        void C3(int n, int seed, Action<string> fail)
        {
            int max = adapter.MaxChars;
            var r = NewRig(n, seed);
            Action<Rig> check = delegate(Rig rr)
            {
                foreach (int slot in rr.Host.Roster.ConnectedSlots())
                {
                    var m = rr.Host.BuildStateFor(slot);
                    string pub = adapter.EncodePublic(m.Public);
                    string priv = adapter.EncodePrivate(m.Private);
                    if (pub.Length > max) fail("Trạng thái chung dài " + pub.Length + " > " + max + " ký tự.");
                    if (priv.Length > max) fail("Thông tin riêng ô " + slot + " dài " + priv.Length + " > " + max + " ký tự.");
                }
            };
            Broadcast(r);
            check(r);
            Solve(r, NoFail, delegate(Rig rr, int s, TCmd c)
            {
                string text = adapter.EncodeCommand(c);
                if (text.Length > max) fail("Lệnh dài " + text.Length + " > " + max + " ký tự.");
                check(rr);
            }, check);
        }

        void C4(int n, int seed, Action<string> fail)
        {
            var r = NewRig(n, seed);
            int slot;
            TCmd command;
            if (!r.Game.TryNextSolveMove(out slot, out command)) { fail("Ván mới không có nước hợp lệ nào để thử (TryNextSolveMove trả false)."); return; }

            ExpectReject(r, "ô -1", delegate { return r.Host.Submit(-1, 1, command); }, fail, RejectReason.InvalidSlot);
            ExpectReject(r, "ô ngoài đội (" + n + ")", delegate { return r.Host.Submit(n, 1, command); }, fail, RejectReason.InvalidSlot);

            object boxed = default(TCmd);
            if (boxed == null)
                ExpectReject(r, "lệnh null", delegate { return r.Host.Submit(slot, r.Replicas[slot].NextSequence(), default(TCmd)); }, fail, RejectReason.NullCommand);

            r.Host.DisconnectSlot(slot);
            ExpectReject(r, "ô đã ngắt kết nối", delegate { return r.Host.Submit(slot, r.Replicas[slot].NextSequence(), command); }, fail, RejectReason.NotConnected);
            r.Host.ConnectSlot(slot);
            r.Replicas[slot].Reset();

            int seq = r.Replicas[slot].NextSequence();
            CommandResult first = r.Host.Submit(slot, seq, command);
            if (!first.IsAccepted) { fail("Nước hợp lệ đầu tiên của bot bị từ chối [" + first.Reason + "]: " + first.Detail); return; }
            ExpectReject(r, "gửi lại cùng số thứ tự", delegate { return r.Host.Submit(slot, seq, command); }, fail, RejectReason.StaleSequence);

            var r2 = NewRig(n, seed);
            int s2;
            TCmd c2;
            if (r2.Game.TryIllegalMove(out s2, out c2))
                ExpectReject(r2, "lệnh sai luật", delegate { return r2.Host.Submit(s2, r2.Replicas[s2].NextSequence(), c2); }, fail, RejectReason.RejectedByRules);
        }

        void C5(int n, int seed, Action<string> fail)
        {
            var r = NewRig(n, seed);
            var declared = new HashSet<string>();
            var seenOwn = new HashSet<string>();
            Action<Rig> check = delegate(Rig rr)
            {
                int[] slots = rr.Host.Roster.ConnectedSlots();
                var priv = new Dictionary<int, string>();
                string pub = null;
                foreach (int slot in slots)
                {
                    var m = rr.Host.BuildStateFor(slot);
                    pub = adapter.EncodePublic(m.Public);
                    priv[slot] = adapter.EncodePrivate(m.Private);
                }
                foreach (int slot in slots)
                {
                    string[] secrets = adapter.SecretsOf(rr.Game, slot) ?? new string[0];
                    foreach (string x in secrets)
                    {
                        if (string.IsNullOrEmpty(x)) continue;
                        string key = slot + ":" + x;
                        declared.Add(key);
                        if (pub != null && pub.IndexOf(x, StringComparison.Ordinal) >= 0)
                            fail("Bí mật của ô " + slot + " LỌT vào trạng thái CHUNG: " + Cut(x));
                        foreach (int other in slots)
                        {
                            if (other == slot) continue;
                            if (priv[other].IndexOf(x, StringComparison.Ordinal) >= 0)
                                fail("Bí mật của ô " + slot + " LỌT vào thông tin riêng của ô " + other + ": " + Cut(x));
                        }
                        if (priv[slot].IndexOf(x, StringComparison.Ordinal) >= 0) seenOwn.Add(key);
                    }
                }
            };
            Broadcast(r);
            check(r);
            Solve(r, NoFail, delegate(Rig rr, int s, TCmd c) { check(rr); }, check);
            foreach (string key in declared)
                if (!seenOwn.Contains(key))
                    fail("Adapter khai bí mật " + Cut(key) + " nhưng không lúc nào thấy nó trong thông tin riêng của chính ô đó (khai sai adapter?).");
        }

        void C6(int n, int seed, Action<string> fail)
        {
            var r = NewRig(n, seed);
            Broadcast(r);
            if (!Solve(r, fail, null, null)) return;
            if (!r.Game.IsSolved) { fail("Ván không báo thắng sau khi bot giải xong."); return; }
            for (int i = 0; i < n; i++)
            {
                var rep = r.Replicas[i];
                if (!rep.HasState) { fail("Ô " + i + " không nhận được trạng thái nào."); return; }
                if (!adapter.IsSolvedPublic(rep.Public)) { fail("Ô " + i + " không thấy báo thắng ở bản sao (IsSolvedPublic = false)."); return; }
                if (rep.Version != r.Host.Version) { fail("Ô " + i + " dừng ở Version " + rep.Version + ", host ở " + r.Host.Version + "."); return; }
            }
        }

        void C7(int n, int seed, Action<string> fail)
        {
            var r = NewRig(n, seed);
            Broadcast(r);
            if (!r.Game.ForceLoss()) { skippedGames++; return; }
            if (r.Game.IsSolved) fail("ForceLoss xong mà ván vẫn báo thắng.");
            r.Host.MarkChanged();
            Broadcast(r);
            foreach (int slot in r.Host.Roster.ConnectedSlots())
            {
                var m = r.Host.BuildStateFor(slot);
                RoundTrip<TPub>("trạng thái chung sau thua", adapter.EncodePublic, adapter.TryDecodePublic, m.Public, fail);
                RoundTrip<TPriv>("thông tin riêng sau thua ô " + slot, adapter.EncodePrivate, adapter.TryDecodePrivate, m.Private, fail);
                var rep = r.Replicas[slot];
                if (!rep.HasState || rep.Version != r.Host.Version) fail("Ô " + slot + " không hội tụ về host sau khi thua.");
                else if (adapter.IsSolvedPublic(rep.Public)) fail("Ô " + slot + " thấy báo THẮNG dù ván đã thua.");
            }
        }

        void C8(int n, int seed, Action<string> fail)
        {
            var r = NewRig(n, seed);
            int lastVersion = r.Host.Version;
            int step = 0;
            StateMessage<TPub, TPriv> oldMessage = null;
            Action<Rig> verify = delegate(Rig rr)
            {
                if (rr.Host.Version < lastVersion) fail("Version của host lùi: " + lastVersion + " -> " + rr.Host.Version + ".");
                lastVersion = rr.Host.Version;
                foreach (int slot in rr.Host.Roster.ConnectedSlots())
                {
                    var late = rr.Host.BuildStateFor(slot);
                    var rep = rr.Replicas[slot];
                    if (late == null) { fail("BuildStateFor(" + slot + ") trả null cho ô đang kết nối."); continue; }
                    if (!rep.HasState || rep.Version != late.Version)
                    {
                        fail("Ô " + slot + ": bản sao Version " + rep.Version + " lệch bản chụp muộn Version " + late.Version + ".");
                        continue;
                    }
                    if (adapter.EncodePublic(rep.Public) != adapter.EncodePublic(late.Public)
                        || adapter.EncodePrivate(rep.Private) != adapter.EncodePrivate(late.Private))
                        fail("Ô " + slot + ": cùng Version nhưng bản phát khác bản chụp muộn (nội dung lệch).");
                }
            };
            Broadcast(r);
            verify(r);
            Solve(r, NoFail, delegate(Rig rr, int slot, TCmd cmd)
            {
                int before = lastVersion;
                verify(rr);
                if (rr.Host.Version <= before) fail("Nước được chấp nhận nhưng Version không tăng (" + before + " -> " + rr.Host.Version + ").");
                step++;
                if (step == 1)
                {
                    oldMessage = rr.Host.BuildStateFor(0);
                    Reconnect(rr, fail);
                }
                else if (step == 2 && oldMessage != null)
                {
                    var rep = rr.Replicas[0];
                    int now = rep.Version;
                    if (oldMessage.Version < now && rep.Receive(oldMessage))
                        fail("Gói cũ (Version " + oldMessage.Version + ") bị áp dụng khi bản sao đã ở Version " + now + ".");
                }
            }, verify);
        }

        void C9(int n, int seed, Action<string> fail)
        {
            var a = Trace(n, seed);
            var b = Trace(n, seed);
            if (a.Count != b.Count) { fail("Hai ván cùng seed có số bước khác nhau: " + a.Count + " và " + b.Count + "."); return; }
            for (int i = 0; i < a.Count; i++)
            {
                if (a[i] == b[i]) continue;
                fail("Hai ván cùng seed lệch ở bước " + i + ": A=" + Cut(a[i]) + " B=" + Cut(b[i]));
                return;
            }
        }
    }
}
