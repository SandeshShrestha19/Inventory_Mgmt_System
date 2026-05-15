namespace ECommerce.Domain.Ports;

public interface IOrderResponseUseCase
{
  Task<OrderResponseModel> GetByIdAsync(Guid id);
}