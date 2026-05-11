using ECommerce.Domain.Entities;
using Ecommerce.Domain.Models;

namespace ECommerce.Domain.Ports;

public interface IUpdateUserUseCase
{
    Task<User> ExecuteAsync(Guid id, UpdateUserModel model);
}