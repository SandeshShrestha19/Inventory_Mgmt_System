using ECommerce.Domain.Models;

public interface ICategoryFacade
{
  IQueryable<CategoryResponseModel> GetAll(Guid? cursorId, int pageSize);
  Task<CategoryResponseModel> GetByIdAsync(Guid id);
  Task<Category> AddAsync(AddCategoryModel addModel);
  Task UpdateAsync(Guid id, UpdateCategoryModel udpateModel);
  Task<bool> DeleteAsync(Guid id);
}