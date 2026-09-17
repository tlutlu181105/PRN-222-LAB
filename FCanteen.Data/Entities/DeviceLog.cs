using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.Data.Entities;

public class DeviceLog
{
    public int Id { get; set; }
    public string Protocol { get; set; } = "";        // "TCP", "UDP", "HTTP", "DNS"
    public string SourceAddress { get; set; } = "";     
    public string Content { get; set; } = "";             
    public DateTime Timestamp { get; set; }                
}
