using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace FCanteen.Web.Models.Cart;

public class CartService
{
    private const string SessionKey = "IngredientCart";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CartService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ISession Session => _httpContextAccessor.HttpContext!.Session;

    public List<CartLine> GetCart()
    {
        var json = Session.GetString(SessionKey);
        if (string.IsNullOrEmpty(json)) return new List<CartLine>();
        return JsonSerializer.Deserialize<List<CartLine>>(json) ?? new List<CartLine>();
    }

    private void SaveCart(List<CartLine> cart)
    {
        Session.SetString(SessionKey, JsonSerializer.Serialize(cart));
    }

    public void AddOrUpdate(int ingredientId, string ingredientName, string unit, decimal quantity)
    {
        var cart = GetCart();
        var existing = cart.FirstOrDefault(c => c.IngredientId == ingredientId);

        if (existing != null)
        {
            existing.Quantity = quantity; // cập nhật số lượng nếu đã có trong giỏ
        }
        else
        {
            cart.Add(new CartLine { IngredientId = ingredientId, IngredientName = ingredientName, Unit = unit, Quantity = quantity });
        }

        SaveCart(cart);
    }

    public void RemoveLine(int ingredientId)
    {
        var cart = GetCart();
        cart.RemoveAll(c => c.IngredientId == ingredientId);
        SaveCart(cart);
    }

    public void Clear()
    {
        Session.Remove(SessionKey);
    }

    public int CountLines() => GetCart().Count;
}