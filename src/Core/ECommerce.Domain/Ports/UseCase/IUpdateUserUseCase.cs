using ECommerce.Domain.Entities;
using Ecommerce.Domain.Models;

namespace ECommerce.Domain.Ports;

public interface IUpdateUserUseCase
{
    Task ExecuteAsync(Guid id, UpdateUserModel model);
}