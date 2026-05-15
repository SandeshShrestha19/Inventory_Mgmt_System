using ECommerce.Domain.Entities;
using ECommerce.Domain.Ports;
using Microsoft.Extensions.Logging;

public class GetOrderQueryUseCase : IGetOrderQueryUseCase
{
  private readonly IOrderRepository _orderRepository;

  public GetOrderQueryUseCase(IOrderRepository orderRepository)
  {
    _orderRepository = orderRepository;
  }
  public IQueryable<OrderResponseModel> GetAll(Guid? cursorId, int pageSize)
  {
    var orders = _orderRepository.GetAllAsync();
    if (cursorId.HasValue)
    {
      orders = orders.Where(x => x.Id > cursorId.Value);
    }
    return orders.OrderBy(x => x.Id)
    .Take(pageSize)
    .Select(x => new OrderResponseModel
    {
      Id = x.Id,
      OrderItems = x.OrderItems,
      TotalPrice = x.TotalPrice,
      OrderDate = x.OrderDate
    });
  }
}