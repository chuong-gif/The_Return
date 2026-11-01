/*
 * Mục đích: CHẨN ĐOÁN TẠM (T-025) vì nhân vật online chạy trong bản build chỉ hiện dáng chữ T còn trong Editor thì có animation. Mỗi 1,5 giây
 * (tối đa 12 lần) ghi vào log tình trạng Animator của nhân vật này: bật/tắt, đã khởi tạo chưa, controller, clip đang chạy, tham số Speed,
 * xoay của xương hông, thời gian tỷ lệ, cửa sổ có đang được focus không. Gỡ file này khi tìm ra nguyên nhân.
 * Hàm: Update.
 */
using UnityEngine;

namespace TheReturn.Network.Online
{
    public sealed class OnlineAnimDiag : MonoBehaviour
    {
        private float nextReport;
        private int reports;

        private void Update()
        {
            if (reports >= 12 || Time.unscaledTime < nextReport) return;
            nextReport = Time.unscaledTime + 1.5f;
            reports++;
            Animator animator = GetComponentInChildren<Animator>(true);
            OnlineAvatar avatar = GetComponent<OnlineAvatar>();
            string who = "slot=" + (avatar != null ? avatar.Slot.ToString() : "?") + " owner=" + (avatar != null && avatar.IsOwner);
            if (animator == null)
            {
                Debug.Log("[AnimDiag] " + who + " KHÔNG có Animator");
                return;
            }
            AnimatorClipInfo[] clips = animator.GetCurrentAnimatorClipInfo(0);
            string clipName = clips.Length > 0 ? clips[0].clip.name : "(không có clip)";
            SkinnedMeshRenderer skin = GetComponentInChildren<SkinnedMeshRenderer>(true);
            Transform hips = skin != null ? skin.rootBone : null;
            Debug.Log(string.Format(
                "[AnimDiag] {0} enabled={1} active={2} init={3} ctrl={4} clip={5} normTime={6:0.00} speedParam={7:0.00} animSpeed={8} bound={9} hipsRot={10} timeScale={11} focus={12} runInBg={13}",
                who, animator.enabled, animator.isActiveAndEnabled, animator.isInitialized,
                animator.runtimeAnimatorController != null ? animator.runtimeAnimatorController.name : "NULL",
                clipName, animator.GetCurrentAnimatorStateInfo(0).normalizedTime, animator.GetFloat("Speed"), animator.speed,
                animator.hasBoundPlayables, hips != null ? hips.localEulerAngles.ToString("0") : "n/a",
                Time.timeScale, Application.isFocused, Application.runInBackground));
        }
    }
}
