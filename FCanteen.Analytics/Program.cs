using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using FCanteen.Data;
using FCanteen.Analytics;

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

Console.WriteLine("=== YC2: So sánh xử lý tuần tự và song song ===");
Console.WriteLine($"Số lõi logic của máy: {Environment.ProcessorCount}");
Console.WriteLine();

// tai du lieu can thiet vao ram 
Console.WriteLine("Đang tải dữ liệu từ database...");
var loadStopwatch = Stopwatch.StartNew();

List<(int Id, string Name)> menuItems;
List<TicketLineFlat> allLines;

using (var db = CreateContext())
{
    var rawMenu = await db.MenuItems.Select(m => new { m.Id, m.Name }).ToListAsync();
    menuItems = rawMenu.Select(m => (m.Id, m.Name)).ToList();

    allLines = await db.TicketLines
        .Select(tl => new TicketLineFlat
        {
            MenuItemId = tl.MenuItemId,
            Revenue = tl.Quantity * tl.UnitPrice,
            Hour = tl.OrderTicket.CreatedAt.Hour
        })
        .ToListAsync();
}

loadStopwatch.Stop();
Console.WriteLine($"Đã tải {menuItems.Count} món, {allLines.Count:N0} dòng TicketLine trong {loadStopwatch.Elapsed.TotalSeconds:F2}s");
Console.WriteLine();

const int RepeatFactor = 300; 

MenuEfficiencyResult CalculateEfficiency(int menuItemId, string menuItemName, List<TicketLineFlat> allLines)
{
    double efficiencyIndex = 0;
    decimal total = 0;

    
    for (int repeat = 0; repeat < RepeatFactor; repeat++)
    {
        var revenueByHour = new decimal[24];

        foreach (var line in allLines)
        {
            if (line.MenuItemId == menuItemId)
            {
                revenueByHour[line.Hour] += line.Revenue;
            }
        }

        total = revenueByHour.Sum();
        double mean = (double)total / 24;

        double sumSquaredDiff = 0;
        for (int h = 0; h < 24; h++)
        {
            double diff = (double)revenueByHour[h] - mean;
            sumSquaredDiff += diff * diff;
        }
        double stdDev = Math.Sqrt(sumSquaredDiff / 24);

        efficiencyIndex = stdDev == 0 ? (double)total : (double)total / (1 + stdDev);
    }

    return new MenuEfficiencyResult
    {
        MenuItemId = menuItemId,
        MenuItemName = menuItemName,
        TotalRevenue = total,
        EfficiencyIndex = efficiencyIndex
    };
}
//==========================================================//
Console.WriteLine("---- PHIÊN BẢN 1: foreach tuần tự ----");
var sw1 = Stopwatch.StartNew();

var results1 = new List<MenuEfficiencyResult>();
foreach (var item in menuItems)
{
    results1.Add(CalculateEfficiency(item.Id, item.Name, allLines));
}

sw1.Stop();
Console.WriteLine($"Thời gian: {sw1.Elapsed.TotalMilliseconds:F0} ms");
Console.WriteLine();
//==========================================================//
Console.WriteLine("---- PHIÊN BẢN 2: Parallel.ForEach ----");
var sw2 = Stopwatch.StartNew();

var results2 = new System.Collections.Concurrent.ConcurrentBag<MenuEfficiencyResult>();
Parallel.ForEach(menuItems, item =>
{
    results2.Add(CalculateEfficiency(item.Id, item.Name, allLines));
});

sw2.Stop();
Console.WriteLine($"Thời gian: {sw2.Elapsed.TotalMilliseconds:F0} ms");
Console.WriteLine();
//==========================================================//
Console.WriteLine("---- PHIÊN BẢN 3: Parallel.For với MaxDegreeOfParallelism khác nhau ----");

int[] degrees = { 2, 4, Environment.ProcessorCount };

foreach (var degree in degrees)
{
    var sw3 = Stopwatch.StartNew();
    var results3 = new System.Collections.Concurrent.ConcurrentBag<MenuEfficiencyResult>();

    var options = new ParallelOptions { MaxDegreeOfParallelism = degree };
    Parallel.For(0, menuItems.Count, options, i =>
    {
        var item = menuItems[i];
        results3.Add(CalculateEfficiency(item.Id, item.Name, allLines));
    });

    sw3.Stop();
    Console.WriteLine($"MaxDegreeOfParallelism = {degree,2}: {sw3.Elapsed.TotalMilliseconds:F0} ms");
}

Console.WriteLine();
//==========================================================//

Console.WriteLine("---- BẢNG TỔNG KẾT (điền thủ công vào báo cáo dựa trên số liệu trên) ----");
Console.WriteLine($"Số lõi logic máy: {Environment.ProcessorCount}");
Console.WriteLine($"foreach tuần tự:        {sw1.Elapsed.TotalMilliseconds,8:F0} ms  (baseline, hệ số tăng tốc = 1.0x)");
Console.WriteLine($"Parallel.ForEach:       {sw2.Elapsed.TotalMilliseconds,8:F0} ms  (hệ số tăng tốc = {sw1.Elapsed.TotalMilliseconds / sw2.Elapsed.TotalMilliseconds:F2}x)");
//==========================================================//
Console.WriteLine();
Console.WriteLine("Đang ghi kết quả vào DailySettlement...");

using (var db = CreateContext())
{
    db.DailySettlements.Add(new FCanteen.Data.Entities.DailySettlement
    {
        Date = DateTime.Today,
        BranchCode = "ALL",
        TotalTickets = 0, 
        TotalRevenue = 0,
        CalculationTimeMs = sw2.ElapsedMilliseconds 
    });
    await db.SaveChangesAsync();
}

Console.WriteLine("Đã ghi xong.");