using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using FCanteen.Data;
using FCanteen.PlinqReports;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var connectionString = configuration.GetConnectionString("FCanteenConnection");

FCanteenContext CreateContext()
{
    var optionsBuilder = new DbContextOptionsBuilder<FCanteenContext>();
    optionsBuilder.UseSqlServer(connectionString, sql => sql.CommandTimeout(180));
    return new FCanteenContext(optionsBuilder.Options);
}

Console.WriteLine("=== YC3: Báo cáo bằng PLINQ ===");
Console.WriteLine($"Số lõi logic của máy: {Environment.ProcessorCount}");
Console.WriteLine();

Console.WriteLine("Đang tải dữ liệu...");
var loadSw = Stopwatch.StartNew();

List<TicketLineForReport> data;
using (var db = CreateContext())
{
    data = await db.TicketLines
        .Select(tl => new TicketLineForReport
        {
            MenuItemId = tl.MenuItemId,
            MenuItemName = tl.MenuItem.Name,
            Revenue = tl.Quantity * tl.UnitPrice,
            CreatedAt = tl.OrderTicket.CreatedAt,
            BranchCode = tl.OrderTicket.BranchCode
        })
        .ToListAsync();
}

loadSw.Stop();
Console.WriteLine($"Đã tải {data.Count:N0} dòng trong {loadSw.Elapsed.TotalSeconds:F2}s");
Console.WriteLine();
//===============================//
Console.WriteLine("==================== BÁO CÁO 1: Top 10 món bán chạy theo doanh thu ====================");

// Phiên bản LINQ tuần tự
var sw1a = Stopwatch.StartNew();
var top10Sequential = data
    .GroupBy(x => new { x.MenuItemId, x.MenuItemName })
    .Select(g => new { g.Key.MenuItemName, TotalRevenue = g.Sum(x => x.Revenue) })
    .OrderByDescending(x => x.TotalRevenue)
    .Take(10)
    .ToList();
sw1a.Stop();

// Phiên bản PLINQ — CẦN AsOrdered() vì đây là bảng xếp hạng, thứ tự Top 1-10 có ý nghĩa nghiệp vụ
var sw1b = Stopwatch.StartNew();
var top10Plinq = data
    .AsParallel()
    .AsOrdered()
    .GroupBy(x => new { x.MenuItemId, x.MenuItemName })
    .Select(g => new { g.Key.MenuItemName, TotalRevenue = g.Sum(x => x.Revenue) })
    .OrderByDescending(x => x.TotalRevenue)
    .Take(10)
    .ToList();
sw1b.Stop();

Console.WriteLine($"LINQ tuần tự: {sw1a.Elapsed.TotalMilliseconds:F1} ms | PLINQ (AsOrdered): {sw1b.Elapsed.TotalMilliseconds:F1} ms");
foreach (var item in top10Plinq)
    Console.WriteLine($"  {item.MenuItemName,-30} {item.TotalRevenue,15:N0}đ");
Console.WriteLine();

//===============================//
Console.WriteLine("==================== BÁO CÁO 2: Doanh thu theo khung giờ ====================");

var sw2a = Stopwatch.StartNew();
var revenueByHourSequential = data
    .GroupBy(x => x.CreatedAt.Hour)
    .Select(g => new { Hour = g.Key, TotalRevenue = g.Sum(x => x.Revenue) })
    .OrderBy(x => x.Hour)
    .ToList();
sw2a.Stop();

// KHÔNG cần AsOrdered() — GroupBy theo giờ không quan tâm thứ tự xử lý của PLINQ,
// vì sau đó ta tự OrderBy(Hour) lại bằng LINQ tuần tự (rẻ, chỉ 24 phần tử) để hiển thị
var sw2b = Stopwatch.StartNew();
var revenueByHourPlinq = data
    .AsParallel()
    .GroupBy(x => x.CreatedAt.Hour)
    .Select(g => new { Hour = g.Key, TotalRevenue = g.Sum(x => x.Revenue) })
    .ToList()
    .OrderBy(x => x.Hour) // sắp xếp lại sau khi PLINQ xong, rẻ vì chỉ có tối đa 24 dòng
    .ToList();
sw2b.Stop();

Console.WriteLine($"LINQ tuần tự: {sw2a.Elapsed.TotalMilliseconds:F1} ms | PLINQ: {sw2b.Elapsed.TotalMilliseconds:F1} ms");
foreach (var item in revenueByHourPlinq)
    Console.WriteLine($"  {item.Hour,2}h: {item.TotalRevenue,15:N0}đ");
