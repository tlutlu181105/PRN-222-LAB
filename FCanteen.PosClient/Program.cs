using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using FCanteen.Data;
using FCanteen.PosClient;

// Bước 1: Lấy tên quầy từ tham số dòng lệnh
string counterName = args.Length > 0 ? args[0] : "QUAY-UNKNOWN";

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

Console.WriteLine("=== FCanteen POS Client ===");
Console.WriteLine($"Quầy: {counterName}");
Console.WriteLine();

while (true)
{
    // Bước 2: Đọc thực đơn từ DB
    List<(int Id, string Name, decimal Price, string Unit)> menu;
    using (var db = CreateContext())
    {
        menu = await db.MenuItems
            .Where(m => m.IsAvailable)
            .OrderBy(m => m.Id)
            .Select(m => new { m.Id, m.Name, m.Price, m.Unit })
            .ToListAsync()
            .ContinueWith(t => t.Result.Select(m => (m.Id, m.Name, m.Price, m.Unit)).ToList());
    }

    Console.WriteLine("---- THỰC ĐƠN ----");
    for (int i = 0; i < menu.Count; i++)
        Console.WriteLine($"{i + 1,2}. {menu[i].Name,-30} {menu[i].Price,10:N0}đ / {menu[i].Unit}");
    Console.WriteLine("------------------");
    Console.WriteLine();

    // Bước 3: Nhập nhiều dòng món
    var lines = new List<OrderLineRequest>();
    decimal tamTinh = 0;

    while (true)
    {
        Console.Write("Chọn món số mấy (Enter để kết thúc đơn): ");
        string? input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input))
            break;

        if (!int.TryParse(input, out int choice) || choice < 1 || choice > menu.Count)
        {
            Console.WriteLine("Số không hợp lệ, thử lại.");
            continue;
        }

        var selected = menu[choice - 1];

        Console.Write("Số lượng: ");
        if (!int.TryParse(Console.ReadLine(), out int quantity) || quantity <= 0)
        {
            Console.WriteLine("Số lượng không hợp lệ, thử lại.");
            continue;
        }

        Console.Write("Ghi chú (Enter nếu không có): ");
        string? note = Console.ReadLine();

        lines.Add(new OrderLineRequest
        {
            MenuItemId = selected.Id,
            Quantity = quantity,
            Note = string.IsNullOrWhiteSpace(note) ? null : note
        });

        tamTinh += selected.Price * quantity;
        Console.WriteLine($"  -> Đã thêm: {selected.Name} x{quantity}. Tạm tính: {tamTinh:N0}đ");
        Console.WriteLine();
    }

    if (lines.Count == 0)
    {
        Console.WriteLine("Đơn trống, không gửi.");
        continue;
    }

    // Bước 4: Gửi lên server, chờ xác nhận
    try
    {
        using var client = new TcpClient();
        await client.ConnectAsync("127.0.0.1", 9500);
        using var stream = client.GetStream();
        using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
        using var reader = new StreamReader(stream, Encoding.UTF8);

        var request = new OrderRequest { CounterName = counterName, Lines = lines };
        var requestJson = JsonSerializer.Serialize(request);
        await writer.WriteLineAsync(requestJson);

        string? responseJson = await reader.ReadLineAsync();
        if (responseJson != null)
        {
            var confirmation = JsonSerializer.Deserialize<OrderConfirmation>(responseJson)!;
            Console.WriteLine();
            Console.WriteLine("==== XÁC NHẬN TỪ BẾP ====");
            Console.WriteLine($"Mã phiếu: #{confirmation.TicketId}");
            Console.WriteLine($"Tổng tiền (server tính): {confirmation.TotalAmount:N0}đ");
            Console.WriteLine($"Thông báo: {confirmation.Message}");
            Console.WriteLine("=========================");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Lỗi kết nối tới bếp: {ex.Message}");
    }

    Console.WriteLine();
    Console.WriteLine("Nhấn Enter để tạo đơn mới, hoặc Ctrl+C để thoát...");
    Console.ReadLine();
    Console.Clear();
}