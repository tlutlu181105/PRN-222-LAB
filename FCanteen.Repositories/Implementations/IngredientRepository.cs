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

public class IngredientRepository : IIngredientRepository
{
    private readonly FCanteenContext _context;

    public IngredientRepository(FCanteenContext context)
    {
        _context = context;
    }

    public async Task<List<Ingredient>> GetAllAsync()
        => await _context.Ingredients.OrderBy(i => i.Id).ToListAsync();

    public async Task<Ingredient?> GetByIdAsync(int id)
        => await _context.Ingredients.FindAsync(id);

    public async Task<Ingredient?> GetByNameAsync(string name)
        => await _context.Ingredients.FirstOrDefaultAsync(i => i.Name == name);

    public async Task UpdateAsync(Ingredient ingredient)
    {
        _context.Ingredients.Update(ingredient);
        await _context.SaveChangesAsync();
    }
}
