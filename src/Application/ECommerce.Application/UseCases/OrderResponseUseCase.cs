using System.ComponentModel.DataAnnotations;
using ECommerce.Domain.Ports;
using Microsoft.Extensions.Logging;

public class OrderResponseUseCase : IOrderResponseUseCase
{
  private readonly IOrderRepository _orderRepository;
  private readonly ILogger<OrderResponseUseCase> _logger;

  public OrderResponseUseCase(IOrderRepository orderRepository, ILogger<OrderResponseUseCase> logger)
  {
    _orderRepository = orderRepository;
    _logger = logger;
  }

  public async Task<OrderResponseModel> GetByIdAsync(Guid id)
  {
    try
    {
      var order = await _orderRepository.GetByIdAsync(id) ?? throw new ValidationException("User not found!");
      return ResponseMapper.ToOrderResponse(order);
    }
    catch(Exception ex)
    {
      _logger.LogInformation(ex, "Failed to retrieve data!");
      throw;
    }
}
    
}