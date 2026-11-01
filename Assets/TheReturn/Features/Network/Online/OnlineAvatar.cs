/*
 * Mục đích: Nhân vật online của MỘT người chơi (T-025), spawn bởi NetworkManager cho từng client. HOST chia ô cố định lúc spawn
 * (NetworkVariable Slot); chủ nhân vật (owner) tự đặt vị trí xuất phát và tự điều khiển qua PrototypePartyController, còn máy khác chỉ
 * thấy nhân vật di chuyển nhờ OwnerNetworkTransform (CharacterController của người khác bị tắt). Đội đầy thì client dư bị ngắt.
 * Hàm: OnNetworkSpawn; OnNetworkDespawn; thuộc tính Slot; sự kiện SlotReady, Removed; ForEachReady.
 */
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace TheReturn.Network.Online
{
    public sealed class OnlineAvatar : NetworkBehaviour
    {
        private static readonly List<OnlineAvatar> Active = new List<OnlineAvatar>();

        private readonly NetworkVariable<int> slot = new NetworkVariable<int>(-1);
        private int announcedSlot = -1;

        /// <summary>Bắn khi nhân vật đã biết ô của mình (mọi máy). Nhận nhân vật.</summary>
        public static event Action<OnlineAvatar> SlotReady;

        /// <summary>Bắn khi nhân vật bị gỡ (người chơi rời). Nhận nhân vật và ô cũ của nó.</summary>
        public static event Action<OnlineAvatar, int> Removed;

        /// <summary>Ô cố định của nhân vật; -1 nếu host chưa chia.</summary>
        public int Slot { get { return announcedSlot >= 0 ? announcedSlot : slot.Value; } }

        /// <summary>Nhận hàm xử lý; gọi nó cho mọi nhân vật đã có ô (để bên đăng ký sau không bỏ lỡ).</summary>
        public static void ForEachReady(Action<OnlineAvatar> handler)
        {
            for (int i = 0; i < Active.Count; i++)
            {
                if (Active[i] != null && Active[i].announcedSlot >= 0) handler(Active[i]);
            }
        }

        /// <summary>Gọi khi spawn trên mạng. HOST chia ô; mọi máy theo dõi ô; máy không phải chủ tắt CharacterController. Không nhận/trả.</summary>
        public override void OnNetworkSpawn()
        {
            Active.Add(this);
            CharacterController body = GetComponent<CharacterController>();
            if (body != null && !IsOwner) body.enabled = false;
            slot.OnValueChanged += OnSlotChanged;
            if (IsServer)
            {
                OnlinePartySlots slots = OnlinePartySlots.Instance;
                int assigned = slots != null ? slots.AssignSlot(OwnerClientId) : -1;
                if (assigned < 0)
                {
                    Debug.LogWarning("[Online] Đội đã đủ người hoặc thiếu OnlinePartySlots: ngắt client " + OwnerClientId);
                    if (OwnerClientId != NetworkManager.ServerClientId) NetworkManager.DisconnectClient(OwnerClientId);
                    return;
                }
                slot.Value = assigned;
            }
            if (slot.Value >= 0) OnSlotChanged(-1, slot.Value);
        }

        /// <summary>Gọi khi nhân vật bị gỡ khỏi mạng: HOST trả ô, mọi máy báo Removed. Không nhận/trả.</summary>
        public override void OnNetworkDespawn()
        {
            slot.OnValueChanged -= OnSlotChanged;
            Active.Remove(this);
            if (IsServer && OnlinePartySlots.Instance != null) OnlinePartySlots.Instance.ReleaseSlot(OwnerClientId);
            if (announcedSlot >= 0)
            {
                int old = announcedSlot;
                announcedSlot = -1;
                Action<OnlineAvatar, int> handler = Removed;
                if (handler != null) handler(this, old);
            }
        }

        private void OnSlotChanged(int previous, int current)
        {
            if (current < 0 || current == announcedSlot) return;
            announcedSlot = current;
            if (IsOwner) TeleportToSpawn(current);
            Action<OnlineAvatar> handler = SlotReady;
            if (handler != null) handler(this);
        }

        private void TeleportToSpawn(int forSlot)
        {
            OnlinePartySlots slots = OnlinePartySlots.Instance;
            if (slots == null) return;
            CharacterController body = GetComponent<CharacterController>();
            if (body != null) body.enabled = false;
            transform.SetPositionAndRotation(slots.SpawnPositionFor(forSlot), Quaternion.identity);
            if (body != null) body.enabled = true;
        }
    }
}
