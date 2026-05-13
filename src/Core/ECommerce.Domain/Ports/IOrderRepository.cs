using ECommerce.Domain.Entities;

namespace ECommerce.Domain.Ports;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id);
    IQueryable<Order> GetAllAsync();
    Task<Order> AddAsync(Order order);
    Task UpdateAsync(Order order);
    Task<bool> DeleteAsync(Guid id);
}