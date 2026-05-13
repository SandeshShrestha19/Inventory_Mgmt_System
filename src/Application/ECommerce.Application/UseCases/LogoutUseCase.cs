using ECommerce.Domain.Ports;
using Microsoft.Extensions.Logging;

public class LogoutUseCase: ILogoutUseCase
{
  private readonly IUserRepository _userRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly ILogger<LogoutUseCase> _logger;

  public LogoutUseCase(IUserRepository userRepository, ILogger<LogoutUseCase> logger, IUnitOfWork unitOfWork)
  {
    _userRepository = userRepository;
    _logger = logger;
    _unitOfWork = unitOfWork;
  }

  public async Task ExecuteAsync(Guid id)
  {
    try
    {
      var user = await _userRepository.GetByIdAsync(id) ?? throw new Exception("User not found!");

      user.IsLoggedIn = false;
      await _userRepository.UpdateAsync(user);
      await _unitOfWork.SaveChangesAsync();

    }catch(Exception ex)
    {
      _logger.LogInformation(ex, "Failed to log out!");
    }
  }
}