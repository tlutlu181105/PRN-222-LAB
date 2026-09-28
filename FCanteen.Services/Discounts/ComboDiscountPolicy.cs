using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace FCanteen.Services.Discounts;

public class ComboDiscountPolicy : IDiscountPolicy
{
    private const decimal AmountPerCombo = 5000m;
    private static readonly string[] MainUnits = { "phần", "tô" };   // món chính tính theo phần/tô
    private static readonly string[] DrinkUnits = { "ly", "chai" };    // nước tính theo ly/chai

    public int Priority => 10;

    public DiscountResult? Apply(DiscountContext context)
    {
        int mains = 0, drinks = 0;

        foreach (var line in context.Ticket.TicketLines)
        {
            if (!context.MenuItems.TryGetValue(line.MenuItemId, out var item)) continue;

            if (MainUnits.Contains(item.Unit)) mains += line.Quantity;
            else if (DrinkUnits.Contains(item.Unit)) drinks += line.Quantity;
        }

        int combos = Math.Min(mains, drinks); // mỗi cặp (1 món chính + 1 nước) là 1 combo
        if (combos == 0) return null;

        var amountOff = Math.Min(combos * AmountPerCombo, context.CurrentAmount);

        return new DiscountResult
        {
            PolicyName = nameof(ComboDiscountPolicy),
            AmountOff = amountOff,
            Description = $"Combo món chính + nước x{combos}, giảm {AmountPerCombo:N0}đ/combo"
        };
    }
}
