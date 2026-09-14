/*
 * Mục đích: Thêm hành lang vào lớp 24 bàn trong cùng scene, dùng prefab graybox.
 * Hàm: Build kiểm tra và dựng phần nối; Block tạo khối prefab; Label tạo nhãn;
 * Zone tạo trigger; StationPrefab tạo/lấy nút dùng lại.
 */
using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
namespace TheReturn.Editor
{
 public static class QuietCorridorSceneBuilder
 {
  const string Feature="Assets/TheReturn/Features/QuietCorridor/";
  const string Shared="Assets/TheReturn/Shared/";
  static Transform root;
  static GameObject block;
  static Material textMaterial;
  /// <summary>Không có đầu vào; thêm hành lang và lưu scene. Từ chối Play, cảnh chưa lưu hoặc đã có hành lang.</summary>
  [MenuItem("The Return/School Floor/Add Quiet Corridor")]
  public static void Build()
  {
   var scene=SceneManager.GetActiveScene();
   if(Application.isPlaying || scene.isDirty) throw new InvalidOperationException("Stop Play and save scene first.");
   if(UnityEngine.Object.FindFirstObjectByType<SchoolFloorFlow>()!=null) throw new InvalidOperationException("Corridor already exists.");
   var attendance=UnityEngine.Object.FindFirstObjectByType<AttendancePrototype>();
   if(attendance==null || attendance.seats.Length!=24) throw new InvalidOperationException("Open the 24-seat classroom.");
   Directory.CreateDirectory(Feature+"Data"); Directory.CreateDirectory(Feature+"Prefabs"); AssetDatabase.Refresh();
   block=AssetDatabase.LoadAssetAtPath<GameObject>(Shared+"Prefabs/Graybox/PF_GB_Block.prefab");
   textMaterial=AssetDatabase.LoadAssetAtPath<Material>(Shared+"Art/Materials/Graybox/WorldText.mat");
   root=new GameObject("QuietCorridor_LogicOnly").transform;
   Block("Floor",new Vector3(5.8f,-.2f,23.7f),new Vector3(4.4f,.4f,28.6f));
   Block("Wall_Left",new Vector3(3.6f,1.8f,23.7f),new Vector3(.2f,3.6f,28.6f));
   Block("Wall_Right",new Vector3(8,1.8f,23.7f),new Vector3(.2f,3.6f,28.6f));
   Block("Wall_End",new Vector3(5.8f,1.8f,38),new Vector3(4.4f,3.6f,.2f));
   Block("Connector_LeftCap",new Vector3(-1.1f,1.8f,9.4f),new Vector3(11.8f,3.6f,.2f));
   Block("Connector_RightCap",new Vector3(6.9f,1.8f,9.4f),new Vector3(.2f,3.6f,.2f));
   var game=root.gameObject.AddComponent<QuietCorridorPrototype>(); game.party=attendance.party;
   string data=Feature+"Data/QuietCorridor_VI.asset";
   game.settings=AssetDatabase.LoadAssetAtPath<QuietCorridorSettings>(data);
   if(game.settings==null) { game.settings=ScriptableObject.CreateInstance<QuietCorridorSettings>(); AssetDatabase.CreateAsset(game.settings,data); }
   var prefab=StationPrefab(); game.stations=new QuietHoldStation[2];
   for(int i=0;i<2;i++)
   {
    var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab);
    go.name=i==0?"Station_A_Near":"Station_B_Far"; go.transform.SetParent(root);
    go.transform.position=new Vector3(3.85f,1.3f,i==0?13:33); go.transform.rotation=Quaternion.Euler(0,-90,0);
    game.stations[i]=go.GetComponent<QuietHoldStation>(); game.stations[i].index=i;
    game.stations[i].label.text=i==0?"A / E: GIU":"B / E: GIU";
    PrefabUtility.RecordPrefabInstancePropertyModifications(game.stations[i]);
    PrefabUtility.RecordPrefabInstancePropertyModifications(game.stations[i].label);
   }
   var door=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Shared+"Prefabs/Graybox/PF_GB_SlidingDoor.prefab"));
   door.name="AcousticGate"; door.transform.SetParent(root); door.transform.position=new Vector3(5.8f,0,29);
   door.transform.localScale=new Vector3(2.8f,1,1); game.gate=door.GetComponent<PuzzleDoor>();
   Block("GateHeader",new Vector3(5.8f,3.3f,29),new Vector3(4.4f,.6f,.3f));
   Label("GateLabel","CUA CACH AM / GIU NUT DE MO",new Vector3(5.8f,3.3f,28.8f),.031f);
   game.safetyZone=Zone("Gate_AntiCrush",new Vector3(5.8f,1.5f,29),new Vector3(4.6f,3.2f,1.8f));
   game.goalZone=Zone("Team_Goal",new Vector3(5.8f,1.5f,34.5f),new Vector3(4.1f,3.5f,7));
   game.checkpoints=new Transform[4];
   for(int i=0;i<4;i++)
   {
    var point=new GameObject("Checkpoint_"+(i+1)).transform; point.SetParent(root);
    point.position=new Vector3(i%2==0?4.8f:6.7f,.05f,10.5f+(i/2)*1.5f); game.checkpoints[i]=point;
   }
   game.speakerIndicator=Block("Speaker",new Vector3(5.8f,2.7f,19),new Vector3(.8f,.5f,.35f)).GetComponent<Renderer>();
   game.speakerIndicator.gameObject.AddComponent<QuietSpeakerFeedback>().session=game;
   Label("SpeakerLabel","LOA RE",new Vector3(5.8f,2.7f,18.8f),.03f);
   Label("StagingLabel","TAP HOP / DOI 2 - 4 NGUOI",new Vector3(5.8f,2.7f,15),.036f);
   Label("GoalLabel","VUNG DICH / CHO DU CA DOI",new Vector3(5.8f,2,37.8f),.04f);
   var flow=new GameObject("SchoolFloor_Flow").AddComponent<SchoolFloorFlow>();
   flow.attendance=attendance; flow.attendanceHud=UnityEngine.Object.FindFirstObjectByType<AttendanceHud>();
   flow.corridor=game; flow.stagingZone=Zone("Team_Staging",new Vector3(5.8f,1.5f,12),new Vector3(4.1f,3.5f,5));
   flow.gameObject.AddComponent<QuietCorridorHud>().flow=flow; attendance.continueIntoMap=true;
   EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
  }
  /// <summary>Nhận tên, tâm, kích thước; trả instance khối có liên kết prefab.</summary>
  static GameObject Block(string name,Vector3 position,Vector3 scale)
  {
   var go=(GameObject)PrefabUtility.InstantiatePrefab(block); go.name=name; go.transform.SetParent(root);
   go.transform.position=position; go.transform.localScale=scale; return go;
  }
  /// <summary>Nhận chữ và pose; trả nhãn có kiểm tra chiều sâu.</summary>
  static TextMesh Label(string name,string value,Vector3 position,float size)
  {
   var go=new GameObject(name); go.transform.SetParent(root); go.transform.position=position;
   var label=go.AddComponent<TextMesh>(); label.text=value; label.fontSize=64; label.characterSize=size;
   label.anchor=TextAnchor.MiddleCenter; label.alignment=TextAlignment.Center;
   go.GetComponent<MeshRenderer>().sharedMaterial=textMaterial; return label;
  }
  /// <summary>Nhận tên, tâm, kích thước; trả vùng trigger không cản di chuyển.</summary>
  static BoxCollider Zone(string name,Vector3 position,Vector3 size)
  {
   var go=new GameObject(name); go.transform.SetParent(root); go.transform.position=position;
   var zone=go.AddComponent<BoxCollider>(); zone.size=size; zone.isTrigger=true; return zone;
  }
  /// <summary>Không có đầu vào; trả prefab nút hiện có hoặc tạo một lần, giữ các lần sửa tay.</summary>
  static GameObject StationPrefab()
  {
   string path=Feature+"Prefabs/PF_QuietHoldStation.prefab";
   var existing=AssetDatabase.LoadAssetAtPath<GameObject>(path); if(existing!=null) return existing;
   var go=new GameObject("PF_QuietHoldStation"); var station=go.AddComponent<QuietHoldStation>();
   var visual=Block("Visual_Replaceable",Vector3.zero,new Vector3(.75f,.85f,.2f));
   visual.transform.SetParent(go.transform); station.indicator=visual.GetComponent<Renderer>();
   station.label=Label("Label","A / E: GIU",new Vector3(0,0,-.12f),.022f); station.label.transform.SetParent(go.transform);
   var prefab=PrefabUtility.SaveAsPrefabAsset(go,path); UnityEngine.Object.DestroyImmediate(go); return prefab;
  }
 }
}
