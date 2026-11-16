/*
 * Mục đích: Cổng CHUNG để về MENU CHÍNH từ bất cứ scene nào (T-048). Core không được biết gì về mạng, nên phần mạng tự đăng ký hai móc
 * (IsOnline, LeaveOnline) lúc chạy; nếu có mạng đang sống thì phải rời phòng + tắt NetworkManager xong rồi mới nạp Menu, không thì nạp thẳng.
 * Thông báo (MenuNotice) do bên mạng hiện ở Menu khi có người rời hoặc mất kết nối.
 * Hàm: ReturnToMenu; (tĩnh) IsOnline, LeaveOnline, LoadMenu, GameFinished, MenuNotice, DefaultMenuScene.
 */
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheReturn
{
    /// <summary>Điểm vào duy nhất để rời ván về Menu chính, dùng được cả khi chơi một máy lẫn online.</summary>
    public static class SessionExit
    {
        /// <summary>Tên scene Menu chính mặc định.</summary>
        public const string DefaultMenuScene = "MainMenu";

        /// <summary>Do bên mạng đăng ký: trả true khi còn hạ tầng mạng sống (cần dọn trước khi về Menu). Null = không có mạng.</summary>
        public static Func<bool> IsOnline;

        /// <summary>Do bên mạng đăng ký: nhận thông báo cho các máy còn lại và một hành động; rời phòng, tắt mạng, dọn xong thì gọi hành động đó.</summary>
        public static Action<string, Action> LeaveOnline;

        /// <summary>Cách nạp scene Menu (mặc định: SceneTransitionService nếu có, không thì SceneManager). Đổi được khi kiểm thử.</summary>
        public static Action<string> LoadMenu = DefaultLoad;

        /// <summary>true khi ván đã kết thúc bằng thắng (màn HOÀN THÀNH đã hiện): từ lúc đó người rời đi không còn làm cả đội bị kéo về Menu.</summary>
        public static bool GameFinished;

        /// <summary>Thông báo cần hiện cho người chơi ở Menu (rỗng = không có); bên hiện xong sẽ xóa.</summary>
        public static string MenuNotice = "";

        /// <summary>Nhận tên scene Menu và thông báo (có thể rỗng); nếu còn mạng thì rời phòng trước rồi mới nạp Menu, không thì nạp ngay.</summary>
        public static void ReturnToMenu(string menuScene, string notice = "")
        {
            if (string.IsNullOrWhiteSpace(menuScene)) return;
            MenuNotice = notice ?? "";
            bool online = false;
            try { online = IsOnline != null && IsOnline(); }
            catch (Exception e) { Debug.LogException(e); }
            if (online && LeaveOnline != null)
            {
                LeaveOnline(MenuNotice, () => Finish(menuScene));
                return;
            }
            Finish(menuScene);
        }

        static void Finish(string menuScene)
        {
            GameFinished = false;
            LoadMenu(menuScene);
        }

        static void DefaultLoad(string sceneName)
        {
            if (SceneTransitionService.Instance != null) SceneTransitionService.Instance.LoadScene(sceneName);
            else SceneManager.LoadScene(sceneName);
        }
    }
}
