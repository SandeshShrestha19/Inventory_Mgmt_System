using Ecommerce.Domain.Models;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Models;

public interface IOrderFacade
{
  IQueryable<OrderResponseModel> GetAll(Guid? cursorId, int pageSize);
  Task<OrderResponseModel> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
  Task<Order> AddAsync(PlaceOrderModel addModel, CancellationToken cancellationToken = default);
  Task UpdateAsync(Guid id, UpdateOrderModel udpateModel, CancellationToken cancellationToken = default);
  Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}