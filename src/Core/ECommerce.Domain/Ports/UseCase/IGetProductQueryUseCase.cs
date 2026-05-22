using ECommerce.Domain.Entities;

namespace ECommerce.Domain.Ports;

public interface IGetProductQueryUseCase
{
  IQueryable<ProductResponseModel> GetAll(Guid? cursor, int pageSize);
}