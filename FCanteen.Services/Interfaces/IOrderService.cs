using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FCanteen.Data.Entities;
using FCanteen.Data.Entities;
using FCanteen.Services.Discounts;

namespace FCanteen.Services.Interfaces;

public class OrderLineRequest
{
    public int MenuItemId { get; set; }
    public int Quantity { get; set; }
    public string? Note { get; set; }
}

public class OrderResult
{
    public OrderTicket Ticket { get; set; } = null!;
    public decimal Subtotal { get; set; }                       // tổng tiền trước giảm giá
    public List<DiscountResult> Discounts { get; set; } = new();  // các chính sách đã áp dụng
}

public interface IOrderService
{
    Task<OrderResult> CreateOrderAsync(
        string counterName, string branchCode, List<OrderLineRequest> lines,
        bool isStudent = false, Staff? staff = null);
}
