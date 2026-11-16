/*
 * Mục đích: Âm giao diện DÙNG CHUNG cho cả game (MainMenu, Sanh_Cho, MapSchoolF và menu Cài đặt), do AudioBootstrap tạo một lần và sống xuyên scene.
 * Không cần gắn vào từng nút: mỗi khung hình dò xem chuột đang trỏ vào nút/công tắc/thanh trượt nào (UGUI) rồi phát cue:
 *   - ui.hover khi chuột VỪA đi vào một điều khiển bấm được;  - ui.click khi bấm nút;  - ui.toggle khi bấm công tắc (không có clip thì dùng ui.click);
 *   - ui.slider lặp nhẹ khi đang KÉO thanh trượt và giá trị đổi.
 * Clip nằm ở asset Resources/SharedAudioProfile (nhóm «TOÀN GAME»). Điều khiển có thành phần UiSoundMute thì im lặng.
 * Chuột đang khóa (đang chơi) thì không phát gì.
 * Hàm: Update; Probe (tìm điều khiển dưới chuột); Press; TickSlider; Play.
 */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TheReturn
{
    /// <summary>Phát âm giao diện cho mọi nút/công tắc/thanh trượt UGUI trong game.</summary>
    public sealed class UiSoundDirector : MonoBehaviour
    {
        [Min(0.01f)] public float sliderTickInterval = 0.05f;

        private readonly List<RaycastResult> hits = new List<RaycastResult>();
        private PointerEventData pointer;
        private Vector2 lastPosition = new Vector2(-1f, -1f);
        private Selectable hovered;
        private Slider dragSlider;
        private float dragValue;
        private float nextTick;

        private void Update()
        {
            EventSystem es = EventSystem.current;
            Mouse mouse = Mouse.current;
            if (es == null || mouse == null || Cursor.lockState == CursorLockMode.Locked)
            {
                hovered = null; dragSlider = null;
                return;
            }
            Vector2 pos = mouse.position.ReadValue();
            bool pressed = mouse.leftButton.wasPressedThisFrame;
            if (pos != lastPosition || pressed || hovered == null)
            {
                lastPosition = pos;
                Selectable now = Probe(es, pos);
                if (now != hovered)
                {
                    hovered = now;
                    if (now != null && now.IsInteractable()) Play(AudioCues.UiHover);
                }
            }
            if (pressed && hovered != null && hovered.IsInteractable()) Press(hovered);
            TickSlider(mouse);
        }

        /// <summary>Nhận EventSystem và vị trí chuột; trả điều khiển (nút/công tắc/thanh trượt) nằm trên cùng dưới chuột, hoặc null nếu trên cùng là thứ khác.</summary>
        private Selectable Probe(EventSystem es, Vector2 pos)
        {
            if (pointer == null || pointer.currentInputModule != es.currentInputModule) pointer = new PointerEventData(es);
            pointer.position = pos;
            hits.Clear();
            es.RaycastAll(pointer, hits);
            for (int i = 0; i < hits.Count; i++)
            {
                GameObject go = hits[i].gameObject;
                if (go == null) continue;
                Selectable sel = go.GetComponentInParent<Selectable>();
                if (sel == null) return null; // vật chắn không phải điều khiển (khung, nền) nằm trên cùng
                if (!(sel is Button) && !(sel is Toggle) && !(sel is Slider)) return null;
                if (sel.GetComponentInParent<UiSoundMute>() != null) return null;
                return sel;
            }
            return null;
        }

        /// <summary>Nhận điều khiển vừa bị bấm; phát cue đúng loại (nút / công tắc) hoặc bắt đầu theo dõi thanh trượt.</summary>
        private void Press(Selectable sel)
        {
            Slider slider = sel as Slider;
            if (slider != null) { dragSlider = slider; dragValue = slider.value; nextTick = 0f; return; }
            if (sel is Toggle)
            {
                AudioService svc = AudioService.Instance;
                Play(svc != null && svc.HasCue(AudioCues.UiToggle) ? AudioCues.UiToggle : AudioCues.UiClick);
                return;
            }
            Play(AudioCues.UiClick);
        }

        /// <summary>Nhận chuột; khi đang giữ chuột trên một thanh trượt và giá trị đổi thì phát ui.slider (giới hạn tốc độ); nhả chuột thì thôi theo dõi.</summary>
        private void TickSlider(Mouse mouse)
        {
            if (dragSlider == null) return;
            if (!mouse.leftButton.isPressed || !dragSlider.isActiveAndEnabled) { dragSlider = null; return; }
            if (Mathf.Abs(dragSlider.value - dragValue) < 0.0001f) return;
            dragValue = dragSlider.value;
            if (Time.unscaledTime < nextTick) return;
            nextTick = Time.unscaledTime + sliderTickInterval;
            Play(AudioCues.UiSlider);
        }

        private static void Play(string cue) { AudioHub.Player.Play(cue); }
    }
}
