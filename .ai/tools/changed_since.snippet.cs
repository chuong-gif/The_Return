// PREFLIGHT: liệt kê file đổi sau một mốc giờ UTC (chỉ ĐỌC). Dán nội dung (bỏ dòng chú thích) vào execute_code của Unity MCP.
// Đổi chuỗi trong DateTime.Parse thành giờ UTC của mục .ai/LOG.md cuối mà bạn đã đọc. Bỏ qua Library/Temp/obj/Logs/.git/UserSettings/node_modules và file .meta.
// Kết quả: số file theo nhóm + 25 file mới nhất. File bạn sắp sửa mà có giờ MỚI HƠN mục LOG nhắc tới nó = có thay đổi chưa ghi (xem PROTOCOL.md mục 3c).
var since = System.DateTime.Parse("2026-10-14 00:00:00", null, System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal);
var root = System.IO.Directory.GetParent(Application.dataPath).FullName;
var skip = new System.Collections.Generic.HashSet<string>{"Library","Temp","obj","Logs",".git","UserSettings","node_modules"};
var files = new System.Collections.Generic.List<string>(System.IO.Directory.GetFiles(root));
foreach (var d in System.IO.Directory.GetDirectories(root)) if (!skip.Contains(System.IO.Path.GetFileName(d))) files.AddRange(System.IO.Directory.GetFiles(d, "*", System.IO.SearchOption.AllDirectories));
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
