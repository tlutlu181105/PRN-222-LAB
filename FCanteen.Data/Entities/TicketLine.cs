using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.Data.Entities;

public class TicketLine
{
    public int Id { get; set; }

    // Khóa ngoại tới OrderTicket
    public int OrderTicketId { get; set; }
    public OrderTicket OrderTicket { get; set; } = null!;

    // Khóa ngoại tới MenuItem
    public int MenuItemId { get; set; }
    public MenuItem MenuItem { get; set; } = null!;

    public int Quantity { get; set; }                  
    public decimal UnitPrice { get; set; }               
    public string? Note { get; set; }                     
}