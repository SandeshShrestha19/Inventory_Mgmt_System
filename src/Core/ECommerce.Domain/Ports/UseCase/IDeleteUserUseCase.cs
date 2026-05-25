namespace ECommerce.Domain.Ports;

public interface IDeleteUserUseCase
{
    Task<bool> DeleteAsync(Guid id);
}