using Microsoft.EntityFrameworkCore;
using FCanteen.Data;
using FCanteen.Repositories.Interfaces;
using FCanteen.Repositories.Implementations;
using FCanteen.Services.Interfaces;
using FCanteen.Services.Implementations;
using FCanteen.Services.Discounts;
using FCanteen.Services.Notifications;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký MVC
builder.Services.AddControllersWithViews();

// Đăng ký DbContext
var connectionString = builder.Configuration.GetConnectionString("FCanteenConnection");
builder.Services.AddDbContext<FCanteenContext>(options =>
    options.UseSqlServer(connectionString, sql => sql.CommandTimeout(180)));

// Đăng ký Repository (giống hệt cách làm ở ConsoleApp Lab 03)
builder.Services.AddScoped<IMenuItemRepository, MenuItemRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IIngredientRepository, IngredientRepository>();
builder.Services.AddScoped<IDiscountLogRepository, DiscountLogRepository>();
builder.Services.AddScoped<IStaffRepository, StaffRepository>();

// Đăng ký Service
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();

// Chính sách giảm giá
builder.Services.AddScoped<IDiscountPolicy, ComboDiscountPolicy>();
builder.Services.AddScoped<IDiscountPolicy, StudentDiscountPolicy>();
builder.Services.AddScoped<IDiscountPolicy, StaffDiscountPolicy>();

// Kênh thông báo
builder.Services.AddScoped<INotificationService, ConsoleNotificationService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
