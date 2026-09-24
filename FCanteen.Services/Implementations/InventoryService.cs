using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FCanteen.Data.Entities;
using FCanteen.Repositories.Interfaces;
using FCanteen.Services.Interfaces;

namespace FCanteen.Services.Implementations;

public class InventoryService : IInventoryService
{
    private readonly IIngredientRepository _ingredientRepository;

    public InventoryService(IIngredientRepository ingredientRepository)
    {
        _ingredientRepository = ingredientRepository;
    }

    public async Task<List<Ingredient>> GetLowStockIngredientsAsync()
    {
        var all = await _ingredientRepository.GetAllAsync();
        return all.Where(i => i.StockQuantity <= i.WarningThreshold).ToList();
    }
}