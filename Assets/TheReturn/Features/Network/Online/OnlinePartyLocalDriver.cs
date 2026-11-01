/*
 * Mục đích: Nối nhân vật online với bộ điều khiển có sẵn PrototypePartyController trên MÁY NÀY (T-025): gắn mọi nhân vật đã spawn vào ô
 * của nó, bật chế độ online cho nhân vật của chính người chơi (chỉ điều khiển ô đó), và mỗi frame chuyển phím/chuột vào TickInput.
 * Phím Tab khóa/thả chuột. Không sửa luật nhiệm vụ.
 * Hàm: Update; xử lý sự kiện OnlineAvatar.SlotReady / Removed.
 */
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheReturn.Network.Online
{
    public sealed class OnlinePartyLocalDriver : MonoBehaviour
    {
        [SerializeField] private PrototypePartyController controller;

        private int localSlot = -1;

        /// <summary>Nhiệm vụ đặt true khi người chơi đang ngồi trên phương tiện (vd ván chạy): không tự đi được.</summary>
        public bool Seated { get; set; }

        private void OnEnable()
        {
            OnlineAvatar.SlotReady += HandleReady;
            OnlineAvatar.Removed += HandleRemoved;
            OnlineAvatar.ForEachReady(HandleReady);
        }

        private void OnDisable()
        {
            OnlineAvatar.SlotReady -= HandleReady;
            OnlineAvatar.Removed -= HandleRemoved;
        }

        private void HandleReady(OnlineAvatar avatar)
        {
            if (controller == null) return;
            controller.BindSlot(avatar.Slot, avatar.transform);
            if (!avatar.IsOwner) return;
            localSlot = avatar.Slot;
            int team = OnlinePartySlots.Instance != null ? OnlinePartySlots.Instance.TeamSize : 4;
            controller.EnterOnlineMode(localSlot, Mathf.Max(team, localSlot + 1), false);
        }

        private void HandleRemoved(OnlineAvatar avatar, int slot)
        {
            if (controller == null) return;
            controller.BindSlot(slot, null);
            if (slot != localSlot) return;
            controller.ExitOnlineMode();
            localSlot = -1;
            PrototypePartyController.SetCursor(false);
        }

        private void Update()
        {
            if (controller == null || !controller.IsOnline) return;
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.tabKey.wasPressedThisFrame)
                PrototypePartyController.SetCursor(Cursor.lockState != CursorLockMode.Locked);
            controller.TickInput(Seated);
        }
    }
}
