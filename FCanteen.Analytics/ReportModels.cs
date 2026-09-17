using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.Analytics;

public class TicketLineForReport
{
    public int MenuItemId { get; set; }
    public string MenuItemName { get; set; } = "";
    public decimal Revenue { get; set; }
    public DateTime CreatedAt { get; set; }
    public string BranchCode { get; set; } = "";
}
