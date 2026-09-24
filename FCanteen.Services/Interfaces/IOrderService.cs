using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FCanteen.Data.Entities;

namespace FCanteen.Services.Interfaces;

public class OrderLineRequest
{
    public int MenuItemId { get; set; }
    public int Quantity { get; set; }
    public string? Note { get; set; }
}

public interface IOrderService
{
    Task<OrderTicket> CreateOrderAsync(string counterName, string branchCode, List<OrderLineRequest> lines);
}
