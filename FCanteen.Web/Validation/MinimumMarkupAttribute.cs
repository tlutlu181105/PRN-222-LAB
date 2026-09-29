using System.ComponentModel.DataAnnotations;
using FCanteen.Services.Interfaces;
using FCanteen.Web.Models;

namespace FCanteen.Web.Validation;

public class MinimumMarkupAttribute : ValidationAttribute
{
    private readonly double _minMarkupPercent;

    public MinimumMarkupAttribute(double minMarkupPercent)
    {
        _minMarkupPercent = minMarkupPercent;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var price = (decimal)value!;
        var model = (MenuItemViewModel)validationContext.ObjectInstance;

        // Lấy DI container thật của request hiện tại thông qua ValidationContext
        var costService = (IMenuItemCostService?)validationContext.GetService(typeof(IMenuItemCostService));
        if (costService == null)
            return ValidationResult.Success; // an toàn: nếu không resolve được service thì bỏ qua, tránh crash

        // ValidationAttribute không hỗ trợ async, nên gọi đồng bộ bằng GetAwaiter().GetResult()
        var cost = costService.GetCostAsync(model.Id).GetAwaiter().GetResult();

        if (cost <= 0)
            return ValidationResult.Success; // món chưa gán nguyên liệu -> chưa đủ dữ liệu để kiểm tra, tạm cho qua

        var minPrice = cost * (1 + (decimal)(_minMarkupPercent / 100.0));
        if (price < minPrice)
        {
            return new ValidationResult(
                $"Giá bán phải lớn hơn giá vốn ({cost:N0}đ) ít nhất {_minMarkupPercent}% (tối thiểu {minPrice:N0}đ)");
        }

        return ValidationResult.Success;
    }
}
