using ECommerce.Domain.Models;
using ECommerce.Domain.Entities;
using Ecommerce.Domain.Models;

namespace ECommerce.Domain.Ports;

public interface IRegisterUserUseCase
{
    Task<User> ExecuteAsync(AddUserModel model);
}