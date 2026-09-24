using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FCanteen.Data;
using FCanteen.Data.Entities;
using FCanteen.Repositories.Interfaces;

namespace FCanteen.Repositories.Implementations;

public class OrderRepository : IOrderRepository
{
    private readonly FCanteenContext _context;

    public OrderRepository(FCanteenContext context)
    {
        _context = context;
    }

    public async Task<OrderTicket> AddAsync(OrderTicket ticket)
    {
        _context.OrderTickets.Add(ticket);
        await _context.SaveChangesAsync();
        return ticket;
    }

    public async Task<OrderTicket?> GetByIdAsync(int id)
        => await _context.OrderTickets
            .Include(t => t.TicketLines)
            .FirstOrDefaultAsync(t => t.Id == id);

    public async Task<List<OrderTicket>> GetPendingAsync()
        => await _context.OrderTickets
            .Where(t => t.Status == "Pending")
            .OrderBy(t => t.CreatedAt)
            .ToListAsync();
}