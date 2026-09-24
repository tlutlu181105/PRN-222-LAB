using FCanteen.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace FCanteen.Data;

public class FCanteenContext : DbContext
{
    public FCanteenContext(DbContextOptions<FCanteenContext> options) : base(options)
    {
    }

    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<OrderTicket> OrderTickets => Set<OrderTicket>();
    public DbSet<TicketLine> TicketLines => Set<TicketLine>();
    public DbSet<DeviceLog> DeviceLogs => Set<DeviceLog>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();          
    public DbSet<DailySettlement> DailySettlements => Set<DailySettlement>();

    public DbSet<Staff> Staffs => Set<Staff>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<MenuItem>().HasData(
            new MenuItem { Id = 1, Code = "CM01", Name = "Cơm gà xối mỡ", Price = 35000, Unit = "phần", IsAvailable = true },
            new MenuItem { Id = 2, Code = "CM02", Name = "Cơm sườn bì chả", Price = 32000, Unit = "phần", IsAvailable = true },
            new MenuItem { Id = 3, Code = "CM03", Name = "Cơm tấm sườn nướng", Price = 30000, Unit = "phần", IsAvailable = true },
            new MenuItem { Id = 4, Code = "CM04", Name = "Cơm chiên dương châu", Price = 28000, Unit = "phần", IsAvailable = true },
            new MenuItem { Id = 5, Code = "CM05", Name = "Cơm gà xé", Price = 30000, Unit = "phần", IsAvailable = true },
            new MenuItem { Id = 6, Code = "MI01", Name = "Mì xào bò", Price = 27000, Unit = "phần", IsAvailable = true },
            new MenuItem { Id = 7, Code = "MI02", Name = "Mì Quảng gà", Price = 30000, Unit = "phần", IsAvailable = true },
            new MenuItem { Id = 8, Code = "PH01", Name = "Phở bò tái", Price = 32000, Unit = "tô", IsAvailable = true },
            new MenuItem { Id = 9, Code = "PH02", Name = "Phở gà", Price = 30000, Unit = "tô", IsAvailable = true },
            new MenuItem { Id = 10, Code = "BU01", Name = "Bún chả cá", Price = 28000, Unit = "tô", IsAvailable = true },
            new MenuItem { Id = 11, Code = "BU02", Name = "Bún bò Huế", Price = 30000, Unit = "tô", IsAvailable = true },
            new MenuItem { Id = 12, Code = "TR01", Name = "Trà đá", Price = 3000, Unit = "ly", IsAvailable = true },
            new MenuItem { Id = 13, Code = "TR02", Name = "Trà tắc", Price = 10000, Unit = "ly", IsAvailable = true },
            new MenuItem { Id = 14, Code = "NC01", Name = "Nước cam ép", Price = 15000, Unit = "ly", IsAvailable = true },
            new MenuItem { Id = 15, Code = "NS01", Name = "Nước suối", Price = 8000, Unit = "chai", IsAvailable = true }
        );

        modelBuilder.Entity<Ingredient>().HasData(
    new Ingredient { Id = 1, Name = "Gạo", Unit = "kg", StockQuantity = 200, WarningThreshold = 20 },
    new Ingredient { Id = 2, Name = "Thịt gà", Unit = "kg", StockQuantity = 100, WarningThreshold = 15 },
    new Ingredient { Id = 3, Name = "Thịt heo", Unit = "kg", StockQuantity = 100, WarningThreshold = 15 },
    new Ingredient { Id = 4, Name = "Thịt bò", Unit = "kg", StockQuantity = 80, WarningThreshold = 10 },
    new Ingredient { Id = 5, Name = "Rau các loại", Unit = "kg", StockQuantity = 150, WarningThreshold = 20 },
    new Ingredient { Id = 6, Name = "Trứng gà", Unit = "quả", StockQuantity = 500, WarningThreshold = 50 },
    new Ingredient { Id = 7, Name = "Dầu ăn", Unit = "lít", StockQuantity = 50, WarningThreshold = 5 },
    new Ingredient { Id = 8, Name = "Nước mắm", Unit = "lít", StockQuantity = 30, WarningThreshold = 5 },
    new Ingredient { Id = 9, Name = "Đường", Unit = "kg", StockQuantity = 40, WarningThreshold = 5 },
    new Ingredient { Id = 10, Name = "Bún/Mì/Phở khô", Unit = "kg", StockQuantity = 100, WarningThreshold = 15 }
);

        modelBuilder.Entity<Staff>().HasData(
    new Staff { Id = 1, StaffCode = "GV001", FullName = "Nguyễn Văn A", Role = "Teacher", BranchCode = "CS01" },
    new Staff { Id = 2, StaffCode = "GV002", FullName = "Trần Thị B", Role = "Teacher", BranchCode = "CS02" },
    new Staff { Id = 3, StaffCode = "NV001", FullName = "Lê Văn C", Role = "Staff", BranchCode = "CS01" },
    new Staff { Id = 4, StaffCode = "NV002", FullName = "Phạm Thị D", Role = "Staff", BranchCode = "CS03" }
);
    }

}