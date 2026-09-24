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

public class MenuItemRepository : IMenuItemRepository
{
    private readonly FCanteenContext _context;

    public MenuItemRepository(FCanteenContext context)
    {
        _context = context;
    }

    public async Task<List<MenuItem>> GetAllAsync()
        => await _context.MenuItems.OrderBy(m => m.Id).ToListAsync();

    public async Task<List<MenuItem>> GetAvailableAsync()
        => await _context.MenuItems.Where(m => m.IsAvailable).OrderBy(m => m.Id).ToListAsync();

    public async Task<MenuItem?> GetByIdAsync(int id)
        => await _context.MenuItems.FindAsync(id);

    public async Task<MenuItem?> GetByCodeAsync(string code)
        => await _context.MenuItems.FirstOrDefaultAsync(m => m.Code == code);

    public async Task UpdateAsync(MenuItem item)
    {
        _context.MenuItems.Update(item);
        await _context.SaveChangesAsync();
    }
}
