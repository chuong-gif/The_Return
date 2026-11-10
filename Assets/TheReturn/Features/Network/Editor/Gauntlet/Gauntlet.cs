/*
 * Mục đích: GAUNTLET (T-039, M4): MỘT lệnh chạy mọi assembly test EditMode của dự án (mạng + Domain + khung + hợp đồng) rồi TỰ quy mọi test đỏ
 * về nhiệm vụ / task / người viết theo .ai/MISSIONS_NET.md. Kết quả ghi thẳng vào .ai/GAUNTLET.md (bảng + kết luận), .ai/FAILURES.md (mỗi test đỏ
 * một dòng F-xxx, không ghi trùng), cột «Gauntlet gần nhất» của MISSIONS_NET.md và thẻ FIX trong .ai/QUEUE.md. Test đã xanh lại thì dòng F cũ đổi
 * thành «XANH-LẠI» (người giữ xác nhận rồi mới FIXED). Chỉ chạy trong Editor, không cần Play, không đụng scene/Build Settings.
 * Cách gọi: menu «TheReturn/Gauntlet/Chạy Gauntlet mạng» (AI: execute_menu_item), rồi đọc .ai/GAUNTLET.md khi dòng đầu hết «ĐANG CHẠY».
 * Hàm: Gauntlet.Run; Collector (RunStarted, RunFinished, TestStarted, TestFinished); Finish; ParseRegistry; WriteGauntlet; UpdateFailures;
 * UpdateRegistry; AddFixCards; các hàm nhỏ (Cell, Clean, Classify).
 */
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace TheReturn.NetworkGauntlet
{
    /// <summary>Chạy Gauntlet: toàn bộ test EditMode, báo cáo lỗi theo nhiệm vụ.</summary>
    public static class Gauntlet
    {
        const string GauntletFile = ".ai/GAUNTLET.md";
        const string MissionsFile = ".ai/MISSIONS_NET.md";
        const string FailuresFile = ".ai/FAILURES.md";
        const string QueueFile = ".ai/QUEUE.md";

        sealed class Reg
        {
            public string Mission, Task, Writer, Phase;
            public int Line;
        }

        sealed class Leaf
        {
            public string Assembly, FullName, Name, Message;
            public TestStatus Status;
            public double Seconds;
        }

        sealed class Agg
        {
            public int Pass, Fail, Skip;
            public double Seconds;
        }

        static TestRunnerApi api;
        static Collector collector;
        static bool running;
        static DateTime startedUtc;
        static List<string> assemblyNames;

        static string Root { get { return Directory.GetParent(Application.dataPath).FullName; } }
        static string Path(string rel) { return System.IO.Path.Combine(Root, rel); }
        static readonly UTF8Encoding Utf8 = new UTF8Encoding(false);

        /// <summary>Chạy Gauntlet: liệt kê mọi assembly *.Tests trong Assets, ghi «ĐANG CHẠY», rồi chạy một lượt EditMode.</summary>
        [MenuItem("TheReturn/Gauntlet/Chạy Gauntlet mạng")]
        public static void Run()
        {
            if (running && (DateTime.UtcNow - startedUtc).TotalMinutes < 10)
            {
                Debug.LogWarning("[Gauntlet] Đang chạy, chờ xong rồi gọi lại.");
                return;
            }
            running = true;
            startedUtc = DateTime.UtcNow;
            assemblyNames = new List<string>();
            foreach (var a in CompilationPipeline.GetAssemblies(AssembliesType.Editor))
            {
                if (!a.name.EndsWith(".Tests", StringComparison.Ordinal) || a.sourceFiles.Length == 0) continue;
                if (!a.sourceFiles[0].Replace('\\', '/').StartsWith("Assets/", StringComparison.Ordinal)) continue;
                assemblyNames.Add(a.name);
            }
            assemblyNames.Sort(StringComparer.Ordinal);
            File.WriteAllText(Path(GauntletFile), "# GAUNTLET — ĐANG CHẠY từ " + Stamp(startedUtc) + " UTC (" + assemblyNames.Count + " assembly)\n\nChờ dòng này đổi thành «Chạy:» rồi mới đọc kết quả.\n", Utf8);

            api = ScriptableObject.CreateInstance<TestRunnerApi>();
            collector = new Collector();
            api.RegisterCallbacks(collector);
            api.Execute(new ExecutionSettings(new Filter { testMode = TestMode.EditMode, assemblyNames = assemblyNames.ToArray() }));
        }

        sealed class Collector : ICallbacks
        {
            public void RunStarted(ITestAdaptor testsToRun) { }
            public void TestStarted(ITestAdaptor test) { }
            public void TestFinished(ITestResultAdaptor result) { }

            public void RunFinished(ITestResultAdaptor result)
            {
                try
                {
                    var leaves = new List<Leaf>();
                    Walk(result, null, leaves);
                    Finish(leaves);
                }
                catch (Exception ex)
                {
                    Debug.LogError("[Gauntlet] Lỗi khi ghi báo cáo: " + ex);
                    File.WriteAllText(Path(GauntletFile), "# GAUNTLET — LỖI CÔNG CỤ\n\n" + ex.Message + "\n", Utf8);
                }
                finally
                {
                    running = false;
                    if (api != null) api.UnregisterCallbacks(this);
                }
            }

            static void Walk(ITestResultAdaptor node, string inheritedAssembly, List<Leaf> leaves)
            {
                string assembly = inheritedAssembly;
                if (node.Test != null && node.Test.TypeInfo != null) assembly = node.Test.TypeInfo.Assembly.GetName().Name;
                if (!node.HasChildren)
                {
                    leaves.Add(new Leaf
                    {
                        Assembly = assembly ?? "?",
                        FullName = node.FullName,
                        Name = node.Name,
                        Message = node.Message,
                        Status = node.TestStatus,
                        Seconds = node.Duration
                    });
                    return;
                }
                foreach (var child in node.Children) Walk(child, assembly, leaves);
            }
        }

        // ------------------------------------------------------------------ báo cáo

        static void Finish(List<Leaf> leaves)
        {
            string[] missionLines = File.ReadAllLines(Path(MissionsFile));
            Dictionary<string, Reg> registry = ParseRegistry(missionLines);
            var aggs = new SortedDictionary<string, Agg>(StringComparer.Ordinal);
            var failures = new List<Leaf>();
            var passed = new HashSet<string>();
            foreach (var l in leaves)
            {
                Agg g;
                if (!aggs.TryGetValue(l.Assembly, out g)) { g = new Agg(); aggs[l.Assembly] = g; }
                g.Seconds += l.Seconds;
                if (l.Status == TestStatus.Passed) { g.Pass++; passed.Add(l.FullName); }
                else if (l.Status == TestStatus.Failed) { g.Fail++; failures.Add(l); }
                else g.Skip++;
            }
            double total = 0;
            foreach (var g2 in aggs.Values) total += g2.Seconds;

            List<string> notes = FailuresAndQueue(failures, passed, registry);
            WriteGauntlet(aggs, registry, failures, total, notes);
            UpdateRegistry(missionLines, registry, aggs, failures, total);
            Debug.Log("[Gauntlet] xong: " + leaves.Count + " test, " + failures.Count + " đỏ. Xem " + GauntletFile);
        }

        /// <summary>Đọc bảng MISSIONS_NET.md: assembly test -> nhiệm vụ / task / người viết / pha (theo tên cột ở dòng tiêu đề).</summary>
        static Dictionary<string, Reg> ParseRegistry(string[] lines)
        {
            var map = new Dictionary<string, Reg>(StringComparer.Ordinal);
            int asmCol = -1, taskCol = -1, writerCol = -1, phaseCol = -1, missionCol = 1;
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (!line.StartsWith("|", StringComparison.Ordinal)) continue;
                string[] c = line.Split('|');
                if (asmCol < 0)
                {
                    for (int k = 0; k < c.Length; k++)
                    {
                        string h = c[k];
                        if (h.Contains("Assembly")) asmCol = k;
                        else if (h.Contains("Task")) taskCol = k;
                        else if (h.Contains("VIẾT BỞI")) writerCol = k;
                        else if (h.Contains("Pha")) phaseCol = k;
                    }
                    continue;
                }
                if (c.Length <= asmCol || line.StartsWith("|---", StringComparison.Ordinal)) continue;
                string asm = c[asmCol].Trim();
                int sp = asm.IndexOf(' ');
                if (sp > 0) asm = asm.Substring(0, sp);
                if (!asm.EndsWith(".Tests", StringComparison.Ordinal)) continue;
                var reg = new Reg
                {
                    Mission = c[missionCol].Trim(),
                    Task = taskCol >= 0 && taskCol < c.Length ? c[taskCol].Trim() : "?",
                    Writer = writerCol >= 0 && writerCol < c.Length ? c[writerCol].Trim() : "?",
                    Phase = phaseCol >= 0 && phaseCol < c.Length ? c[phaseCol].Trim() : "?",
                    Line = i
                };
                map[asm] = reg;
            }
            return map;
        }

        /// <summary>Phân loại lỗi: MISSION (sửa trong file nhiệm vụ), FRAMEWORK (khung/hợp đồng), DOMAIN (không sửa; báo chủ dự án).</summary>
        static string Classify(string assembly, Dictionary<string, Reg> registry)
        {
            if (registry.ContainsKey(assembly)) return "MISSION";
            if (assembly.StartsWith("TheReturn.Network.", StringComparison.Ordinal)) return "FRAMEWORK";
            if (assembly.Contains("Domain")) return "DOMAIN";
            return "KHÁC";
        }

        static string MethodOf(string name)
        {
            int p = name.IndexOf('(');
            return p > 0 ? name.Substring(0, p) : name;
        }

        static string Clean(string text, int max)
        {
            if (string.IsNullOrEmpty(text)) return "(không có thông điệp)";
            string s = text.Replace("\r", " ").Replace("\n", " ").Replace("|", "/").Trim();
            while (s.Contains("  ")) s = s.Replace("  ", " ");
            return s.Length > max ? s.Substring(0, max) + "..." : s;
        }

        static string Stamp(DateTime t) { return t.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture); }

        /// <summary>Cập nhật FAILURES.md (thêm dòng mới, đổi dòng cũ đã xanh lại) và QUEUE.md (thẻ FIX). Trả các ghi chú in vào GAUNTLET.md.</summary>
        static List<string> FailuresAndQueue(List<Leaf> failures, HashSet<string> passed, Dictionary<string, Reg> registry)
        {
            var notes = new List<string>();
            var lines = new List<string>(File.ReadAllLines(Path(FailuresFile)));
            int nextId = 1;
            foreach (string l in lines)
            {
                if (!l.StartsWith("| F-", StringComparison.Ordinal)) continue;
                int v;
                string id = l.Split('|')[1].Trim().Substring(2);
                if (int.TryParse(id, NumberStyles.Integer, CultureInfo.InvariantCulture, out v) && v >= nextId) nextId = v + 1;
            }

            // nhiều nhiệm vụ đỏ cùng một ca => nghi lỗi khung
            var missionsByMethod = new Dictionary<string, HashSet<string>>();
            foreach (var f in failures)
            {
                if (Classify(f.Assembly, registry) != "MISSION") continue;
                string key = MethodOf(f.Name);
                HashSet<string> set;
                if (!missionsByMethod.TryGetValue(key, out set)) { set = new HashSet<string>(); missionsByMethod[key] = set; }
                set.Add(registry[f.Assembly].Mission);
            }

            var currentKeys = new HashSet<string>();
            foreach (var f in failures) currentKeys.Add("`" + f.FullName + "`");

            // dòng cũ đã xanh lại
            for (int i = 0; i < lines.Count; i++)
            {
                if (!lines[i].StartsWith("| F-", StringComparison.Ordinal)) continue;
                string[] c = lines[i].Split('|');
                if (c.Length < 10) continue;
                string status = c[8].Trim();
                if (status.StartsWith("FIXED", StringComparison.Ordinal) || status.StartsWith("XANH-LẠI", StringComparison.Ordinal)) continue;
                string test = c[4].Trim();
                if (!test.StartsWith("`", StringComparison.Ordinal)) continue;
                string name = test.Trim('`');
                if (passed.Contains(name))
                {
                    c[8] = " XANH-LẠI " + Stamp(DateTime.UtcNow) + " (chờ người giữ xác nhận FIXED) ";
                    lines[i] = string.Join("|", c);
                    notes.Add(c[1].Trim() + " " + name + " đã xanh lại.");
                }
            }

            // dòng mới
            var newCards = new List<string>();
            foreach (var f in failures)
            {
                string key = "`" + f.FullName + "`";
                bool open = false;
                foreach (string l in lines)
                {
                    if (!l.StartsWith("| F-", StringComparison.Ordinal) || !l.Contains(key)) continue;
                    string st = l.Split('|').Length > 8 ? l.Split('|')[8].Trim() : "";
                    if (!st.StartsWith("FIXED", StringComparison.Ordinal) && !st.StartsWith("XANH-LẠI", StringComparison.Ordinal)) { open = true; break; }
                }
                if (open) continue;
                string kind = Classify(f.Assembly, registry);
                Reg reg;
                registry.TryGetValue(f.Assembly, out reg);
                string mission = reg != null ? reg.Mission : f.Assembly;
                string note = "";
                if (kind == "MISSION" && missionsByMethod[MethodOf(f.Name)].Count >= 2) { kind = "FRAMEWORK?"; note = " [nhiều nhiệm vụ đỏ cùng ca: nghi lỗi khung]"; }
                string fid = "F-" + nextId.ToString("000", CultureInfo.InvariantCulture);
                nextId++;
                lines.Add("| " + fid + " | " + mission + " | " + kind + " | " + key + " | " + Clean(f.Message, 240) + note + " | " + Stamp(DateTime.UtcNow) + " (Gauntlet) | " + (reg != null ? reg.Writer : "?") + " | OPEN | FIX-" + fid + " |");
                newCards.Add("| FIX-" + fid + " | " + (reg != null ? reg.Task : "T-039") + " | Sửa " + fid + " (" + kind + ") trong " + mission + ": test " + MethodOf(f.Name) + " | file của nhiệm vụ " + mission + " (xem FAILURES.md " + fid + ") | Chạy lại Gauntlet: test đó xanh | - | READY | " + Stamp(DateTime.UtcNow) + " |");
                notes.Add(fid + " mới: " + mission + " [" + kind + "] " + MethodOf(f.Name));
            }
            lines.RemoveAll(l => l.StartsWith("(chưa có lỗi", StringComparison.Ordinal));
            File.WriteAllText(Path(FailuresFile), string.Join("\n", lines) + "\n", Utf8);
            if (newCards.Count > 0) AddFixCards(newCards);
            return notes;
        }

        /// <summary>Chèn thẻ FIX mới vào cuối mục «## FIX» của QUEUE.md (thay dòng «(chưa có)» nếu còn).</summary>
        static void AddFixCards(List<string> cards)
        {
            var lines = new List<string>(File.ReadAllLines(Path(QueueFile)));
            int fix = lines.FindIndex(l => l.StartsWith("## FIX", StringComparison.Ordinal));
            int queue = lines.FindIndex(l => l.StartsWith("## HÀNG ĐỢI", StringComparison.Ordinal));
            if (fix < 0 || queue < 0 || queue < fix) return;
            int placeholder = lines.FindIndex(fix, queue - fix, l => l.StartsWith("(chưa có", StringComparison.Ordinal));
            if (placeholder >= 0) lines.RemoveAt(placeholder);
            queue = lines.FindIndex(l => l.StartsWith("## HÀNG ĐỢI", StringComparison.Ordinal));
            int insertAt = queue;
            while (insertAt - 1 > fix && lines[insertAt - 1].Trim().Length == 0) insertAt--;
            bool hasHeader = lines.FindIndex(fix, insertAt - fix, l => l.StartsWith("| ID |", StringComparison.Ordinal)) >= 0;
            var add = new List<string>();
            if (!hasHeader)
            {
                add.Add("| ID | Task | Việc (1 câu) | File tạo/sửa | XONG KHI | Cần trước | Trạng thái | Cập nhật |");
                add.Add("|---|---|---|---|---|---|---|---|");
            }
            add.AddRange(cards);
            lines.InsertRange(insertAt, add);
            File.WriteAllText(Path(QueueFile), string.Join("\n", lines) + "\n", Utf8);
        }

        /// <summary>Ghi .ai/GAUNTLET.md.</summary>
        static void WriteGauntlet(SortedDictionary<string, Agg> aggs, Dictionary<string, Reg> registry, List<Leaf> failures, double seconds, List<string> notes)
        {
            int pass = 0, fail = 0, skip = 0;
            foreach (var g in aggs.Values) { pass += g.Pass; fail += g.Fail; skip += g.Skip; }
            var sb = new StringBuilder();
            sb.AppendLine("# GAUNTLET — kết quả chạy MỘT lượt (ghi tự động bởi menu TheReturn/Gauntlet/Chạy Gauntlet mạng)");
            sb.AppendLine();
            sb.AppendLine("Chạy: " + Stamp(startedUtc) + " UTC | tổng thời gian test: " + seconds.ToString("0.0", CultureInfo.InvariantCulture) + " s | assembly có test: " + aggs.Count
                + " | test: " + (pass + fail + skip) + " | đạt " + pass + " | đỏ " + fail + " | bỏ qua " + skip + " | KẾT LUẬN: " + (fail == 0 ? "XANH" : "ĐỎ"));
            sb.AppendLine();
            sb.AppendLine("| Assembly | Nhiệm vụ | Task | Viết bởi | Pha | Test | Đạt | Đỏ | Bỏ qua | Giây |");
            sb.AppendLine("|---|---|---|---|---|---|---|---|---|---|");
            foreach (var pair in aggs)
            {
                Reg reg;
                registry.TryGetValue(pair.Key, out reg);
                Agg g = pair.Value;
                sb.AppendLine("| " + pair.Key + " | " + (reg != null ? reg.Mission : "-") + " | " + (reg != null ? reg.Task : "-") + " | " + (reg != null ? reg.Writer : "-")
                    + " | " + (reg != null ? reg.Phase : "-") + " | " + (g.Pass + g.Fail + g.Skip) + " | " + g.Pass + " | " + g.Fail + " | " + g.Skip + " | " + g.Seconds.ToString("0.00", CultureInfo.InvariantCulture) + " |");
            }
            sb.AppendLine();
            if (failures.Count > 0)
            {
                sb.AppendLine("## Test đỏ (đã ghi vào FAILURES.md + thẻ FIX trong QUEUE.md)");
                int shown = 0;
                foreach (var f in failures)
                {
                    if (shown++ >= 40) { sb.AppendLine("- ... và " + (failures.Count - 40) + " test đỏ khác (xem FAILURES.md)."); break; }
                    sb.AppendLine("- [" + f.Assembly + "] " + f.FullName + " : " + Clean(f.Message, 200));
                }
                sb.AppendLine();
            }
            if (notes.Count > 0)
            {
                sb.AppendLine("## Ghi chú");
                foreach (string n in notes) sb.AppendLine("- " + n);
                sb.AppendLine();
            }
            var missing = new List<string>();
            foreach (var key in registry.Keys) if (!aggs.ContainsKey(key)) missing.Add(key);
            var unregistered = new List<string>();
            foreach (var key in aggs.Keys)
                if (key.Contains(".Network.") && !registry.ContainsKey(key) && !key.StartsWith("TheReturn.Network.", StringComparison.Ordinal)) unregistered.Add(key);
            if (missing.Count > 0 || unregistered.Count > 0)
            {
                sb.AppendLine("## Cảnh báo registry (MISSIONS_NET.md lệch với dự án)");
                foreach (string m in missing) sb.AppendLine("- Registry có " + m + " nhưng không chạy được/không tìm thấy assembly.");
                foreach (string u in unregistered) sb.AppendLine("- Assembly mạng " + u + " CHƯA có trong MISSIONS_NET.md: thêm một dòng (viết bởi ai, task nào).");
            }
            File.WriteAllText(Path(GauntletFile), sb.ToString(), Utf8);
        }

        /// <summary>Cập nhật cột «Gauntlet gần nhất» của từng nhiệm vụ trong MISSIONS_NET.md.</summary>
        static void UpdateRegistry(string[] lines, Dictionary<string, Reg> registry, SortedDictionary<string, Agg> aggs, List<Leaf> failures, double seconds)
        {
            int gauCol = -1;
            foreach (string l in lines)
            {
                if (!l.StartsWith("|", StringComparison.Ordinal)) continue;
                string[] h = l.Split('|');
                for (int k = 0; k < h.Length; k++) if (h[k].Contains("Gauntlet gần nhất")) gauCol = k;
                if (gauCol >= 0) break;
            }
            if (gauCol < 0) return;
            foreach (var pair in registry)
            {
                Agg g;
                if (!aggs.TryGetValue(pair.Key, out g)) continue;
                string[] c = lines[pair.Value.Line].Split('|');
                if (c.Length <= gauCol) continue;
                int all = g.Pass + g.Fail + g.Skip;
                c[gauCol] = " " + Stamp(startedUtc) + " " + (g.Fail == 0 ? "XANH " + g.Pass + "/" + all : "ĐỎ " + g.Fail + " lỗi/" + all) + " ";
                lines[pair.Value.Line] = string.Join("|", c);
            }
            File.WriteAllText(Path(MissionsFile), string.Join("\n", lines) + "\n", Utf8);
        }
    }
}
