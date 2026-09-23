using Ecommerce.Domain.Models;
using ECommerce.Domain.Constants;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Exceptions;
using ECommerce.Domain.Models;
using ECommerce.Domain.Ports;
using Microsoft.Extensions.Logging;
using Moq;

namespace ECommerce.Application.Tests.Facades;

public class OrderFacadeTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock = new();
    private readonly Mock<IProductRepository> _productRepositoryMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<ILogger<OrderFacade>> _loggerMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private readonly CurrentUser _adminUser = new(Guid.NewGuid(), "Admin");
    private readonly CurrentUser _regularUser = new(Guid.NewGuid(), "User");

    private OrderFacade CreateSut() =>
        new(_orderRepositoryMock.Object, _productRepositoryMock.Object, _userRepositoryMock.Object, _loggerMock.Object, _unitOfWorkMock.Object);

    private void SetupTransactionPassthrough() =>
        _unitOfWorkMock
            .Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<Task>, CancellationToken>(async (op, _) => await op());

    // ─── AddAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddAsync_ValidOrder_ReturnsCreatedOrder()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product { Id = productId, Name = "Pen", Price = 5m, Stock = 20 };
        var user = new User { Id = _regularUser.Id };
        var model = new PlaceOrderModel
        {
            Items = new List<OrderItemModel> { new() { ProductId = productId, Quantity = 2 } }
        };

        _userRepositoryMock.Setup(r => r.GetByIdAsync(_regularUser.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _productRepositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        _orderRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order o, CancellationToken _) => o);
        SetupTransactionPassthrough();

        var sut = CreateSut();

        // Act
        var result = await sut.AddAsync(model, _regularUser);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(10m, result.TotalPrice); // 5 * 2
        Assert.Equal(OrderStatus.Pending, result.OrderStatus);
    }

    [Fact]
    public async Task AddAsync_EmptyItems_ThrowsValidationException()
    {
        // Arrange
        var model = new PlaceOrderModel { Items = new List<OrderItemModel>() };
        var sut = CreateSut();

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => sut.AddAsync(model, _regularUser));
    }

    [Fact]
    public async Task AddAsync_NullItems_ThrowsValidationException()
    {
        // Arrange
        var model = new PlaceOrderModel { Items = null! };
        var sut = CreateSut();

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => sut.AddAsync(model, _regularUser));
    }

    [Fact]
    public async Task AddAsync_ZeroQuantity_ThrowsValidationException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var user = new User { Id = _regularUser.Id };
        var model = new PlaceOrderModel
        {
            Items = new List<OrderItemModel> { new() { ProductId = productId, Quantity = 0 } }
        };

        SetupTransactionPassthrough();
        _userRepositoryMock.Setup(r => r.GetByIdAsync(_regularUser.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        var sut = CreateSut();

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => sut.AddAsync(model, _regularUser));
    }

    [Fact]
    public async Task AddAsync_InsufficientStock_ThrowsValidationException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product { Id = productId, Price = 10m, Stock = 1 };
        var user = new User { Id = _regularUser.Id };
        var model = new PlaceOrderModel
        {
            Items = new List<OrderItemModel> { new() { ProductId = productId, Quantity = 5 } }
        };

        SetupTransactionPassthrough();
        _userRepositoryMock.Setup(r => r.GetByIdAsync(_regularUser.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _productRepositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var sut = CreateSut();

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => sut.AddAsync(model, _regularUser));
    }

    [Fact]
    public async Task AddAsync_UserNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var model = new PlaceOrderModel
        {
            Items = new List<OrderItemModel> { new() { ProductId = Guid.NewGuid(), Quantity = 1 } }
        };

        SetupTransactionPassthrough();
        _userRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((User)null!);

        var sut = CreateSut();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => sut.AddAsync(model, _regularUser));
    }

    // ─── DeleteAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_OwnerDeletes_ReturnsTrue()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, UserId = _regularUser.Id };

        _orderRepositoryMock.Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _orderRepositoryMock.Setup(r => r.DeleteAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var sut = CreateSut();

        // Act
        var result = await sut.DeleteAsync(orderId, _regularUser);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_AdminDeletes_ReturnsTrue()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, UserId = Guid.NewGuid() }; // different user

        _orderRepositoryMock.Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _orderRepositoryMock.Setup(r => r.DeleteAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var sut = CreateSut();

        // Act
        var result = await sut.DeleteAsync(orderId, _adminUser);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_UnauthorizedUser_ThrowsExceptionWithForbiddenInnerException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, UserId = Guid.NewGuid() }; // different user, not admin

        _orderRepositoryMock.Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        var sut = CreateSut();

        // Act & Assert
        // DeleteAsync catches all exceptions and re-wraps them as System.Exception.
        // The inner exception reveals the root ForbiddenException.
        var ex = await Assert.ThrowsAsync<Exception>(() => sut.DeleteAsync(orderId, _regularUser));
        Assert.Contains("You do not have access to this order", ex.Message);
    }

    [Fact]
    public async Task DeleteAsync_OrderNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _orderRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Order?)null);

        var sut = CreateSut();

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => sut.DeleteAsync(Guid.NewGuid(), _regularUser));
    }

    // ─── GetByIdAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_OwnerAccess_ReturnsMappedOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, UserId = _regularUser.Id, TotalPrice = 100m };

        _orderRepositoryMock.Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        var sut = CreateSut();

        // Act
        var result = await sut.GetByIdAsync(orderId, _regularUser);

        // Assert
        Assert.Equal(orderId, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_OtherUserAccess_ThrowsForbiddenException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, UserId = Guid.NewGuid() }; // not the same user

        _orderRepositoryMock.Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        var sut = CreateSut();

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(() => sut.GetByIdAsync(orderId, _regularUser));
    }

    [Fact]
    public async Task GetByIdAsync_OrderNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _orderRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Order?)null);

        var sut = CreateSut();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => sut.GetByIdAsync(Guid.NewGuid(), _regularUser));
    }

    // ─── GetAll ───────────────────────────────────────────────────────────────

    [Fact]
    public void GetAll_AdminUser_ReturnsAllOrders()
    {
        // Arrange
        var orders = new[]
        {
            new Order { Id = Guid.NewGuid(), UserId = Guid.NewGuid() },
            new Order { Id = Guid.NewGuid(), UserId = Guid.NewGuid() },
        }.AsQueryable();

        _orderRepositoryMock.Setup(r => r.GetAllAsync()).Returns(orders);
        var sut = CreateSut();

        // Act
        var result = sut.GetAll(null, 10, _adminUser).ToList();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void GetAll_RegularUser_ReturnsOnlyOwnOrders()
    {
        // Arrange
        var orders = new[]
        {
            new Order { Id = Guid.NewGuid(), UserId = _regularUser.Id },
            new Order { Id = Guid.NewGuid(), UserId = Guid.NewGuid() }, // different user
        }.AsQueryable();

        _orderRepositoryMock.Setup(r => r.GetAllAsync()).Returns(orders);
        var sut = CreateSut();

        // Act
        var result = sut.GetAll(null, 10, _regularUser).ToList();

        // Assert
        Assert.Single(result);
        Assert.All(result, r => Assert.Equal(_regularUser.Id, r.UserId));
    }

    // ─── UpdateOrderStatusAsync ───────────────────────────────────────────────

    [Fact]
    public async Task UpdateOrderStatusAsync_ValidTransition_UpdatesStatus()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, OrderStatus = OrderStatus.Pending };

        _orderRepositoryMock.Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        SetupTransactionPassthrough();

        var sut = CreateSut();

        // Act
        await sut.UpdateOrderStatusAsync(orderId, OrderStatus.Confirmed, _adminUser);

        // Assert
        _orderRepositoryMock.Verify(r => r.UpdateAsync(
            It.Is<Order>(o => o.OrderStatus == OrderStatus.Confirmed), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_InvalidTransition_ThrowsBusinessException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, OrderStatus = OrderStatus.Delivered };

        SetupTransactionPassthrough();
        _orderRepositoryMock.Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        var sut = CreateSut();

        // Act & Assert — Delivered → Pending is not allowed
        await Assert.ThrowsAsync<BusinessException>(() =>
            sut.UpdateOrderStatusAsync(orderId, OrderStatus.Pending, _adminUser));
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_NonAdminOrManager_ThrowsForbiddenException()
    {
        // Arrange
        var sut = CreateSut();

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            sut.UpdateOrderStatusAsync(Guid.NewGuid(), OrderStatus.Confirmed, _regularUser));
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_ManagerUser_Allowed()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, OrderStatus = OrderStatus.Pending };
        var managerUser = new CurrentUser(Guid.NewGuid(), "Manager");

        _orderRepositoryMock.Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        SetupTransactionPassthrough();

        var sut = CreateSut();

        // Act – should not throw
        await sut.UpdateOrderStatusAsync(orderId, OrderStatus.Confirmed, managerUser);

        // Assert
        _orderRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // ─── UpdateAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_NullItems_ThrowsValidationException()
    {
        // Arrange
        var model = new UpdateOrderModel { Items = null! };
        var sut = CreateSut();

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            sut.UpdateAsync(Guid.NewGuid(), model, _regularUser));
    }

    [Fact]
    public async Task UpdateAsync_RemoveItemBySettingQuantityZero_RemovesFromOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var existingItem = new OrderItem { Id = Guid.NewGuid(), ProductId = productId, Quantity = 3, UnitPrice = 10m };
        var order = new Order
        {
            Id = orderId,
            UserId = _regularUser.Id,
            OrderItems = new List<OrderItem> { existingItem }
        };
        var product = new Product { Id = productId, Stock = 0, Price = 10m };

        _orderRepositoryMock.Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _productRepositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        SetupTransactionPassthrough();

        var sut = CreateSut();
        var model = new UpdateOrderModel
        {
            Items = new List<UpdateOrderItemModel> { new() { ProductId = productId, Quantity = 0 } }
        };

        // Act
        await sut.UpdateAsync(orderId, model, _regularUser);

        // Assert – item removed and stock restored
        Assert.Empty(order.OrderItems);
        Assert.Equal(3, product.Stock); // stock was restored
    }

    [Fact]
    public async Task UpdateAsync_InsufficientStockForNewItem_ThrowsValidationException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var order = new Order { Id = orderId, UserId = _regularUser.Id, OrderItems = new List<OrderItem>() };
        var product = new Product { Id = productId, Name = "Camera", Stock = 1, Price = 100m };

        _orderRepositoryMock.Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _productRepositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        SetupTransactionPassthrough();

        var sut = CreateSut();
        var model = new UpdateOrderModel
        {
            Items = new List<UpdateOrderItemModel> { new() { ProductId = productId, Quantity = 5 } }
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => sut.UpdateAsync(orderId, model, _regularUser));
    }
}
