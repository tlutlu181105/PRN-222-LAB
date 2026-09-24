using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FCanteen.Data.Entities;
using FCanteen.Repositories.Interfaces;
using FCanteen.Services.Interfaces;

namespace FCanteen.Services.Implementations;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMenuItemRepository _menuItemRepository;

    // Constructor Injection: nhận Repository qua interface, không tự "new"
    public OrderService(IOrderRepository orderRepository, IMenuItemRepository menuItemRepository)
    {
        _orderRepository = orderRepository;
        _menuItemRepository = menuItemRepository;
    }

    public async Task<OrderTicket> CreateOrderAsync(string counterName, string branchCode, List<OrderLineRequest> lines)
    {
        var ticket = new OrderTicket
        {
            CounterName = counterName,
            BranchCode = branchCode,
            CreatedAt = DateTime.Now,
            Status = "Pending"
        };

        decimal total = 0;

        foreach (var line in lines)
        {
            var menuItem = await _menuItemRepository.GetByIdAsync(line.MenuItemId);
            if (menuItem == null)
                throw new Exception($"Không tìm thấy món có Id = {line.MenuItemId}");

            var unitPrice = menuItem.Price;
            total += unitPrice * line.Quantity;

            ticket.TicketLines.Add(new TicketLine
            {
                MenuItemId = line.MenuItemId,
                Quantity = line.Quantity,
                UnitPrice = unitPrice,
                Note = line.Note
            });
        }

        ticket.TotalAmount = total;

        return await _orderRepository.AddAsync(ticket);
    }
}