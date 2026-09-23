using ECommerce.Domain.Entities;
using ECommerce.Domain.Exceptions;
using ECommerce.Domain.Models;
using ECommerce.Domain.Ports;
using Microsoft.Extensions.Logging;
using Moq;

namespace ECommerce.Application.Tests.Facades;

public class ProductFacadeTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock = new();
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock = new();
    private readonly Mock<IGeminiFacade> _geminiFacadeMock = new();
    private readonly Mock<ILogger<ProductFacade>> _loggerMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private ProductFacade CreateSut() =>
        new(_productRepositoryMock.Object, _categoryRepositoryMock.Object, _geminiFacadeMock.Object, _loggerMock.Object, _unitOfWorkMock.Object);

    // ─── AddAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddAsync_ValidModel_ReturnsProduct()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var model = new AddProductModel
        {
            Name = "Laptop",
            Description = "A fast laptop",
            Price = 999.99m,
            Stock = 10,
            CategoryId = categoryId,
            ProductImages = new List<ProductImageModel>()
        };
        var expected = new Product { Id = Guid.NewGuid(), Name = "Laptop" };

        _productRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var sut = CreateSut();

        // Act
        var result = await sut.AddAsync(model);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task AddAsync_EmptyName_ThrowsValidationException(string? name)
    {
        // Arrange
        var model = new AddProductModel { Name = name!, Price = 10m, Stock = 1 };
        var sut = CreateSut();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => sut.AddAsync(model));
        Assert.Contains("Product name is required", ex.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task AddAsync_InvalidPrice_ThrowsValidationException(decimal price)
    {
        // Arrange
        var model = new AddProductModel { Name = "Test", Price = price, Stock = 1 };
        var sut = CreateSut();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => sut.AddAsync(model));
        Assert.Contains("Price must be greater than 0", ex.Message);
    }

    [Fact]
    public async Task AddAsync_NegativeStock_ThrowsValidationException()
    {
        // Arrange
        var model = new AddProductModel { Name = "Test", Price = 10m, Stock = -1 };
        var sut = CreateSut();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => sut.AddAsync(model));
        Assert.Contains("Stock cannot be negative", ex.Message);
    }

    [Fact]
    public async Task AddAsync_NoDescription_GeneratesViaGemini()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var model = new AddProductModel
        {
            Name = "Headphones",
            Description = "",        // empty → triggers Gemini
            Price = 50m,
            Stock = 5,
            CategoryId = categoryId,
            ProductImages = new List<ProductImageModel>()
        };
        var category = new Category { Id = categoryId, Name = "Audio" };

        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _geminiFacadeMock
            .Setup(g => g.GenerateTextAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Auto-generated description.");

        _productRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product p, CancellationToken _) => p);

        var sut = CreateSut();

        // Act
        await sut.AddAsync(model);

        // Assert – Gemini was called
        _geminiFacadeMock.Verify(g => g.GenerateTextAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddAsync_GeminiFails_AddsProductWithEmptyDescription()
    {
        // Arrange
        var model = new AddProductModel
        {
            Name = "Tablet",
            Description = "",
            Price = 300m,
            Stock = 3,
            CategoryId = Guid.NewGuid(),
            ProductImages = new List<ProductImageModel>()
        };

        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        _geminiFacadeMock
            .Setup(g => g.GenerateTextAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Gemini unavailable"));

        Product? capturedProduct = null;
        _productRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>((p, _) => capturedProduct = p)
            .ReturnsAsync((Product p, CancellationToken _) => p);

        var sut = CreateSut();

        // Act
        await sut.AddAsync(model);

        // Assert – description degrades gracefully to empty
        Assert.Equal(string.Empty, capturedProduct!.Description);
    }

    // ─── GetByIdAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsMappedResponse()
    {
        // Arrange
        var id = Guid.NewGuid();
        var product = new Product { Id = id, Name = "Keyboard", Price = 75m };

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var sut = CreateSut();

        // Act
        var result = await sut.GetByIdAsync(id);

        // Assert
        Assert.Equal(id, result.Id);
        Assert.Equal("Keyboard", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ThrowsNotFoundException()
    {
        // Arrange
        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var sut = CreateSut();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => sut.GetByIdAsync(Guid.NewGuid()));
    }

    // ─── DeleteAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_Success_ReturnsTrue()
    {
        // Arrange
        _productRepositoryMock
            .Setup(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var sut = CreateSut();

        // Act
        var result = await sut.DeleteAsync(Guid.NewGuid());

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_RepositoryThrows_RethrowsWrappedException()
    {
        // Arrange
        _productRepositoryMock
            .Setup(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB error"));

        var sut = CreateSut();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => sut.DeleteAsync(Guid.NewGuid()));
        Assert.Contains("Failed to delete product", ex.Message);
    }

    // ─── GetAll ───────────────────────────────────────────────────────────────

    [Fact]
    public void GetAll_NoCursor_ReturnsPagedResults()
    {
        // Arrange
        var products = new[]
        {
            new Product { Id = Guid.NewGuid(), Name = "A", Price = 10m },
            new Product { Id = Guid.NewGuid(), Name = "B", Price = 20m },
        }.AsQueryable();

        _productRepositoryMock.Setup(r => r.GetAllAsync()).Returns(products);
        var sut = CreateSut();

        // Act
        var result = sut.GetAll(null, 10).ToList();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void GetAll_PageSize1_ReturnsOnlyOneResult()
    {
        // Arrange
        var products = new[]
        {
            new Product { Id = Guid.Parse("00000000-0000-0000-0000-000000000001"), Name = "A", Price = 10m },
            new Product { Id = Guid.Parse("00000000-0000-0000-0000-000000000002"), Name = "B", Price = 20m },
        }.AsQueryable();

        _productRepositoryMock.Setup(r => r.GetAllAsync()).Returns(products);
        var sut = CreateSut();

        // Act
        var result = sut.GetAll(null, 1).ToList();

        // Assert
        Assert.Single(result);
    }

    // ─── UpdateAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ValidFields_UpdatesProduct()
    {
        // Arrange
        var id = Guid.NewGuid();
        var product = new Product { Id = id, Name = "Old", Price = 100m, Stock = 5 };
        var model = new UpdateProductModel { Name = "New", Price = 200m, Stock = 10 };

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var sut = CreateSut();

        // Act
        await sut.UpdateAsync(id, model);

        // Assert
        _productRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Product>(p =>
            p.Name == "New" && p.Price == 200m && p.Stock == 10), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ProductNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var sut = CreateSut();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            sut.UpdateAsync(Guid.NewGuid(), new UpdateProductModel()));
    }

    [Fact]
    public async Task UpdateAsync_InvalidPrice_ThrowsValidationException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var product = new Product { Id = id, Name = "X", Price = 100m };
        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var model = new UpdateProductModel { Price = -5m };
        var sut = CreateSut();

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => sut.UpdateAsync(id, model));
    }

    [Fact]
    public async Task UpdateAsync_NegativeStock_ThrowsValidationException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var product = new Product { Id = id, Name = "X", Price = 100m, Stock = 5 };
        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var model = new UpdateProductModel { Stock = -1 };
        var sut = CreateSut();

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => sut.UpdateAsync(id, model));
    }

    [Fact]
    public async Task UpdateAsync_EmptyDescription_RegeneratesViaGemini()
    {
        // Arrange
        var id = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var product = new Product { Id = id, Name = "Mouse", Price = 30m, CategoryId = categoryId };
        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Category { Id = categoryId, Name = "Peripherals" });

        _geminiFacadeMock
            .Setup(g => g.GenerateTextAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Fresh description");

        var model = new UpdateProductModel { Description = "" }; // empty → trigger Gemini
        var sut = CreateSut();

        // Act
        await sut.UpdateAsync(id, model);

        // Assert
        _geminiFacadeMock.Verify(g => g.GenerateTextAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // ─── IncreaseStockAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task IncreaseStockAsync_ValidQuantity_IncreasesStock()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product { Id = productId, Stock = 10 };

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var sut = CreateSut();

        // Act
        await sut.IncreaseStockAsync(productId, 5);

        // Assert
        Assert.Equal(15, product.Stock);
        _productRepositoryMock.Verify(r => r.UpdateAsync(product, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ─── DecreaseStockAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task DecreaseStockAsync_ValidQuantity_DecreasesStock()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product { Id = productId, Stock = 10 };

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var sut = CreateSut();

        // Act
        await sut.DecreaseStockAsync(productId, 4);

        // Assert
        Assert.Equal(6, product.Stock);
        _productRepositoryMock.Verify(r => r.UpdateAsync(product, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DecreaseStockAsync_ExceedsStock_ThrowsBusinessException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product { Id = productId, Stock = 2 };

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var sut = CreateSut();

        // Act & Assert
        await Assert.ThrowsAsync<BusinessException>(() => sut.DecreaseStockAsync(productId, 10));
    }
}
