using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.Data.Entities;

public class OrderTicket
{
    public int Id { get; set; }
    public string CounterName { get; set; } = "";     
    public decimal TotalAmount { get; set; }            
    public DateTime CreatedAt { get; set; }              
    public string Status { get; set; } = "Pending";

    public string BranchCode { get; set; } = "";
    public ICollection<TicketLine> TicketLines { get; set; } = new List<TicketLine>();
}
