using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace FCanteen.Data.Entities;

public class DiscountPolicyLog
{
    public int Id { get; set; }
    public int OrderTicketId { get; set; }
    public OrderTicket OrderTicket { get; set; } = null!;
    public string PolicyName { get; set; } = "";
    public decimal AmountOff { get; set; }
    public string Description { get; set; } = "";
    public DateTime AppliedAt { get; set; }
}
