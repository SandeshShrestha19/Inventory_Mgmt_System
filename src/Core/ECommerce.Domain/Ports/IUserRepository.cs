using ECommerce.Domain.Entities;

namespace ECommerce.Domain.Ports;

public interface IUserRepository
{
    Task<User> GetByIdAsync(Guid id);
    IQueryable<User> GetAllAsync();
    Task<User> AddAsync(User user);
    Task UpdateAsync(User user);
    Task<bool> DeleteAsync(Guid id);
}