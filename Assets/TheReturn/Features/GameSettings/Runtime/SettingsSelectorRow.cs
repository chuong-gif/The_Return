using System;
using UnityEngine;
using UnityEngine.UI;

namespace TheReturn
{
    /// <summary>Dòng bộ chọn «‹ giá trị ›»: hai nút mũi tên và ô chữ giá trị; danh sách lựa chọn do code đặt lúc chạy.</summary>
    public sealed class SettingsSelectorRow : MonoBehaviour
    {
        public string id;
        public Button previous;
        public Button next;
        public Text valueText;

        string[] options = { "-" };
        int index;
        Action<int> changed;
        bool wired;

        /// <summary>Nhận danh sách lựa chọn, chỉ số đang chọn và hàm xử lý; đặt lại bộ chọn (không bắn sự kiện) và nối hai nút mũi tên một lần.</summary>
        public void Setup(string[] opts, int startIndex, Action<int> onChanged)
        {
            options = opts != null && opts.Length > 0 ? opts : new[] { "-" };
            index = Mathf.Clamp(startIndex, 0, options.Length - 1);
            changed = onChanged;
            Refresh();
            if (wired) return;
            wired = true;
            if (previous != null) previous.onClick.AddListener(() => Step(-1));
            if (next != null) next.onClick.AddListener(() => Step(1));
        }

        /// <summary>Nhận bước ±1; đi vòng quanh danh sách, cập nhật chữ và báo cho hàm xử lý.</summary>
        void Step(int delta)
        {
            index = (index + delta + options.Length) % options.Length;
            Refresh();
            if (changed != null) changed(index);
        }

        /// <summary>Không nhận tham số; ghi lại chữ giá trị theo chỉ số hiện tại.</summary>
        void Refresh() { if (valueText != null) valueText.text = options[index]; }
    }
}
