namespace ECommerce.Domain.Ports;

public interface IDeleteOrderUseCase
{
    Task<bool> DeleteAsync(Guid id);
}