/*
 * Mục đích: Bộ phát âm thanh duy nhất của game (T-018): pool AudioSource, nhạc nền chuyển mượt, âm lượng theo kênh.
 * Hàm: Play/PlayAt/PlayMusic/StopMusic (IAudioPlayer); SetBusVolume/GetBusVolume; Current (bộ phát hiện có hoặc NullAudioPlayer).
 * Âm lượng tổng do GameSettingsRuntime chỉnh qua AudioListener.volume; ở đây chỉ nhân thêm hệ số kênh.
 */
using System.Collections.Generic;
using UnityEngine;

namespace TheReturn
{
    /// <summary>MonoBehaviour phát cue theo AudioCueBank; một object trong scene.</summary>
    public sealed class AudioService : MonoBehaviour, IAudioPlayer
    {
        public AudioCueBank bank;
        [Min(4)] public int poolSize = 12;
        [Min(1f)] public float spatialMaxDistance = 30f;
        [Min(0.1f)] public float musicFadeSeconds = 1.5f;
        public float[] busVolume = { 1f, 1f, 0.6f, 1f, 0.7f };

        readonly AudioCueRules rules = new AudioCueRules(16);
        readonly List<AudioSource> pool = new List<AudioSource>();
        readonly HashSet<string> missing = new HashSet<string>();
        readonly AudioSource[] music = new AudioSource[2];
        int musicIndex;
        string musicCue = "";
        AudioSource ambience;
        string ambienceCue = "";
        float ambienceCueVolume;
        float musicCueVolume;
        public static AudioService Instance { get; private set; }

        /// <summary>Bộ phát hiện có: AudioService đang sống, không thì bộ phát rỗng (gọi an toàn ở mọi nơi).</summary>
        public static IAudioPlayer Current { get { return Instance != null ? (IAudioPlayer)Instance : NullAudioPlayer.Instance; } }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            AudioHub.Set(this);
            ambience = MakeSource("Ambience"); ambience.loop = true; ambience.volume = 0f;
            for (int i = 0; i < poolSize; i++) pool.Add(MakeSource("Sfx_" + i));
            for (int i = 0; i < 2; i++) { music[i] = MakeSource("Music_" + i); music[i].loop = true; music[i].volume = 0f; }
        }

        void OnDestroy() { if (Instance == this) { Instance = null; AudioHub.Clear(this); } }

        AudioSource MakeSource(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var s = go.AddComponent<AudioSource>();
            s.playOnAwake = false;
            return s;
        }

        /// <summary>Nhận kênh; trả hệ số âm lượng của kênh (0..1).</summary>
        public float GetBusVolume(AudioBus bus) { int i = (int)bus; return i >= 0 && i < busVolume.Length ? busVolume[i] : 1f; }

        /// <summary>Nhận kênh và giá trị; đặt hệ số âm lượng kênh (kẹp 0..1); nhạc đang phát cập nhật ở khung hình kế.</summary>
        public void SetBusVolume(AudioBus bus, float value)
        {
            int i = (int)bus; if (i < 0 || i >= busVolume.Length) return;
            busVolume[i] = Mathf.Clamp01(value);
        }

        /// <summary>Nhận mã cue; phát 2D.</summary>
        public void Play(string cue) { Emit(cue, false, Vector3.zero); }

        /// <summary>Nhận mã cue và tọa độ; phát 3D tại vị trí đó.</summary>
        public void PlayAt(string cue, float x, float y, float z) { Emit(cue, true, new Vector3(x, y, z)); }

