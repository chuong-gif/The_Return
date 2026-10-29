// Mục đích: người chơi thử cho T-009 S1. Chủ sở hữu (owner) tự điều khiển WASD, NetworkTransform (owner-authoritative) đồng bộ vị trí.
//   Màu thân theo OwnerClientId để phân biệt người chơi.
// Hàm: OnNetworkSpawn, Update.
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheReturn.Network
{
    public sealed class NetworkTestPlayer : NetworkBehaviour
    {
        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private Renderer bodyRenderer;

        /// <summary>Gọi khi object được spawn trên mạng: đặt màu theo owner, đặt vị trí xuất phát cho owner. Nhận/Trả: không.</summary>
        public override void OnNetworkSpawn()
        {
            if (bodyRenderer != null)
                bodyRenderer.material.color = Color.HSVToRGB((OwnerClientId * 0.23f) % 1f, 0.7f, 0.95f);
            if (IsOwner)
                transform.position = new Vector3(OwnerClientId * 1.5f, 1f, 0f);
        }

        /// <summary>Mỗi frame: chỉ owner đọc WASD và di chuyển trên mặt phẳng XZ. Nhận/Trả: không.</summary>
        private void Update()
        {
            if (!IsSpawned || !IsOwner) return;
            var kb = Keyboard.current;
            if (kb == null) return;
            var dir = Vector3.zero;
            if (kb.wKey.isPressed) dir.z += 1f;
            if (kb.sKey.isPressed) dir.z -= 1f;
            if (kb.dKey.isPressed) dir.x += 1f;
            if (kb.aKey.isPressed) dir.x -= 1f;
            if (dir.sqrMagnitude > 0f)
                transform.position += dir.normalized * moveSpeed * Time.deltaTime;
        }
    }
}
