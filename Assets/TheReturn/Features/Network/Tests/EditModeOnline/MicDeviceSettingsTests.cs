/*
 * Mục đích: Test lựa chọn micro thu âm (T-055): so khớp tên mềm giữa tên Unity và tên Vivox, lưu/đọc lựa chọn, sự kiện Changed, danh sách ưu tiên nguồn Vivox.
 */
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace TheReturn.Network.Online.Tests
{
    public class MicDeviceSettingsTests
    {
        private const string Key = "TheReturn.Set.MicDevice";
        private string saved;

        [SetUp]
        public void SetUp()
        {
            saved = PlayerPrefs.GetString(Key, "");
            PlayerPrefs.DeleteKey(Key);
            MicDeviceSettings.SetExternalNames(null);
        }

        [TearDown]
        public void TearDown()
        {
            if (string.IsNullOrEmpty(saved)) PlayerPrefs.DeleteKey(Key); else PlayerPrefs.SetString(Key, saved);
            MicDeviceSettings.SetExternalNames(null);
        }

        private static readonly string[] Names = { "Microphone (Realtek(R) Audio)", "Microphone Array (Intel® Smart Sound Technology for Digital Microphones)", "Default System Device" };

        [Test]
        public void FindMatch_ExactName_IgnoresCase()
        {
            Assert.AreEqual(0, MicDeviceSettings.FindMatch(Names, "microphone (realtek(r) audio)", 0));
        }

        [Test]
        public void FindMatch_UnityTruncatedName_MatchesFullVivoxName()
        {
            Assert.AreEqual(1, MicDeviceSettings.FindMatch(Names, "Microphone Array (Intel® Smart Sound", 0));
        }

        [Test]
        public void FindMatch_Unknown_Or_Empty_ReturnsMinusOne()
        {
            Assert.AreEqual(-1, MicDeviceSettings.FindMatch(Names, "Tai nghe Bluetooth", 0));
            Assert.AreEqual(-1, MicDeviceSettings.FindMatch(Names, "", 0));
            Assert.AreEqual(-1, MicDeviceSettings.FindMatch(null, "x", 0));
        }

        [Test]
        public void FindMatch_RespectsStartIndex()
        {
            Assert.AreEqual(-1, MicDeviceSettings.FindMatch(Names, "Microphone (Realtek(R) Audio)", 1));
        }

        [Test]
        public void Preferred_DefaultsToEmpty_AndPersists()
        {
            Assert.AreEqual("", MicDeviceSettings.Preferred);
            MicDeviceSettings.Preferred = "Mic máy";
            Assert.AreEqual("Mic máy", MicDeviceSettings.Preferred);
            Assert.AreEqual("Mic máy", PlayerPrefs.GetString(Key, ""));
        }

        [Test]
        public void Preferred_RaisesChanged_OnlyWhenValueActuallyChanges()
        {
            int count = 0;
            System.Action handler = () => count++;
            MicDeviceSettings.Changed += handler;
            try
            {
                MicDeviceSettings.Preferred = "A";
                MicDeviceSettings.Preferred = "A";
                MicDeviceSettings.Preferred = "B";
                MicDeviceSettings.Reset();
                Assert.AreEqual(3, count);
                Assert.AreEqual("", MicDeviceSettings.Preferred);
            }
            finally { MicDeviceSettings.Changed -= handler; }
        }

        [Test]
        public void AvailableNames_PrefersVivoxList_WhenSet()
        {
            MicDeviceSettings.SetExternalNames(new List<string> { "Mic Vivox 1", "Mic Vivox 2" });
            List<string> names = MicDeviceSettings.AvailableNames();
            Assert.AreEqual(2, names.Count);
            Assert.AreEqual("Mic Vivox 1", names[0]);
            names.Add("sửa bản sao");
            Assert.AreEqual(2, MicDeviceSettings.AvailableNames().Count);
        }

        [Test]
        public void SetExternalNames_NullOrEmpty_FallsBackToUnityDevices()
        {
            MicDeviceSettings.SetExternalNames(new List<string> { "X" });
            MicDeviceSettings.SetExternalNames(new List<string>());
            Assert.AreEqual(Microphone.devices.Length, MicDeviceSettings.AvailableNames().Count);
        }
    }
}
