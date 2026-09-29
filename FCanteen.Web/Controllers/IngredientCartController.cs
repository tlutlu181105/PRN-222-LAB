using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FCanteen.Data;
using FCanteen.Data.Entities;
using FCanteen.Web.Models.Cart;

namespace FCanteen.Web.Controllers;

public class IngredientCartController : Controller
{
    private readonly FCanteenContext _context;
    private readonly CartService _cartService;

    public IngredientCartController(FCanteenContext context, CartService cartService)
    {
        _context = context;
        _cartService = cartService;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.Suppliers = await _context.Suppliers.ToListAsync();
        return View(_cartService.GetCart());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateQuantity(int ingredientId, decimal quantity)
    {
        var cart = _cartService.GetCart();
        var line = cart.FirstOrDefault(c => c.IngredientId == ingredientId);
        if (line != null && quantity > 0)
        {
            _cartService.AddOrUpdate(line.IngredientId, line.IngredientName, line.Unit, quantity);
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RemoveLine(int ingredientId)
    {
        _cartService.RemoveLine(ingredientId);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(int supplierId)
    {
        var cart = _cartService.GetCart();
        if (cart.Count == 0)
        {
            TempData["CartMessage"] = "Giỏ đặt hàng đang trống.";
            return RedirectToAction(nameof(Index));
        }

        var order = new PurchaseOrder
        {
            SupplierId = supplierId,
            CreatedAt = DateTime.Now,
            Status = "Pending"
        };

        foreach (var line in cart)
        {
            order.Lines.Add(new PurchaseOrderLine
            {
                IngredientId = line.IngredientId,
                Quantity = line.Quantity
            });
        }

        _context.PurchaseOrders.Add(order);
        await _context.SaveChangesAsync();

        _cartService.Clear(); // xoá Session sau khi đã ghi xuống DB — đúng yêu cầu đề bài

        TempData["CartMessage"] = $"Đã tạo phiếu đặt hàng #{order.Id} gửi nhà cung cấp thành công.";
        return RedirectToAction(nameof(Index));
    }
}