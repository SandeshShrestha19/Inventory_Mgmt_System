using ECommerce.Domain.Entities;
using ECommerce.Domain.Ports;
using Microsoft.Extensions.Logging;

public class UpdateOrderUseCase : IUpdateOrderUseCase
{
  private readonly IOrderRepository _orderRepository;
  private readonly IProductRepository _productRepository;
  private readonly ILogger<UpdateOrderUseCase> _logger;

  public UpdateOrderUseCase(
      IOrderRepository orderRepository,
      ILogger<UpdateOrderUseCase> logger,
      IProductRepository productRepository)
  {
    _orderRepository = orderRepository;
    _logger = logger;
    _productRepository = productRepository;
  }

  public async Task<Order> ExecuteAsync(Guid id, UpdateOrderModel model)
  {
    try
    {
      var order = await _orderRepository.GetByIdAsync(id) ?? throw new Exception("Order not found!");

      foreach (var item in model.Items)
      {
        var product = await _productRepository.GetByIdAsync(item.ProductId) ?? throw new Exception("Product not found!");

        var existingItem = order.OrderItems
                    .FirstOrDefault(oi => oi.ProductId == item.ProductId);

        if (item.Quantity == 0)
        {
          if (existingItem != null)
          {
            product.Stock += existingItem.Quantity;
            await _productRepository.UpdateAsync(product);
            order.OrderItems.Remove(existingItem);
          }
        }
        else if (existingItem != null)
        {
          int difference = item.Quantity - existingItem.Quantity;

          if (difference > 0 && product.Stock < difference)
          {
            throw new Exception($"Insufficient stock for {product.Name}!");
          }

          product.Stock -= difference;
          await _productRepository.UpdateAsync(product);
          existingItem.Quantity = item.Quantity;
        }
        else
        {
          if (product.Stock < item.Quantity)
          {
            throw new Exception($"Insufficient stock for {product.Name}!");
          }

          product.Stock -= item.Quantity;
          await _productRepository.UpdateAsync(product);

          order.OrderItems.Add(new OrderItem
          {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            ProductId = product.Id,
            Quantity = item.Quantity,
            UnitPrice = product.Price
          });
        }
      }
      order.TotalPrice = order.OrderItems.Sum(oi => oi.UnitPrice * oi.Quantity);

      return await _orderRepository.UpdateAsync(order);
    }
    catch (Exception ex)
    {
      _logger.LogInformation(ex, "Error while updating order!");
      throw;
    }
  }
}