using System;
using UnityEngine;
using UnityEngine.UI;

namespace TheReturn
{
    /// <summary>Dòng đổi phím: hành động, nút bấm để gán và ô chữ hiện tên phím.</summary>
    public sealed class SettingsKeyRow : MonoBehaviour
    {
        public GameAction action;
        public Button button;
        public Text label;
    }
}
