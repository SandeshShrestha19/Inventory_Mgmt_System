using Ecommerce.Domain.Models;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Exceptions;
using ECommerce.Domain.Models;
using ECommerce.Domain.Ports;
using Microsoft.Extensions.Logging;
using Moq;

namespace ECommerce.Application.Tests.Facades;

public class UserFacadeTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<ILogger<UserFacade>> _loggerMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private UserFacade CreateSut() =>
        new(_userRepositoryMock.Object, _loggerMock.Object, _unitOfWorkMock.Object);

    // ─── AddAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddAsync_ValidModel_ReturnsCreatedUser()
    {
        // Arrange
        var model = new AddUserModel
        {
            Name = "Alice",
            Username = "alice99",
            Email = "alice@example.com",
            Password = "P@ssword1",
            PhoneNumber = "1234567890"
        };
        var expected = new User { Id = Guid.NewGuid(), Name = "Alice", Email = "alice@example.com" };

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(model.Email, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        _userRepositoryMock.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var sut = CreateSut();

        // Act
        var result = await sut.AddAsync(model);

        // Assert
        Assert.Equal(expected.Id, result.Id);
        Assert.Equal("Alice", result.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task AddAsync_EmptyName_ThrowsValidationException(string? name)
    {
        // Arrange
        var model = new AddUserModel { Name = name!, Username = "u", Email = "a@b.com", Password = "P@ssword1" };
        var sut = CreateSut();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidationException>(() => sut.AddAsync(model));
        Assert.Contains("Name", ex.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task AddAsync_EmptyUsername_ThrowsValidationException(string? username)
    {
        // Arrange
        var model = new AddUserModel { Name = "Alice", Username = username!, Email = "a@b.com", Password = "P@ssword1" };
        var sut = CreateSut();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidationException>(() => sut.AddAsync(model));
        Assert.Contains("Username", ex.Message);
    }

    [Fact]
    public async Task AddAsync_EmailWithoutAt_ThrowsValidationException()
    {
        // Arrange
        var model = new AddUserModel { Name = "Alice", Username = "alice", Email = "notanemail", Password = "P@ssword1" };
        var sut = CreateSut();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidationException>(() => sut.AddAsync(model));
        Assert.Contains("@", ex.Message);
    }

    [Fact]
    public async Task AddAsync_PasswordWithoutSpecialChar_ThrowsValidationException()
    {
        // Arrange
        var model = new AddUserModel { Name = "Alice", Username = "alice", Email = "a@b.com", Password = "Password1" };
        var sut = CreateSut();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidationException>(() => sut.AddAsync(model));
        Assert.Contains("special character", ex.Message);
    }

    [Fact]
    public async Task AddAsync_PasswordTooShort_ThrowsValidationException()
    {
        // Arrange
        var model = new AddUserModel { Name = "Alice", Username = "alice", Email = "a@b.com", Password = "P@1" };
        var sut = CreateSut();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidationException>(() => sut.AddAsync(model));
        Assert.Contains("8 characters", ex.Message);
    }

    [Fact]
    public async Task AddAsync_DuplicateEmail_ThrowsConflictException()
    {
        // Arrange
        var existingUser = new User { Id = Guid.NewGuid(), Email = "existing@example.com" };
        var model = new AddUserModel
        {
            Name = "Bob",
            Username = "bob",
            Email = "existing@example.com",
            Password = "P@ssword1"
        };

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(model.Email, It.IsAny<CancellationToken>())).ReturnsAsync(existingUser);

        var sut = CreateSut();

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => sut.AddAsync(model));
    }

    [Fact]
    public async Task AddAsync_ValidModel_StoresEmailLowerCase()
    {
        // Arrange
        var model = new AddUserModel
        {
            Name = "Alice",
            Username = "alice",
            Email = "Alice@Example.COM",
            Password = "P@ssword1"
        };

        User? capturedUser = null;
        _userRepositoryMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        _userRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, _) => capturedUser = u)
            .ReturnsAsync((User u, CancellationToken _) => u);

        var sut = CreateSut();

        // Act
        await sut.AddAsync(model);

        // Assert
        Assert.Equal("alice@example.com", capturedUser!.Email);
    }

    // ─── GetByIdAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsMappedUser()
    {
        // Arrange
        var id = Guid.NewGuid();
        var user = new User { Id = id, Name = "Carol", Email = "carol@test.com" };

        _userRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var sut = CreateSut();

        // Act
        var result = await sut.GetByIdAsync(id);

        // Assert
        Assert.Equal(id, result.Id);
        Assert.Equal("Carol", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ThrowsNotFoundException()
    {
        // Arrange
        _userRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((User)null!);

        var sut = CreateSut();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => sut.GetByIdAsync(Guid.NewGuid()));
    }

    // ─── DeleteAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ValidId_ReturnsTrue()
    {
        // Arrange
        var id = Guid.NewGuid();
        _userRepositoryMock.Setup(r => r.DeleteAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

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
        _userRepositoryMock
            .Setup(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB error"));

        var sut = CreateSut();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => sut.DeleteAsync(Guid.NewGuid()));
        Assert.Contains("Failed to delete", ex.Message);
    }

    // ─── GetAll ───────────────────────────────────────────────────────────────

    [Fact]
    public void GetAll_NoCursor_ReturnsAllWithinPageSize()
    {
        // Arrange
        var users = new[]
        {
            new User { Id = Guid.NewGuid(), Name = "A" },
            new User { Id = Guid.NewGuid(), Name = "B" },
            new User { Id = Guid.NewGuid(), Name = "C" },
        }.AsQueryable();

        _userRepositoryMock.Setup(r => r.GetAllAsync()).Returns(users);
        var sut = CreateSut();

        // Act
        var result = sut.GetAll(null, 10).ToList();

        // Assert
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void GetAll_PageSize2_ReturnsOnlyTwo()
    {
        // Arrange
        var users = new[]
        {
            new User { Id = Guid.Parse("00000000-0000-0000-0000-000000000001"), Name = "A" },
            new User { Id = Guid.Parse("00000000-0000-0000-0000-000000000002"), Name = "B" },
            new User { Id = Guid.Parse("00000000-0000-0000-0000-000000000003"), Name = "C" },
        }.AsQueryable();

        _userRepositoryMock.Setup(r => r.GetAllAsync()).Returns(users);
        var sut = CreateSut();

        // Act
        var result = sut.GetAll(null, 2).ToList();

        // Assert
        Assert.Equal(2, result.Count);
    }

    // ─── UpdateAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ValidFields_UpdatesUser()
    {
        // Arrange
        var id = Guid.NewGuid();
        var user = new User { Id = id, Name = "Old", Email = "old@x.com", Username = "old_user" };
        var model = new UpdateUserModel { Name = "New", Username = "new_user" };

        _userRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var sut = CreateSut();

        // Act
        await sut.UpdateAsync(id, model);

        // Assert
        _userRepositoryMock.Verify(r => r.UpdateAsync(It.Is<User>(u =>
            u.Name == "New" && u.Username == "new_user"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_UserNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _userRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((User)null!);

        var sut = CreateSut();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => sut.UpdateAsync(Guid.NewGuid(), new UpdateUserModel()));
    }

    [Fact]
    public async Task UpdateAsync_InvalidEmail_ThrowsValidationException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var user = new User { Id = id, Name = "Alice", Email = "alice@x.com" };
        _userRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var model = new UpdateUserModel { Email = "not-an-email" };
        var sut = CreateSut();

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => sut.UpdateAsync(id, model));
    }

    [Fact]
    public async Task UpdateAsync_DuplicateEmail_ThrowsConflictException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var user = new User { Id = id, Name = "Alice", Email = "alice@x.com" };
        var otherUser = new User { Id = Guid.NewGuid(), Email = "other@x.com" }; // different user with same email

        _userRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _userRepositoryMock.Setup(r => r.GetByEmailAsync("other@x.com", It.IsAny<CancellationToken>())).ReturnsAsync(otherUser);

        var model = new UpdateUserModel { Email = "other@x.com" };
        var sut = CreateSut();

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => sut.UpdateAsync(id, model));
    }

    [Fact]
    public async Task UpdateAsync_ShortPassword_ThrowsValidationException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var user = new User { Id = id, Name = "Alice", Email = "a@b.com" };
        _userRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var model = new UpdateUserModel { Password = "P@1" };
        var sut = CreateSut();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidationException>(() => sut.UpdateAsync(id, model));
        Assert.Contains("8 characters", ex.Message);
    }

    [Fact]
    public async Task UpdateAsync_PasswordWithoutSpecialChar_ThrowsValidationException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var user = new User { Id = id, Name = "Alice", Email = "a@b.com" };
        _userRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var model = new UpdateUserModel { Password = "Password1" };
        var sut = CreateSut();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidationException>(() => sut.UpdateAsync(id, model));
        Assert.Contains("special character", ex.Message);
    }
}
