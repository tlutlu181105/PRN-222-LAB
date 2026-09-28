using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace FCanteen.Services.Discounts;

public class StudentDiscountPolicy : IDiscountPolicy
{
    public int Priority => 20;

    public DiscountResult? Apply(DiscountContext context)
    {
        if (!context.IsStudent) return null;

        var amountOff = Math.Round(context.CurrentAmount * 0.10m, 0);
        if (amountOff <= 0) return null;

        return new DiscountResult
        {
            PolicyName = nameof(StudentDiscountPolicy),
            AmountOff = amountOff,
            Description = "Giảm 10% cho sinh viên"
        };
    }
}