/*
 * Mục đích: Bảng cue -> clip (T-018). Chủ dự án thay clip thật chỉ bằng cách kéo clip vào bảng này, không sửa code.
 * Hàm: TryGet(cue, out entry) tra cue; Pick(entry) chọn ngẫu nhiên một clip trong danh sách; Rebuild() dựng lại bảng tra khi sửa tay lúc chạy.
 */
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheReturn
{
    /// <summary>ScriptableObject lưu cấu hình từng cue âm thanh.</summary>
    [CreateAssetMenu(fileName = "AudioCueBank", menuName = "The Return/Audio Cue Bank")]
    public sealed class AudioCueBank : ScriptableObject
    {
        /// <summary>Một cue: mã, các clip (chọn ngẫu nhiên), âm lượng, dao động cao độ, kênh, 2D/3D, thời gian nghỉ và lặp.</summary>
        [Serializable]
        public sealed class Entry
        {
            public string cue;
            public AudioClip[] clips;
            [Range(0f, 1f)] public float volume = 1f;
            [Range(0.5f, 2f)] public float pitchMin = 1f;
            [Range(0.5f, 2f)] public float pitchMax = 1f;
            public AudioBus bus = AudioBus.Sfx;
            public bool spatial;
            [Min(0f)] public float cooldown = 0.05f;
            public bool loop;
        }

        public Entry[] entries = new Entry[0];
        Dictionary<string, Entry> map;

        /// <summary>Không nhận tham số; dựng lại bảng tra từ danh sách entries (gọi khi sửa danh sách lúc chạy).</summary>
        public void Rebuild()
        {
            map = new Dictionary<string, Entry>();
            if (entries == null) return;
            foreach (var e in entries) if (e != null && !string.IsNullOrEmpty(e.cue)) map[e.cue] = e;
        }

        /// <summary>Nhận mã cue; trả true và entry nếu có cue đó và có ít nhất một clip.</summary>
        public bool TryGet(string cue, out Entry entry)
        {
            if (map == null) Rebuild();
            if (!string.IsNullOrEmpty(cue) && map.TryGetValue(cue, out entry) && entry.clips != null && entry.clips.Length > 0) return true;
            entry = null; return false;
        }

        /// <summary>Nhận entry; trả một clip ngẫu nhiên khác null, hoặc null nếu không có.</summary>
        public static AudioClip Pick(Entry entry)
        {
            if (entry == null || entry.clips == null || entry.clips.Length == 0) return null;
            for (int tries = 0; tries < 4; tries++)
            {
                var c = entry.clips[UnityEngine.Random.Range(0, entry.clips.Length)];
                if (c != null) return c;
            }
            foreach (var c in entry.clips) if (c != null) return c;
            return null;
        }

        void OnValidate() { map = null; }
    }
}
