/*
 * Mục đích: Kiểm chứng đề sửa điểm và vòng chơi nối cả ba nhiệm vụ trên scene thật.
 * Hàm: Check ghi kết quả; Run chạy kiểm tra và lưu báo cáo; Domain thử seed/quyền/hoàn tác;
 * ReadAll đánh dấu hồ sơ trong kiểm tra luật; Place đồng bộ vị trí; Walk kiểm tra đường collider;
 * Integrated giải ba nhiệm vụ với đội 2–4 và kiểm tra reset không mất tiến trình.
 */
using System;
using System.IO;
using UnityEngine;
namespace TheReturn.Editor
{
    public static class GradeRepairChecks
    {
        static int assertions;
        static readonly System.Collections.Generic.List<string> groups = new System.Collections.Generic.List<string>();

        /// <summary>Nhận điều kiện và nhãn; tăng số kiểm tra hoặc ném lỗi có tên bước.</summary>
        static void Check(bool value, string name)
        {
            if (!value) throw new Exception("GRADE CHECK FAILED: " + name);
            assertions++;
        }

        /// <summary>Không nhận tham số; chạy trong Play, trả JSON và ghi bằng chứng ngoài Assets.</summary>
        public static string Run()
        {
            if (!Application.isPlaying) throw new InvalidOperationException("Use Play mode.");
            assertions = 0;
            groups.Clear();
            Domain();
            var flow = UnityEngine.Object.FindFirstObjectByType<SchoolFloorFlow>();
            float settle = flow.attendance.settleSeconds;
            try { for (int count = 2; count <= 4; count++) Integrated(flow, count); }
            finally
            {
                flow.attendance.settleSeconds = settle;
                PrototypePartyController.SetCursor(false);
            }
            string json = JsonUtility.ToJson(new Report { assertions = assertions, seeds = 90, groups = groups.ToArray() }, true);
            Directory.CreateDirectory("Design/Verification");
            File.WriteAllText("Design/Verification/GradeRepair.json", json);
            return json;
        }

        [Serializable] sealed class Report { public int assertions; public int seeds; public string[] groups; }

        /// <summary>Nhận state; đánh dấu bốn hồ sơ đúng vai để kiểm tra riêng phần sửa điểm.</summary>
        static void ReadAll(GradeRepairState state)
        {
            for (int i = 0; i < 4; i++) Check(state.MarkRead(i, state.Round.OwnerOf(i)), "read owner");
        }

