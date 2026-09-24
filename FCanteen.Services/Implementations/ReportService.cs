using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FCanteen.Data;
using FCanteen.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FCanteen.Services.Implementations;

public class ReportService : IReportService
{
    private readonly FCanteenContext _context;

    public ReportService(FCanteenContext context)
    {
        _context = context;
    }

    public async Task<List<(string MenuItemName, decimal Revenue)>> GetTopSellingItemsAsync(int top)
    {
        var result = await _context.TicketLines
            .GroupBy(tl => tl.MenuItem.Name)
            .Select(g => new { Name = g.Key, Revenue = g.Sum(x => x.Quantity * x.UnitPrice) })
            .OrderByDescending(x => x.Revenue)
            .Take(top)
            .ToListAsync();

        return result.Select(x => (x.Name, x.Revenue)).ToList();
    }
}