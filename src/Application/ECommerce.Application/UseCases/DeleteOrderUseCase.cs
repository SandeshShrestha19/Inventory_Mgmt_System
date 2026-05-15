using ECommerce.Domain.Ports;
using Microsoft.Extensions.Logging;

public class DeleteOrderUseCase : IDeleteOrderUseCase
{
  private readonly IOrderRepository _orderRepository;
  private readonly ILogger<DeleteOrderUseCase> _logger;

  public DeleteOrderUseCase(IOrderRepository orderRepository, ILogger<DeleteOrderUseCase> logger)
  {
    _orderRepository = orderRepository;
    _logger = logger;
  }

  public async Task<bool> DeleteAsync(Guid id)
  {
    try
    {
      await _orderRepository.DeleteAsync(id);
      return true;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to delete product");
      throw new Exception($"Failed to delete product: {ex.Message}");
    }

  }
}