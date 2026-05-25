using ECommerce.Domain.Exceptions;
using ECommerce.Domain.Ports;
using Microsoft.Extensions.Logging;

public class UserResponseUseCase : IUserResponseUseCase
{
  private readonly IUserRepository _userRepository;
  private readonly ILogger<UserResponseUseCase> _logger;

  public UserResponseUseCase(IUserRepository userRepository, ILogger<UserResponseUseCase> logger)
  {
    _userRepository = userRepository;
    _logger = logger;
  }

  public async Task<UserResponseModel> GetByIdAsync(Guid id)
  {
    try
    {
      var user = await _userRepository.GetByIdAsync(id) ?? throw NotFoundException.User();
      return ResponseMapper.ToUserResponse(user);
    }
    catch(Exception ex)
    {
      _logger.LogInformation(ex, "Failed to retrieve data!");
      throw;
    }
}
    
}