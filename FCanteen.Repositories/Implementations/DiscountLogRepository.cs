using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FCanteen.Data;
using FCanteen.Data.Entities;
using FCanteen.Repositories.Interfaces;

namespace FCanteen.Repositories.Implementations;

public class DiscountLogRepository : IDiscountLogRepository
{
    private readonly FCanteenContext _context;

    public DiscountLogRepository(FCanteenContext context)
    {
        _context = context;
    }

    public async Task AddRangeAsync(IEnumerable<DiscountPolicyLog> logs)
    {
        _context.DiscountPolicyLogs.AddRange(logs);
        await _context.SaveChangesAsync();
    }
}
