/*
 * Mục đích: Lưu lựa chọn MICRO THU ÂM của người chơi (T-055) mà không phụ thuộc Vivox, để menu Cài đặt (GameSettings) và dịch vụ thoại (Voice) cùng dùng.
 * Rỗng = dùng micro mặc định của hệ thống. Danh sách tên: ưu tiên danh sách do Vivox báo khi đã vào kênh thoại (đúng tên Vivox dùng),
 * chưa vào kênh thì lấy từ Unity Microphone.devices. Tên của Unity và Vivox có thể khác nhau chút (Windows cắt tên dài) nên so khớp mềm.
 * Hàm: Preferred; SetExternalNames; AvailableNames; FindMatch; Reset; sự kiện Changed.
 */
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheReturn
{
    /// <summary>Micro thu âm người chơi chọn (lưu PlayerPrefs) và danh sách micro hiện có.</summary>
    public static class MicDeviceSettings
    {
        private const string Key = "TheReturn.Set.MicDevice";
        private static List<string> external;

        /// <summary>Bắn khi người chơi đổi micro (kể cả về mặc định).</summary>
        public static event Action Changed;

        /// <summary>Tên micro đã chọn; rỗng = mặc định hệ thống. Đặt giá trị mới thì lưu và báo Changed.</summary>
        public static string Preferred
        {
            get { return PlayerPrefs.GetString(Key, ""); }
            set
            {
                string v = value ?? "";
                if (v == Preferred) return;
                PlayerPrefs.SetString(Key, v);
                PlayerPrefs.Save();
                Action handler = Changed;
                if (handler != null) handler();
            }
        }

        /// <summary>Nhận danh sách tên micro do Vivox báo (hoặc null khi rời kênh thoại); dùng thay cho danh sách của Unity.</summary>
        public static void SetExternalNames(IList<string> names)
        {
            external = names != null && names.Count > 0 ? new List<string>(names) : null;
        }

        /// <summary>Không nhận tham số; trả bản sao danh sách tên micro hiện có (ưu tiên danh sách Vivox, không thì Microphone.devices).</summary>
        public static List<string> AvailableNames()
        {
            if (external != null) return new List<string>(external);
            List<string> list = new List<string>();
            foreach (string d in Microphone.devices) if (!string.IsNullOrEmpty(d)) list.Add(d);
            return list;
        }

        /// <summary>Nhận danh sách tên, tên cần tìm và chỉ số bắt đầu; trả chỉ số khớp (bằng nhau, rồi một tên là phần đầu của tên kia, rồi chứa nhau), -1 nếu không có.</summary>
        public static int FindMatch(IList<string> names, string wanted, int start)
        {
            if (names == null || string.IsNullOrEmpty(wanted)) return -1;
            for (int i = Mathf.Max(0, start); i < names.Count; i++)
                if (string.Equals(names[i], wanted, StringComparison.OrdinalIgnoreCase)) return i;
            for (int i = Mathf.Max(0, start); i < names.Count; i++)
                if (names[i].StartsWith(wanted, StringComparison.OrdinalIgnoreCase) || wanted.StartsWith(names[i], StringComparison.OrdinalIgnoreCase)) return i;
            for (int i = Mathf.Max(0, start); i < names.Count; i++)
                if (names[i].IndexOf(wanted, StringComparison.OrdinalIgnoreCase) >= 0 || wanted.IndexOf(names[i], StringComparison.OrdinalIgnoreCase) >= 0) return i;
            return -1;
        }

        /// <summary>Không nhận tham số; về micro mặc định của hệ thống.</summary>
        public static void Reset() { Preferred = ""; }
    }
}
