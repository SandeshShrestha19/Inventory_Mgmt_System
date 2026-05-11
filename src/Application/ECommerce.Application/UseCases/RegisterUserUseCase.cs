using Ecommerce.Domain.Models;
using ECommerce.Domain.Entities;
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
        throw new Exception("Name can't be empty!");
      }
      if (!model.Email.Contains('@'))
      {
        throw new Exception("Email should contain '@'!");
      }
      var user = new User
      {
        Id = Guid.NewGuid(),
        Name = model.Name,
        Email = model.Email,
      };
      if (!string.IsNullOrWhiteSpace(model.Password))
      {
        user.SetPassword(BCrypt.Net.BCrypt.HashPassword(model.Password));
      }
      return await _userRepository.AddAsync(user);
      
    }
    catch(Exception ex)
    {
      _logger.LogInformation(ex, "Failed to add user!");
      throw;
    }
  }
}