        /// <summary>Không nhận tham số; kiểm tra 90 đề, mọi thứ tự hợp lệ, phiếu sai, giới hạn điểm và hoàn tác.</summary>
        static void Domain()
        {
            int[][] orders = { new[] { 0, 2, 1 }, new[] { 2, 0, 1 }, new[] { 2, 1, 0 } };
            for (int count = 2; count <= 4; count++)
            for (int seed = 0; seed < 30; seed++)
            {
                var round = new GradeRepairRound(count, seed);
                var same = new GradeRepairRound(count, seed);
                for (int row = 0; row < 4; row++)
                    Check(round.InitialScores[row] == same.InitialScores[row] && round.Codes[row] == same.Codes[row], "seed reproducible");
                for (int player = 0; player < count; player++)
                {
                    int owned = 0;
                    for (int doc = 0; doc < 4; doc++) if (round.OwnerOf(doc) == player) owned++;
                    Check(owned > 0, "every player has evidence");
                }
                var state = new GradeRepairState(round);
                Check(!state.MarkRead(0, -1) && !state.MarkRead(4, 0), "invalid reader IDs");
                Check(!state.MarkRead(0, 1), "private evidence owner");
                Check(state.Submit() == GradeResult.MissingEvidence, "unread evidence");
                ReadAll(state);
                Check(state.Submit() == GradeResult.MissingVouchers, "missing vouchers");
                Check(state.Apply(-1, -1, 0) == GradeResult.Invalid && state.Apply(0, -1, 4) == GradeResult.Invalid, "bad command IDs");
                Check(state.Apply(2, 0, 0) == GradeResult.Invalid, "same source target");
                Check(state.Apply(1, -1, round.CorrectTarget(1)) == GradeResult.OutOfRange && !state.Used(1), "overflow preserves voucher");
                foreach (int[] order in orders)
                {
                    state.Reset();
                    foreach (int voucher in order)
                        Check(state.Apply(voucher, round.CorrectSource(voucher), round.CorrectTarget(voucher)) == GradeResult.Ok, "valid order");
                    Check(state.Submit() == GradeResult.Solved, "valid proof solves");
                    Check(state.Score(round.CorrectTarget(0)) == 6 && state.Score(round.CorrectTarget(1)) == 10, "correct final scores");
                    Check(state.Undo() == GradeResult.Solved && state.Apply(0, -1, 0) == GradeResult.Solved, "solved is locked");
                }
                state.Reset();
                state.Apply(2, round.CorrectSource(2), round.CorrectTarget(2));
                state.Apply(0, -1, round.CorrectTarget(1));
                state.Apply(1, -1, round.CorrectTarget(0));
                Check(state.Submit() == GradeResult.WrongRecord, "correct totals but swapped vouchers rejected");
                state.Reset();
                state.Apply(0, -1, round.CorrectTarget(0));
                Check(state.Apply(0, -1, round.CorrectTarget(0)) == GradeResult.AlreadyUsed, "duplicate voucher");
                state.Apply(2, round.CorrectSource(2), round.CorrectTarget(2));
                Check(state.Undo() == GradeResult.Ok && !state.Used(2), "undo restores transfer voucher");
                Check(state.Score(round.CorrectSource(2)) == 9 && state.Score(round.CorrectTarget(2)) == 7, "undo restores both scores");
                Check(state.Undo() == GradeResult.Ok && !state.Used(0), "undo restores supplement");
                Check(state.Undo() == GradeResult.NothingToUndo, "empty undo");
                for (int i = 0; i < 4; i++) Check(state.Score(i) == round.InitialScores[i] && state.Read(i), "reset keeps knowledge");
            }
            groups.Add("90 seeds / reproducibility / every role contributes / valid operation orders / wrong evidence / undo / bounds");
        }

        /// <summary>Nhận party, vai và điểm; teleport rồi cập nhật collider cho các phép kiểm tra tức thì.</summary>
        static void Place(PrototypePartyController party, int actor, Vector3 position)
        {
            party.Teleport(actor, position);
            Physics.SyncTransforms();
        }

        /// <summary>Nhận party, vai và đích; bước qua collider, trả true khi đi tới được.</summary>
        static bool Walk(PrototypePartyController party, int actor, Vector3 target)
        {
            var controller = party.players[actor].GetComponent<CharacterController>();
            for (int i = 0; i < 100; i++)
            {
                Vector3 delta = target - party.players[actor].position;
                delta.y = 0;
                if (delta.magnitude < .08f) return true;
                controller.Move(Vector3.ClampMagnitude(delta, .08f));
                Physics.SyncTransforms();
            }
            return false;
        }

