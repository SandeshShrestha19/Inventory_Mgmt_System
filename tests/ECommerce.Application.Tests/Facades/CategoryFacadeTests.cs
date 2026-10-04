using ECommerce.Domain.Entities;
using ECommerce.Domain.Exceptions;
using ECommerce.Domain.Models;
using ECommerce.Domain.Ports;
using Microsoft.Extensions.Logging;
using Moq;

namespace ECommerce.Application.Tests.Facades;

public class CategoryFacadeTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock = new();
    private readonly Mock<IProductRepository> _productRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ILogger<ProductFacade>> _loggerMock = new();

    private CategoryFacade CreateSut() =>
        new(_productRepositoryMock.Object, _loggerMock.Object, _unitOfWorkMock.Object, _categoryRepositoryMock.Object);

    // ─── AddAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddAsync_ValidModel_ReturnsCategory()
    {
        // Arrange
        var model = new AddCategoryModel { Name = "Electronics" };
        var expected = new Category { Id = Guid.NewGuid(), Name = "Electronics" };

        _categoryRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var sut = CreateSut();

        // Act
        var result = await sut.AddAsync(model);

        // Assert
        Assert.Equal(expected, result);
        _categoryRepositoryMock.Verify(r => r.AddAsync(It.Is<Category>(c => c.Name == "Electronics"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task AddAsync_EmptyOrNullName_ThrowsValidationException(string? name)
    {
        // Arrange
        var model = new AddCategoryModel { Name = name! };
        var sut = CreateSut();

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => sut.AddAsync(model));
        _categoryRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("Books")]
    [InlineData("books")]
    [InlineData("BOOKS")]
    public async Task AddAsync_DuplicateNameDifferentCase_ThrowsConflictException(string duplicateName)
    {
        // Arrange
        var model = new AddCategoryModel { Name = duplicateName };
        _categoryRepositoryMock
            .Setup(r => r.ExistsWithNameAsync(duplicateName, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var sut = CreateSut();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(() => sut.AddAsync(model));
        Assert.Contains(duplicateName, ex.Message);
        _categoryRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddAsync_NameExceedsFiftyCharacters_ThrowsValidationException()
    {
        // DEMO FAILING TEST: Presentation requirement for category name length limit (<= 50 chars)
        // Currently fails because maximum length validation is not yet enforced in CategoryFacade.
        var longName = new string('A', 51);
        var model = new AddCategoryModel { Name = longName };
        var sut = CreateSut();

        await Assert.ThrowsAsync<ValidationException>(() => sut.AddAsync(model));
    }

    // ─── GetByIdAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsMappedResponse()
    {
        // Arrange
        var id = Guid.NewGuid();
        var category = new Category { Id = id, Name = "Books" };

        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var sut = CreateSut();

        // Act
        var result = await sut.GetByIdAsync(id);

        // Assert
        Assert.Equal(id, result.Id);
        Assert.Equal("Books", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ThrowsNotFoundException()
    {
        // Arrange
        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var sut = CreateSut();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => sut.GetByIdAsync(Guid.NewGuid()));
    }

    // ─── DeleteAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ExistingId_ReturnsTrue()
    {
        // Arrange
        var id = Guid.NewGuid();
        _categoryRepositoryMock
            .Setup(r => r.DeleteAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var sut = CreateSut();

        // Act
        var result = await sut.DeleteAsync(id);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_RepositoryThrows_RethrowsWrappedException()
    {
        // Arrange
        _categoryRepositoryMock
            .Setup(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB error"));

        var sut = CreateSut();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => sut.DeleteAsync(Guid.NewGuid()));
        Assert.Contains("Failed to delete category", ex.Message);
    }

    // ─── GetAll ───────────────────────────────────────────────────────────────

    [Fact]
    public void GetAll_NoCursor_ReturnsPagedResults()
    {
        // Arrange
        var categories = new[]
        {
            new Category { Id = Guid.NewGuid(), Name = "A" },
            new Category { Id = Guid.NewGuid(), Name = "B" },
        }.AsQueryable();

        _categoryRepositoryMock.Setup(r => r.GetAllAsync()).Returns(categories);
        var sut = CreateSut();

        // Act
        var result = sut.GetAll(null, 10).ToList();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void GetAll_WithCursor_FiltersAndReturnsResults()
    {
        // Arrange
        var id1 = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var id2 = Guid.Parse("00000000-0000-0000-0000-000000000002");
        var id3 = Guid.Parse("00000000-0000-0000-0000-000000000003");

        var categories = new[]
        {
            new Category { Id = id1, Name = "A" },
            new Category { Id = id2, Name = "B" },
            new Category { Id = id3, Name = "C" },
        }.AsQueryable();

        _categoryRepositoryMock.Setup(r => r.GetAllAsync()).Returns(categories);
        var sut = CreateSut();

        // Act — request items whose Id is greater than id1
        var result = sut.GetAll(id1, 10).ToList();

        // Assert
        Assert.All(result, r => Assert.True(r.Id > id1));
    }

    // ─── UpdateAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ExistingCategory_UpdatesNameAndAssignsProducts()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var productIds = new List<Guid> { Guid.NewGuid() };
        var category = new Category { Id = categoryId, Name = "Old Name" };
        var updateModel = new UpdateCategoryModel { Name = "New Name", ProductIds = productIds };

        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _unitOfWorkMock
            .Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<Task>, CancellationToken>(async (op, _) => await op());

        var sut = CreateSut();

        // Act
        await sut.UpdateAsync(categoryId, updateModel);

        // Assert
        _categoryRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Category>(c => c.Name == "New Name"), It.IsAny<CancellationToken>()), Times.Once);
        _productRepositoryMock.Verify(r => r.AssignProductsToCategoryAsync(productIds, categoryId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_CategoryNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _unitOfWorkMock
            .Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<Task>, CancellationToken>(async (op, _) => await op());

        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var sut = CreateSut();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            sut.UpdateAsync(Guid.NewGuid(), new UpdateCategoryModel { Name = "X" }));
    }

    [Theory]
    [InlineData("Fiction")]
    [InlineData("FICTION")]
    [InlineData("fiction")]
    public async Task UpdateAsync_NewNameAlreadyExistsCaseInsensitive_ThrowsConflictException(string newName)
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var existingCategory = new Category { Id = categoryId, Name = "Non-Fiction" };

        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCategory);

        _categoryRepositoryMock
            .Setup(r => r.ExistsWithNameAsync(newName, categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _unitOfWorkMock
            .Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<Task>, CancellationToken>(async (op, _) => await op());

        var sut = CreateSut();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(() =>
            sut.UpdateAsync(categoryId, new UpdateCategoryModel { Name = newName }));
        Assert.Contains(newName, ex.Message);
    }

    [Fact]
    public async Task UpdateAsync_SameNameDifferentCase_AllowsUpdateWithoutConflict()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var existingCategory = new Category { Id = categoryId, Name = "Books" };

        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCategory);

        _unitOfWorkMock
            .Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<Task>, CancellationToken>(async (op, _) => await op());

        var sut = CreateSut();

        // Act
        await sut.UpdateAsync(categoryId, new UpdateCategoryModel { Name = "BOOKS" });

        // Assert - should NOT query ExistsWithNameAsync since it's the same name
        _categoryRepositoryMock.Verify(r => r.ExistsWithNameAsync(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
        _categoryRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Category>(c => c.Name == "BOOKS"), It.IsAny<CancellationToken>()), Times.Once);
    }
}
