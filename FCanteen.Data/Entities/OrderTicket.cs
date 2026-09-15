using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.Data.Entities;

public class OrderTicket
{
    public int Id { get; set; }
    public string CounterName { get; set; } = "";     // tên quầy gửi, ví dụ "QUAY01"
    public decimal TotalAmount { get; set; }            // tổng tiền — do SERVER tính, không nhận từ client
    public DateTime CreatedAt { get; set; }              // thời điểm tạo phiếu
    public string Status { get; set; } = "Pending";       // "Pending", "Cooking", "Done"...

    // 1 phiếu có nhiều dòng món
    public ICollection<TicketLine> TicketLines { get; set; } = new List<TicketLine>();
}
