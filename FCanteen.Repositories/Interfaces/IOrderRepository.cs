using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FCanteen.Data.Entities;

namespace FCanteen.Repositories.Interfaces;

public interface IOrderRepository
{
    Task<OrderTicket> AddAsync(OrderTicket ticket);
    Task<OrderTicket?> GetByIdAsync(int id);
    Task<List<OrderTicket>> GetPendingAsync();
}
