/*
 * Mục đích: BIỂN TÊN trên đầu nhân vật của người khác trong map online (T-055): hiện «số • tên người chơi» (tên nhập ở panel Chơi online, lấy từ OnlineNames theo ô), xoay về phía camera.
 * Để người chơi đối chiếu manh mối nhiệm vụ Điểm danh («Lan ngồi cạnh Hùng») với nhân vật thật. OnlineAvatar tự gắn component này cho nhân vật không phải của mình.
 * Hàm: LateUpdate; EnsureTag; OnDestroy.
 */
using UnityEngine;

namespace TheReturn.Network.Online
{
    /// <summary>Biển tên 3D (TextMesh) trên đầu một nhân vật online.</summary>
    [RequireComponent(typeof(OnlineAvatar))]
    public sealed class OnlineNameTag : MonoBehaviour
    {
        private OnlineAvatar avatar;
        private TextMesh tag;
        private Font font;
        private CharacterController body;

        private void Awake()
        {
            avatar = GetComponent<OnlineAvatar>();
            body = GetComponent<CharacterController>();
        }

        /// <summary>Mỗi khung hình (sau khi nhân vật di chuyển): cập nhật chữ, đặt biển trên đầu và xoay về phía camera. Chưa có tên thì ẩn biển.</summary>
        private void LateUpdate()
        {
            int slot = avatar != null ? avatar.Slot : -1;
            string who = slot >= 0 ? OnlineNames.OfSlot(slot) : null;
            Camera cam = Camera.main;
            if (who == null || cam == null)
            {
                if (tag != null && tag.gameObject.activeSelf) tag.gameObject.SetActive(false);
                return;
            }
            EnsureTag();
            if (!tag.gameObject.activeSelf) tag.gameObject.SetActive(true);
            string label = (slot + 1) + " • " + who;
            if (tag.text != label) tag.text = label;
            float top = body != null ? body.center.y + body.height * 0.5f + 0.35f : 2.2f;
            tag.transform.position = transform.position + Vector3.up * top;
            tag.transform.rotation = Quaternion.LookRotation(tag.transform.position - cam.transform.position);
        }

        /// <summary>Không nhận tham số; tạo TextMesh biển tên một lần (font có sẵn của Unity, màu vàng nhạt).</summary>
        private void EnsureTag()
        {
            if (tag != null) return;
            if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            GameObject go = new GameObject("NameTag_" + (avatar.Slot + 1), typeof(TextMesh));
            tag = go.GetComponent<TextMesh>();
            tag.font = font;
            tag.fontSize = 64;
            tag.characterSize = 0.09f;
            tag.anchor = TextAnchor.MiddleCenter;
            tag.alignment = TextAlignment.Center;
            tag.color = new Color(1f, 0.95f, 0.6f);
            go.GetComponent<MeshRenderer>().sharedMaterial = font.material;
        }

        private void OnDestroy()
        {
            if (tag != null) Destroy(tag.gameObject);
        }
    }
}
