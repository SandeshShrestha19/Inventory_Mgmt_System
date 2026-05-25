using Ecommerce.Domain.Models;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Exceptions;
using ECommerce.Domain.Ports;
using Microsoft.Extensions.Logging;

public class RegisterUserUseCase : IRegisterUserUseCase
{
  private readonly IUserRepository _userRepository;
  private readonly ILogger<RegisterUserUseCase> _logger;

  public RegisterUserUseCase(IUserRepository userRepository, ILogger<RegisterUserUseCase> logger)
  {
    _userRepository = userRepository;
    _logger = logger;
  }

  public async Task<User> ExecuteAsync(AddUserModel model)
  {
    try
    {
      if (string.IsNullOrWhiteSpace(model.Name))
      {
        throw new ValidationException("Name can't be empty!");
      }
      if (!model.Email.Contains('@'))
      {
        throw new ValidationException("Email should contain '@'!");
      }
      var existingEmail = await _userRepository.GetByEmail(model.Email);
      if (existingEmail != null)
      {
        throw ConflictException.EmailAlreadyExists();
      }
      var user = new User
      {
        Id = Guid.NewGuid(),
        Name = model.Name,
        Email = model.Email,
        Role = model.Role,
        Password = PasswordHashHandler.HashPassword(model.Password),
        CreatedAt = DateTime.UtcNow,
        IsActive = true,
        IsLoggedIn = false
      };
      return await _userRepository.AddAsync(user);

    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to add user!");
      throw;
    }
  }
}