using ECommerce.Domain.Models;
using ECommerce.Domain.Entities;

namespace ECommerce.Domain.Ports;

public interface IUpdateOrderUseCase
{
    Task ExecuteAsync(Guid id, UpdateOrderModel model);
}