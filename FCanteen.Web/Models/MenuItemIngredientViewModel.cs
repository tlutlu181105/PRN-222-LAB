namespace FCanteen.Web.Models;


public class MenuItemIngredientRow
{
    public int Id { get; set; }               
    public int IngredientId { get; set; }
    public string IngredientName { get; set; } = "";
    public string Unit { get; set; } = "";
    public decimal Quantity { get; set; }    
    public decimal UnitCost { get; set; }     
    public decimal LineCost => Quantity * UnitCost;   
}


public class ManageIngredientsViewModel
{
    public int MenuItemId { get; set; }
    public string MenuItemName { get; set; } = "";
    public decimal MenuItemPrice { get; set; }

    public List<MenuItemIngredientRow> AssignedIngredients { get; set; } = new();

    //  dropdown chọn nguyên liệu
    public List<CategoryOption> AllIngredients { get; set; } = new();

    public decimal TotalCost => AssignedIngredients.Sum(r => r.LineCost);   // giá vốn của món
    public decimal MinimumPrice => TotalCost * 1.2m;                        // giá bán tối thiểu (vốn + 20%)
}