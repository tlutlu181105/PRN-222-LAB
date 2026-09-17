using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using FCanteen.Data;
using FCanteen.Data.Entities;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var connectionString = configuration.GetConnectionString("FCanteenConnection");

FCanteenContext CreateContext()
{
    var optionsBuilder = new DbContextOptionsBuilder<FCanteenContext>();
    optionsBuilder.UseSqlServer(connectionString, sql => sql.CommandTimeout(180));
    var context = new FCanteenContext(optionsBuilder.Options);
    context.ChangeTracker.AutoDetectChangesEnabled = false; 
    return context;
}

const int TotalTickets = 50_000;
const int BatchSize = 2000;
string[] branches = { "CS01", "CS02", "CS03" };

Console.WriteLine("=== FCanteen Data Seeder ===");
Console.WriteLine($"Mục tiêu: {TotalTickets:N0} OrderTicket, mỗi phiếu 3-5 dòng...");
Console.WriteLine();


List<(int Id, decimal Price)> menuItems;
using (var db = CreateContext())
{
    var raw = await db.MenuItems.Select(m => new { m.Id, m.Price }).ToListAsync();
    menuItems = raw.Select(m => (m.Id, m.Price)).ToList();
}

if (menuItems.Count == 0)
{
    Console.WriteLine("Không có MenuItem nào trong DB. Hãy chắc chắn Lab 01 đã seed xong. Dừng lại.");
    return;
}

var random = Random.Shared;
var startDate = DateTime.Today.AddMonths(-6);
var endDate = DateTime.Today;
int totalDays = (endDate - startDate).Days;

var stopwatch = Stopwatch.StartNew();
int ticketsCreated = 0;
int linesCreated = 0;

using (var db = CreateContext())
{
    var batch = new List<OrderTicket>();

    for (int i = 0; i < TotalTickets; i++)
    {
        var randomDay = startDate.AddDays(random.Next(totalDays));
        int hour = PickPeakWeightedHour(random);
        int minute = random.Next(60);
        int second = random.Next(60);
        var createdAt = new DateTime(randomDay.Year, randomDay.Month, randomDay.Day, hour, minute, second);

        var branch = branches[random.Next(branches.Length)];

        var ticket = new OrderTicket
        {
            CounterName = $"QUAY{random.Next(1, 4):00}",
            BranchCode = branch,
            CreatedAt = createdAt,
            Status = "Done"
        };

        int lineCount = random.Next(3, 6); 
        decimal total = 0;

        for (int j = 0; j < lineCount; j++)
        {
            var item = menuItems[random.Next(menuItems.Count)];
            int qty = random.Next(1, 4);

            ticket.TicketLines.Add(new TicketLine
            {
                MenuItemId = item.Id,
                Quantity = qty,
                UnitPrice = item.Price
            });

            total += item.Price * qty;
            linesCreated++;
        }

        ticket.TotalAmount = total;
        batch.Add(ticket);
        ticketsCreated++;

        
        if (batch.Count >= BatchSize)
        {
            db.OrderTickets.AddRange(batch);
            await db.SaveChangesAsync();
            db.ChangeTracker.Clear(); 
            batch.Clear();

            Console.WriteLine($"Đã tạo {ticketsCreated:N0}/{TotalTickets:N0} phiếu... ({stopwatch.Elapsed.TotalSeconds:F1}s)");
        }
    }

    if (batch.Count > 0)
    {
        db.OrderTickets.AddRange(batch);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
    }
}

stopwatch.Stop();

Console.WriteLine();
Console.WriteLine("=== HOÀN TẤT ===");
Console.WriteLine($"Tổng số OrderTicket: {ticketsCreated:N0}");
Console.WriteLine($"Tổng số TicketLine:  {linesCreated:N0}");
Console.WriteLine($"Thời gian sinh dữ liệu: {stopwatch.Elapsed.TotalSeconds:F2} giây");


int PickPeakWeightedHour(Random rnd)
{
    if (rnd.NextDouble() < 0.65) 
    {
        return rnd.Next(11, 14); 
    }
    else
    {
        return rnd.Next(7, 22); 
    }
}