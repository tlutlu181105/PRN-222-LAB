using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace FCanteen.Data.Entities;

public class Staff
{
    public int Id { get; set; }
    public string StaffCode { get; set; } = "";   // mã nhân viên
    public string FullName { get; set; } = "";      // họ tên
    public string Role { get; set; } = "";            // vai trò: "Teacher", "Staff", "Admin"...
    public string BranchCode { get; set; } = "";        // cơ sở làm việc
}
