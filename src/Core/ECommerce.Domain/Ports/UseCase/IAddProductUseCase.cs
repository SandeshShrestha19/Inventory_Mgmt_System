using ECommerce.Domain.Models;
using ECommerce.Domain.Entities;

namespace ECommerce.Domain.Ports;

public interface IAddProductUseCase
{
    Task<Product> ExecuteAsync(AddProductModel model);
}