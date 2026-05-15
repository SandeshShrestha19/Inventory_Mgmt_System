using ECommerce.Domain.Entities;

namespace ECommerce.Domain.Ports;

public interface IGetOrderQueryUseCase
{
  IQueryable<OrderResponseModel> GetAll(Guid? cursor, int pageSize);
}