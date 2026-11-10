/*
 * Mục đích: Phần HOST của nhiệm vụ «Hợp xướng» online (T-037 GĐ2), kế thừa MissionOnlineHostBase (T-039 Q-007). Phần chung (theo dõi nhân vật,
 * dựng HostAuthority, nhận lệnh từ MissionNetBridge, phát trạng thái đúng từng người, dọn khi hết làm host) nằm ở lớp cơ sở; file này chỉ khai
 * dựng ChoirHostRules (seed, cửa sổ giữa hai nốt theo số người), đồng hồ (Tick, quá giờ thì dãy về nốt đầu), codec, và IChoirWorld đo khoảng
 * cách từ nhân vật tới bục THEO VỊ TRÍ HOST THẤY (client không tự khai).
 * Hàm: TryCreateRules; TickRules; EncodePublic/EncodePrivate; TryDecodeCommand; OnBuilt; OnTeardown; kiểu lồng HostWorld (IChoirWorld).
 */
using TheReturn.Network.Framework;
using TheReturn.Network.Online;
using UnityEngine;

namespace TheReturn.ChoirNet.Online
{
    public sealed class ChoirOnlineHost : MissionOnlineHostBase<ChoirCommand, ChoirPublicState, ChoirPrivateState>
    {
        [SerializeField] private ChoirPad[] pads = new ChoirPad[0];
        [SerializeField] private ChoirSettings settings;
        [Tooltip("Khoảng cách tối đa (m) từ nhân vật tới bục để được bấm.")]
        [SerializeField] private float padReach = 3.5f;

        private ChoirHostRules rules;
        private int builtSeed;

        protected override string LogTag { get { return "ChoirHost"; } }

        protected override bool TryCreateRules(int teamSize, out IHostRules<ChoirCommand, ChoirPublicState, ChoirPrivateState> built)
        {
            built = null;
            if (pads == null || pads.Length < ChoirRound.PadCount) return false;
            for (int i = 0; i < ChoirRound.PadCount; i++) if (pads[i] == null) return false;
            builtSeed = settings != null && settings.fixedSeed != 0 ? settings.fixedSeed : System.Environment.TickCount;
            rules = new ChoirHostRules(teamSize, builtSeed, new HostWorld(this));
            built = rules;
            return true;
        }

        protected override bool TickRules(float deltaSeconds)
        {
            return rules != null && rules.Tick(deltaSeconds);
        }

        protected override string EncodePublic(ChoirPublicState state) { return ChoirCodec.EncodePublic(state); }

        protected override string EncodePrivate(ChoirPrivateState state) { return ChoirCodec.EncodePrivate(state); }

        protected override bool TryDecodeCommand(string payload, out ChoirCommand command) { return ChoirCodec.TryDecodeCommand(payload, out command); }

        protected override void OnBuilt(int teamSize)
        {
            Debug.Log("[ChoirHost] Bắt đầu Hợp xướng: " + teamSize + " người, seed " + builtSeed);
        }

        protected override void OnTeardown()
        {
            rules = null;
        }

        private sealed class HostWorld : IChoirWorld
        {
            private readonly ChoirOnlineHost host;

            public HostWorld(ChoirOnlineHost owner)
            {
                host = owner;
            }

            public bool IsNearPad(int slot, int pad)
            {
                Transform body;
                if (pad < 0 || pad >= host.pads.Length || host.pads[pad] == null) return false;
                if (!host.TryGetAvatar(slot, out body)) return false;
                return Vector3.Distance(body.position, host.pads[pad].transform.position) <= host.padReach;
            }
        }
    }
}
