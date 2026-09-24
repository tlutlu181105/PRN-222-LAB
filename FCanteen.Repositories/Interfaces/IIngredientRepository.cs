using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FCanteen.Data.Entities;

namespace FCanteen.Repositories.Interfaces;

public interface IIngredientRepository
{
    Task<List<Ingredient>> GetAllAsync();
    Task<Ingredient?> GetByIdAsync(int id);
    Task<Ingredient?> GetByNameAsync(string name);
    Task UpdateAsync(Ingredient ingredient);
}
