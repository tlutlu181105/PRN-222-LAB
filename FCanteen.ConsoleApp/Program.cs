using FCanteen.Data;
using FCanteen.Repositories.Implementations;
using FCanteen.Repositories.Interfaces;
using FCanteen.Services.Implementations;
using FCanteen.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FCanteen.Services.Discounts;
using FCanteen.Services.Notifications;

var builder = Host.CreateApplicationBuilder(args);

// Đọc connection string từ appsettings.json (builder.Configuration đã tự nạp sẵn)
var connectionString = builder.Configuration.GetConnectionString("FCanteenConnection");

// Đăng ký DbContext vào DI container — mặc định là Scoped (mỗi lần "request" mới 1 instance)
builder.Services.AddDbContext<FCanteenContext>(options =>
    options.UseSqlServer(connectionString, sql => sql.CommandTimeout(180)));

// Đăng ký Repository: interface -> implementation
builder.Services.AddScoped<IMenuItemRepository, MenuItemRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IIngredientRepository, IngredientRepository>();

//
builder.Services.AddScoped<IDiscountLogRepository, DiscountLogRepository>();
builder.Services.AddScoped<IStaffRepository, StaffRepository>();

// Chính sách giảm giá: mỗi chính sách 1 dòng. Thêm chính sách mới = thêm 1 dòng ở đây.
builder.Services.AddScoped<IDiscountPolicy, ComboDiscountPolicy>();
builder.Services.AddScoped<IDiscountPolicy, StudentDiscountPolicy>();
builder.Services.AddScoped<IDiscountPolicy, StaffDiscountPolicy>();
// Đăng ký Service: interface -> implementation
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
//
builder.Services.AddScoped<INotificationService, ConsoleNotificationService>();

var host = builder.Build();

Console.WriteLine("=== FCanteen Console App (DI Architecture) ===");
Console.WriteLine();

// Lấy Service ra từ container để dùng thử — KHÔNG có dòng "new OrderService(...)" nào ở đây
using (var scope = host.Services.CreateScope())
{
    var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
    var reportService = scope.ServiceProvider.GetRequiredService<IReportService>();
    var inventoryService = scope.ServiceProvider.GetRequiredService<IInventoryService>();

    Console.WriteLine("Đã lấy thành công 3 Service từ DI container:");
    Console.WriteLine($"  IOrderService     -> {orderService.GetType().Name}");
    Console.WriteLine($"  IReportService    -> {reportService.GetType().Name}");
    Console.WriteLine($"  IInventoryService -> {inventoryService.GetType().Name}");
    Console.WriteLine();

    // Test nhanh 1 chức năng thật để chắc chắn cả chuỗi DI hoạt động đúng
    Console.WriteLine("Top 5 món bán chạy nhất:");
    var top5 = await reportService.GetTopSellingItemsAsync(5);
    foreach (var (name, revenue) in top5)
        Console.WriteLine($"  {name,-30} {revenue,15:N0}đ");

    Console.WriteLine();
    Console.WriteLine("Nguyên liệu sắp hết hàng:");

    Console.WriteLine();
    Console.WriteLine("=== TEST CHÍNH SÁCH GIẢM GIÁ ===");

    Console.WriteLine();
    Console.WriteLine("=== TEST KÊNH THÔNG BÁO ===");
    await orderService.CreateOrderAsync("QUAY02", "CS02", new List<OrderLineRequest>
    {
        new() { MenuItemId = 3, Quantity = 2 }
    });

    var staffRepo = scope.ServiceProvider.GetRequiredService<IStaffRepository>();
    var teacher = await staffRepo.GetByCodeAsync("GV001");

    // 1 món chính (Id 1) + 1 nước (Id 12) => đủ điều kiện combo
    var lines = new List<OrderLineRequest>
    {
        new() { MenuItemId = 1, Quantity = 1 },
        new() { MenuItemId = 12, Quantity = 1 }
    };

    void PrintResult(string title, OrderResult r)
    {
        Console.WriteLine($"--- {title} ---");
        Console.WriteLine($"  Tạm tính: {r.Subtotal:N0}đ");
        foreach (var d in r.Discounts)
            Console.WriteLine($"  - {d.Description}: -{d.AmountOff:N0}đ");
        Console.WriteLine($"  Thành tiền: {r.Ticket.TotalAmount:N0}đ (phiếu #{r.Ticket.Id})");
    }

    PrintResult("Khách thường", await orderService.CreateOrderAsync("QUAY01", "CS01", lines));
    PrintResult("Sinh viên", await orderService.CreateOrderAsync("QUAY01", "CS01", lines, isStudent: true));
    PrintResult("Giảng viên GV001", await orderService.CreateOrderAsync("QUAY01", "CS01", lines, staff: teacher));
    PrintResult("Sinh viên + Giảng viên", await orderService.CreateOrderAsync("QUAY01", "CS01", lines, isStudent: true, staff: teacher));
    var lowStock = await inventoryService.GetLowStockIngredientsAsync();
    if (lowStock.Count == 0)
        Console.WriteLine("  (Không có nguyên liệu nào dưới ngưỡng cảnh báo)");
    foreach (var ing in lowStock)
        Console.WriteLine($"  {ing.Name}: còn {ing.StockQuantity} {ing.Unit} (ngưỡng {ing.WarningThreshold})");
}