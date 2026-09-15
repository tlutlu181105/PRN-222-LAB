using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.KitchenServer;

// Dữ liệu client gửi lên: 1 dòng món trong phiếu order
public class OrderLineRequest
{
    public int MenuItemId { get; set; }
    public int Quantity { get; set; }
    public string? Note { get; set; }
}

// Dữ liệu client gửi lên: toàn bộ phiếu order
public class OrderRequest
{
    public string CounterName { get; set; } = "";
    public List<OrderLineRequest> Lines { get; set; } = new();
}

// Dữ liệu server trả về sau khi xử lý xong
public class OrderConfirmation
{
    public int TicketId { get; set; }
    public decimal TotalAmount { get; set; }
    public string Message { get; set; } = "";
}