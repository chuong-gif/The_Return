/*
 * Mục đích: Dựng scene MainMenu (T-008) bằng code: nền, bốn panel (menu chính, nhập tên/mã, phòng chờ nhóm, Settings), EventSystem, camera,
 * nối mọi tham chiếu vào MainMenuController, lưu scene và đặt MainMenu lên ĐẦU Build Settings.
 * AN TOÀN: KHÔNG có MenuItem; chỉ chạy khi được gọi (MainMenuSceneBuilder.Build). Từ chối ghi đè nếu scene đã tồn tại (trừ khi overwrite = true).
 * Dựng ở chế độ Additive rồi đóng, nên scene đang mở của người dùng không bị thay.
 * Hàm: Build; Resources; Panel; Label; MakeButton; MakeInput; MakeSlider; MakeToggle; Place; AddToBuildSettings.
 */
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TheReturn.MainMenuEditor
{
    /// <summary>Trình dựng scene MainMenu.</summary>
    public static class MainMenuSceneBuilder
    {
        public const string ScenePath = "Assets/TheReturn/Maps/MainMenu/Scenes/MainMenu.unity";

        static readonly Color PanelColor = new Color(.07f, .09f, .14f, .94f);
        static readonly Color ButtonColor = new Color(.16f, .28f, .42f, 1f);
        static readonly Color AccentColor = new Color(.18f, .5f, .35f, 1f);
        static readonly Color WarnColor = new Color(.45f, .2f, .2f, 1f);
        static readonly Color TextColor = new Color(.92f, .96f, 1f, 1f);
        static Font font;

        /// <summary>Nhận cờ ghi đè; dựng và lưu scene MainMenu, trả thông báo kết quả (rỗng nếu bị từ chối vì đã tồn tại).</summary>
        public static string Build(bool overwrite = false)
        {
            if (!overwrite && System.IO.File.Exists(ScenePath)) return "Scene đã tồn tại, không ghi đè: " + ScenePath;
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);

            var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            SceneManager.MoveGameObjectToScene(camGo, scene);
            camGo.tag = "MainCamera";
            var cam = camGo.GetComponent<Camera>(); cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(.03f, .04f, .07f); cam.orthographic = true;
            var es = new GameObject("UI_EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            SceneManager.MoveGameObjectToScene(es, scene);

            var canvasGo = new GameObject("MainMenuCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(MainMenuController));
            SceneManager.MoveGameObjectToScene(canvasGo, scene);
            canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = .5f;
            var c = canvasGo.GetComponent<MainMenuController>();
            DefaultControls.Resources res = Resources_();

            var bg = new GameObject("Background", typeof(RectTransform), typeof(Image)); bg.transform.SetParent(canvasGo.transform, false);
            var bgRect = bg.GetComponent<RectTransform>(); bgRect.anchorMin = Vector2.zero; bgRect.anchorMax = Vector2.one; bgRect.offsetMin = bgRect.offsetMax = Vector2.zero;
            bg.GetComponent<Image>().color = new Color(.03f, .04f, .07f, 1f);

            // ----- Menu chính -----
            RectTransform main = Panel("Panel_Main", canvasGo.transform, new Vector2(1000, 760));
            Label(main, "Title", "THE RETURN", 110, new Vector2(0, 250), new Vector2(900, 140));
            Label(main, "Subtitle", "Tầng học • co-op 2–4 người", 40, new Vector2(0, 150), new Vector2(900, 60));
            c.playButton = MakeButton(res, main, "Btn_Play", "CHƠI", new Vector2(0, 20), new Vector2(560, 100), AccentColor);
            c.settingsButton = MakeButton(res, main, "Btn_Settings", "CÀI ĐẶT", new Vector2(0, -110), new Vector2(560, 100), ButtonColor);
            c.quitButton = MakeButton(res, main, "Btn_Quit", "THOÁT", new Vector2(0, -240), new Vector2(560, 100), WarnColor);
            c.mainPanel = main.gameObject;

            // ----- Nhập tên / mã -----
            RectTransform entry = Panel("Panel_PartyEntry", canvasGo.transform, new Vector2(1100, 860));
            Label(entry, "Title", "NHÓM CHƠI", 72, new Vector2(0, 360), new Vector2(900, 100));
            Label(entry, "NameLabel", "Tên của bạn", 34, new Vector2(0, 270), new Vector2(800, 50));
            c.nameInput = MakeInput(res, entry, "Input_Name", "Nhập tên...", new Vector2(0, 205), new Vector2(640, 70), 16, InputField.CharacterValidation.None);
            c.createButton = MakeButton(res, entry, "Btn_Create", "TẠO PHÒNG", new Vector2(0, 85), new Vector2(560, 90), AccentColor);
            Label(entry, "OrLabel", "hoặc nhập mã phòng của bạn bè", 32, new Vector2(0, -10), new Vector2(900, 50));
            c.codeInput = MakeInput(res, entry, "Input_Code", "MÃ 4 KÝ TỰ", new Vector2(0, -85), new Vector2(360, 80), 4, InputField.CharacterValidation.Alphanumeric);
            c.joinButton = MakeButton(res, entry, "Btn_Join", "VÀO PHÒNG", new Vector2(0, -195), new Vector2(560, 90), ButtonColor);
            c.entryMessage = Label(entry, "Message", "", 30, new Vector2(0, -290), new Vector2(980, 70));
            c.entryBackButton = MakeButton(res, entry, "Btn_Back", "QUAY LẠI", new Vector2(0, -375), new Vector2(360, 70), WarnColor);
            c.partyEntryPanel = entry.gameObject;

            // ----- Phòng chờ -----
            RectTransform room = Panel("Panel_PartyRoom", canvasGo.transform, new Vector2(1300, 980));
            c.roomCodeLabel = Label(room, "CodeLabel", "MÃ PHÒNG: ----", 76, new Vector2(0, 415), new Vector2(1100, 110));
            c.memberRows = new Text[4];
            for (int i = 0; i < 4; i++)
            {
                var row = new GameObject("MemberRow_" + (i + 1), typeof(RectTransform), typeof(Image)); row.transform.SetParent(room, false);
                Place(row.GetComponent<RectTransform>(), new Vector2(0, 295 - i * 85), new Vector2(1100, 74));
                row.GetComponent<Image>().color = new Color(.13f, .17f, .25f, 1f);
                c.memberRows[i] = Label(row.transform, "Text", "", 34, Vector2.zero, new Vector2(1060, 70));
                c.memberRows[i].alignment = TextAnchor.MiddleLeft;
            }
            c.readyButton = MakeButton(res, room, "Btn_Ready", "Sẵn sàng", new Vector2(-300, -75), new Vector2(520, 90), AccentColor);
            c.readyButtonLabel = c.readyButton.GetComponentInChildren<Text>();
            c.leaveButton = MakeButton(res, room, "Btn_Leave", "Rời phòng", new Vector2(300, -75), new Vector2(520, 90), WarnColor);
            c.addSimulatedButton = MakeButton(res, room, "Btn_AddSim", "+ Người giả lập", new Vector2(-300, -180), new Vector2(520, 80), ButtonColor);
            c.removeSimulatedButton = MakeButton(res, room, "Btn_RemoveSim", "− Người giả lập", new Vector2(300, -180), new Vector2(520, 80), ButtonColor);
            c.startButton = MakeButton(res, room, "Btn_Start", "BẮT ĐẦU", new Vector2(0, -300), new Vector2(640, 110), AccentColor);
            c.roomMessage = Label(room, "Message", "", 32, new Vector2(0, -400), new Vector2(1200, 60));
            c.partyRoomPanel = room.gameObject;

            // ----- Settings -----
            RectTransform set = Panel("Panel_Settings", canvasGo.transform, new Vector2(1100, 780));
            Label(set, "Title", "CÀI ĐẶT", 72, new Vector2(0, 310), new Vector2(900, 100));
            c.volumeLabel = Label(set, "VolumeLabel", "Âm lượng: 80%", 38, new Vector2(0, 195), new Vector2(900, 60));
            c.volumeSlider = MakeSlider(res, set, "Slider_Volume", new Vector2(0, 135), new Vector2(760, 40), 0f, 1f);
            c.sensitivityLabel = Label(set, "SensitivityLabel", "Độ nhạy chuột: x1.0", 38, new Vector2(0, 40), new Vector2(900, 60));
            c.sensitivitySlider = MakeSlider(res, set, "Slider_Sensitivity", new Vector2(0, -20), new Vector2(760, 40), 0.2f, 3f);
            c.fullscreenToggle = MakeToggle(res, set, "Toggle_Fullscreen", "Toàn màn hình", new Vector2(0, -130), new Vector2(520, 60));
            c.resetSettingsButton = MakeButton(res, set, "Btn_Reset", "MẶC ĐỊNH", new Vector2(-250, -250), new Vector2(440, 90), ButtonColor);
            c.settingsBackButton = MakeButton(res, set, "Btn_Back", "QUAY LẠI", new Vector2(250, -250), new Vector2(440, 90), AccentColor);
            Label(set, "Note", "Thay đổi được lưu ngay.", 28, new Vector2(0, -340), new Vector2(900, 50));
            c.settingsPanel = set.gameObject;

            Label(canvasGo.transform, "Hint", "Esc: quay lại", 26, new Vector2(0, -500), new Vector2(900, 40));
            entry.gameObject.SetActive(false); room.gameObject.SetActive(false); set.gameObject.SetActive(false);
            EditorUtility.SetDirty(c);

            bool saved = EditorSceneManager.SaveScene(scene, ScenePath);
            EditorSceneManager.CloseScene(scene, true);
            AddToBuildSettings(ScenePath);
            return "Đã dựng và lưu " + ScenePath + " (saved=" + saved + ")";
        }

        /// <summary>Không nhận tham số; trả bộ sprite chuẩn của uGUI cho DefaultControls.</summary>
        static DefaultControls.Resources Resources_()
        {
            return new DefaultControls.Resources
            {
                standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd"),
                background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd"),
                inputField = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/InputFieldBackground.psd"),
                knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd"),
                checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd"),
                mask = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UIMask.psd")
            };
        }

        /// <summary>Nhận tên, cha và kích thước; tạo panel nền giữa màn hình.</summary>
        static RectTransform Panel(string name, Transform parent, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>(); Place(rect, Vector2.zero, size);
            go.GetComponent<Image>().color = PanelColor;
            return rect;
        }

        /// <summary>Nhận cha, tên, chữ, cỡ chữ, vị trí và kích thước; tạo Text căn giữa.</summary>
        static Text Label(Transform parent, string name, string text, int size, Vector2 pos, Vector2 dim)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent, false);
            Place(go.GetComponent<RectTransform>(), pos, dim);
            var t = go.GetComponent<Text>(); t.font = font; t.fontSize = size; t.alignment = TextAnchor.MiddleCenter; t.color = TextColor; t.text = text;
            t.resizeTextForBestFit = true; t.resizeTextMinSize = 12; t.resizeTextMaxSize = size; t.raycastTarget = false;
            return t;
        }

        /// <summary>Nhận tài nguyên, cha, tên, chữ, vị trí, kích thước và màu; tạo nút uGUI chuẩn.</summary>
        static Button MakeButton(DefaultControls.Resources res, Transform parent, string name, string label, Vector2 pos, Vector2 dim, Color color)
        {
            GameObject go = DefaultControls.CreateButton(res); go.name = name; go.transform.SetParent(parent, false);
            Place(go.GetComponent<RectTransform>(), pos, dim);
            go.GetComponent<Image>().color = color;
            var t = go.GetComponentInChildren<Text>(); t.font = font; t.text = label; t.fontSize = 42; t.color = Color.white; t.resizeTextForBestFit = true; t.resizeTextMinSize = 14; t.resizeTextMaxSize = 42;
            return go.GetComponent<Button>();
        }

        /// <summary>Nhận tài nguyên, cha, tên, gợi ý, vị trí, kích thước, giới hạn ký tự và kiểu ký tự; tạo ô nhập.</summary>
        static InputField MakeInput(DefaultControls.Resources res, Transform parent, string name, string hint, Vector2 pos, Vector2 dim, int limit, InputField.CharacterValidation validation)
        {
            GameObject go = DefaultControls.CreateInputField(res); go.name = name; go.transform.SetParent(parent, false);
            Place(go.GetComponent<RectTransform>(), pos, dim);
            var field = go.GetComponent<InputField>(); field.characterLimit = limit; field.characterValidation = validation;
            var text = field.textComponent; text.font = font; text.fontSize = 36; text.alignment = TextAnchor.MiddleCenter; text.color = new Color(.1f, .1f, .1f);
            var ph = field.placeholder as Text; if (ph != null) { ph.font = font; ph.fontSize = 32; ph.text = hint; ph.alignment = TextAnchor.MiddleCenter; }
            return field;
        }

        /// <summary>Nhận tài nguyên, cha, tên, vị trí, kích thước và khoảng giá trị; tạo thanh trượt.</summary>
        static Slider MakeSlider(DefaultControls.Resources res, Transform parent, string name, Vector2 pos, Vector2 dim, float min, float max)
        {
            GameObject go = DefaultControls.CreateSlider(res); go.name = name; go.transform.SetParent(parent, false);
            Place(go.GetComponent<RectTransform>(), pos, dim);
            var s = go.GetComponent<Slider>(); s.minValue = min; s.maxValue = max; s.wholeNumbers = false; s.value = min;
            var handle = go.transform.Find("Handle Slide Area/Handle") as RectTransform; if (handle != null) handle.sizeDelta = new Vector2(40, 0);
            return s;
        }

        /// <summary>Nhận tài nguyên, cha, tên, chữ, vị trí và kích thước; tạo công tắc có nhãn lớn.</summary>
        static Toggle MakeToggle(DefaultControls.Resources res, Transform parent, string name, string label, Vector2 pos, Vector2 dim)
        {
            GameObject go = DefaultControls.CreateToggle(res); go.name = name; go.transform.SetParent(parent, false);
            Place(go.GetComponent<RectTransform>(), pos, dim);
            var bgRect = go.transform.Find("Background") as RectTransform; if (bgRect != null) { bgRect.sizeDelta = new Vector2(52, 52); bgRect.anchoredPosition = new Vector2(30, -30); }
            var t = go.GetComponentInChildren<Text>(); t.font = font; t.text = label; t.fontSize = 38; t.color = TextColor;
            var lr = t.rectTransform; lr.offsetMin = new Vector2(80, 0);
            return go.GetComponent<Toggle>();
        }

        /// <summary>Nhận RectTransform, vị trí và kích thước; đặt neo giữa màn hình.</summary>
        static void Place(RectTransform rect, Vector2 pos, Vector2 dim)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f); rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = pos; rect.sizeDelta = dim;
        }

        /// <summary>Nhận đường dẫn scene; đặt scene lên đầu Build Settings (thêm mới hoặc chuyển lên đầu nếu đã có), giữ nguyên các scene khác.</summary>
        static void AddToBuildSettings(string path)
        {
            var list = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            list.RemoveAll(s => s.path == path);
            list.Insert(0, new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = list.ToArray();
        }
    }
}
