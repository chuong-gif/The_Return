/*
 * Mục đích: Phần HOST của nhiệm vụ «Bảng cầu dao» online (T-031), nay kế thừa MissionOnlineHostBase (T-039 M5). Phần chung (theo dõi nhân vật, dựng
 * HostAuthority, nhận lệnh từ MissionNetBridge, phát trạng thái đúng từng người, dọn khi hết làm host) nằm ở lớp cơ sở; file này chỉ khai
 * dựng BreakerHostRules (số công tắc đủ cho đội), đồng hồ giữ khớp (Tick), codec, và IBreakerWorld lấy vị trí nhân vật + công tắc trong scene.
 * Hàm: TryCreateRules; TickRules; Encode/Decode; OnBuilt; OnTeardown; SolutionMaskForTest; kiểu lồng HostWorld (IBreakerWorld).
 * Trường serialize giữ NGUYÊN tên (switches, settings, switchReach; bridge ở lớp cơ sở) để scene NetworkBreakerTest không mất tham chiếu.
 */
using TheReturn.Network.Framework;
using TheReturn.Network.Online;
using UnityEngine;

namespace TheReturn.BreakerNet.Online
{
    public sealed class BreakerOnlineHost : MissionOnlineHostBase<BreakerCommand, BreakerPublicState, BreakerPrivateState>
    {
        [SerializeField] private BreakerSwitch[] switches = new BreakerSwitch[0];
        [SerializeField] private BreakerSettings settings;
        [Tooltip("Khoảng cách tối đa (m) từ nhân vật tới công tắc để được bấm (bản 1 máy dùng tia 3 m).")]
        [SerializeField] private float switchReach = 3.5f;

        private BreakerHostRules rules;
        private int builtSeed;
        private float builtHold;

        /// <summary>Chỉ để KIỂM THỬ TỰ ĐỘNG trên host: mặt nạ công tắc đáp án, hoặc -1 nếu chưa dựng đề. Không bao giờ gửi qua mạng.</summary>
        public int SolutionMaskForTest { get { return rules != null ? rules.Round.SolutionSwitches : -1; } }

        protected override string LogTag { get { return "BreakerHost"; } }

        protected override bool TryCreateRules(int teamSize, out IHostRules<BreakerCommand, BreakerPublicState, BreakerPrivateState> built)
        {
            built = null;
            int needed = teamSize + 2;
            if (switches == null || switches.Length < needed) return false;
            for (int i = 0; i < needed; i++) if (switches[i] == null) return false;
            builtSeed = settings != null && settings.fixedSeed != 0 ? settings.fixedSeed : System.Environment.TickCount;
            builtHold = settings != null ? settings.holdSeconds : 3f;
            rules = new BreakerHostRules(teamSize, builtSeed, new HostWorld(this), builtHold);
            built = rules;
            return true;
        }

        protected override bool TickRules(float deltaSeconds)
        {
            return rules != null && rules.Tick(deltaSeconds);
        }

        protected override string EncodePublic(BreakerPublicState state) { return BreakerCodec.EncodePublic(state); }

        protected override string EncodePrivate(BreakerPrivateState state) { return BreakerCodec.EncodePrivate(state); }

        protected override bool TryDecodeCommand(string payload, out BreakerCommand command) { return BreakerCodec.TryDecodeCommand(payload, out command); }

        protected override void OnBuilt(int teamSize)
        {
            Debug.Log("[BreakerHost] Bắt đầu nhiệm vụ cầu dao: " + teamSize + " người, seed " + builtSeed + ", giữ khớp " + builtHold + " giây");
        }

        protected override void OnTeardown()
        {
            rules = null;
        }

        private sealed class HostWorld : IBreakerWorld
        {
            private readonly BreakerOnlineHost host;

            public HostWorld(BreakerOnlineHost owner)
            {
                host = owner;
            }

            public bool IsNearSwitch(int slot, int sw)
            {
                Transform body;
                if (sw < 0 || sw >= host.switches.Length || host.switches[sw] == null) return false;
                if (!host.TryGetAvatar(slot, out body)) return false;
                return Vector3.Distance(body.position, host.switches[sw].transform.position) <= host.switchReach;
            }
        }
    }
}