Console.WriteLine();

//===============================//
Console.WriteLine("==================== BÁO CÁO 3: Cơ sở doanh thu cao nhất từng tháng ====================");

var sw3a = Stopwatch.StartNew();
var topBranchPerMonthSequential = data
    .GroupBy(x => new { x.CreatedAt.Year, x.CreatedAt.Month })
    .Select(g => new
    {
        g.Key.Year,
        g.Key.Month,
        TopBranch = g.GroupBy(x => x.BranchCode)
                      .Select(bg => new { Branch = bg.Key, Revenue = bg.Sum(x => x.Revenue) })
                      .OrderByDescending(x => x.Revenue)
                      .First()
    })
    .OrderBy(x => x.Year).ThenBy(x => x.Month)
    .ToList();
sw3a.Stop();

// KHÔNG cần AsOrdered() — mỗi tháng độc lập tính "cơ sở nào cao nhất", thứ tự xử lý các tháng
// giữa các luồng không ảnh hưởng kết quả; sắp xếp lại theo tháng ở bước cuối (rẻ, chỉ 6 dòng)
var sw3b = Stopwatch.StartNew();
var topBranchPerMonthPlinq = data
    .AsParallel()
    .GroupBy(x => new { x.CreatedAt.Year, x.CreatedAt.Month })
    .Select(g => new
    {
        g.Key.Year,
        g.Key.Month,
        TopBranch = g.GroupBy(x => x.BranchCode)
                      .Select(bg => new { Branch = bg.Key, Revenue = bg.Sum(x => x.Revenue) })
                      .OrderByDescending(x => x.Revenue)
                      .First()
    })
    .ToList()
    .OrderBy(x => x.Year).ThenBy(x => x.Month)
    .ToList();
sw3b.Stop();

Console.WriteLine($"LINQ tuần tự: {sw3a.Elapsed.TotalMilliseconds:F1} ms | PLINQ: {sw3b.Elapsed.TotalMilliseconds:F1} ms");
foreach (var item in topBranchPerMonthPlinq)
    Console.WriteLine($"  Tháng {item.Month:00}/{item.Year}: {item.TopBranch.Branch} - {item.TopBranch.Revenue:N0}đ");
Console.WriteLine();

//===============================//
Console.WriteLine("==================== BÁO CÁO 4: Món có doanh thu dưới 1% tổng doanh thu ====================");

decimal grandTotal = data.Sum(x => x.Revenue);
decimal threshold = grandTotal * 0.01m;

var sw4a = Stopwatch.StartNew();
var lowRevenueSequential = data
    .GroupBy(x => new { x.MenuItemId, x.MenuItemName })
    .Select(g => new { g.Key.MenuItemName, TotalRevenue = g.Sum(x => x.Revenue) })
    .Where(x => x.TotalRevenue < threshold)
    .ToList();
sw4a.Stop();

// KHÔNG cần AsOrdered() — đây là phép LỌC (Where), không phải bảng xếp hạng,
// thứ tự các món "đề xuất loại bỏ" trong danh sách không mang ý nghĩa nghiệp vụ nào
var sw4b = Stopwatch.StartNew();
var lowRevenuePlinq = data
    .AsParallel()
    .GroupBy(x => new { x.MenuItemId, x.MenuItemName })
    .Select(g => new { g.Key.MenuItemName, TotalRevenue = g.Sum(x => x.Revenue) })
    .Where(x => x.TotalRevenue < threshold)
    .ToList();
sw4b.Stop();

Console.WriteLine($"Tổng doanh thu: {grandTotal:N0}đ | Ngưỡng 1%: {threshold:N0}đ");
Console.WriteLine($"LINQ tuần tự: {sw4a.Elapsed.TotalMilliseconds:F1} ms | PLINQ: {sw4b.Elapsed.TotalMilliseconds:F1} ms");
if (lowRevenuePlinq.Count == 0)
    Console.WriteLine("  (Không có món nào dưới ngưỡng 1% — dữ liệu seed khá đồng đều)");
foreach (var item in lowRevenuePlinq)
    Console.WriteLine($"  ĐỀ XUẤT LOẠI BỎ: {item.MenuItemName,-30} {item.TotalRevenue,15:N0}đ");