using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace FCanteen.Services.Discounts;

public class StaffDiscountPolicy : IDiscountPolicy
{
    public int Priority => 30;

    public DiscountResult? Apply(DiscountContext context)
    {
        if (context.Staff == null) return null;
        if (context.Staff.Role != "Teacher" && context.Staff.Role != "Staff") return null;

        var amountOff = Math.Round(context.CurrentAmount * 0.15m, 0);
        if (amountOff <= 0) return null;

        return new DiscountResult
        {
            PolicyName = nameof(StaffDiscountPolicy),
            AmountOff = amountOff,
            Description = $"Giảm 15% cho {context.Staff.Role} ({context.Staff.StaffCode})"
        };
    }
}
