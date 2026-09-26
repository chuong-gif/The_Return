/*
 * Mục đích: Cung cấp một điểm chuyển scene dùng chung với màn hình mờ dần, tải bất đồng bộ và chống gọi lặp.
 * Danh sách hàm:
 * - Awake: bảo đảm chỉ có một dịch vụ tồn tại xuyên scene.
 * - LoadScene: nhận tên scene; bắt đầu chuyển cảnh nếu chưa có lần tải nào đang chạy.
 * - LoadRoutine: làm tối màn hình, tải scene bất đồng bộ rồi làm sáng scene mới.
 * - Fade: nhận alpha đích; nội suy lớp phủ và không trả dữ liệu.
 * - OnGUI: vẽ lớp phủ đen không phụ thuộc Canvas để dùng được trong mọi scene.
 */
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheReturn
{
    public sealed class SceneTransitionService : MonoBehaviour
    {
        public static SceneTransitionService Instance { get; private set; }
        [SerializeField, Min(.05f)] float fadeSeconds = .45f;
        float overlayAlpha;
        bool loading;

        /// <summary>Không nhận đầu vào; giữ một instance xuyên scene và loại bỏ instance trùng.</summary>
        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>Nhận tên scene trong Build Settings; bỏ qua tên rỗng hoặc lần gọi khi đang tải.</summary>
        public void LoadScene(string sceneName)
        {
            if (loading || string.IsNullOrWhiteSpace(sceneName)) return;
            StartCoroutine(LoadRoutine(sceneName));
        }

        /// <summary>Nhận tên scene; trả IEnumerator để làm tối, tải bất đồng bộ và làm sáng màn hình.</summary>
        IEnumerator LoadRoutine(string sceneName)
        {
            loading = true;
            yield return Fade(1f);
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            if (operation == null)
            {
                Debug.LogError("Không thể tải scene: " + sceneName, this);
                yield return Fade(0f);
                loading = false;
                yield break;
            }
            while (!operation.isDone) yield return null;
            yield return null;
            yield return Fade(0f);
            loading = false;
        }

        /// <summary>Nhận alpha 0–1; trả IEnumerator nội suy độ tối theo thời gian thực.</summary>
        IEnumerator Fade(float target)
        {
            float start = overlayAlpha;
            float elapsed = 0f;
            while (elapsed < fadeSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                overlayAlpha = Mathf.Lerp(start, target, Mathf.Clamp01(elapsed / fadeSeconds));
                yield return null;
            }
            overlayAlpha = target;
        }

        /// <summary>Không nhận đầu vào; vẽ lớp phủ đen toàn màn hình theo alpha hiện tại.</summary>
        void OnGUI()
        {
            if (overlayAlpha <= .001f) return;
            Color previous = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, overlayAlpha);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = previous;
        }
    }
}
