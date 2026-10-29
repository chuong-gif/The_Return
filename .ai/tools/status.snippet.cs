// status.snippet.cs — MỘT lần execute_code thay cho 4-6 lần đọc preflight. Trả ≤ 15 dòng. Dán nguyên vào execute_code.
var root = System.IO.Directory.GetParent(UnityEngine.Application.dataPath).FullName; var ai = System.IO.Path.Combine(root, ".ai");
var sc = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
string r = "play=" + UnityEditor.EditorApplication.isPlaying + " compiling=" + UnityEditor.EditorApplication.isCompiling + " scene=" + sc.name + " dirty=" + sc.isDirty + "\n";
foreach (var l in System.IO.File.ReadAllLines(System.IO.Path.Combine(ai, "CURRENT.md"))) if (l.StartsWith("Scene:") || l.StartsWith("Cập nhật:")) r += (l.Length > 260 ? l.Substring(0, 260) : l) + "\n";
var lg = System.IO.File.ReadAllLines(System.IO.Path.Combine(ai, "LOG.md")); int shown = 0;
for (int i = lg.Length - 1; i >= 0 && shown < 3; i--) if (lg[i].StartsWith("### ")) { r += lg[i].Substring(0, System.Math.Min(150, lg[i].Length)) + "\n"; shown++; }
var lastLog = System.IO.File.GetLastWriteTime(System.IO.Path.Combine(ai, "LOG.md")); int stale = 0; string names = "";
foreach (var f in System.IO.Directory.GetFiles(System.IO.Path.Combine(root, "Assets/TheReturn"), "*.cs", System.IO.SearchOption.AllDirectories)) if (System.IO.File.GetLastWriteTime(f) > lastLog) { stale++; if (stale <= 6) names += System.IO.Path.GetFileName(f) + " "; }
r += "file .cs đổi SAU lần ghi LOG cuối: " + stale + " " + names + "\n";
return r;
