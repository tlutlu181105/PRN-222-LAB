using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FCanteen.Data;
using FCanteen.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FCanteen.Services.Implementations;

public class MenuItemCostService : IMenuItemCostService
{
    private readonly FCanteenContext _context;

    public MenuItemCostService(FCanteenContext context)
    {
        _context = context;
    }

    public async Task<decimal> GetCostAsync(int menuItemId)
    {
        if (menuItemId <= 0) return 0; 

        return await _context.MenuItemIngredients
            .Where(mi => mi.MenuItemId == menuItemId)
            .SumAsync(mi => mi.Quantity * mi.Ingredient.UnitCost);
    }
}
