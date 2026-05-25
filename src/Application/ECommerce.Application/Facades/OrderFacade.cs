using Ecommerce.Domain.Models;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Ports;

public class OrderFacade : IOrderFacade
{
  private readonly IPlaceOrderUseCase _placeOrderUseCase;
  private readonly IUpdateOrderUseCase _updateOrderUseCase;
  private readonly IDeleteOrderUseCase _deleteOrderUseCase;
  private readonly IGetOrderQueryUseCase _getOrderQueryUseCase;
  private readonly IOrderResponseUseCase _getOrderByIdUseCase;
  public OrderFacade(IPlaceOrderUseCase placeOrderUseCase, IUpdateOrderUseCase updateOrderUseCase, IDeleteOrderUseCase deleteOrderUseCase, IGetOrderQueryUseCase getOrderQueryUseCase, IOrderResponseUseCase getOrderByIdUseCase)
  {
    _placeOrderUseCase = placeOrderUseCase;
    _updateOrderUseCase = updateOrderUseCase;
    _deleteOrderUseCase = deleteOrderUseCase;
    _getOrderQueryUseCase = getOrderQueryUseCase;
    _getOrderByIdUseCase = getOrderByIdUseCase;
  }

  public async Task<Order> AddAsync(PlaceOrderModel addModel)
  {
    return await _placeOrderUseCase.ExecuteAsync(addModel);
  }

  public async Task<bool> DeleteAsync(Guid id)
  {
    return await _deleteOrderUseCase.DeleteAsync(id);
  }

  public IQueryable<OrderResponseModel> GetAll(Guid? cursorId, int pageSize)
  {
    return _getOrderQueryUseCase.GetAll(cursorId, pageSize);
  }

  public async Task<OrderResponseModel> GetByIdAsync(Guid id)
  {
    return await _getOrderByIdUseCase.GetByIdAsync(id);
  }

  public async Task UpdateAsync(Guid id, UpdateOrderModel updateModel)
  {
    await _updateOrderUseCase.ExecuteAsync(id, updateModel);
  }
}