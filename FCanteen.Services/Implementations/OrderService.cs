using FCanteen.Data.Entities;
using FCanteen.Repositories.Interfaces;
using FCanteen.Services.Discounts;
using FCanteen.Services.Interfaces;
using FCanteen.Services.Notifications;

namespace FCanteen.Services.Implementations;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly IDiscountLogRepository _discountLogRepository;
    private readonly IEnumerable<IDiscountPolicy> _discountPolicies;
    private readonly INotificationService _notificationService;

    public OrderService(
        IOrderRepository orderRepository,
        IMenuItemRepository menuItemRepository,
        IDiscountLogRepository discountLogRepository,
        IEnumerable<IDiscountPolicy> discountPolicies,   // DI tự gom TẤT CẢ chính sách đã đăng ký
        INotificationService notificationService)      

    {
        _orderRepository = orderRepository;
        _menuItemRepository = menuItemRepository;
        _discountLogRepository = discountLogRepository;
        _discountPolicies = discountPolicies;
        _notificationService = notificationService;
    }

    public async Task<OrderResult> CreateOrderAsync(
        string counterName, string branchCode, List<OrderLineRequest> lines,
        bool isStudent = false, Staff? staff = null)
    {
        var ticket = new OrderTicket
        {
            CounterName = counterName,
            BranchCode = branchCode,
            CreatedAt = DateTime.Now,
            Status = "Pending"
        };

        var menuItems = new Dictionary<int, MenuItem>();
        decimal subtotal = 0;

        foreach (var line in lines)
        {
            var menuItem = await _menuItemRepository.GetByIdAsync(line.MenuItemId)
                ?? throw new Exception($"Không tìm thấy món có Id = {line.MenuItemId}");

            menuItems[menuItem.Id] = menuItem;
            subtotal += menuItem.Price * line.Quantity;

            ticket.TicketLines.Add(new TicketLine
            {
                MenuItemId = line.MenuItemId,
                Quantity = line.Quantity,
                UnitPrice = menuItem.Price,
                Note = line.Note
            });
        }

        // Áp dụng lần lượt các chính sách theo Priority. OrderService KHÔNG biết tên chính sách nào cụ thể.
        var context = new DiscountContext
        {
            Ticket = ticket,
            MenuItems = menuItems,
            Staff = staff,
            IsStudent = isStudent,
            CurrentAmount = subtotal
        };

        var applied = new List<DiscountResult>();
        foreach (var policy in _discountPolicies.OrderBy(p => p.Priority))
        {
            var result = policy.Apply(context);
            if (result == null) continue;

            context.CurrentAmount -= result.AmountOff;
            applied.Add(result);
        }

        ticket.TotalAmount = context.CurrentAmount;   // tổng tiền SAU giảm giá
        await _orderRepository.AddAsync(ticket);

        if (applied.Count > 0)
        {
            var logs = applied.Select(r => new DiscountPolicyLog
            {
                OrderTicketId = ticket.Id,
                PolicyName = r.PolicyName,
                AmountOff = r.AmountOff,
                Description = r.Description,
                AppliedAt = DateTime.Now
            });
            await _discountLogRepository.AddRangeAsync(logs);
        }
        await _notificationService.SendAsync(
    $"Phiếu mới #{ticket.Id}",
    $"{counterName} ({branchCode}) - {ticket.TicketLines.Count} dòng món - thành tiền {ticket.TotalAmount:N0}đ");
        return new OrderResult { Ticket = ticket, Subtotal = subtotal, Discounts = applied };
    }
}