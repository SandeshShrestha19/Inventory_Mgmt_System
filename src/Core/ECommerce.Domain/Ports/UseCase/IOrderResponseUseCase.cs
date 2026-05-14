namespace ECommerce.Domain.Ports;

public interface IOrderResponseUseCase
{
  Task<OrderResponseModel> ExecuteAsync(Guid id);
}