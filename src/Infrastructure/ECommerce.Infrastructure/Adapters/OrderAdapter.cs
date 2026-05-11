using ECommerce.Domain.Entities;
using ECommerce.Domain.Ports;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Adapters;

public class OrderAdapter : IOrderRepository
{
    private readonly AppDbContext _context;

    public OrderAdapter(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<Order> GetAllAsync() => _context.Orders
                                                  .Include(o => o.OrderItems)
                                                  .ThenInclude(oi => oi.Product)
                                                  .AsQueryable();

    public async Task<Order?> GetByIdAsync(Guid id) =>
        await _context.Orders
                      .Include(o => o.OrderItems)
                      .ThenInclude(oi => oi.Product)
                      .FirstOrDefaultAsync(o => o.Id == id);

    public async Task<Order> AddAsync(Order order)
    {
        _context.Orders.Add(order);
        return order;
    }

    public async Task<Order> UpdateAsync(Order order)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
        return order;
    }
    public async Task<bool> DeleteAsync(Guid id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order != null)
        {
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
        }
        return true;
    }
}