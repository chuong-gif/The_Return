// PREFLIGHT: liệt kê thay đổi sau mốc UTC trong Core4/shared và .ai (chỉ ĐỌC). Dán nội dung vào execute_code của Unity MCP.
// Đổi chuỗi trong DateTime.Parse thành giờ UTC của mục .ai/LOG.md cuối mà bạn đã đọc. Chỉ quét Core4/shared; bỏ file .meta.
// Kết quả: số file theo nhóm + 25 file mới nhất. File bạn sắp sửa mà có giờ MỚI HƠN mục LOG nhắc tới nó = có thay đổi chưa ghi (xem PROTOCOL.md mục 3c).
var since = System.DateTime.Parse("2026-10-14 00:00:00", null, System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal);
var root = System.IO.Directory.GetParent(Application.dataPath).FullName;
var files = new System.Collections.Generic.List<string>();
var scopes = new[]{
  ".ai", "Assets/TheReturn/Core", "Assets/TheReturn/Shared", "Assets/TheReturn/Features/Network",
  "Assets/TheReturn/Features/Party", "Assets/TheReturn/Features/GameSettings",
  "Assets/TheReturn/Features/Attendance", "Assets/TheReturn/Features/BreakerPanel",
  "Assets/TheReturn/Features/Choir", "Assets/TheReturn/Features/PlankAndKey",
  "Assets/TheReturn/Maps/SchoolFloor/Runtime", "Assets/TheReturn/Maps/Lobby/Runtime",
  "Assets/TheReturn/Maps/MainMenu/Runtime", "Assets/TheReturn/Scenes/Maps/MapSchoolF.unity"
};
foreach (var scope in scopes) {
  var path = System.IO.Path.Combine(root, scope);
  if (System.IO.File.Exists(path)) files.Add(path);
  else if (System.IO.Directory.Exists(path)) files.AddRange(System.IO.Directory.GetFiles(path, "*", System.IO.SearchOption.AllDirectories));
}
foreach (var name in new[]{"AGENTS.md","CLAUDE.md"}) {
  var path = System.IO.Path.Combine(root, name);
  if (System.IO.File.Exists(path)) files.Add(path);
}
var hits = new System.Collections.Generic.List<string[]>();
foreach (var f in files) {
  if (f.EndsWith(".meta")) continue;
  var t = System.IO.File.GetLastWriteTimeUtc(f);
  if (t > since) hits.Add(new[]{ t.ToString("yyyy-MM-dd HH:mm"), f.Substring(root.Length + 1).Replace("\\", "/") });
}
hits.Sort((a, b) => string.CompareOrdinal(b[0], a[0]));
var sb = new System.Text.StringBuilder();
sb.AppendLine("since(UTC)=" + since.ToString("u") + " scanned=" + files.Count + " changed=" + hits.Count);
var groups = new System.Collections.Generic.SortedDictionary<string,int>();
foreach (var h in hits) { var p = h[1].Split('/'); var g = p.Length > 2 ? p[0] + "/" + p[1] : p[0]; groups[g] = groups.ContainsKey(g) ? groups[g] + 1 : 1; }
foreach (var kv in groups) sb.AppendLine("  " + kv.Key + ": " + kv.Value);
for (int i = 0; i < hits.Count && i < 25; i++) sb.AppendLine(hits[i][0] + "  " + hits[i][1]);
return sb.ToString();
