/*
 * Mục đích: Kiểm chứng luật/lượt và việc nối bài kiểm tra với ba nhiệm vụ trước.
 * Hàm: Check ghi kiểm tra; Run điều phối và lưu JSON; Domain thử các biến thể;
 * Reachable kiểm chứng không kẹt luật bằng duyệt trạng thái; Replay dựng lại đường;
 * Key định danh trạng thái; Integrated thử scene; Place đặt vai; Walk thử đường collider.
 */
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
namespace TheReturn.Editor
{
    public static class NavigationExamChecks
    {
        static int checks;
        static readonly int[] dx = { 0, 1, 0, -1 };
        static readonly int[] dy = { -1, 0, 1, 0 };

        /// <summary>Nhận điều kiện và tên; tăng bộ đếm hoặc dừng với lỗi rõ bước.</summary>
        static void Check(bool value,string name)
        {
            if(!value) throw new Exception("EXAM CHECK FAILED: " + name);
            checks++;
        }

        /// <summary>Không nhận tham số; chạy luật và scene trong Play, trả JSON và ghi báo cáo.</summary>
        public static string Run()
        {
            if(!Application.isPlaying) throw new InvalidOperationException("Run in Play.");
            checks=0;
            Domain();
            var flow=UnityEngine.Object.FindFirstObjectByType<SchoolFloorFlow>();
            float settle=flow.attendance.settleSeconds;
            try { for(int n=2;n<=4;n++) Integrated(flow,n); }
            finally { flow.attendance.settleSeconds=settle; PrototypePartyController.SetCursor(false); }
            string json=JsonUtility.ToJson(new Report { assertions=checks, configurations=12, allReachableStatesCanFinish=true },true);
            Directory.CreateDirectory("Design/Verification");
            File.WriteAllText("Design/Verification/NavigationExam.json",json);
            return json;
        }
        [Serializable] sealed class Report { public int assertions; public int configurations; public bool allReachableStatesCanFinish; }

        /// <summary>Nhận state; trả khóa gồm vị trí, lượt và dấu, không dùng số bước để tránh vòng lặp vô hạn.</summary>
        static string Key(NavigationExamState state) => state.Position + ":" + state.Turn + ":" + state.Stamps;

        /// <summary>Nhận đội, seed và dãy hướng; trả state tái dựng bằng các bước hợp lệ.</summary>
        static NavigationExamState Replay(int n,int seed,List<int> path)
        {
            var state=new NavigationExamState(n,seed);
            foreach(int dir in path) state.Move(state.Turn,dx[dir],dy[dir]);
            return state;
        }

        /// <summary>Nhận đội/seed; duyệt mọi trạng thái tới được và lan ngược từ đích để chứng minh không kẹt luật.</summary>
        static void Reachable(int n,int seed)
        {
            var queue=new Queue<List<int>>();
            var seen=new HashSet<string>();
            var reverse=new Dictionary<string,List<string>>();
            var goals=new HashSet<string>();
            queue.Enqueue(new List<int>());
            while(queue.Count>0)
            {
                var path=queue.Dequeue();
                var state=Replay(n,seed,path);
                string from=Key(state);
                if(!seen.Add(from)) continue;
                if(state.Solved) { goals.Add(from); continue; }
                for(int dir=0;dir<4;dir++)
                {
                    var next=Replay(n,seed,path);
                    var result=next.Move(next.Turn,dx[dir],dy[dir]);
                    if(result!=ExamResult.Moved && result!=ExamResult.Solved) continue;
                    string to=Key(next);
                    if(!reverse.ContainsKey(to)) reverse[to]=new List<string>();
                    reverse[to].Add(from);
                    var nextPath=new List<int>(path) { dir };
                    queue.Enqueue(nextPath);
                }
            }
            var pending=new Queue<string>(goals);
            while(pending.Count>0)
            {
                string to=pending.Dequeue();
                if(!reverse.ContainsKey(to)) continue;
                foreach(string from in reverse[to]) if(goals.Add(from)) pending.Enqueue(from);
            }
            Check(seen.Count>1,"reachable graph not empty");
            foreach(string state in seen) Check(goals.Contains(state),"reachable state can finish");
        }

        /// <summary>Không nhận tham số; kiểm tra 2–4 người và bốn biến thể, quyền lượt, bước sai, dấu, reset và đường.</summary>
        static void Domain()
        {
            for(int n=2;n<=4;n++)
            for(int seed=0;seed<4;seed++)
            {
                var s=new NavigationExamState(n,seed);
                Check(s.Move(-1,1,0)==ExamResult.Invalid,"invalid actor");
                Check(s.Move(1,1,0)==ExamResult.WrongTurn,"wrong turn");
                Check(s.Move(0,1,1)==ExamResult.Invalid && s.Steps==0,"diagonal rejected");
                int bad=s.Cell(3);
                Check(s.Move(0,bad%3-s.Position%3,bad/3-s.Position/3)==ExamResult.Blocked,"blocked cell");
                Check(s.Position==s.Start && s.Turn==0 && s.Stamps==0,"bad step keeps state");
                for(int actor=0;actor<n;actor++)
                {
                    bool owns=false;
                    for(int clue=0;clue<4;clue++) if(s.OwnerOf(clue)==actor) owns=true;
                    Check(owns,"every role owns clue");
                }
                foreach(int template in new[]{1,4,5,8})
                {
                    int next=s.Cell(template), actor=s.Turn, before=s.Steps;
                    var result=s.Move(actor,next%3-s.Position%3,next/3-s.Position/3);
                    Check(result==(template==8?ExamResult.Solved:ExamResult.Moved),"solution path");
                    Check(s.Steps==before+1,"single step");
                    if(template!=8) Check(s.Turn==(actor+1)%n,"round robin");
                    if(template==5)
                    {
                        Check(s.Stamps==2,"ordered stamps");
                        int back=s.FirstStamp;
                        Check(s.Move(s.Turn,back%3-s.Position%3,back/3-s.Position/3)==ExamResult.OneWay,"reverse edge rejected");
                    }
                }
                Check(s.Solved&&s.Stamps==2,"solved with both stamps");
                int final=s.Position;
                Check(s.Move(0,1,0)==ExamResult.Solved&&s.Position==final,"solved frozen");
                s.Reset();
                Check(!s.Solved&&s.Position==s.Start&&s.Turn==0&&s.Stamps==0,"reset same puzzle");
                Reachable(n,seed);
            }
        }

