/*
 * Mục đích: Kiểm chứng luật và scene nhiệm vụ giáo viên cho đội 2–4, gồm thời gian, phân công và tiến trình trước.
 * Hàm: Check xác nhận điều kiện; Run chạy/lưu báo cáo; Domain thử luật thuần;
 * Integrated thử launcher, tầm tương tác và reset; Aim đặt vai nhìn đúng đích.
 */
using System;
using System.IO;
using UnityEngine;

namespace TheReturn.Editor
{
    public static class TeacherTruthChecks
    {
        static int checks;

        /// <summary>Nhận điều kiện và nhãn; tăng bộ đếm hoặc dừng với lỗi đúng bước.</summary>
        static void Check(bool value,string label)
        {
            if(!value)throw new Exception("TEACHER TRUTH: "+label);
            checks++;
        }

        /// <summary>Không nhận tham số; chạy luật/scene trong Play, lưu JSON và trả báo cáo.</summary>
        public static string Run()
        {
            if(!Application.isPlaying)throw new InvalidOperationException("Run in Play mode.");
            checks=0;
            Domain();
            var flow=UnityEngine.Object.FindFirstObjectByType<SchoolFloorFlow>();
            for(int count=2;count<=4;count++)Integrated(flow,count);
            string json="{\"assertions\":"+checks+",\"partyCases\":3,\"variants\":3,\"windowSeconds\":3}";
            Directory.CreateDirectory("Design/Verification");
            File.WriteAllText("Design/Verification/TeacherTruth.json",json);
            PrototypePartyController.SetCursor(false);
            return json;
        }

        /// <summary>Không nhận tham số; kiểm 2–4 người, ba biến thể, phụ đề riêng, timeout, thiếu người và nghiệm thành công.</summary>
        static void Domain()
        {
            for(int count=2;count<=4;count++)
            for(int variant=0;variant<3;variant++)
            {
                var state=new TeacherTruthState(count,variant,3f);
                Check(state.AnnouncementVariant==variant,"variant");
                Check(!state.Hear(-1,0)&&!state.Hear(0,4),"invalid hear");
                Check(state.Hear(0,0)&&state.HasHeard(0,0)&&!state.HasHeard(1,0),"private subtitle");
                Check(state.Confirm(0,0)==ManualConfirmResult.Confirmed,"first confirm");
                Check(state.WindowActive&&Math.Abs(state.Remaining-3f)<.001f,"window starts");
                Check(state.Confirm(1%count,0)==ManualConfirmResult.AlreadyConfirmed,"duplicate channel");
                Check(!state.Tick(2.9f)&&state.ConfirmedCount==1,"within grace");
                Check(state.Tick(.11f)&&state.ConfirmedCount==0&&!state.WindowActive,"timeout reset");
                Check(state.HasHeard(0,0),"timeout keeps subtitle");
                for(int channel=0;channel<4;channel++)
                {
                    var result=state.Confirm(0,channel);
                    if(channel<3)Check(result==ManualConfirmResult.Confirmed,"same actor partial");
                    else Check(result==ManualConfirmResult.NeedEveryPlayer,"all actors required");
                }
                Check(state.ConfirmedCount==0&&!state.Solved,"missing player resets");
                int[] actors=count==2?new[]{0,1,0,1}:count==3?new[]{0,1,2,0}:new[]{0,1,2,3};
                for(int channel=0;channel<4;channel++)
                {
                    var result=state.Confirm(actors[channel],channel);
                    Check(result==(channel==3?ManualConfirmResult.Solved:ManualConfirmResult.Confirmed),"scaled solution");
                }
                Check(state.Solved&&state.ParticipantCount==count&&!state.Tick(10),"solved stable");
            }
        }

        /// <summary>Nhận game/vai/đích/khoảng lùi; đặt camera hướng vào đích và đồng bộ vật lý.</summary>
        static void Aim(TeacherTruthPrototype game,int actor,Transform target,float offset,Vector3 localTarget)
        {
            Vector3 position=target.position+Vector3.back*offset;
            position.y=.05f;
            game.party.Teleport(actor,position);
            game.party.SwitchRole(actor,false);
            Vector3 delta=target.TransformPoint(localTarget)-game.party.viewCamera.transform.position;
            game.party.players[actor].rotation=Quaternion.LookRotation(new Vector3(delta.x,0,delta.z));
            game.party.viewCamera.transform.rotation=Quaternion.LookRotation(delta);
            Physics.SyncTransforms();
        }

        /// <summary>Nhận flow/cỡ đội; kiểm test nhanh, tương tác thật, đủ người, mở cửa, retry và reset toàn map.</summary>
        static void Integrated(SchoolFloorFlow flow,int count)
        {
            var launcher=flow.GetComponent<MissionTestLauncher>();
            Check(launcher.Launch(5,count),"launch "+count);
            var game=flow.teacherTruth;
            Check(game.Active&&game.State.PlayerCount==count,"active scalable");
            Check(flow.dontLookBack.State.Solved&&!flow.dontLookBack.Active&&flow.dontLookBack.exitDoor.IsOpen,"previous mission preserved");
            Check(flow.navigationExam.State.Solved&&flow.gradeRepair.State.Solved&&flow.corridor.State.Solved&&flow.attendance.State.Solved,"all previous solved");
            Aim(game,0,game.teachers[0].transform,2f,new Vector3(0,1.3f,0));
            Check(game.Hear(game.teachers[0]),"real teacher reach");
            Check(game.State.HasHeard(0,0),"subtitle recorded");
            game.party.Teleport(0,game.checkpoints[0].position);
            Physics.SyncTransforms();
            Check(!game.Hear(game.teachers[0]),"remote teacher rejected");
            int[] actors=count==2?new[]{0,1,0,1}:count==3?new[]{0,1,2,0}:new[]{0,1,2,3};
            for(int channel=0;channel<4;channel++)
            {
                Aim(game,actors[channel],game.stations[channel].transform,2f,Vector3.zero);
                var result=game.Confirm(game.stations[channel]);
                Check(result==(channel==3?ManualConfirmResult.Solved:ManualConfirmResult.Confirmed),"station "+channel);
            }
            Check(game.State.Solved&&game.exitDoor.IsOpen,"door opens");
            Check(game.stations[0].label.text.Contains("VAI"),"world visual");
            game.Retry();
            Check(!game.State.Solved&&!game.exitDoor.IsOpen&&game.State.ConfirmedCount==0,"retry current only");
            Check(flow.dontLookBack.State.Solved,"retry keeps prior mission");
            flow.RestartMap();
            Check(!game.Active&&!game.exitDoor.IsOpen&&!flow.dontLookBack.exitDoor.IsOpen,"whole map reset");
        }
    }
}
