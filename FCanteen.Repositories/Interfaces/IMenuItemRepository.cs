using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FCanteen.Data.Entities;

namespace FCanteen.Repositories.Interfaces;

public interface IMenuItemRepository
{
    Task<List<MenuItem>> GetAllAsync();
    Task<List<MenuItem>> GetAvailableAsync();
    Task<MenuItem?> GetByIdAsync(int id);
    Task<MenuItem?> GetByCodeAsync(string code);
    Task UpdateAsync(MenuItem item);
}