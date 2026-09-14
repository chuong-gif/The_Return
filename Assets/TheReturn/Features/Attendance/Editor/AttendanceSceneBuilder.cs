/*
 * Mục đích: Dựng lớp học logic 24 chỗ từ các prefab dùng lại, không xây cảnh trang trí.
 * Danh sách hàm:
 * - Build: tạo lại scene điểm danh và gán các tham chiếu.
 * - CreateReusablePrefabs: tạo block, bàn ghế, cửa và prefab điểm danh nếu chưa có.
 * - SavePrefab: lưu đối tượng tạm thành prefab rồi hủy đối tượng tạm.
 * - Spawn: tạo prefab instance liên kết asset.
 * - Cube / Label: tạo hình khối hoặc nhãn phục vụ logic.
 * - Material: lấy hoặc tạo vật liệu graybox.
 * - CreatePlayers: dựng bốn slot điều khiển dùng trong bản thử.
 * - CreateCatalog: tạo asset text lần đầu, giữ text đã sửa trong các lần sau.
 */
using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace TheReturn.Editor
{
    public static class AttendanceSceneBuilder
    {
        const string Root = "Assets/TheReturn/";
        const string Feature = Root + "Features/Attendance/";
        const string Shared = Root + "Shared/";
        public const string ScenePath = Feature + "Scenes/AttendancePrototype.unity";
        static Material surface, interaction, dark, textMaterial;
        static GameObject blockPrefab, deskPrefab, seatPrefab, doorPrefab;

        /// <summary>Không nhận tham số; tạo scene 6×4 và lưu. Từ chối khi Play hoặc có scene đang sửa chưa lưu.</summary>
        [MenuItem("The Return/Attendance/Rebuild Logic Classroom")]
        public static void Build()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Stop Play mode first.");
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Save your scene changes before rebuilding.");
            Scene previous = SceneManager.GetActiveScene();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            if (previous.path == ScenePath) EditorSceneManager.CloseScene(previous, true);

            surface = Material("M_GB_Surface", new Color(.54f, .58f, .59f));
            interaction = Material("M_GB_Interaction", new Color(.22f, .35f, .38f));
            dark = Material("M_GB_Board", new Color(.08f, .13f, .15f));
            textMaterial = AssetDatabase.LoadAssetAtPath<Material>(Shared + "Art/Materials/Graybox/WorldText.mat");
            CreateReusablePrefabs();

            var environment = new GameObject("Environment_LogicOnly").transform;
            // Một sàn, các tường biên và lối ra; không cửa sổ, gạch lát, tủ hoặc vật trang trí.
            Spawn(blockPrefab, "Floor", new Vector3(0, -.2f, -2), new Vector3(14, .4f, 23), environment);
            Spawn(blockPrefab, "Wall_Left", new Vector3(-7, 1.8f, -2), new Vector3(.2f, 3.6f, 23), environment);
            Spawn(blockPrefab, "Wall_Right", new Vector3(7, 1.8f, -2), new Vector3(.2f, 3.6f, 23), environment);
            Spawn(blockPrefab, "Wall_Back", new Vector3(0, 1.8f, -13.5f), new Vector3(14, 3.6f, .2f), environment);
            Spawn(blockPrefab, "Wall_Front", new Vector3(-1, 1.8f, 8), new Vector3(12, 3.6f, .2f), environment);
            Spawn(blockPrefab, "Wall_DoorEdge", new Vector3(6.8f, 1.8f, 8), new Vector3(.4f, 3.6f, .2f), environment);
            Spawn(blockPrefab, "Wall_DoorTop", new Vector3(5.8f, 3.35f, 8), new Vector3(1.6f, .5f, .2f), environment);

            var puzzleRoot = new GameObject("Puzzle_Attendance").transform;
            var game = puzzleRoot.gameObject.AddComponent<AttendancePrototype>();
            game.textCatalog = CreateCatalog();
            game.seats = new AttendanceSeat[AttendanceRound.SeatCount];
            var seating = new GameObject("Seating_6x4").transform;
            seating.SetParent(puzzleRoot);
            for (int row = 0; row < AttendanceRound.Rows; row++)
            {
                for (int column = 0; column < AttendanceRound.Columns; column++)
                {
                    int id = row * AttendanceRound.Columns + column;
                    Vector3 position = new Vector3(-4.5f + column * 3f, 0, 5 - row * 2.7f);
                    GameObject instance = Spawn(seatPrefab, AttendanceRound.SeatCode(id), position, Vector3.one, seating);
                    var seat = instance.GetComponent<AttendanceSeat>();
                    seat.Configure(id);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(seat);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(seat.label);
                    game.seats[id] = seat;
                }
                Label("Row_" + (row + 1), "H" + (row + 1),
                    new Vector3(-6.65f, 1.4f, 5 - row * 2.7f), .065f, puzzleRoot);
            }

            GameObject board = Cube("InstructionBoard", new Vector3(-.5f, 2.1f, 7.8f), new Vector3(7.8f, 2, .15f), dark, puzzleRoot);
            game.board = Label("Instructions", "LOP 101 / 24 GHE\nH1 GAN BANG / C1 BEN TRAI\nDOI 2 - 4 NGUOI",
                new Vector3(-.5f, 2.1f, 7.7f), .052f, puzzleRoot);
            game.exitDoor = Spawn(doorPrefab, "ExitDoor", new Vector3(5.8f, 0, 8), Vector3.one, puzzleRoot).GetComponent<PuzzleDoor>();
            Label("ExitLabel", "EXIT", new Vector3(5.8f, 3.1f, 7.8f), .065f, puzzleRoot);
            var exit = new GameObject("ExitZone");
            exit.transform.SetParent(puzzleRoot);
            exit.transform.position = new Vector3(5.8f, 1.5f, 8.8f);
            game.exitZone = exit.AddComponent<BoxCollider>();
            game.exitZone.size = new Vector3(1.5f, 3, 1);
            game.exitZone.isTrigger = true;

            game.party = CreatePlayers(game.textCatalog);
            var hud = new GameObject("AttendanceHUD").AddComponent<AttendanceHud>();
            hud.session = game;
            var sun = new GameObject("Directional Light").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.1f;
            sun.transform.rotation = Quaternion.Euler(45, -25, 0);
            sun.shadows = LightShadows.Soft;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.55f, .57f, .60f);
            RenderSettings.fog = false;
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Selection.activeObject = game.textCatalog;
            Debug.Log("ATTENDANCE_CLASSROOM_READY: 24 nested prefab instances, 2–4 participants.");
        }

        /// <summary>Không nhận tham số; lấy hoặc tạo các prefab. Prefab đã chỉnh tay được giữ nguyên.</summary>
        static void CreateReusablePrefabs()
        {
            string sharedPath = Shared + "Prefabs/Graybox/";
            blockPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(sharedPath + "PF_GB_Block.prefab");
            if (blockPrefab == null)
                blockPrefab = SavePrefab(Cube("PF_GB_Block", Vector3.zero, Vector3.one, surface), sharedPath + "PF_GB_Block.prefab");

            deskPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(sharedPath + "PF_GB_DeskChair.prefab");
            if (deskPrefab == null)
            {
                var root = new GameObject("PF_GB_DeskChair");
                Cube("Desk", new Vector3(0, .8f, .65f), new Vector3(1.45f, .12f, .7f), surface, root.transform);
                Cube("DeskSupport", new Vector3(0, .37f, .65f), new Vector3(.75f, .74f, .3f), surface, root.transform);
                Cube("ChairSeat", new Vector3(0, .45f, -.3f), new Vector3(.65f, .12f, .6f), surface, root.transform);
                Cube("ChairBack", new Vector3(0, .86f, -.64f), new Vector3(.65f, .8f, .08f), surface, root.transform);
                Cube("ChairSupport", new Vector3(0, .2f, -.3f), new Vector3(.35f, .4f, .35f), surface, root.transform);
                deskPrefab = SavePrefab(root, sharedPath + "PF_GB_DeskChair.prefab");
            }

            seatPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Feature + "Prefabs/PF_AttendanceSeat.prefab");
            if (seatPrefab == null)
            {
                var root = new GameObject("PF_AttendanceSeat");
                Spawn(deskPrefab, "Visual_Replaceable", Vector3.zero, Vector3.one, root.transform);
                var seat = root.AddComponent<AttendanceSeat>();
                seat.sitPoint = new GameObject("SitPoint").transform;
                seat.sitPoint.SetParent(root.transform);
                seat.sitPoint.localPosition = new Vector3(0, .04f, -.25f);
                seat.standPoint = new GameObject("StandPoint").transform;
                seat.standPoint.SetParent(root.transform);
                seat.standPoint.localPosition = new Vector3(0, .04f, -1.1f);
                seat.indicator = Cube("StatusPlate", new Vector3(0, 1.03f, .84f), new Vector3(.85f, .32f, .05f), interaction, root.transform).GetComponent<Renderer>();
                seat.label = Label("SeatAddress", "H1-C1", new Vector3(0, 1.04f, .80f), .020f, root.transform);
                seatPrefab = SavePrefab(root, Feature + "Prefabs/PF_AttendanceSeat.prefab");
            }

            doorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(sharedPath + "PF_GB_SlidingDoor.prefab");
            if (doorPrefab == null)
            {
                var root = new GameObject("PF_GB_SlidingDoor");
                var door = root.AddComponent<PuzzleDoor>();
                door.panel = Cube("Panel", new Vector3(0, 1.45f, 0), new Vector3(1.55f, 2.9f, .18f), interaction, root.transform).transform;
                doorPrefab = SavePrefab(root, sharedPath + "PF_GB_SlidingDoor.prefab");
            }
        }

        /// <summary>Nhận đối tượng tạm và đường dẫn; trả prefab asset sau khi lưu, hủy đối tượng tạm trong scene.</summary>
        static GameObject SavePrefab(GameObject temporary, string path)
        {
            GameObject asset = PrefabUtility.SaveAsPrefabAsset(temporary, path);
            UnityEngine.Object.DestroyImmediate(temporary);
            return asset;
        }

        /// <summary>Nhận prefab và transform đích; trả instance có liên kết prefab, có thể thay visual hàng loạt.</summary>
        static GameObject Spawn(GameObject prefab, string name, Vector3 position, Vector3 scale, Transform parent = null)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = name;
            instance.transform.SetParent(parent);
            instance.transform.position = position;
            instance.transform.localScale = scale;
            return instance;
        }

        /// <summary>Nhận tên, pose, vật liệu và parent; trả cube có collider dùng để dựng prefab logic.</summary>
        static GameObject Cube(string name, Vector3 position, Vector3 scale, Material material, Transform parent = null)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            return go;
        }

        /// <summary>Nhận nội dung, vị trí, kích thước và parent; trả nhãn có kiểm tra chiều sâu, không xuyên tường.</summary>
        static TextMesh Label(string name, string value, Vector3 position, float size, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = position;
            var text = go.AddComponent<TextMesh>();
            text.text = value;
            text.fontSize = 64;
            text.characterSize = size;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.color = Color.white;
            go.GetComponent<MeshRenderer>().sharedMaterial = textMaterial;
            return text;
        }

        /// <summary>Nhận tên và màu; lấy asset hiện có hoặc tạo material URP, trả tham chiếu dùng chung.</summary>
        static Material Material(string name, Color color)
        {
            string path = Shared + "Art/Materials/Graybox/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new InvalidOperationException("URP Lit shader missing.");
            material = new Material(shader) { name = name, color = color };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        /// <summary>Nhận catalog tên; trả bộ điều khiển có 4 slot và camera, số slot hoạt động do session quyết định.</summary>
        static PrototypePartyController CreatePlayers(AttendanceTextCatalog catalog)
        {
            var controller = new GameObject("LocalParty").AddComponent<PrototypePartyController>();
            controller.players = new Transform[4];
            controller.bodies = new Renderer[4];
            for (int i = 0; i < 4; i++)
            {
                var player = new GameObject("PlayerSlot_" + (i + 1)).transform;
                player.SetParent(controller.transform);
                player.position = new Vector3(-4.5f + i * 3, .05f, -11.5f);
                var cc = player.gameObject.AddComponent<CharacterController>();
                cc.height = 1.75f;
                cc.radius = .27f;
                cc.center = Vector3.up * .9f;
                cc.stepOffset = .25f;
                var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                body.name = "Visual_Replaceable";
                body.transform.SetParent(player, false);
                body.transform.localPosition = Vector3.up * .9f;
                body.transform.localScale = new Vector3(.5f, .85f, .5f);
                UnityEngine.Object.DestroyImmediate(body.GetComponent<Collider>());
                body.GetComponent<Renderer>().sharedMaterial = interaction;
                controller.players[i] = player;
                controller.bodies[i] = body.GetComponent<Renderer>();
            }
            var camera = new GameObject("Main Camera").AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.fieldOfView = 70;
            camera.nearClipPlane = .05f;
            camera.transform.position = controller.players[0].position + Vector3.up * 1.65f;
            camera.gameObject.AddComponent<AudioListener>();
            controller.viewCamera = camera;
            return controller;
        }

        /// <summary>Không nhận tham số; lấy catalog đã sửa hoặc tạo mặc định lần đầu, không ghi đè text của người dùng.</summary>
        static AttendanceTextCatalog CreateCatalog()
        {
            string path = Feature + "Data/AttendanceText_VI.asset";
            var catalog = AssetDatabase.LoadAssetAtPath<AttendanceTextCatalog>(path);
            if (catalog != null) return catalog;
            catalog = ScriptableObject.CreateInstance<AttendanceTextCatalog>();
            catalog.InitializeDefaults();
            AssetDatabase.CreateAsset(catalog, path);
            return catalog;
        }
    }
}
