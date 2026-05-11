using ECommerce.Domain.Entities;
using ECommerce.Domain.Models;
using ECommerce.Domain.Ports;
using Microsoft.Extensions.Logging;

namespace ECommerce.Application.UseCases;

public class PlaceOrderUseCase : IPlaceOrderUseCase
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<PlaceOrderUseCase> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public PlaceOrderUseCase(
        IOrderRepository orderRepository,
        ILogger<PlaceOrderUseCase> logger,
        IUserRepository userRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _logger = logger;
        _userRepository = userRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Order> ExecuteAsync(PlaceOrderModel model)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(model.UserId)
                ?? throw new Exception("User not found!");

            var orderItems = new List<OrderItem>();

            foreach (var item in model.Items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId)
                    ?? throw new Exception($"Product {item.ProductId} not found!");

                if (product.Stock < item.Quantity)
                    throw new Exception("Product stock is less than order quantity!");

                product.Stock -= item.Quantity;
                await _productRepository.UpdateAsync(product);

                orderItems.Add(new OrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price
                });
            }

            var totalPrice = orderItems.Sum(oi => oi.UnitPrice * oi.Quantity);

            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = model.UserId,
                OrderItems = orderItems,
                TotalPrice = totalPrice
            };

            await _orderRepository.AddAsync(order); 
            await _unitOfWork.SaveChangesAsync();   
            _logger.LogInformation("Placing order for user: {UserId}", model.UserId);
            return order;                     
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to place order!");
            throw;
        }
    }
}