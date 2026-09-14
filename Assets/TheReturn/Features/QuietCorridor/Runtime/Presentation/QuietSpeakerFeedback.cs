/*
 * Mục đích: Phản hồi âm thanh cho loa rè, không đọc microphone; có thể thay bằng AudioClip riêng.
 * Hàm: Awake chuẩn bị nguồn và âm mẫu; Update bật/tắt theo luật; OnDestroy giải phóng âm tạo tạm.
 */
using UnityEngine;
namespace TheReturn
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class QuietSpeakerFeedback : MonoBehaviour
    {
        public QuietCorridorPrototype session;
        public AudioClip replacementClip;
        [Range(0,1)] public float volume = .08f;
        AudioSource source;
        AudioClip generated;
        /// <summary>Không có đầu vào; cấu hình nguồn 3D và tạo âm rè mẫu nếu chưa gán clip.</summary>
        void Awake()
        {
            source = GetComponent<AudioSource>();
            source.playOnAwake = false; source.loop = true; source.volume = volume;
            source.spatialBlend = .8f; source.minDistance = 4; source.maxDistance = 35;
            source.rolloffMode = AudioRolloffMode.Linear;
            if (replacementClip != null) source.clip = replacementClip;
            else
            {
                const int rate = 22050;
                float[] samples = new float[rate];
                for (int i=0;i<samples.Length;i++)
                    samples[i] = .3f * Mathf.Sin(2*Mathf.PI*100*i/rate) + .15f*Mathf.Sin(2*Mathf.PI*173*i/rate);
                generated = AudioClip.Create("Prototype_Speaker_Buzz",rate,1,rate,false);
                generated.SetData(samples,0); source.clip=generated;
            }
        }
        /// <summary>Không có đầu vào; chỉ phát khi loa đang bật và game đang chơi, dừng trong menu.</summary>
        void Update()
        {
            bool play = session.Active && session.State != null && session.State.SpeakerOn &&
                Cursor.lockState == CursorLockMode.Locked;
            if (play && !source.isPlaying) source.Play();
            if (!play && source.isPlaying) source.Stop();
        }
        /// <summary>Không có đầu vào; giải phóng clip tạm, giữ nguyên clip asset do người dùng gán.</summary>
        void OnDestroy() { if (generated != null) Destroy(generated); }
    }
}
