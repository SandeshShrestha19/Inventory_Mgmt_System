namespace ECommerce.Domain.Ports;

public interface ICategoryRepository
{
  Task<Category?> GetByIdAsync(Guid id);
  IQueryable<Category> GetAllAsync();
  Task<Category> AddAsync(Category category);
  Task UpdateAsync(Category category);
  Task<bool> DeleteAsync(Guid id);

}