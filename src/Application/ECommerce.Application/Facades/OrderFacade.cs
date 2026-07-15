using Ecommerce.Domain.Models;
using ECommerce.Domain.Constants;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Exceptions;
using ECommerce.Domain.Ports;
using Microsoft.Extensions.Logging;

public class OrderFacade : IOrderFacade
{
  private readonly IOrderRepository _orderRepository;
  private readonly IProductRepository _productRepository;
  private readonly IUserRepository _userRepository;
  private readonly ILogger<OrderFacade> _logger;
  private readonly IUnitOfWork _unitOfWork;

  public OrderFacade(IOrderRepository orderRepository, IProductRepository productRepository, IUserRepository userRepository, ILogger<OrderFacade> logger, IUnitOfWork unitOfWork)
  {
    _orderRepository = orderRepository;
    _productRepository = productRepository;
    _userRepository = userRepository;
    _logger = logger;
    _unitOfWork = unitOfWork;
  }

  public async Task<Order> AddAsync(PlaceOrderModel model, CancellationToken cancellationToken = default)
  {
    try
    {
      var user = await _userRepository.GetByIdAsync(model.UserId, cancellationToken)
          ?? throw NotFoundException.User();

      var orderItems = new List<OrderItem>();

      foreach (var item in model.Items)
      {
        var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken)
            ?? throw NotFoundException.Product();

        if (product.Stock < item.Quantity)
          throw new ValidationException("Product stock is less than order quantity!");

        product.Stock -= item.Quantity;
        await _productRepository.UpdateAsync(product, cancellationToken);

        orderItems.Add(new OrderItem
        {
          Id = Guid.CreateVersion7(),
          ProductId = product.Id,
          Quantity = item.Quantity,
          UnitPrice = product.Price
        });
      }

      var totalPrice = orderItems.Sum(oi => oi.UnitPrice * oi.Quantity);

      var order = new Order
      {
        Id = Guid.CreateVersion7(),
        UserId = model.UserId,
        OrderItems = orderItems,
        TotalPrice = totalPrice,
        OrderStatus = OrderStatus.Pending
      };

      await _orderRepository.AddAsync(order, cancellationToken);
      await _unitOfWork.SaveChangesAsync(cancellationToken);
      _logger.LogInformation("Placing order for user: {UserId}", model.UserId);
      return order;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to place order!");
      throw;
    }
  }

  public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
  {
    try
    {
      await _orderRepository.DeleteAsync(id, cancellationToken);
      return true;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to delete product");
      throw new Exception($"Failed to delete product: {ex.Message}");
    }

  }

  public async Task UpdateAsync(Guid id, UpdateOrderModel model, CancellationToken cancellationToken = default)
  {
    try
    {
      var order = await _orderRepository.GetByIdAsync(id, cancellationToken) ?? throw NotFoundException.Order();

      foreach (var item in model.Items)
      {
        var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken) ?? throw NotFoundException.Product();

        var existingItem = order.OrderItems
                    .FirstOrDefault(oi => oi.ProductId == item.ProductId);

        if (item.Quantity == 0)
        {
          if (existingItem != null)
          {
            product.Stock += existingItem.Quantity;
            await _productRepository.UpdateAsync(product, cancellationToken);
            order.OrderItems.Remove(existingItem);
          }
        }
        else if (existingItem != null)
        {
          int difference = item.Quantity - existingItem.Quantity;

          if (difference > 0 && product.Stock < difference)
          {
            throw new ValidationException($"Insufficient stock for {product.Name}!");
          }

          product.Stock -= difference;
          await _productRepository.UpdateAsync(product, cancellationToken);
          existingItem.Quantity = item.Quantity;
        }
        else
        {
          if (product.Stock < item.Quantity)
          {
            throw new ValidationException($"Insufficient stock for {product.Name}!");
          }

          product.Stock -= item.Quantity;
          await _productRepository.UpdateAsync(product, cancellationToken);

          order.OrderItems.Add(new OrderItem
          {
            Id = Guid.CreateVersion7(),
            OrderId = order.Id,
            ProductId = product.Id,
            Quantity = item.Quantity,
            UnitPrice = product.Price
          });
        }
      }
      order.TotalPrice = order.OrderItems.Sum(oi => oi.UnitPrice * oi.Quantity);
      order.OrderStatus = model.OrderStatus ?? OrderStatus.Pending;

      await _orderRepository.UpdateAsync(order, cancellationToken);
    }
    catch (Exception ex)
    {
      _logger.LogInformation(ex, "Error while updating order!");
      throw;
    }
  }

  public IQueryable<OrderResponseModel> GetAll(Guid? cursorId, int pageSize)
  {
    var orders = _orderRepository.GetAllAsync();
    if (cursorId.HasValue)
    {
      orders = orders.Where(x => x.Id > cursorId.Value);
    }
    return orders.OrderBy(x => x.Id)
    .Take(pageSize)
    .Select(x => new OrderResponseModel
    {
      Id = x.Id,
      OrderItems = x.OrderItems,
      TotalPrice = x.TotalPrice,
      OrderDate = x.OrderDate,
      OrderStatus = x.OrderStatus
    });
  }

  public async Task<OrderResponseModel> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    try
    {
      var order = await _orderRepository.GetByIdAsync(id, cancellationToken) ?? throw new ValidationException("User not found!");
      return ResponseMapper.ToOrderResponse(order);
    }
    catch (Exception ex)
    {
      _logger.LogInformation(ex, "Failed to retrieve data!");
      throw;
    }
  }
}