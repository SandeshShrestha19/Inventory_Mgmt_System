using ECommerce.Domain.Entities;

namespace ECommerce.Domain.Ports;
public interface IIncreaseProductStockUseCase
{
  Task<Product> IncreaseProductStockAsync(Guid productId, int increasingQuantity);
}