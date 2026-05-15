using ECommerce.Domain.Entities;
using ECommerce.Domain.Models;

public interface IProductFacade
{
  IQueryable<ProductResponseModel> GetAll(Guid? cursorId, int pageSize);
  Task<ProductResponseModel> GetByIdAsync(Guid id);
  Task<Product> AddAsync(AddProductModel addModel);
  Task UpdateAsync(Guid id, UpdateProductModel udpateModel);
  Task<bool> DeleteAsync(Guid id);
}