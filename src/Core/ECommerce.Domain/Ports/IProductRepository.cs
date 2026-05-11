using ECommerce.Domain.Entities;

namespace ECommerce.Domain.Ports;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id);
    IQueryable<Product> GetAllAsync();
    Task<Product> AddAsync(Product product);
    Task<Product> UpdateAsync(Product product);
    Task<bool> DeleteAsync(Guid id);
}