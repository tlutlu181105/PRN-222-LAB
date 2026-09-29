using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FCanteen.Data;
using FCanteen.Data.Entities;
using FCanteen.Services.Interfaces;
using FCanteen.Web.Models;

namespace FCanteen.Web.Controllers;

public class MenuItemsController : Controller
{
    private readonly FCanteenContext _context;
    private readonly IMenuItemCostService _costService;

    public MenuItemsController(FCanteenContext context, IMenuItemCostService costService)
    {
        _context = context;
        _costService = costService;
    }

    // GET: MenuItems
    public async Task<IActionResult> Index()
    {
        var items = await _context.MenuItems
            .Include(m => m.Category)
            .OrderBy(m => m.Id)
            .ToListAsync();
        return View(items);
    }

    // GET: MenuItems/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var item = await _context.MenuItems
            .Include(m => m.Category)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (item == null) return NotFound();
        return View(item);
    }

    // GET: MenuItems/Create
    public async Task<IActionResult> Create()
    {
        var vm = new MenuItemViewModel
        {
            CategoryOptions = await GetCategoryOptionsAsync()
        };
        return View(vm);
    }

    // POST: MenuItems/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MenuItemViewModel vm)
    {
        await ValidateDuplicateCodeAsync(vm.Code, currentId: null);

        if (!ModelState.IsValid)
        {
            vm.CategoryOptions = await GetCategoryOptionsAsync();
            return View(vm);
        }

        var entity = new MenuItem
        {
            Code = vm.Code,
            Name = vm.Name,
            Price = vm.Price,
            Unit = vm.Unit,
            IsAvailable = vm.IsAvailable,
            CategoryId = vm.CategoryId
        };

        _context.MenuItems.Add(entity);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Đã thêm món '{entity.Name}' thành công.";
        return RedirectToAction(nameof(Index));
    }

    // GET: MenuItems/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var entity = await _context.MenuItems.FindAsync(id);
        if (entity == null) return NotFound();

        var vm = new MenuItemViewModel
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            Price = entity.Price,
            Unit = entity.Unit,
            IsAvailable = entity.IsAvailable,
            CategoryId = entity.CategoryId,
            CategoryOptions = await GetCategoryOptionsAsync()
        };
        return View(vm);
    }

    // POST: MenuItems/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MenuItemViewModel vm)
    {
        if (id != vm.Id) return NotFound();

        await ValidateDuplicateCodeAsync(vm.Code, currentId: id);

        if (!ModelState.IsValid)
        {
            vm.CategoryOptions = await GetCategoryOptionsAsync();
            return View(vm);
        }

        var entity = await _context.MenuItems.FindAsync(id);
        if (entity == null) return NotFound();

        entity.Code = vm.Code;
        entity.Name = vm.Name;
        entity.Price = vm.Price;
        entity.Unit = vm.Unit;
        entity.IsAvailable = vm.IsAvailable;
        entity.CategoryId = vm.CategoryId;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Đã cập nhật món '{entity.Name}' thành công.";
        return RedirectToAction(nameof(Index));
    }

    // GET: MenuItems/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.MenuItems
            .Include(m => m.Category)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (entity == null) return NotFound();
        return View(entity);
    }

    // POST: MenuItems/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var entity = await _context.MenuItems.FindAsync(id);
        if (entity != null)
        {
            _context.MenuItems.Remove(entity);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã xoá món '{entity.Name}'.";
        }
        return RedirectToAction(nameof(Index));
    }

    // ----- Helper -----

    private async Task<List<CategoryOption>> GetCategoryOptionsAsync()
    {
        return await _context.Categories
            .Select(c => new CategoryOption { Id = c.Id, Name = c.Name })
            .ToListAsync();
    }

    // Đúng yêu cầu đề bài: chống trùng mã món bằng ModelState.AddModelError ở tầng server
    private async Task ValidateDuplicateCodeAsync(string code, int? currentId)
    {
        var exists = await _context.MenuItems
            .AnyAsync(m => m.Code == code && m.Id != (currentId ?? 0));

        if (exists)
        {
            ModelState.AddModelError(nameof(MenuItemViewModel.Code), "Mã món này đã tồn tại, vui lòng chọn mã khác.");
        }
    }
}