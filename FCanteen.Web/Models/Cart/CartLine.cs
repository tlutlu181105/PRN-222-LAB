namespace FCanteen.Web.Models.Cart;

public class CartLine
{
    public int IngredientId { get; set; }
    public string IngredientName { get; set; } = "";
    public string Unit { get; set; } = "";
    public decimal Quantity { get; set; }
}