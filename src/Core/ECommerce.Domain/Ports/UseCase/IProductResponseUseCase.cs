using ECommerce.Domain.Entities;

namespace ECommerce.Domain.Ports;

public interface IProductResponseUseCase
{
  Task<ProductResponseModel> GetByIdAsync(Guid id);
}