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
    public async Task<IActionResult> Index(string? searchTerm, int? categoryId, bool? isAvailable, string sortBy = "name", string sortDir = "asc")
    {
        var query = _context.MenuItems.Include(m => m.Category).AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(m => m.Name.Contains(searchTerm) || m.Code.Contains(searchTerm));
        }

        // loc theo nhom mon 
        if (categoryId.HasValue)
        {
            query = query.Where(m => m.CategoryId == categoryId.Value);
        }

     
        if (isAvailable.HasValue)
        {
            query = query.Where(m => m.IsAvailable == isAvailable.Value);
        }

     
        query = (sortBy, sortDir) switch
        {
            ("price", "desc") => query.OrderByDescending(m => m.Price),
            ("price", "asc") => query.OrderBy(m => m.Price),
            ("name", "desc") => query.OrderByDescending(m => m.Name),
            _ => query.OrderBy(m => m.Name)
        };

        var items = await query.ToListAsync();
        // Lưu lại các giá trị đang lọc/sắp xếp để View tự vẽ lại trạng thái UI (giữ nguyên khi chuyển trang)
        ViewData["SearchTerm"] = searchTerm;
        ViewData["CategoryId"] = categoryId;
        ViewData["IsAvailable"] = isAvailable;
        ViewData["SortBy"] = sortBy;
        ViewData["SortDir"] = sortDir;

        
        // Danh sách Category (ViewBag là lớp bọc động (dynamic) quanh cùng 1 ViewData)
        ViewBag.Categories = await _context.Categories.ToListAsync();

        // dùng ở Create/Edit/Delete (TempData["SuccessMessage"]) hiển thị thông báo

        return View(items);
    }

    // GET: MenuItems/Details/
    public async Task<IActionResult> Details(int id)
    {
        var item = await _context.MenuItems
            .Include(m => m.Category)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (item == null) return NotFound();

        ViewData["Cost"] = await _costService.GetCostAsync(id); //xem giá vốn 
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