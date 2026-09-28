using FCanteen.Data.Entities;

namespace FCanteen.Services.Discounts;

public class DiscountContext
{
    public OrderTicket Ticket { get; set; } = null!;
    public IReadOnlyDictionary<int, MenuItem> MenuItems { get; set; } = new Dictionary<int, MenuItem>(); // để chính sách tra loại món (món chính/nước)
    public Staff? Staff { get; set; }          // null nếu khách không phải nhân viên/giảng viên
    public bool IsStudent { get; set; }          // true nếu khách là sinh viên
    public decimal CurrentAmount { get; set; }     // số tiền HIỆN TẠI, sau khi các chính sách ưu tiên cao hơn đã giảm
}

public class DiscountResult
{
    public string PolicyName { get; set; } = "";
    public decimal AmountOff { get; set; }   // số tiền được giảm (dương)
    public string Description { get; set; } = "";
}

public interface IDiscountPolicy
{
    // Số nhỏ hơn = ưu tiên áp dụng trước. Dùng để sắp xếp thứ tự khi áp dụng nhiều chính sách cùng lúc.
    int Priority { get; }

    // Trả về null nếu chính sách này không áp dụng được cho đơn hàng/khách hàng này
    DiscountResult? Apply(DiscountContext context);
}