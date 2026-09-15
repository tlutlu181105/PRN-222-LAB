using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.PosClient;

public class OrderLineRequest
{
    public int MenuItemId { get; set; }
    public int Quantity { get; set; }
    public string? Note { get; set; }
}

public class OrderRequest
{
    public string CounterName { get; set; } = "";
    public List<OrderLineRequest> Lines { get; set; } = new();
}

public class OrderConfirmation
{
    public int TicketId { get; set; }
    public decimal TotalAmount { get; set; }
    public string Message { get; set; } = "";
}
