using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.Analytics;

public class TicketLineFlat
{
    public int MenuItemId { get; set; }
    public decimal Revenue { get; set; }   
    public int Hour { get; set; }            
}

public class MenuEfficiencyResult
{
    public int MenuItemId { get; set; }
    public string MenuItemName { get; set; } = "";
    public decimal TotalRevenue { get; set; }
    public double EfficiencyIndex { get; set; }  
}
