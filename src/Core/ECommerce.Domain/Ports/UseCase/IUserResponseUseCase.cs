using ECommerce.Domain.Entities;

namespace ECommerce.Domain.Ports;

public interface IUserResponseUseCase
{
  Task<UserResponseModel> ExecuteAsync(Guid id);
}