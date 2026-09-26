/*
 * Mục đích: Kiểm chứng nhiệm vụ 5 bằng luật thuần và scene thật cho đội 2–4.
 * Hàm: Check kiểm điều kiện; Run chạy/lưu báo cáo; Domain thử seed/góc/mã;
 * Integrated kiểm chuyển nhiệm vụ, quan sát, UI và reset; Aim bố trí hai vai để đọc lưng;
 * Walk kiểm đường collider của từng làn.
 */
using System;
using System.IO;
using UnityEngine;
namespace TheReturn.Editor
{
    public static class DontLookBackChecks
    {
        static int checks;
        /// <summary>Nhận điều kiện và nhãn; tăng đếm hoặc dừng với lỗi đúng bước.</summary>
        static void Check(bool value,string label)
        {
            if(!value)throw new Exception("DONT LOOK BACK: "+label);checks++;
        }
        /// <summary>Không nhận tham số; chạy trong Play, lưu báo cáo kiểm thử và trả số điều kiện đã qua.</summary>
        public static string Run()
        {
            if(!Application.isPlaying)throw new InvalidOperationException("Run in Play mode.");
            checks=0;Domain();
            var flow=UnityEngine.Object.FindFirstObjectByType<SchoolFloorFlow>();
            for(int n=2;n<=4;n++)Integrated(flow,n);
            string result="{\"assertions\":"+checks+",\"seedPartyCases\":300,\"scenePartyCases\":3}";
            Directory.CreateDirectory("Design/Verification");File.WriteAllText("Design/Verification/DontLookBack.json",result);
            PrototypePartyController.SetCursor(false);return result;
        }
        /// <summary>Không nhận tham số; kiểm 100 seed × 3 cỡ đội, mã đúng/sai, tự đọc và dung sai góc liên tục.</summary>
        static void Domain()
        {
            for(int n=2;n<=4;n++)
            for(int seed=0;seed<100;seed++)
            {
                var s=new DontLookBackState(n,seed);
                var same=new DontLookBackState(n,seed);
                Check(!s.Observe(0,0),"cannot read own");
                Check(s.Submit(true)==BackLockResult.NeedWitnesses,"no guessing without witnesses");
                for(int i=0;i<n;i++)
                {
                    Check(s.SymbolOf(i)==same.SymbolOf(i),"seed deterministic");
                    for(int j=0;j<i;j++)Check(s.SymbolOf(i)!=s.SymbolOf(j),"unique symbols");
                    Check(s.Observe((i+1)%n,i),"other observes");
                    Check(!s.HasSeen(i,i),"private own remains unknown");
                    Check(s.Select(i,(s.SymbolOf(i)+1)%DontLookBackState.SymbolCount),"valid input");
                }
                Check(s.Submit(false)==BackLockResult.NeedParty,"whole party required");
                Check(s.Submit(true)==BackLockResult.WrongCode,"wrong input");
                Check(!s.SampleLook(0,111,.59f),"grace .59");
                Check(!s.SampleLook(0,110,.1f),"boundary resets");
                Check(!s.SampleLook(0,180,.4f),"new exposure");
                Check(!s.SampleLook(1,180,.21f),"different actor does not combine");
                Check(s.SampleLook(0,180,.21f),"continuous bad angle");
                Check(s.WitnessedCount==n&&s.Penalties==1,"penalty keeps notes");
                Check(!s.SampleLook(-1,180,10)&&!s.SampleLook(0,180,float.NaN),"invalid samples");
                for(int i=0;i<n;i++)s.Select(i,s.SymbolOf(i));
                Check(s.Submit(true)==BackLockResult.Solved,"correct input");
                Check(!s.SampleLook(0,180,1),"solved removes restriction");
            }
        }
        /// <summary>Nhận controller/vai đọc/chủ thẻ; đặt hai người ở hai làn, ngắm đúng phía lưng và đồng bộ collider.</summary>
        public static void Aim(DontLookBackPrototype game,int observer,int subject)
        {
            for(int i=0;i<game.party.PartySize;i++)game.party.Teleport(i,new Vector3(i%2==0?3.8f:7.8f,.05f,94+i/2*2));
            float sx=subject%2==0?3.8f:7.8f;
            float ox=observer%2==0?3.8f:7.8f;
            game.party.Teleport(subject,new Vector3(sx,.05f,88));
            game.party.Teleport(observer,new Vector3(ox,.05f,84));
            game.party.SwitchRole(observer,false);
            Vector3 target=game.party.players[subject].TransformPoint(new Vector3(0,1.35f,-.38f));
            Vector3 delta=target-game.party.viewCamera.transform.position;
            game.party.players[observer].rotation=Quaternion.LookRotation(new Vector3(delta.x,0,delta.z));
            game.party.viewCamera.transform.rotation=Quaternion.LookRotation(delta);
            Physics.SyncTransforms();
        }
        /// <summary>Nhận flow và cỡ đội; kiểm launcher, nhìn thật, khóa Canvas, checkpoint, giữ kết quả trước và restart.</summary>
        static void Integrated(SchoolFloorFlow flow,int count)
        {
            var launcher=flow.GetComponent<MissionTestLauncher>();
            Check(launcher.Launch(4,count),"launch "+count);
            var game=flow.dontLookBack;
            Check(game.Active&&game.Armed&&game.State.PlayerCount==count,"active scalable");
            Check(flow.navigationExam.State.Solved&&!flow.navigationExam.Active&&flow.navigationExam.exitDoor.IsOpen,"exam preserved");
            Check(flow.gradeRepair.State.Solved&&flow.corridor.State.Solved&&flow.attendance.State.Solved,"prior progress");
            Check(!game.TryOpen(),"remote UI blocked");
            for(int subject=0;subject<count;subject++)
            {
                int observer=subject%2==0?1:0;
                Aim(game,observer,subject);
                Check(game.Angle(observer)<80,"safe cross lane angle");
                Check(game.CanRead(subject)&&game.Read(subject),"real line of sight "+subject);
                Check(!game.Read(observer),"no own read");
                game.party.players[subject].rotation=Quaternion.Euler(0,180,0);
                Check(!game.CanRead(subject),"front cannot reveal");
                game.party.players[subject].rotation=Quaternion.identity;
            }
            Check(game.State.WitnessedCount==count,"all last-player cards readable");
            var view=UnityEngine.Object.FindFirstObjectByType<SchoolFloorCanvasView>();
            Check(view.missionButtons.Length>=5&&view.backSlots.Length==4,"prefab UI references");
            for(int i=0;i<count;i++)game.party.Teleport(i,new Vector3(i%2==0?3.8f:7.8f,.05f,i<2?99.1f:97.8f));
            game.party.SwitchRole(0,false);Physics.SyncTransforms();
            Check(game.TryOpen(),"near lock opens");
            for(int i=0;i<count;i++)
            {
                for(int press=0;press<=game.State.SymbolOf(i);press++)view.backSlots[i].onClick.Invoke();
                Check(game.State.Entry(i)==game.State.SymbolOf(i),"canvas chooses");
            }
            game.party.Teleport(count-1,game.checkpoints[count-1].position);Physics.SyncTransforms();
            Check(game.Submit()==BackLockResult.NeedParty,"remote teammate rejected");
            game.party.Teleport(count-1,new Vector3((count-1)%2==0?3.8f:7.8f,.05f,count-1<2?99.1f:97.8f));Physics.SyncTransforms();
            view.backSubmit.onClick.Invoke();
            Check(game.State.Solved&&game.exitDoor.IsOpen,"canvas unlocks");
            game.ClosePanel();game.Retry();
            Check(!game.State.Solved&&game.State.WitnessedCount==0&&!game.exitDoor.IsOpen,"retry resets mission");
            Check(flow.navigationExam.State.Solved,"retry keeps exam");
            Walk(game);
            flow.RestartMap();
            Check(!game.Active&&!game.exitDoor.IsOpen&&!flow.navigationExam.exitDoor.IsOpen,"whole map reset");
        }
        /// <summary>Nhận controller; thử đi hai làn bằng CharacterController và kiểm vách ngăn không đi xuyên.</summary>
        static void Walk(DontLookBackPrototype game)
        {
            for(int i=1;i<game.party.PartySize;i++)game.party.Teleport(i,new Vector3(2.5f,.05f,94+i));
            Physics.SyncTransforms();
            for(int lane=0;lane<2;lane++)
            {
                var player=game.party.players[0];
                game.party.Teleport(0,new Vector3(lane==0?3.8f:7.8f,.05f,83));
                var cc=player.GetComponent<CharacterController>();
                for(int step=0;step<140;step++)cc.Move(new Vector3(0,-.02f,.1f));
                Check(player.position.z>96.5f,"walk lane "+lane);
            }
            game.party.Teleport(0,new Vector3(3.8f,.05f,90));
            var controller=game.party.players[0].GetComponent<CharacterController>();
            for(int step=0;step<40;step++)controller.Move(new Vector3(.1f,-.02f,0));
            Check(game.party.players[0].position.x<5.7f,"divider collision");
        }
    }
}
