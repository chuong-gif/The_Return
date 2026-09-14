/*
 * Mục đích: Kiểm chứng luật, chuyển nhiệm vụ, bàn giao, checkpoint và cửa cho đội 2–4 người.
 * Hàm: Run chạy trong Play và ghi báo cáo; Check ghi một kiểm tra hoặc báo lỗi;
 * Domain kiểm tra thời gian/ồn/giữ; Integrated kiểm tra vòng chơi trong scene thực;
 * Place đặt người và đồng bộ collider; Walk kiểm tra đường đi bằng CharacterController.
 */
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace TheReturn.Editor
{
 public static class QuietCorridorChecks
 {
  static readonly List<string> passed=new List<string>();
  /// <summary>Nhận điều kiện và tên; ghi kiểm tra đạt, ném lỗi có tên nếu không đạt.</summary>
  static void Check(bool ok,string name) { if(!ok) throw new Exception("QUIET CHECK FAILED: "+name); passed.Add(name); }
  /// <summary>Không có đầu vào; chạy kiểm chứng Play mode và trả JSON, giữ bằng chứng trong Design.</summary>
  public static string Run()
  {
   if(!Application.isPlaying) throw new InvalidOperationException("Run in Play mode.");
   passed.Clear(); Domain();
   var flow=UnityEngine.Object.FindFirstObjectByType<SchoolFloorFlow>();
   float settle=flow.attendance.settleSeconds;
   try { for(int count=2;count<=4;count++) Integrated(flow,count); }
   finally { flow.attendance.settleSeconds=settle; PrototypePartyController.SetCursor(false); }
   string json=JsonUtility.ToJson(new Report { assertions=passed.Count, checks=passed.ToArray() },true);
   Directory.CreateDirectory("Design/Verification"); File.WriteAllText("Design/Verification/QuietCorridor.json",json);
   return json;
  }
  [Serializable] sealed class Report { public int assertions; public string[] checks; }
  /// <summary>Không có đầu vào; kiểm tra biên ân hạn, tốc độ ồn, hai người khác nhau và cờ reset.</summary>
  static void Domain()
  {
   for(int n=2;n<=4;n++)
   {
    var s=new QuietCorridorState(n);
    Check(!s.Hold(-1,0)&&!s.Hold(2,0)&&!s.Hold(0,n),"Domain "+n+": invalid identities");
    Check(s.Hold(0,0)&&!s.Hold(0,1)&&!s.Hold(1,0),"Domain "+n+": unique station ownership");
    s.Tick(2,1); Check(Mathf.Abs(s.Noise-40)<.001f,"Domain "+n+": sprint adds 20/sec");
    s.Tick(1,0); Check(Mathf.Abs(s.Noise-25)<.001f,"Domain "+n+": silence recovers 15/sec");
    s.Release(0); s.Tick(2.9f,0);
    Check(!s.SpeakerOn&&s.GateRequested&&s.Grace>0,"Domain "+n+": 3-second grace");
    s.Tick(.2f,0); Check(s.SpeakerOn&&Mathf.Abs(s.Noise-1.5f)<.01f,"Domain "+n+": split frame at grace edge");
    s.Tick(10,0); Check(s.Failed&&!s.Hold(0,0),"Domain "+n+": fail at 100");
    s.Reset(); Check(!s.Failed&&s.Noise==0&&!s.HandedOver&&s.Owner(0)==-1,"Domain "+n+": reset");
    Check(!s.TryComplete(true),"Domain "+n+": cannot finish without handover");
    s.Hold(0,0); s.Hold(1,1); s.Release(0);
    Check(s.HandedOver&&!s.TryComplete(false)&&s.TryComplete(true),"Domain "+n+": all players required");
    s.Tick(100,4); Check(s.Solved&&!s.Failed&&s.GateRequested,"Domain "+n+": solved stays open");
   }
   var whole=new QuietCorridorState(2); whole.Hold(0,0); whole.Release(0); whole.Tick(4,1);
   var small=new QuietCorridorState(2); small.Hold(0,0); small.Release(0);
   for(int i=0;i<40;i++) small.Tick(.1f,1);
   Check(Mathf.Abs(whole.Noise-small.Noise)<.01f,"Domain: stable across frame sizes");
  }
  /// <summary>Nhận party, vai và vị trí; teleport rồi cập nhật broadphase để kiểm tra collider ngay.</summary>
  static void Place(PrototypePartyController party,int player,Vector3 point)
  { party.Teleport(player,point); Physics.SyncTransforms(); }
  /// <summary>Nhận party, vai, đích; bước bằng collider từng đoạn, trả true nếu tới đích không bị chặn.</summary>
  static bool Walk(PrototypePartyController party,int player,Vector3 target)
  {
   var cc=party.players[player].GetComponent<CharacterController>();
   for(int i=0;i<500;i++)
   {
    Vector3 delta=target-party.players[player].position; delta.y=0;
    if(delta.magnitude<.08f) return true;
    cc.Move(Vector3.ClampMagnitude(delta,.08f)); Physics.SyncTransforms();
   }
   return false;
  }
  /// <summary>Nhận flow và số người; giải lớp, chuyển đúng đội rồi kiểm tra nhiệm vụ 2 trên collider thật.</summary>
  static void Integrated(SchoolFloorFlow flow,int n)
  {
   string tag="Scene "+n+": "; var a=flow.attendance; var g=flow.corridor; var p=a.party;
   flow.RestartMap(); a.SetParticipantCount(n); a.StartNewRound(2026+n); a.BeginSession(); a.settleSeconds=0;
   var solution=a.State.Round.CopySolution();
   for(int i=0;i<n;i++)
   { a.SwitchRole(i); Place(p,i,a.seats[solution[i]].standPoint.position); Check(a.TrySitActive(solution[i]),tag+"sit "+i); }
   a.TickPuzzle(); Check(a.State.Solved&&a.exitDoor.IsOpen,tag+"attendance opens exit");
   for(int i=0;i<n;i++) { a.SwitchRole(i); a.StandActive(); }
   for(int i=0;i<n-1;i++) Place(p,i,g.checkpoints[i].position);
   flow.SendMessage("Update"); Check(!g.Active,tag+"wait for last player");
   Place(p,n-1,g.checkpoints[n-1].position); flow.SendMessage("Update");
   Check(g.Active&&a.ExternalControl&&!flow.attendanceHud.enabled&&g.State.PlayerCount==n,tag+"same scene transition");
   Check(!g.TryToggleStation(0,n)&&!g.TryToggleStation(1,0),tag+"reject invalid or remote interaction");
   Place(p,0,new Vector3(4.9f,.05f,13));
   Check(g.TryToggleStation(0,0),tag+"near station hold"); g.TickSimulation(.1f,0);
   Check(g.gate.IsOpen&&!g.State.SpeakerOn,tag+"hold opens gate");
   g.gate.SetOpen(true,true);
   Place(p,1,new Vector3(5.8f,.05f,16));
   Check(Walk(p,1,new Vector3(5.8f,.05f,33)),tag+"walk corridor through open gate");
   Place(p,1,new Vector3(4.9f,.05f,33));
   Check(g.TryToggleStation(1,1)&&g.State.HandedOver,tag+"far player takes over");
   g.State.Release(0); g.TickSimulation(.1f,0); Check(g.gate.IsOpen,tag+"far station keeps gate open");
   for(int i=0;i<n;i++) if(i!=1) Place(p,i,new Vector3(5.7f+(i%2)*.8f,.05f,32+i));
   g.TickSimulation(.1f,0); Check(g.State.Solved&&a.State.Solved,tag+"all players finish");
   g.Retry(); Check(!g.State.Solved&&a.State.Solved&&a.exitDoor.IsOpen,tag+"retry preserves attendance");
   g.TickSimulation(7,0);
   Check(g.Failures==1&&g.State.Noise==0&&a.State.Solved,tag+"noise failure preserves attendance");
   for(int i=0;i<n;i++) Check(Vector3.Distance(p.players[i].position,g.checkpoints[i].position)<.01f,tag+"checkpoint "+i);
   Place(p,0,new Vector3(5.8f,.05f,29)); g.TickSimulation(.1f,0);
   Check(g.SafetyHolding&&g.gate.IsOpen,tag+"anti-crush occupied");
   Place(p,0,new Vector3(5.8f,.05f,27)); g.TickSimulation(.1f,0);
   Check(!g.SafetyHolding&&!g.gate.IsOpen,tag+"anti-crush clear");
   Place(p,0,new Vector3(4.9f,.05f,13)); g.TryToggleStation(0,0);
   p.players[0].gameObject.SetActive(false); g.TickSimulation(.1f,0);
   Check(g.State.Owner(0)==-1,tag+"lost holder releases");
   p.players[0].gameObject.SetActive(true);
   g.Retry(); PrototypePartyController.SetCursor(false); float noise=g.State.Noise;
   g.SendMessage("Update"); Check(g.State.Noise==noise,tag+"menu pauses simulation");
   a.exitDoor.SetOpen(true,true); Place(p,0,new Vector3(5.8f,.05f,7));
   Check(Walk(p,0,new Vector3(5.8f,.05f,10)),tag+"classroom connector passable");
  }
 }
}
