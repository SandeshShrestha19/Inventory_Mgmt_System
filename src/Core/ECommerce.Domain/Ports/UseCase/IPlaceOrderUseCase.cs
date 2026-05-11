using ECommerce.Domain.Entities;

namespace ECommerce.Domain.Ports;

public interface IPlaceOrderUseCase
{
  Task<Order> ExecuteAsync(PlaceOrderModel model);
}