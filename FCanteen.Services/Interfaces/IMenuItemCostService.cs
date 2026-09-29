using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace FCanteen.Services.Interfaces;

public interface IMenuItemCostService
{
    Task<decimal> GetCostAsync(int menuItemId);
}
