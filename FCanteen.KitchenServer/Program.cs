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
const string MenuSyncUrl = "https://raw.githubusercontent.com/tlutlu181105/PRN-222-LAB/refs/heads/lab01/menu-sync.json";
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

_ = Task.Run(async () =>
{
    while (true)
    {
        Console.WriteLine();
        Console.WriteLine("Gõ mã món để đánh dấu HẾT HÀNG, gõ SYNC để đồng bộ giá, hoặc Enter để bỏ qua:");
        string? input = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(input)) continue;

        if (input.Trim().Equals("SYNC", StringComparison.OrdinalIgnoreCase))
        {
            await SyncMenuPricesAsync();
            continue;
        }

        using var db = CreateContext();
        var item = await db.MenuItems.FirstOrDefaultAsync(m => m.Code == input.Trim());
        if (item == null)
        {
            Console.WriteLine($"Không tìm thấy món có mã '{input}'.");
            continue;
        }

        item.IsAvailable = false;
        await db.SaveChangesAsync();
        Console.WriteLine($"Đã đánh dấu '{item.Name}' hết hàng. Đang phát UDP thông báo...");

        await BroadcastSoldOutAsync(item.Id, item.Name);
    }
});

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
async Task BroadcastSoldOutAsync(int menuItemId, string menuItemName)
{
    using var udpClient = new UdpClient();
    udpClient.EnableBroadcast = true;

    string message = $"SOLD_OUT:{menuItemId}:{menuItemName}";
    byte[] data = Encoding.UTF8.GetBytes(message);

    await udpClient.SendAsync(data, data.Length, new IPEndPoint(IPAddress.Broadcast, 9600));
    await LogAsync("UDP", "255.255.255.255:9600", $"Broadcast hết món: {menuItemName}");

    Console.WriteLine($"Đã phát UDP: {message}");
}

async Task SyncMenuPricesAsync()
{
    Console.WriteLine($"Bắt đầu đồng bộ giá từ: {MenuSyncUrl}");

    // Yêu cầu đề bài: dùng Uri để phân tích địa chỉ
    var uri = new Uri(MenuSyncUrl);
    Console.WriteLine($"  Scheme: {uri.Scheme}");
    Console.WriteLine($"  Host:   {uri.Host}");
    Console.WriteLine($"  Port:   {uri.Port}");

    // Yêu cầu đề bài: dùng Dns để phân giải tên miền
    try
    {
        var addresses = await Dns.GetHostAddressesAsync(uri.Host);
        Console.WriteLine("  Địa chỉ IP phân giải được:");
        foreach (var ip in addresses)
            Console.WriteLine($"    - {ip}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  Không phân giải được DNS: {ex.Message}");
    }

    using var http = new HttpClient();
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    HttpResponseMessage response;

    try
    {
        response = await http.GetAsync(uri);
    }
    catch (Exception ex)
    {
        stopwatch.Stop();
        Console.WriteLine($"Lỗi khi gọi HTTP: {ex.Message}");
        await LogAsync("HTTP", uri.Host, $"Lỗi kết nối: {ex.Message}, Thời gian: {stopwatch.ElapsedMilliseconds}ms");
        return;
    }

    stopwatch.Stop();
    string statusInfo = $"Status: {(int)response.StatusCode} {response.StatusCode}, Thời gian: {stopwatch.ElapsedMilliseconds}ms";
    Console.WriteLine(statusInfo);

    await LogAsync("HTTP", uri.Host, statusInfo);

    if (!response.IsSuccessStatusCode)
    {
        Console.WriteLine("Đồng bộ thất bại do lỗi HTTP.");
        return;
    }

    var json = await response.Content.ReadAsStringAsync();
    var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    var remoteItems = JsonSerializer.Deserialize<List<RemoteMenuItem>>(json, jsonOptions);

    if (remoteItems == null || remoteItems.Count == 0)
    {
        Console.WriteLine("File JSON rỗng hoặc không đọc được.");
        return;
    }

    using var db = CreateContext();
    int updatedCount = 0;

    foreach (var remote in remoteItems)
    {
        var local = await db.MenuItems.FirstOrDefaultAsync(m => m.Code == remote.Code);
        if (local == null)
        {
            Console.WriteLine($"  Không tìm thấy món có mã '{remote.Code}' trong DB, bỏ qua.");
            continue;
        }

        if (local.Price != remote.Price)
        {
            Console.WriteLine($"  Cập nhật giá '{local.Name}': {local.Price:N0}đ -> {remote.Price:N0}đ");
            local.Price = remote.Price;
            updatedCount++;
        }
    }

    await db.SaveChangesAsync();
    Console.WriteLine($"Đồng bộ hoàn tất. Đã cập nhật {updatedCount} món.");
}