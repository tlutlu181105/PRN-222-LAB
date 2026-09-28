using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FCanteen.Data;
using FCanteen.Data.Entities;
using FCanteen.Repositories.Interfaces;

namespace FCanteen.Repositories.Implementations;

public class StaffRepository : IStaffRepository
{
    private readonly FCanteenContext _context;

    public StaffRepository(FCanteenContext context)
    {
        _context = context;
    }

    public async Task<Staff?> GetByCodeAsync(string staffCode)
        => await _context.Staffs.FirstOrDefaultAsync(s => s.StaffCode == staffCode);
}