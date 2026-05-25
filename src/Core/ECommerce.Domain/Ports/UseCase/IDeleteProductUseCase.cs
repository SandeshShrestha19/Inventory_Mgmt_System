namespace ECommerce.Domain.Ports;

public interface IDeleteProductUseCase
{
    Task<bool> DeleteAsync(Guid id);
}