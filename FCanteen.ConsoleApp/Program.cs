using FCanteen.Data;
using FCanteen.Repositories.Implementations;
using FCanteen.Repositories.Interfaces;
using FCanteen.Services.Implementations;
using FCanteen.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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

// Đăng ký Service: interface -> implementation
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();

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
    var lowStock = await inventoryService.GetLowStockIngredientsAsync();
    if (lowStock.Count == 0)
        Console.WriteLine("  (Không có nguyên liệu nào dưới ngưỡng cảnh báo)");
    foreach (var ing in lowStock)
        Console.WriteLine($"  {ing.Name}: còn {ing.StockQuantity} {ing.Unit} (ngưỡng {ing.WarningThreshold})");
}