// Cách chạy lại: dán NGUYÊN NỘI DUNG file này (không cần dòng chú thích) vào tool execute_code của Unity MCP (action=execute).
// Chỉ ĐỌC project, chỉ GHI .ai/MAPS/index-scripts.md. Compiler codedom = C# 6. Nhớ cập nhật ngày sinh trong tiêu đề nếu đổi.
var root = System.IO.Directory.GetParent(Application.dataPath).FullName;
var rxType = new System.Text.RegularExpressions.Regex(@"^\s*(?:public\s+|internal\s+|private\s+|protected\s+)?(?:static\s+|sealed\s+|abstract\s+|partial\s+)*(class|struct|interface|enum)\s+(\w+)", System.Text.RegularExpressions.RegexOptions.Multiline);
var rxNs = new System.Text.RegularExpressions.Regex(@"^\s*namespace\s+([\w\.]+)", System.Text.RegularExpressions.RegexOptions.Multiline);
var rxGuid = new System.Text.RegularExpressions.Regex("guid: ([0-9a-f]{32})");
var rows = new System.Collections.Generic.List<string[]>();
var known = new System.Collections.Generic.HashSet<string>();
foreach (var f in System.IO.Directory.GetFiles(Application.dataPath, "*.cs", System.IO.SearchOption.AllDirectories)) {
  var rel = f.Replace(Application.dataPath, "Assets").Replace("\\","/");
  if (rel.Contains("/ThirdParty/") || rel.Contains("/TutorialInfo/")) continue;
  var t = System.IO.File.ReadAllText(f);
  var types = new System.Collections.Generic.List<string>();
  foreach (System.Text.RegularExpressions.Match m in rxType.Matches(t)) types.Add(m.Groups[1].Value + " " + m.Groups[2].Value);
  string guid = "";
  if (System.IO.File.Exists(f + ".meta")) guid = rxGuid.Match(System.IO.File.ReadAllText(f + ".meta")).Groups[1].Value;
  rows.Add(new[]{ rel, t.Split('\n').Length.ToString(), rxNs.Match(t).Groups[1].Value, string.Join(", ", types.ToArray()), guid });
  if (guid != "") known.Add(guid);
}
var used = new System.Collections.Generic.Dictionary<string,System.Collections.Generic.List<string>>();
int yaml = 0;
foreach (var ext in new[]{"*.unity","*.prefab","*.asset"}) foreach (var f in System.IO.Directory.GetFiles(System.IO.Path.Combine(Application.dataPath, "TheReturn"), ext, System.IO.SearchOption.AllDirectories)) {
  var name = System.IO.Path.GetFileName(f);
  if (name.StartsWith("Before")) continue;
  var t = System.IO.File.ReadAllText(f);
  if (!t.StartsWith("%YAML")) continue;
  yaml++;
  var seen = new System.Collections.Generic.HashSet<string>();
  foreach (System.Text.RegularExpressions.Match m in rxGuid.Matches(t)) {
    var g = m.Groups[1].Value;
    if (known.Contains(g) && seen.Add(g)) { if (!used.ContainsKey(g)) used[g] = new System.Collections.Generic.List<string>(); used[g].Add(name); }
  }
}
rows.Sort((a,b)=>string.CompareOrdinal(a[0], b[0]));
var sb = new System.Text.StringBuilder();
sb.AppendLine("# Index script / symbol / nơi dùng (TỰ SINH — đừng sửa tay)\n");
sb.AppendLine("Sinh bằng .ai/tools/rebuild_index.snippet.cs. Phạm vi: *.cs ngoài ThirdParty/TutorialInfo. Dùng-bởi = scene/prefab/asset YAML dưới Assets/TheReturn chứa GUID script (bỏ file Before*). Quét " + yaml + " file YAML. Bản đồ gần đúng: không thấy tham chiếu tạo bằng code. Source và Unity là nguồn sự thật.\n");
sb.AppendLine("| File | Dòng | Namespace | Kiểu | Dùng-bởi |");
sb.AppendLine("|---|---|---|---|---|");
foreach (var r in rows) {
  string u = "-";
  System.Collections.Generic.List<string> l;
  if (r[4] != "" && used.TryGetValue(r[4], out l)) u = l.Count + ": " + string.Join(", ", l.GetRange(0, System.Math.Min(3, l.Count)).ToArray());
  sb.AppendLine("| " + r[0].Replace("Assets/TheReturn/","") + " | " + r[1] + " | " + r[2] + " | " + r[3] + " | " + u + " |");
}
var outp = System.IO.Path.Combine(root, ".ai\\MAPS\\index-scripts.md");
System.IO.File.WriteAllText(outp, sb.ToString().Replace("\r\n","\n"), new System.Text.UTF8Encoding(false));
return "written " + rows.Count + " rows";
