using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FCanteen.Data;
using FCanteen.Web.Models.Cart;

namespace FCanteen.Web.Controllers;

public class IngredientsController : Controller
{
    private readonly FCanteenContext _context;
    private readonly CartService _cartService;

    public IngredientsController(FCanteenContext context, CartService cartService)
    {
        _context = context;
        _cartService = cartService;
    }

    public async Task<IActionResult> Index()
    {
        var ingredients = await _context.Ingredients.OrderBy(i => i.Name).ToListAsync();
        return View(ingredients);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToCart(int ingredientId, decimal quantity)
    {
        if (quantity <= 0)
        {
            TempData["CartMessage"] = "Số lượng phải lớn hơn 0.";
            return RedirectToAction(nameof(Index));
        }

        var ingredient = await _context.Ingredients.FindAsync(ingredientId);
        if (ingredient == null) return NotFound();

        _cartService.AddOrUpdate(ingredient.Id, ingredient.Name, ingredient.Unit, quantity);

        TempData["CartMessage"] = $"Đã thêm '{ingredient.Name}' vào giỏ đặt hàng.";
        return RedirectToAction(nameof(Index));
    }
}
