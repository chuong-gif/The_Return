// Sinh bảng BOARD trong .ai/START_HERE.md từ .ai/TASKS.md (chạy SAU KHI sửa TASKS). Dán vào execute_code của Unity MCP.
var ai = System.IO.Path.Combine(System.IO.Directory.GetParent(UnityEngine.Application.dataPath).FullName, ".ai");
var sp = System.IO.Path.Combine(ai, "START_HERE.md"); var s = System.IO.File.ReadAllText(sp);
var sb = new System.Text.StringBuilder();
foreach (var l in System.IO.File.ReadAllLines(System.IO.Path.Combine(ai, "TASKS.md"))) {
  if (!l.StartsWith("| T-")) continue; var c = l.Split('|'); if (c.Length < 6) continue;
  string t = c[5].Trim(); if (t.Length > 90) t = t.Substring(0, 90) + "…";
  string o = c[3].Trim(); if (o.Length > 28) o = o.Substring(0, 28) + "…";
  sb.AppendLine("- " + c[1].Trim() + " " + c[2].Trim() + " | " + o + " | " + t);
}
int a = s.IndexOf("<!--BOARD-->"), b = s.IndexOf("<!--/BOARD-->");
if (a < 0 || b < a) return "markers missing";
s = s.Substring(0, a + 12) + "\n" + sb + s.Substring(b);
System.IO.File.WriteAllText(sp, s, new System.Text.UTF8Encoding(false));
return "board ok";