        /// <summary>Nhận map và số người; kiểm tra nối nhiệm vụ, quyền/tầm đọc, bảng, cửa và giữ tiến trình trước.</summary>
        static void Integrated(SchoolFloorFlow flow, int count)
        {
            var a = flow.attendance;
            var q = flow.corridor;
            var g = flow.gradeRepair;
            var p = a.party;
            flow.RestartMap();
            a.SetParticipantCount(count);
            a.StartNewRound(900 + count);
            a.BeginSession();
            a.settleSeconds = 0;
            int[] answer = a.State.Round.CopySolution();
            for (int i = 0; i < count; i++)
            {
                a.SwitchRole(i);
                Place(p, i, a.seats[answer[i]].standPoint.position);
                Check(a.TrySitActive(answer[i]), "attendance seat");
            }
            a.TickPuzzle();
            Check(a.State.Solved, "attendance solved");
            for (int i = 0; i < count; i++)
            {
                a.SwitchRole(i); a.StandActive(); Place(p, i, q.checkpoints[i].position);
            }
            flow.SendMessage("Update");
            Place(p, 0, new Vector3(4.9f, .05f, 13));
            Check(q.TryToggleStation(0, 0), "hold A");
            Place(p, 1, new Vector3(4.9f, .05f, 33));
            Check(q.TryToggleStation(1, 1), "hold B");
            for (int i = 0; i < count; i++) Place(p, i, new Vector3(5.2f + i % 2 * 1.2f, .05f, 32 + i));
            q.TickSimulation(.1f, 0);
            Check(q.State.Solved, "corridor solved");
            flow.SendMessage("Update");
            Check(g.Active && !q.Active && q.State.Solved && q.State.Owner(0) == -1 && flow.gradeEntrance.IsOpen, "preserve corridor on transition");
            Check(g.State.Round.PlayerCount == count, "grade roster scales");
            flow.gradeEntrance.SetOpen(true, true);
            Place(p, 0, new Vector3(5.8f, .05f, 36.5f));
            Check(Walk(p, 0, new Vector3(5.8f, .05f, 40)), "room entrance passable");
            p.SwitchRole(0, false);
            Check(!g.TryOpen(g.board) && g.Apply(0, -1, 0) == GradeResult.Invalid, "remote board rejected");
            Place(p, 0, new Vector3(-1.4f, .05f, 43));
            Check(!g.CanReach(g.documents[0]), "wall blocks evidence");
            Place(p, 0, g.checkpoints[0].position);
            for (int doc = 0; doc < 4; doc++)
            {
                int owner = g.State.Round.OwnerOf(doc);
                int other = (owner + 1) % count;
                Vector3 front = g.documents[doc].transform.position + new Vector3(0, 0, -2);
                front.y = .05f;
                Place(p, other, front);
                p.SwitchRole(other, false);
                Check(!g.TryOpen(g.documents[doc]), "wrong role cannot read");
                Place(p, other, g.checkpoints[other].position);
                Place(p, owner, front);
                p.SwitchRole(owner, false);
                Check(g.TryOpen(g.documents[doc]) && g.State.Read(doc), "owner reads evidence");
                Check(g.Apply(0, -1, 0) == GradeResult.Invalid, "document is not board");
                g.ClosePanel();
                Place(p, owner, g.checkpoints[owner].position);
            }
            Place(p, 0, new Vector3(5.8f, .05f, 51.5f));
            p.SwitchRole(0, false);
            Check(g.TryOpen(g.board), "open board");
            Check(g.Submit() == GradeResult.MissingVouchers && !g.exitDoor.IsOpen, "wrong submission keeps door closed");
            foreach (int voucher in new[] { 2, 0, 1 })
                Check(g.Apply(voucher, g.State.Round.CorrectSource(voucher), g.State.Round.CorrectTarget(voucher)) == GradeResult.Ok, "apply proof");
            Check(g.Undo() == GradeResult.Ok && !g.State.Used(1), "UI undo restores voucher");
            Check(g.Apply(1, -1, g.State.Round.CorrectTarget(1)) == GradeResult.Ok, "reapply voucher");
            Check(g.Submit() == GradeResult.Solved && g.exitDoor.IsOpen, "proof opens room exit");
            g.ClosePanel();
            g.exitDoor.SetOpen(true, true);
            Place(p, 0, new Vector3(5.8f, .05f, 54.5f));
            Check(Walk(p, 0, new Vector3(5.8f, .05f, 58)), "exit passable");
            g.Retry();
            Check(!g.State.Solved && a.State.Solved && q.State.Solved && flow.gradeEntrance.IsOpen, "retry preserves first two tasks");
            Check(!g.exitDoor.IsOpen && g.State.UndoCount == 0 && g.State.Read(0), "retry restores grade room");
            for (int i = 0; i < count; i++)
                Check(Vector3.Distance(p.players[i].position, g.checkpoints[i].position) < .01f, "grade checkpoint");
            g.NewCase(1000 + count);
            Check(g.State.Round.Seed == 1000 + count && !g.State.Read(0) && a.State.Solved && q.State.Solved, "new case preserves prior progress");
            groups.Add("Team " + count + ": all three tasks / physical doors / private evidence / board authorization / retry");
            PrototypePartyController.SetCursor(false);
        }
    }
}
