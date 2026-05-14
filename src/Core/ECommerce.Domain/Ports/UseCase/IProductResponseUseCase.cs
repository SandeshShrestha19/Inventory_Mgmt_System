using ECommerce.Domain.Entities;

namespace ECommerce.Domain.Ports;

public interface IProductResponseUseCase
{
  Task<ProductResponseModel> ExecuteAsync(Guid id);
}