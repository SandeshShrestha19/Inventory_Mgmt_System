using ECommerce.Domain.Entities;

namespace ECommerce.Domain.Ports;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id);
    IQueryable<Product> GetAllAsync();
    Task<Product> AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task<bool> DeleteAsync(Guid id);
    Task AssignProductsToCategoryAsync(ICollection<Guid> productIds, Guid categoryId);
}