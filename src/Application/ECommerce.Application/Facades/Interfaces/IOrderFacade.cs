using Ecommerce.Domain.Models;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Models;

public interface IOrderFacade
{
  IQueryable<OrderResponseModel> GetAll(Guid? cursorId, int pageSize);
  Task<OrderResponseModel> GetByIdAsync(Guid id);
  Task<Order> AddAsync(PlaceOrderModel addModel);
  Task UpdateAsync(Guid id, UpdateOrderModel udpateModel);
  Task<bool> DeleteAsync(Guid id);
}