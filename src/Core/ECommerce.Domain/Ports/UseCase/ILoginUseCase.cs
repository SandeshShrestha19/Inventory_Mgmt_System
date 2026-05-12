using ECommerce.Domain.Models;

namespace ECommerce.Domain.Ports;
public interface ILoginUseCase
{
  Task<string> ExecuteAsync(LoginModel model);
}