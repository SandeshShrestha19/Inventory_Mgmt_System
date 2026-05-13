using ECommerce.Domain.Models;
using ECommerce.Domain.Entities;

namespace ECommerce.Domain.Ports;

public interface IUpdateProductUseCase
{
    Task ExecuteAsync(Guid id, UpdateProductModel model);
}