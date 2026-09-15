using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.Data.Entities;

public class MenuItem
{
    public int Id { get; set; }
    public string Code { get; set; } = "";         // mã món, ví dụ "CM01"
    public string Name { get; set; } = "";          // tên món, ví dụ "Cơm gà xối mỡ"
    public decimal Price { get; set; }               // giá bán hiện tại
    public string Unit { get; set; } = "";            // đơn vị tính: "phần", "ly", "chai"...
    public bool IsAvailable { get; set; } = true;      // còn bán hay đã hết hàng

    // Navigation property: 1 món có thể xuất hiện trong nhiều TicketLine
    public ICollection<TicketLine> TicketLines { get; set; } = new List<TicketLine>();
}