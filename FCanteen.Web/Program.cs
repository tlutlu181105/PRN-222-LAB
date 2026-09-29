using FCanteen.Data;
using FCanteen.Repositories.Implementations;
using FCanteen.Repositories.Interfaces;
using FCanteen.Services.Discounts;
using FCanteen.Services.Implementations;
using FCanteen.Services.Interfaces;
using FCanteen.Services.Notifications;
using FCanteen.Web.Models.Cart;
using Microsoft.EntityFrameworkCore;

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
builder.Services.AddScoped<IMenuItemCostService, MenuItemCostService>();

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

//session
builder.Services.AddDistributedMemoryCache(); // nơi Session thực sự lưu dữ liệu (bộ nhớ RAM của server)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session tự hết hạn nếu không hoạt động 30 phút
    options.Cookie.HttpOnly = true;                  // cookie session không truy cập được từ JavaScript (bảo mật)
});

//dăng ký IHttpContextAccessor và CartService vào DI
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CartService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();          // MỚI — phải đặt ở đây
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
