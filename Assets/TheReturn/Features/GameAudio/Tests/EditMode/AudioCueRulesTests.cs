using NUnit.Framework;

namespace TheReturn.Tests
{
    public sealed class AudioCueRulesTests
    {
        [Test]
        public void Cooldown_BlocksRepeatThenAllows()
        {
            var r = new AudioCueRules();
            Assert.IsTrue(r.TryAllow(AudioCues.Step, 0.0, 0.3f));
            Assert.IsFalse(r.TryAllow(AudioCues.Step, 0.1, 0.3f));
            Assert.IsTrue(r.TryAllow(AudioCues.Step, 0.35, 0.3f));
        }

        [Test]
        public void DifferentCues_DoNotBlockEachOther()
        {
            var r = new AudioCueRules();
            Assert.IsTrue(r.TryAllow(AudioCues.Step, 0.0, 1f));
            Assert.IsTrue(r.TryAllow(AudioCues.UiClick, 0.0, 1f));
        }

        [Test]
        public void ZeroCooldown_AlwaysAllowedUntilRateLimit()
        {
            var r = new AudioCueRules(3);
            Assert.IsTrue(r.TryAllow("a", 0.0, 0f));
            Assert.IsTrue(r.TryAllow("a", 0.0, 0f));
            Assert.IsTrue(r.TryAllow("a", 0.0, 0f));
            Assert.IsFalse(r.TryAllow("a", 0.5, 0f));
            Assert.IsTrue(r.TryAllow("a", 1.2, 0f));
        }

        [Test]
        public void EmptyCue_IsRejected()
        {
            var r = new AudioCueRules();
            Assert.IsFalse(r.TryAllow("", 0, 0f));
            Assert.IsFalse(r.TryAllow(null, 0, 0f));
        }

        [Test]
        public void Reset_ClearsHistory()
        {
            var r = new AudioCueRules();
            r.TryAllow("x", 0, 5f);
            Assert.IsFalse(r.TryAllow("x", 1, 5f));
            r.Reset();
            Assert.IsTrue(r.TryAllow("x", 1, 5f));
        }

        [Test]
        public void NullPlayer_DoesNothing()
        {
            IAudioPlayer p = NullAudioPlayer.Instance;
            p.Play(AudioCues.UiClick); p.PlayAt(AudioCues.Step, 1, 2, 3); p.PlayMusic(AudioCues.MusicYard); p.StopMusic();
            Assert.Pass();
        }
    }
}
