using ECommerce.Domain.Models;

namespace ECommerce.Domain.Ports;
public interface ILogoutUseCase
{
  Task ExecuteAsync(Guid id, string refreshToken);
}