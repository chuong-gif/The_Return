/*
 * Mục đích: Test NUnit EditMode cho cài đặt người chơi (T-008): giá trị mặc định, cắt khoảng, bất biến, mô hình lưu và bắn sự kiện.
 * Hàm: các test Data_*, Model_*.
 */
using System;
using NUnit.Framework;

namespace TheReturn.Tests
{
    public sealed class GameSettingsTests
    {
        [Test]
        public void Data_DefaultsMatchConstants()
        {
            GameSettingsData d = GameSettingsData.Defaults();
            Assert.AreEqual(GameSettingsData.DefaultVolume, d.Volume);
            Assert.AreEqual(GameSettingsData.DefaultSensitivity, d.MouseSensitivity);
            Assert.AreEqual(GameSettingsData.DefaultFullscreen, d.Fullscreen);
            Assert.AreEqual(80, d.VolumePercent);
        }

        [Test]
        public void Data_ValuesAreClampedToRange()
        {
            var d = new GameSettingsData(5f, 99f, false);
            Assert.AreEqual(1f, d.Volume);
            Assert.AreEqual(GameSettingsData.MaxSensitivity, d.MouseSensitivity);
            var e = new GameSettingsData(-1f, 0f, true);
            Assert.AreEqual(0f, e.Volume);
            Assert.AreEqual(GameSettingsData.MinSensitivity, e.MouseSensitivity);
        }

        [Test]
        public void Data_NotANumberFallsBackToDefaults()
        {
            var d = new GameSettingsData(float.NaN, float.PositiveInfinity, true);
            Assert.AreEqual(GameSettingsData.DefaultVolume, d.Volume);
            Assert.AreEqual(GameSettingsData.DefaultSensitivity, d.MouseSensitivity);
        }

        [Test]
        public void Data_WithMethodsReturnCopiesAndKeepOriginal()
        {
            GameSettingsData a = GameSettingsData.Defaults();
            GameSettingsData b = a.WithVolume(0.3f).WithMouseSensitivity(2f).WithFullscreen(false);
            Assert.AreEqual(GameSettingsData.DefaultVolume, a.Volume);
            Assert.AreEqual(0.3f, b.Volume);
            Assert.AreEqual(2f, b.MouseSensitivity);
            Assert.IsFalse(b.Fullscreen);
            Assert.AreNotEqual(a, b);
        }

        [Test]
        public void Data_EqualityIsByValue()
        {
            Assert.AreEqual(new GameSettingsData(0.5f, 1f, true), new GameSettingsData(0.5f, 1f, true));
            Assert.AreEqual(new GameSettingsData(0.5f, 1f, true).GetHashCode(), new GameSettingsData(0.5f, 1f, true).GetHashCode());
            Assert.AreNotEqual(new GameSettingsData(0.5f, 1f, true), new GameSettingsData(0.6f, 1f, true));
            Assert.IsFalse(new GameSettingsData(0.5f, 1f, true).Equals(null));
        }

        [Test]
        public void Data_VolumePercentRounds()
        {
            Assert.AreEqual(0, new GameSettingsData(0f, 1f, true).VolumePercent);
            Assert.AreEqual(100, new GameSettingsData(1f, 1f, true).VolumePercent);
            Assert.AreEqual(33, new GameSettingsData(0.334f, 1f, true).VolumePercent);
        }

        [Test]
        public void Model_LoadsFromStoreAndRejectsNullStore()
        {
            var store = new InMemorySettingsStore();
            store.Save(new GameSettingsData(0.2f, 1.5f, false));
            var m = new GameSettingsModel(store);
            Assert.AreEqual(0.2f, m.Current.Volume);
            Assert.AreEqual(1.5f, m.Current.MouseSensitivity);
            Assert.IsFalse(m.Current.Fullscreen);
            Assert.Throws<ArgumentNullException>(() => new GameSettingsModel(null));
        }

        [Test]
        public void Model_ChangeSavesImmediatelyAndRaisesChangedOnce()
        {
            var store = new InMemorySettingsStore();
            var m = new GameSettingsModel(store);
            int fired = 0; GameSettingsData last = null;
            m.Changed += d => { fired++; last = d; };
            m.SetVolume(0.5f);
            Assert.AreEqual(1, fired);
            Assert.AreEqual(0.5f, last.Volume);
            Assert.AreEqual(0.5f, store.Load().Volume);
            Assert.AreEqual(1, store.SaveCount);
        }

        [Test]
        public void Model_SameValueDoesNotSaveOrRaise()
        {
            var store = new InMemorySettingsStore();
            var m = new GameSettingsModel(store);
            int fired = 0;
            m.Changed += d => fired++;
            m.SetVolume(GameSettingsData.DefaultVolume);
            m.SetMouseSensitivity(GameSettingsData.DefaultSensitivity);
            m.SetFullscreen(GameSettingsData.DefaultFullscreen);
            Assert.AreEqual(0, fired);
            Assert.AreEqual(0, store.SaveCount);
        }

        [Test]
        public void Model_OutOfRangeValuesAreClampedBeforeSaving()
        {
            var store = new InMemorySettingsStore();
            var m = new GameSettingsModel(store);
            m.SetMouseSensitivity(50f);
            Assert.AreEqual(GameSettingsData.MaxSensitivity, m.Current.MouseSensitivity);
            Assert.AreEqual(GameSettingsData.MaxSensitivity, store.Load().MouseSensitivity);
            m.SetVolume(-3f);
            Assert.AreEqual(0f, m.Current.Volume);
        }

        [Test]
        public void Model_ResetToDefaultsRestoresAllThree()
        {
            var store = new InMemorySettingsStore();
            var m = new GameSettingsModel(store);
            m.SetVolume(0.1f); m.SetMouseSensitivity(2.5f); m.SetFullscreen(false);
            int fired = 0;
            m.Changed += d => fired++;
            m.ResetToDefaults();
            Assert.AreEqual(GameSettingsData.Defaults(), m.Current);
            Assert.AreEqual(GameSettingsData.Defaults(), store.Load());
            Assert.AreEqual(1, fired);
            m.ResetToDefaults();
            Assert.AreEqual(1, fired, "đã mặc định thì không bắn lại");
        }

        [Test]
        public void Store_SaveRejectsNullAndDefaultsWhenEmpty()
        {
            var store = new InMemorySettingsStore();
            Assert.AreEqual(GameSettingsData.Defaults(), store.Load());
            Assert.Throws<ArgumentNullException>(() => store.Save(null));
            Assert.AreEqual(0, store.SaveCount);
        }

        [Test]
        public void Model_ExplicitSaveWritesCurrentValue()
        {
            var store = new InMemorySettingsStore();
            var m = new GameSettingsModel(store);
            m.Save();
            Assert.AreEqual(1, store.SaveCount);
            Assert.AreEqual(m.Current, store.Load());
        }
    }
}
