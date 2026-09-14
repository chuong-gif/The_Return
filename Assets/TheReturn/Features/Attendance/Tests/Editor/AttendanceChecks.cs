/*
 * Mục đích: Kiểm chứng sinh đề 2–4 người trên 24 ghế, khả năng chơi lại và sự cần thiết của manh mối.
 * Danh sách hàm:
 * - Check: ghi assertion hoặc ném lỗi.
 * - Run: kiểm tra nhiều seed, điều kiện ghế và text; xuất báo cáo JSON.
 * - CheckRound: kiểm tra một đề có đúng một nghiệm và mọi người góp thông tin.
 * - CheckState: kiểm tra chiếm ghế, sửa sai, thành công và reset cho từng cỡ đội.
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TheReturn.Editor
{
    public static class AttendanceChecks
    {
        static int assertions;

        /// <summary>Nhận điều kiện và tên lỗi; tăng bộ đếm, ném exception nếu điều kiện sai.</summary>
        static void Check(bool condition, string message)
        {
            assertions++;
            if (!condition) throw new InvalidOperationException(message);
        }

        /// <summary>Không nhận tham số; kiểm tra 90 đề và xuất kết quả có seed để tái hiện lỗi.</summary>
        [MenuItem("The Return/Attendance/Validate Generated Puzzles")]
        public static void Run()
        {
            assertions = 0;
            var timer = Stopwatch.StartNew();
            var catalog = AssetDatabase.LoadAssetAtPath<AttendanceTextCatalog>(
                "Assets/TheReturn/Features/Attendance/Data/AttendanceText_VI.asset");
            string error;
            Check(catalog != null && catalog.ValidateCatalog(out error), "Catalog missing or invalid.");
            var reachedSeats = new HashSet<int>();
            var seenKinds = new HashSet<AttendanceRuleKind>();
            for (int count = 2; count <= 4; count++)
            {
                var signatures = new HashSet<string>();
                string[] names = new string[count];
                Array.Copy(catalog.roleNames, names, count);
                for (int seed = 0; seed < 30; seed++)
                {
                    AttendanceRound round = AttendanceGenerator.Generate(names, seed);
                    CheckRound(round);
                    foreach (int seat in round.CopySolution()) reachedSeats.Add(seat);
                    foreach (AttendanceRule rule in round.Rules) seenKinds.Add(rule.Kind);
                    signatures.Add(string.Join(",", round.CopySolution()));
                    string[] packets = catalog.BuildPackets(round);
                    Check(packets.Length == count, "Packet count.");
                    foreach (string packet in packets) Check(!string.IsNullOrWhiteSpace(packet) && !packet.Contains("{"), "Bad text packet.");
                    var repeated = AttendanceGenerator.Generate(names, seed);
                    Check(string.Join(",", round.CopySolution()) == string.Join(",", repeated.CopySolution()), "Seed must reproduce answer.");
                    CheckState(round);
                }
                Check(signatures.Count > 20, "Insufficient answer variation for " + count);
            }
            Check(reachedSeats.Count == 24, "Not all 24 seats are sampled.");
            Check(seenKinds.Count >= 8, "Insufficient rule variation.");
            Directory.CreateDirectory("Design/Verification");
            string report = "{\n  \"rounds\": 90,\n  \"assertions\": " + assertions +
                ",\n  \"seatCoverage\": " + reachedSeats.Count + ",\n  \"ruleKinds\": " + seenKinds.Count +
                ",\n  \"milliseconds\": " + timer.ElapsedMilliseconds + ",\n  \"passed\": true\n}";
            File.WriteAllText("Design/Verification/AttendanceV2.json", report);
            UnityEngine.Debug.Log("ATTENDANCE_V2_PASS: " + assertions + " assertions, 90 rounds, 24 seats, " + timer.ElapsedMilliseconds + " ms.");
        }

        /// <summary>Nhận một đề; xác minh nghiệm duy nhất và bỏ cả gói của bất kỳ người nào đều làm đề thiếu thông tin.</summary>
        static void CheckRound(AttendanceRound round)
        {
            Check(AttendanceGenerator.CountSolutions(round.PlayerCount, round.Rules) == 1, "Not unique: seed " + round.Seed);
            for (int owner = 0; owner < round.PlayerCount; owner++)
            {
                var remaining = new List<AttendanceRule>();
                int owned = 0;
                for (int i = 0; i < round.Rules.Count; i++)
                {
                    if (round.ClueOwners[i] == owner) owned++;
                    else remaining.Add(round.Rules[i]);
                }
                Check(owned > 0, "Empty role packet.");
                Check(AttendanceGenerator.CountSolutions(round.PlayerCount, remaining) > 1, "Role not essential.");
            }
        }

        /// <summary>Nhận một đề; thử thao tác sai/đúng trên trạng thái, không phụ thuộc scene hoặc input.</summary>
        static void CheckState(AttendanceRound round)
        {
            var state = new AttendanceState(round);
            int[] answer = round.CopySolution();
            Check(!state.TrySit(-1, 0) && !state.TrySit(round.PlayerCount, 0) && !state.TrySit(0, 24), "Invalid index accepted.");
            Check(state.TrySit(0, answer[0]), "Initial seat failed.");
            Check(!state.TrySit(1, answer[0]) && !state.TrySit(0, (answer[0] + 1) % 24), "Seat conflict accepted.");
            Check(!state.Evaluate(), "Incomplete group solved.");
            state.Reset();
            for (int i = 0; i < round.PlayerCount; i++) state.TrySit(i, answer[(i + 1) % round.PlayerCount]);
            Check(!state.Evaluate(), "Wrong permutation solved.");
            state.Reset();
            for (int i = 0; i < round.PlayerCount; i++) Check(state.TrySit(i, answer[i]), "Correct seat rejected.");
            Check(state.Evaluate(), "Correct group failed.");
            Check(state.Stand(0) && state.Solved, "Door state must persist after standing.");
            state.Reset();
            Check(!state.Solved && state.OccupiedCount == 0, "Retry state failed.");
        }
    }
}
