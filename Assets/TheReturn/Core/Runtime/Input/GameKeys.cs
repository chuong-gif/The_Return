/*
 * Mục đích: Phím điều khiển do người chơi tự gán. Mọi script gameplay đọc phím qua lớp này (keyboard.Control(GameAction.X)) thay vì
 * gọi thẳng keyboard.eKey..., nhờ vậy menu Cài đặt đổi phím được và giá trị đổi có hiệu lực ngay, lưu giữa các lần chơi bằng PlayerPrefs.
 * Luật: mỗi hành động có đúng một phím; gán phím đang thuộc hành động khác thì HOÁN ĐỔI hai phím (không bao giờ có hai hành động chung phím).
 * Phím dành riêng, không cho gán: Esc, 1-4 (đổi vai khi test một máy), F1-F12, phím hệ thống/IME. Phím đổi vai 1-4 cố ý không nằm trong danh sách đổi.
 * Hàm: Get; Set; Reset; ResetAll; IsAllowed; Label; Group; DisplayName; Control (mở rộng cho Keyboard); sự kiện Changed.
 */
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace TheReturn
{
    /// <summary>Các hành động có thể đổi phím.</summary>
    public enum GameAction
    {
        MoveForward,
        MoveBack,
        MoveLeft,
        MoveRight,
        Jump,
        Sprint,
        Interact,
        Retry,
        Grab,
        Hint,
        Notebook,
        PauseMenu,
        ToggleMic
    }

    /// <summary>Kho phím điều khiển dùng chung.</summary>
    public static class GameKeys
    {
        const string Prefix = "TheReturn.Key.";

        static readonly GameAction[] all = (GameAction[])Enum.GetValues(typeof(GameAction));
        static readonly Key[] keys = new Key[all.Length];
        static bool loaded;

        /// <summary>Bắn mỗi khi một phím được đổi hoặc khôi phục.</summary>
        public static event Action Changed;

        /// <summary>Không nhận tham số; xoá cờ đã nạp để lần chạy Play sau nạp lại từ PlayerPrefs (an toàn khi tắt Domain Reload).</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { loaded = false; Changed = null; }

        /// <summary>Danh sách mọi hành động theo thứ tự hiển thị.</summary>
        public static IReadOnlyList<GameAction> All { get { return all; } }

        /// <summary>Nhận hành động; trả phím đang gán (mặc định nếu chưa đổi).</summary>
        public static Key Get(GameAction action)
        {
            EnsureLoaded();
            return keys[(int)action];
        }

        /// <summary>Nhận hành động và phím mới; trả false (không đổi gì) nếu phím bị dành riêng. Phím đang thuộc hành động khác thì hai bên hoán đổi, tên hành động bị hoán đổi trả qua swapped (null nếu không có).</summary>
        public static bool Set(GameAction action, Key key, out GameAction? swapped)
        {
            swapped = null;
            EnsureLoaded();
            if (!IsAllowed(key)) return false;
            Key old = keys[(int)action];
            if (old == key) return true;
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] != action && keys[i] == key)
                {
                    keys[i] = old;
                    swapped = all[i];
                    Save(all[i]);
                    break;
                }
            }
            keys[(int)action] = key;
            Save(action);
            if (Changed != null) Changed();
            return true;
        }

        /// <summary>Nhận hành động; đưa về phím mặc định (hoán đổi nếu phím mặc định đang bị hành động khác giữ).</summary>
        public static void Reset(GameAction action)
        {
            GameAction? swapped;
            Set(action, Default(action), out swapped);
        }

        /// <summary>Không nhận tham số; đưa mọi phím về mặc định và lưu.</summary>
        public static void ResetAll()
        {
            EnsureLoaded();
            for (int i = 0; i < all.Length; i++) { keys[i] = Default(all[i]); Save(all[i]); }
            if (Changed != null) Changed();
        }

        /// <summary>Nhận phím; trả true nếu được phép gán (không phải Esc, 1-4, F1-F12 hay phím hệ thống).</summary>
        public static bool IsAllowed(Key key)
        {
            if (key == Key.None || !Enum.IsDefined(typeof(Key), key)) return false;
            if (key == Key.Escape || key == Key.ContextMenu || key == Key.IMESelected) return false;
            if (key >= Key.Digit1 && key <= Key.Digit4) return false;
            if (key >= Key.Numpad1 && key <= Key.Numpad4) return false;
            if (key >= Key.F1 && key <= Key.F12) return false;
            if (key >= Key.OEM1 && key <= Key.OEM5) return false;
            return true;
        }

        /// <summary>Nhận hành động; trả phím mặc định.</summary>
        public static Key Default(GameAction action)
        {
            switch (action)
            {
                case GameAction.MoveForward: return Key.W;
                case GameAction.MoveBack: return Key.S;
                case GameAction.MoveLeft: return Key.A;
                case GameAction.MoveRight: return Key.D;
                case GameAction.Jump: return Key.Space;
                case GameAction.Sprint: return Key.LeftShift;
                case GameAction.Interact: return Key.E;
                case GameAction.Retry: return Key.R;
                case GameAction.Grab: return Key.G;
                case GameAction.Hint: return Key.H;
                case GameAction.Notebook: return Key.N;
                case GameAction.PauseMenu: return Key.Tab;
                default: return Key.V;
            }
        }

        /// <summary>Nhận hành động; trả tên tiếng Việt hiển thị trong menu.</summary>
        public static string Label(GameAction action)
        {
            switch (action)
            {
                case GameAction.MoveForward: return "Đi tới";
                case GameAction.MoveBack: return "Đi lùi";
                case GameAction.MoveLeft: return "Sang trái";
                case GameAction.MoveRight: return "Sang phải";
                case GameAction.Jump: return "Nhảy";
                case GameAction.Sprint: return "Chạy nhanh";
                case GameAction.Interact: return "Tương tác";
                case GameAction.Retry: return "Làm lại nhiệm vụ";
                case GameAction.Grab: return "Thả vật đang cầm";
                case GameAction.Hint: return "Xem gợi ý";
                case GameAction.Notebook: return "Mở sổ tay";
                case GameAction.PauseMenu: return "Menu tạm dừng";
                default: return "Bật/tắt mic";
            }
        }

        /// <summary>Nhận hành động; trả tên nhóm hiển thị (Di chuyển, Tương tác, Hệ thống).</summary>
        public static string Group(GameAction action)
        {
            if (action <= GameAction.Sprint) return "Di chuyển";
            if (action <= GameAction.Notebook) return "Tương tác";
            return "Hệ thống";
        }

        /// <summary>Nhận phím; trả tên ngắn dễ đọc (ưu tiên tên theo bàn phím đang cắm).</summary>
        public static string DisplayName(Key key)
        {
            switch (key)
            {
                case Key.Space: return "Space";
                case Key.LeftShift: return "Shift trái";
                case Key.RightShift: return "Shift phải";
                case Key.LeftCtrl: return "Ctrl trái";
                case Key.RightCtrl: return "Ctrl phải";
                case Key.LeftAlt: return "Alt trái";
                case Key.RightAlt: return "Alt phải";
                case Key.UpArrow: return "Mũi tên lên";
                case Key.DownArrow: return "Mũi tên xuống";
                case Key.LeftArrow: return "Mũi tên trái";
                case Key.RightArrow: return "Mũi tên phải";
                case Key.Backquote: return "`";
            }
            Keyboard kb = Keyboard.current;
            if (kb != null)
            {
                KeyControl control = kb[key];
                if (control != null && !string.IsNullOrEmpty(control.displayName)) return control.displayName.ToUpperInvariant();
            }
            return key.ToString().ToUpperInvariant();
        }

        /// <summary>Nhận bàn phím và hành động; trả điều khiển phím đang gán để đọc isPressed/wasPressedThisFrame.</summary>
        public static KeyControl Control(this Keyboard keyboard, GameAction action)
        {
            return keyboard[Get(action)];
        }

        /// <summary>Không nhận tham số; nạp phím đã lưu một lần, giá trị hỏng hoặc trùng thì dùng mặc định.</summary>
        static void EnsureLoaded()
        {
            if (loaded) return;
            loaded = true;
            for (int i = 0; i < all.Length; i++)
            {
                int stored = PlayerPrefs.GetInt(Prefix + all[i], (int)Default(all[i]));
                Key key = Enum.IsDefined(typeof(Key), stored) ? (Key)stored : Default(all[i]);
                keys[i] = IsAllowed(key) ? key : Default(all[i]);
            }
            for (int i = 0; i < all.Length; i++)
                for (int j = 0; j < i; j++)
                    if (keys[i] == keys[j]) { for (int k = 0; k < all.Length; k++) keys[k] = Default(all[k]); return; }
        }

        /// <summary>Nhận hành động; ghi phím hiện tại vào PlayerPrefs.</summary>
        static void Save(GameAction action)
        {
            PlayerPrefs.SetInt(Prefix + action, (int)keys[(int)action]);
            PlayerPrefs.Save();
        }
    }
}
