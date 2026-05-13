using ECommerce.Domain.Ports;
using Microsoft.Extensions.Logging;

public class LogoutUseCase: ILogoutUseCase
{
  private readonly IUserRepository _userRepository;
  private readonly IRefreshTokenRepository _refreshTokenRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly ILogger<LogoutUseCase> _logger;

  public LogoutUseCase(IUserRepository userRepository, ILogger<LogoutUseCase> logger, IUnitOfWork unitOfWork, IRefreshTokenRepository refreshTokenRepository)
  {
    _userRepository = userRepository;
    _logger = logger;
    _unitOfWork = unitOfWork;
    _refreshTokenRepository = refreshTokenRepository;
  }

  public async Task ExecuteAsync(Guid id, string refreshToken)
  {
    try
    {
      var user = await _userRepository.GetByIdAsync(id) ?? throw new Exception("User not found!");

      user.IsLoggedIn = false;
      await _userRepository.UpdateAsync(user);

      var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken) ?? throw new Exception("Refresh token not found!");

      storedToken.IsRevoked = true;
      await _refreshTokenRepository.UpdateASync(storedToken);
      await _unitOfWork.SaveChangesAsync();

    }catch(Exception ex)
    {
      _logger.LogInformation(ex, "Failed to log out!");
    }
  }
}