using ECommerce.Domain.Entities;

namespace ECommerce.Domain.Ports;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id);
    IQueryable<Order> GetAllAsync();
    Task AddAsync(Order order);
    Task UpdateAsync(Order order);
    Task DeleteAsync(Guid id);
}