        void Emit(string cue, bool at, Vector3 pos)
        {
            if (bank == null || string.IsNullOrEmpty(cue)) return;
            AudioCueBank.Entry e;
            if (!bank.TryGet(cue, out e)) { if (missing.Add(cue)) Debug.LogWarning("AudioService: chưa có clip cho cue '" + cue + "'"); return; }
            if (!rules.TryAllow(cue, Time.unscaledTimeAsDouble, e.cooldown)) return;
            var clip = AudioCueBank.Pick(e); if (clip == null) return;
            var s = FreeSource();
            var cam = Camera.main;
            s.transform.position = at ? pos : (cam != null ? cam.transform.position : transform.position);
            s.clip = clip; s.loop = e.loop;
            s.spatialBlend = at || e.spatial ? 1f : 0f;
            s.maxDistance = spatialMaxDistance; s.rolloffMode = AudioRolloffMode.Linear;
            s.pitch = Random.Range(Mathf.Min(e.pitchMin, e.pitchMax), Mathf.Max(e.pitchMin, e.pitchMax));
            s.volume = e.volume * GetBusVolume(e.bus);
            s.Play();
        }

        AudioSource FreeSource()
        {
            foreach (var s in pool) if (!s.isPlaying) return s;
            AudioSource oldest = pool[0]; float best = -1f;
            foreach (var s in pool) if (s.time > best) { best = s.time; oldest = s; }
            oldest.Stop(); return oldest;
        }

        /// <summary>Nhận mã cue nhạc; chuyển mượt sang nhạc đó (cùng cue thì giữ nguyên; rỗng hoặc thiếu clip thì tắt dần).</summary>
        public void PlayMusic(string cue)
        {
            cue = cue ?? "";
            if (cue == musicCue) return;
            AudioCueBank.Entry e = null;
            if (cue.Length > 0 && (bank == null || !bank.TryGet(cue, out e)))
            {
                if (bank != null && missing.Add(cue)) Debug.LogWarning("AudioService: chưa có clip nhạc '" + cue + "'");
                cue = ""; e = null;
            }
            musicCue = cue;
            musicIndex = 1 - musicIndex;
            if (e != null)
            {
                var next = music[musicIndex];
                next.clip = AudioCueBank.Pick(e); next.spatialBlend = 0f; next.loop = true; next.pitch = 1f; next.volume = 0f;
                musicCueVolume = e.volume;
                next.Play();
            }
            else musicCueVolume = 0f;
        }

        /// <summary>Không nhận tham số; tắt nhạc nền dần.</summary>
        public void StopMusic() { PlayMusic(""); }

        /// <summary>Nhận mã cue nền môi trường (lặp); cùng cue thì giữ, rỗng hoặc thiếu clip thì tắt dần.</summary>
        public void PlayAmbience(string cue)
        {
            cue = cue ?? "";
            if (cue == ambienceCue) return;
            AudioCueBank.Entry e = null;
            if (cue.Length > 0 && (bank == null || !bank.TryGet(cue, out e))) { if (bank != null && missing.Add(cue)) Debug.LogWarning("AudioService: chưa có clip nền '" + cue + "'"); cue = ""; e = null; }
            ambienceCue = cue;
            ambienceCueVolume = e != null ? e.volume : 0f;
            if (e != null) { ambience.clip = AudioCueBank.Pick(e); ambience.spatialBlend = 0f; ambience.loop = true; ambience.volume = 0f; ambience.Play(); }
        }

        void Update()
        {
            float step = Time.unscaledDeltaTime / musicFadeSeconds;
            float live = musicCueVolume * GetBusVolume(AudioBus.Music);
            float ambTarget = ambienceCueVolume * GetBusVolume(AudioBus.Ambience);
            ambience.volume = Mathf.MoveTowards(ambience.volume, ambTarget, step);
            if (ambience.isPlaying && ambience.volume <= 0f && ambTarget <= 0f) ambience.Stop();
            for (int i = 0; i < 2; i++)
            {
                float target = i == musicIndex ? live : 0f;
                music[i].volume = Mathf.MoveTowards(music[i].volume, target, step);
                if (music[i].isPlaying && music[i].volume <= 0f && target <= 0f) music[i].Stop();
            }
        }
    }
}
