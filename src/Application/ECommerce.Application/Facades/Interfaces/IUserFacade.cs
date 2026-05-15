using Ecommerce.Domain.Models;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Models;

public interface IUserFacade
{
  IQueryable<UserResponseModel> GetAll(Guid? cursorId, int pageSize);
  Task<UserResponseModel> GetByIdAsync(Guid id);
  Task<User> AddAsync(AddUserModel addModel);
  Task UpdateAsync(Guid id, UpdateUserModel udpateModel);
  Task<bool> DeleteAsync(Guid id);
}