        /// <summary>Nhận party/vai/vị trí; đặt người và đồng bộ collider để kiểm tra ngay.</summary>
        static void Place(PrototypePartyController party,int actor,Vector3 position)
        {
            party.Teleport(actor,position);
            Physics.SyncTransforms();
        }

        /// <summary>Nhận người và đích; bước với CharacterController, trả true khi tới đích.</summary>
        static bool Walk(PrototypePartyController p,int actor,Vector3 target)
        {
            var cc=p.players[actor].GetComponent<CharacterController>();
            for(int i=0;i<100;i++)
            {
                Vector3 delta=target-p.players[actor].position;
                delta.y=0;
                if(delta.magnitude<.08f) return true;
                cc.Move(Vector3.ClampMagnitude(delta,.08f));
                Physics.SyncTransforms();
            }
            return false;
        }

        /// <summary>Nhận map và số người; chuẩn bị ba nhiệm vụ trước rồi kiểm tra chuyển, bước, cửa và reset bài kiểm tra.</summary>
        static void Integrated(SchoolFloorFlow flow,int n)
        {
            // Dùng lại kiểm tra tích hợp nhiệm vụ 3 để tránh tạo một đường tắt không đi qua map.
            typeof(GradeRepairChecks).GetMethod("Integrated",System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Static).Invoke(null,new object[]{flow,n});
            var g=flow.gradeRepair;
            var e=flow.navigationExam;
            var p=g.party;
            for(int i=0;i<4;i++) g.State.MarkRead(i,g.State.Round.OwnerOf(i));
            Place(p,0,new Vector3(5.8f,.05f,51.5f));
            p.SwitchRole(0,false);
            Check(g.TryOpen(g.board),"grade board");
            foreach(int voucher in new[]{2,0,1}) Check(g.Apply(voucher,g.State.Round.CorrectSource(voucher),
                g.State.Round.CorrectTarget(voucher))==GradeResult.Ok,"grade proof");
            Check(g.Submit()==GradeResult.Solved,"grade complete");
            flow.SendMessage("Update");
            Check(!e.Active,"do not interrupt open grade board");
            g.ClosePanel();
            flow.SendMessage("Update");
            Check(e.Active&&!g.Active&&g.State.Solved&&g.exitDoor.IsOpen,"grade result preserved");
            Check(e.State.PlayerCount==n,"exam roster");
            Place(p,0,new Vector3(5.8f,.05f,59.5f));
            Check(Walk(p,0,new Vector3(5.8f,.05f,63)),"exam connector passable");
            Check(!e.TryOpen()&&e.Step(1,0,0)==ExamResult.Invalid,"remote board rejected");
            for(int i=0;i<n;i++) Place(p,i,new Vector3(3.8f+i*1.3f,.05f,67.8f));
            p.SwitchRole(1,false);
            Check(e.TryOpen(),"nonturn actor can inspect");
            int next=e.State.Cell(1);
            Check(e.Step(next%3-e.State.Position%3,next/3-e.State.Position/3,e.State.Steps)==ExamResult.WrongTurn,"nonturn cannot move");
            e.ClosePanel();
            foreach(int template in new[]{1,4,5,8})
            {
                p.SwitchRole(e.State.Turn,false);
                Check(e.TryOpen(),"current player near board");
                next=e.State.Cell(template);
                int prior=e.State.Steps;
                Check(e.Step(next%3-e.State.Position%3,next/3-e.State.Position/3,prior)==
                    (template==8?ExamResult.Solved:ExamResult.Moved),"scene valid step");
                Check(e.Step(1,0,prior)==ExamResult.Invalid,"stale command rejected");
                e.ClosePanel();
            }
            Check(e.State.Solved&&e.exitDoor.IsOpen,"exam opens exit");
            e.exitDoor.SetOpen(true,true);
            Place(p,0,new Vector3(5.8f,.05f,75.5f));
            Check(Walk(p,0,new Vector3(5.8f,.05f,79)),"exam exit passable");
            e.ResetRound();
            Check(!e.State.Solved&&!e.exitDoor.IsOpen&&g.State.Solved&&flow.corridor.State.Solved&&
                flow.attendance.State.Solved,"retry preserves three previous tasks");
            int old=e.State.Variant;
            e.ResetRound(true);
            Check(e.State.Variant==(old+1)%4&&e.State.Turn==0,"next variant");
            for(int i=0;i<n;i++) Check(Vector3.Distance(p.players[i].position,e.checkpoints[i].position)<.01f,"exam checkpoint");
        }
    }
}
