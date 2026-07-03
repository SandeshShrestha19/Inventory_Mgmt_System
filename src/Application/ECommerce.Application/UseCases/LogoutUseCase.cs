using ECommerce.Domain.Exceptions;
using ECommerce.Domain.Ports;
using Microsoft.Extensions.Logging;

public class LogoutUseCase : ILogoutUseCase
{
  private readonly IUserRepository _userRepository;
  private readonly IRefreshTokenRepository _refreshTokenRepository;
  private readonly IBlacklistedTokenRepository _blacklistedTokenRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly ILogger<LogoutUseCase> _logger;

  public LogoutUseCase(IUserRepository userRepository, ILogger<LogoutUseCase> logger, IUnitOfWork unitOfWork, IRefreshTokenRepository refreshTokenRepository, IBlacklistedTokenRepository blacklistedTokenRepository)
  {
    _userRepository = userRepository;
    _logger = logger;
    _unitOfWork = unitOfWork;
    _refreshTokenRepository = refreshTokenRepository;
    _blacklistedTokenRepository = blacklistedTokenRepository;
  }

  public async Task ExecuteAsync(Guid id, string refreshToken, string jti, DateTime expiresAt)
  {
    try
    {
      var user = await _userRepository.GetByIdAsync(id) ?? throw NotFoundException.User();

      user.IsLoggedIn = false;
      await _userRepository.UpdateAsync(user);

      var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken) ?? throw NotFoundException.RefreshToken();

      storedToken.IsRevoked = true;
      await _refreshTokenRepository.UpdateASync(storedToken);

      await _blacklistedTokenRepository.AddAsync(new BlacklistedToken
      {
        Id = Guid.CreateVersion7(),
        Jti = jti,
        ExpiresAt = expiresAt,
        CreatedAt = DateTime.UtcNow
      });

      await _unitOfWork.SaveChangesAsync();

    }
    catch (Exception ex)
    {
      _logger.LogInformation(ex, "Failed to log out!");
    }
  }
}