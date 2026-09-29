using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FCanteen.Data;
using FCanteen.Data.Entities;
using FCanteen.Web.Models;

namespace FCanteen.Web.Controllers;

public class MenuItemIngredientsController : Controller
{
    private readonly FCanteenContext _context;

    public MenuItemIngredientsController(FCanteenContext context)
    {
        _context = context;
    }

    // GET: /MenuItemIngredients/Manage?menuItemId=1
    public async Task<IActionResult> Manage(int menuItemId)
    {
        var menuItem = await _context.MenuItems.FindAsync(menuItemId);
        if (menuItem == null) return NotFound();

        var vm = await BuildViewModelAsync(menuItem);
        return View(vm);
    }

    // POST: thêm nguyên liệu vào món (hoặc cập nhật định lượng nếu đã có)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int menuItemId, int ingredientId, decimal quantity)
    {
        if (quantity <= 0)
        {
            TempData["IngredientMessage"] = "Định lượng phải lớn hơn 0.";
            return RedirectToAction(nameof(Manage), new { menuItemId });
        }

        var menuItemExists = await _context.MenuItems.AnyAsync(m => m.Id == menuItemId);
        var ingredientExists = await _context.Ingredients.AnyAsync(i => i.Id == ingredientId);
        if (!menuItemExists || !ingredientExists) return NotFound();

        // Đã gán nguyên liệu này rồi thì cập nhật định lượng, không tạo dòng trùng
        var existing = await _context.MenuItemIngredients
            .FirstOrDefaultAsync(m => m.MenuItemId == menuItemId && m.IngredientId == ingredientId);

        if (existing != null)
        {
            existing.Quantity = quantity;
        }
        else
        {
            _context.MenuItemIngredients.Add(new MenuItemIngredient
            {
                MenuItemId = menuItemId,
                IngredientId = ingredientId,
                Quantity = quantity
            });
        }

        await _context.SaveChangesAsync();

        TempData["IngredientMessage"] = existing != null
            ? "Đã cập nhật định lượng."
            : "Đã gán nguyên liệu cho món.";
        return RedirectToAction(nameof(Manage), new { menuItemId });
    }

    // POST: gỡ nguyên liệu khỏi món
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int id, int menuItemId)
    {
        var row = await _context.MenuItemIngredients.FindAsync(id);
        if (row != null)
        {
            _context.MenuItemIngredients.Remove(row);
            await _context.SaveChangesAsync();
            TempData["IngredientMessage"] = "Đã gỡ nguyên liệu khỏi món.";
        }
        return RedirectToAction(nameof(Manage), new { menuItemId });
    }

    // ----- Helper -----
    private async Task<ManageIngredientsViewModel> BuildViewModelAsync(MenuItem menuItem)
    {
        var assigned = await _context.MenuItemIngredients
            .Where(m => m.MenuItemId == menuItem.Id)
            .Select(m => new MenuItemIngredientRow
            {
                Id = m.Id,
                IngredientId = m.IngredientId,
                IngredientName = m.Ingredient.Name,
                Unit = m.Ingredient.Unit,
                Quantity = m.Quantity,
                UnitCost = m.Ingredient.UnitCost
            })
            .ToListAsync();

        var allIngredients = await _context.Ingredients
            .OrderBy(i => i.Name)
            .Select(i => new CategoryOption { Id = i.Id, Name = i.Name })
            .ToListAsync();

        return new ManageIngredientsViewModel
        {
            MenuItemId = menuItem.Id,
            MenuItemName = menuItem.Name,
            MenuItemPrice = menuItem.Price,
            AssignedIngredients = assigned,
            AllIngredients = allIngredients
        };
    }
}