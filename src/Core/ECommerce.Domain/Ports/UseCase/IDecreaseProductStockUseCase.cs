using ECommerce.Domain.Entities;

namespace ECommerce.Domain.Ports;
public interface IDecreaseProductStockUseCase
{
  Task<Product> DecreaseProductStockAsync(Guid productId, int decreasingQuantity);
}