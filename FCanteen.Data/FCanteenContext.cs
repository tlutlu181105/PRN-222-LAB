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
    }
}