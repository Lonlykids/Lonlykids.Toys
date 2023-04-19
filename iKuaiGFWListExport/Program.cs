using System.Text;

Console.WriteLine("<<GFW列表 转 爱快域名分流导入文件>>");
using var httpClient = new HttpClient();
Console.WriteLine("下载中->https://raw.githubusercontent.com/gfwlist/gfwlist/master/gfwlist.txt");
var response = await httpClient.GetAsync("https://raw.githubusercontent.com/gfwlist/gfwlist/master/gfwlist.txt");
var base64_GFWContent = await response.Content.ReadAsStringAsync();
var byte_gfwContent = Convert.FromBase64String(base64_GFWContent);
var gfwContent = Encoding.UTF8.GetString(byte_gfwContent);
var list_GFW = gfwContent.Split('\n')
                         .Where(p => (p.StartsWith('.') || p.StartsWith("||")) && !p.Contains('/'))
                         .Select(p => p.TrimStart('.'))
                         .Select(p => p.TrimStart('|'))
                         .Select(p => p.TrimStart('|'))
                         .Distinct()
                         .ToList();
Console.WriteLine("GFW列表下载成功 , 请输入IP分组名称:");
var src_Addr = Console.ReadLine();
if (string.IsNullOrWhiteSpace(src_Addr))
{
    Console.WriteLine("IP分组名称不能为空");
    await Task.Delay(1000);
    return;
}
Console.WriteLine("分流端口(默认wan2):");
var str_interface = Console.ReadLine();
if (string.IsNullOrWhiteSpace(str_interface)) str_interface = "wan2";
Console.WriteLine("保存路径(默认程序目录):");
var fileSaveDirPath = Console.ReadLine();
if (string.IsNullOrWhiteSpace(fileSaveDirPath)) fileSaveDirPath = AppDomain.CurrentDomain.BaseDirectory;
if (!Directory.Exists(fileSaveDirPath))
{
    Console.WriteLine("目录不存在");
    await Task.Delay(1000);
    return;
}
var fileSavePath = $"{fileSaveDirPath}/ikuaiDomainExport.txt";
if (File.Exists(fileSavePath)) File.Delete(fileSavePath);
var skipRow = 0;
var sb = new StringBuilder();
var rowId = 2;
sb.AppendLine($"id=1 enabled=yes comment=Custom domain=gaoqing.fm interface={str_interface} src_addr={src_Addr} week=1234567 time=00:00-23:59");
while (true)
{
    var lines = list_GFW.Skip(skipRow).Take(1000);
    if (lines == default || !lines.Any()) break;
    skipRow += lines.Count();
    sb.AppendLine($"id={rowId} enabled=yes comment=GFWList_{rowId - 1} domain={string.Join(',', lines)} interface={str_interface} src_addr={src_Addr} week=1234567 time=00:00-23:59");
    rowId++;
    if (lines.Count() < 1000) break;
}
File.WriteAllText(fileSavePath, sb.ToString());