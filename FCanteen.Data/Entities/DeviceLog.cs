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
    public string SourceAddress { get; set; } = "";     // địa chỉ nguồn, ví dụ "192.168.1.5:54321"
    public string Content { get; set; } = "";             // nội dung log, ví dụ "Kết nối mới" hoặc "Status: 200, Time: 120ms"
    public DateTime Timestamp { get; set; }                // thời gian ghi log
}
