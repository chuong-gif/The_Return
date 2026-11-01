/*
 * Mục đích: Hai luật nhiệm vụ GIẢ dùng riêng cho test khung mạng (T-024). ClaimRules: mỗi người nhận một «món» chưa ai nhận
 * (lệnh = số món; trạng thái chung = ai đã nhận món nào; thông tin riêng = mã bí mật của từng ô + số món đã nhận). EchoRules: lệnh là chuỗi.
 * Hai lớp đếm số lần được gọi và có công tắc ném lỗi để test độ bền của host.
 * Hàm: ClaimRules (Validate, Apply, BuildPublicState, BuildPrivateState, ClaimedBy, ClaimsOf, SecretFor); EchoRules.
 */
using System;
using System.Collections.Generic;

namespace TheReturn.Network.Framework.Tests
{
    /// <summary>Luật giả: mỗi món chỉ một người nhận.</summary>
    public sealed class ClaimRules : IHostRules<int, int[], int>
    {
        readonly int[] claimedBy;

        public int ValidateCalls;
        public int ApplyCalls;
        public int PublicBuilds;
        public int LastSender = -1;
        public bool ThrowOnValidate;
        public bool ThrowOnApply;
        public bool MutateThenThrow;
        public readonly List<int> PrivateRequests = new List<int>();

        /// <summary>Nhận số món.</summary>
        public ClaimRules(int itemCount)
        {
            claimedBy = new int[itemCount];
            for (int i = 0; i < itemCount; i++) claimedBy[i] = -1;
        }

        /// <summary>Bản sao: món i do ô nào nhận (-1 là chưa ai).</summary>
        public int[] ClaimedBy { get { return (int[])claimedBy.Clone(); } }

        /// <summary>Nhận ô; trả số món ô đó đã nhận.</summary>
        public int ClaimsOf(int slot)
        {
            int count = 0;
            for (int i = 0; i < claimedBy.Length; i++)
            {
                if (claimedBy[i] == slot) count++;
            }
            return count;
        }

        /// <summary>Mã riêng của một ô khi đã nhận claims món: (ô+1)*100 + claims.</summary>
        public static int SecretFor(int slot, int claims)
        {
            return (slot + 1) * 100 + claims;
        }

        public RuleVerdict Validate(int senderSlot, int command)
        {
            ValidateCalls++;
            if (ThrowOnValidate) throw new InvalidOperationException("hỏng khi kiểm");
            if (command < 0 || command >= claimedBy.Length) return RuleVerdict.Reject("Món không tồn tại.");
            if (claimedBy[command] != -1) return RuleVerdict.Reject("Món đã có người nhận.");
            return RuleVerdict.Accept();
        }

        public void Apply(int senderSlot, int command)
        {
            ApplyCalls++;
            LastSender = senderSlot;
            if (MutateThenThrow)
            {
                claimedBy[command] = senderSlot;
                throw new InvalidOperationException("hỏng sau khi đổi");
            }
            if (ThrowOnApply) throw new InvalidOperationException("hỏng khi chạy");
            claimedBy[command] = senderSlot;
        }

        public int[] BuildPublicState()
        {
            PublicBuilds++;
            return (int[])claimedBy.Clone();
        }

        public int BuildPrivateState(int slot)
        {
            PrivateRequests.Add(slot);
            return SecretFor(slot, ClaimsOf(slot));
        }
    }

    /// <summary>Luật giả với lệnh kiểu chuỗi (để thử lệnh rỗng).</summary>
    public sealed class EchoRules : IHostRules<string, string, NoPrivate>
    {
        public string Last;

        public RuleVerdict Validate(int senderSlot, string command)
        {
            return RuleVerdict.Accept();
        }

        public void Apply(int senderSlot, string command)
        {
            Last = command;
        }

        public string BuildPublicState()
        {
            return Last;
        }

        public NoPrivate BuildPrivateState(int slot)
        {
            return default(NoPrivate);
        }
    }
}
