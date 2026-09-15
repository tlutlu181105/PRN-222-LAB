using FCanteen.Data;
using FCanteen.Data.Entities;
using FCanteen.KitchenServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var connectionString = configuration.GetConnectionString("FCanteenConnection");

FCanteenContext CreateContext()
{
    var optionsBuilder = new DbContextOptionsBuilder<FCanteenContext>();
    optionsBuilder.UseSqlServer(connectionString);
    return new FCanteenContext(optionsBuilder.Options);
}

var listener = new TcpListener(IPAddress.Any, 9500);
listener.Start();
Console.WriteLine("=== FCanteen Kitchen Server ===");
Console.WriteLine("Đang lắng nghe cổng 9500...");

while (true)
{
    TcpClient client = await listener.AcceptTcpClientAsync();
    _ = HandleClientAsync(client);
}

async Task LogAsync(string protocol, string sourceAddress, string content)
{
    using var db = CreateContext();
    db.DeviceLogs.Add(new DeviceLog
    {
        Protocol = protocol,
        SourceAddress = sourceAddress,
        Content = content,
        Timestamp = DateTime.Now
    });
    await db.SaveChangesAsync();
}

async Task HandleClientAsync(TcpClient client)
{
    var remoteAddr = client.Client.RemoteEndPoint?.ToString() ?? "unknown";
    Console.WriteLine($"[+] Kết nối mới từ {remoteAddr}");
    await LogAsync("TCP", remoteAddr, "Kết nối mới");

    try
    {
        using var stream = client.GetStream();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

        string? json = await reader.ReadLineAsync();
        if (json != null)
        {
            var confirmation = await ProcessOrderAsync(json);
            var responseJson = JsonSerializer.Serialize(confirmation);
            await writer.WriteLineAsync(responseJson);
            Console.WriteLine($"[✓] Đã xử lý phiếu #{confirmation.TicketId} - {confirmation.TotalAmount:N0}đ");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[!] Lỗi xử lý kết nối {remoteAddr}: {ex.Message}");
    }
    finally
    {
        await LogAsync("TCP", remoteAddr, "Đóng kết nối");
        client.Close();
        Console.WriteLine($"[-] Đóng kết nối {remoteAddr}");
    }
}

async Task<OrderConfirmation> ProcessOrderAsync(string json)
{
    var request = JsonSerializer.Deserialize<OrderRequest>(json)!;
    using var db = CreateContext();
    using var transaction = await db.Database.BeginTransactionAsync();

    try
    {
        var ticket = new OrderTicket
        {
            CounterName = request.CounterName,
            CreatedAt = DateTime.Now,
            Status = "Pending"
        };

        decimal total = 0;

        foreach (var line in request.Lines)
        {
            var menuItem = await db.MenuItems.FindAsync(line.MenuItemId);
            if (menuItem == null)
                throw new Exception($"Không tìm thấy món có Id = {line.MenuItemId}");

            var unitPrice = menuItem.Price; // Server tự lấy giá thật, không tin client
            total += unitPrice * line.Quantity;

            ticket.TicketLines.Add(new TicketLine
            {
                MenuItemId = line.MenuItemId,
                Quantity = line.Quantity,
                UnitPrice = unitPrice,
                Note = line.Note
            });
        }

        ticket.TotalAmount = total;

        db.OrderTickets.Add(ticket);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();

        await PrintPendingTicketsAsync(db);

        return new OrderConfirmation
        {
            TicketId = ticket.Id,
            TotalAmount = ticket.TotalAmount,
            Message = "Đặt món thành công"
        };
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}

async Task PrintPendingTicketsAsync(FCanteenContext db)
{
    var pending = await db.OrderTickets
        .Where(t => t.Status == "Pending")
        .OrderBy(t => t.CreatedAt)
        .ToListAsync();

    Console.WriteLine("---- Danh sách phiếu đang chờ chế biến ----");
    foreach (var t in pending)
        Console.WriteLine($"  #{t.Id} - {t.CounterName} - {t.TotalAmount:N0}đ - {t.CreatedAt:HH:mm:ss}");
    Console.WriteLine("--------------------------------------------");
}