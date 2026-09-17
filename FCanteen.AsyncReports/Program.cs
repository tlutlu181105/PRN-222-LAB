using FCanteen.Data;
using FCanteen.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

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

Console.WriteLine("=== YC4: Bất đồng bộ với EF Core ===");
Console.WriteLine();
//===============================//
async Task<int> GetTotalTicketCountAsync(CancellationToken ct)
{
    using var db = CreateContext();
    return await db.OrderTickets.CountAsync(ct);
}

async Task<decimal> GetTotalRevenueAsync(CancellationToken ct)
{
    using var db = CreateContext();
    return await db.OrderTickets.SumAsync(t => t.TotalAmount, ct);
}

async Task<string> GetTopMenuItemAsync(CancellationToken ct)
{
    using var db = CreateContext();
    var top = await db.TicketLines
        .GroupBy(tl => tl.MenuItem.Name)
        .Select(g => new { Name = g.Key, Revenue = g.Sum(x => x.Quantity * x.UnitPrice) })
        .OrderByDescending(x => x.Revenue)
        .FirstOrDefaultAsync(ct);
    return top?.Name ?? "Không có dữ liệu";
}

async Task<string> GetTopBranchAsync(CancellationToken ct)
{
    using var db = CreateContext();
    var top = await db.OrderTickets
        .GroupBy(t => t.BranchCode)
        .Select(g => new { Branch = g.Key, Revenue = g.Sum(x => x.TotalAmount) })
        .OrderByDescending(x => x.Revenue)
        .FirstOrDefaultAsync(ct);
    return top?.Branch ?? "Không có dữ liệu";
}
//===============================//
async Task GenerateDailyReportAsync(CancellationToken ct)
{
    Console.WriteLine("---- Chạy 4 truy vấn ĐỒNG THỜI bằng Task.WhenAll ----");
    var sw = Stopwatch.StartNew();

    Task<int> taskCount = GetTotalTicketCountAsync(ct);
    Task<decimal> taskRevenue = GetTotalRevenueAsync(ct);
    Task<string> taskTopMenu = GetTopMenuItemAsync(ct);
    Task<string> taskTopBranch = GetTopBranchAsync(ct);

    await Task.WhenAll(taskCount, taskRevenue, taskTopMenu, taskTopBranch);

    sw.Stop();

    Console.WriteLine($"Tổng số phiếu: {taskCount.Result:N0}");
    Console.WriteLine($"Tổng doanh thu: {taskRevenue.Result:N0}đ");
    Console.WriteLine($"Món bán chạy nhất: {taskTopMenu.Result}");
    Console.WriteLine($"Cơ sở doanh thu cao nhất: {taskTopBranch.Result}");
    Console.WriteLine($"Thời gian (song song): {sw.Elapsed.TotalMilliseconds:F0} ms");
    Console.WriteLine();
}
//===============================//
async Task GenerateDailyReportSequentialAsync(CancellationToken ct)
{
    Console.WriteLine("---- Chạy 4 truy vấn TUẦN TỰ (await nối tiếp) ----");
    var sw = Stopwatch.StartNew();

    int count = await GetTotalTicketCountAsync(ct);
    decimal revenue = await GetTotalRevenueAsync(ct);
    string topMenu = await GetTopMenuItemAsync(ct);
    string topBranch = await GetTopBranchAsync(ct);

    sw.Stop();

    Console.WriteLine($"Tổng số phiếu: {count:N0}");
    Console.WriteLine($"Tổng doanh thu: {revenue:N0}đ");
    Console.WriteLine($"Món bán chạy nhất: {topMenu}");
    Console.WriteLine($"Cơ sở doanh thu cao nhất: {topBranch}");
    Console.WriteLine($"Thời gian (tuần tự): {sw.Elapsed.TotalMilliseconds:F0} ms");
    Console.WriteLine();
}
//===============================//

async IAsyncEnumerable<OrderTicket> GetHighValueTicketsAsync(decimal minAmount, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
{
    using var db = CreateContext();
    var query = db.OrderTickets
        .Where(t => t.TotalAmount >= minAmount)
        .OrderByDescending(t => t.TotalAmount)
        .AsAsyncEnumerable();

    await foreach (var ticket in query.WithCancellation(ct))
    {
        yield return ticket;
    }
}
Console.WriteLine("---- Duyệt các phiếu có giá trị >= 300,000đ bằng IAsyncEnumerable ----");
int highValueCount = 0;
decimal highValueSum = 0;

await foreach (var ticket in GetHighValueTicketsAsync(300_000))
{
    highValueCount++;
    highValueSum += ticket.TotalAmount;

    if (highValueCount <= 5) // chỉ in 5 dòng đầu ra màn hình cho gọn
        Console.WriteLine($"  #{ticket.Id} - {ticket.BranchCode} - {ticket.TotalAmount:N0}đ");
}

Console.WriteLine($"Tổng số phiếu >= 300,000đ: {highValueCount:N0}, tổng giá trị: {highValueSum:N0}đ");
Console.WriteLine();
//===============================//
Console.WriteLine("---- DEMO: CancellationToken bị kích hoạt (giả lập vượt thời gian) ----");
try
{
    using var shortCts = new CancellationTokenSource(TimeSpan.FromMilliseconds(1));
    await Task.Delay(50); // đợi 1 chút để chắc chắn token đã hết hạn trước khi query chạy
    await GenerateDailyReportSequentialAsync(shortCts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("!!! Đã bắt được OperationCanceledException đúng như thiết kế — token hết hạn sau 1ms !!!");
}
Console.WriteLine();
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

try
{
    await GenerateDailyReportSequentialAsync(cts.Token);
    await GenerateDailyReportAsync(cts.Token);

    Console.WriteLine("=== Cả 2 phiên bản chạy thành công trong thời hạn 30 giây ===");
}
catch (OperationCanceledException)
{
    Console.WriteLine();
    Console.WriteLine("!!! Quá trình đã bị HỦY vì vượt quá 30 giây (CancellationToken kích hoạt) !!!");
}