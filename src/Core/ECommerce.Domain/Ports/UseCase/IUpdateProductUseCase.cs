using ECommerce.Domain.Models;
using ECommerce.Domain.Entities;

namespace ECommerce.Domain.Ports;

public interface IUpdateProductUseCase
{
    Task<Product> ExecuteAsync(Guid id, UpdateProductModel model);
}