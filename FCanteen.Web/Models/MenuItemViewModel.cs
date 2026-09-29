using FCanteen.Web.Validation;
using System.ComponentModel.DataAnnotations;


namespace FCanteen.Web.Models;

public class MenuItemViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Mã món là bắt buộc")]
    [RegularExpression(@"^MON-\d{4}$", ErrorMessage = "Mã món phải theo định dạng MON-0001")]
    [Display(Name = "Mã món")]
    public string Code { get; set; } = "";

    [Required(ErrorMessage = "Tên món là bắt buộc")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Tên món phải từ 3 đến 100 ký tự")]
    [Display(Name = "Tên món")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Giá bán là bắt buộc")]
    [Range(1000, 1000000, ErrorMessage = "Giá bán phải trong khoảng 1,000đ - 1,000,000đ")]
    [MinimumMarkup(20)]   //gia ban phai lon hon gia von it nhat 20%
    [Display(Name = "Giá bán")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Đơn vị tính là bắt buộc")]
    [Display(Name = "Đơn vị tính")]
    public string Unit { get; set; } = "";

    [Display(Name = "Còn bán")]
    public bool IsAvailable { get; set; } = true;

    [Display(Name = "Nhóm món")]
    public int? CategoryId { get; set; }

    // Dùng để đổ danh sách category vào dropdown, không phải field nhập liệu
    public List<CategoryOption> CategoryOptions { get; set; } = new();
}

public class CategoryOption
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}
