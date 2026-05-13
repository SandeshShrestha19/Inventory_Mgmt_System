using ECommerce.Domain.Ports;
using Microsoft.Extensions.Logging;
using ECommerce.Application.UseCases;
using ECommerce.Applcation.Helpers;

namespace ECommerce.Application.UseCases;

public class RefreshTokenUseCase : IRefreshTokenUseCase
{
  private readonly IRefreshTokenRepository _refreshTokenRepository;
  private readonly IUserRepository _userRepository;
  private readonly ILogger<RefreshTokenUseCase> _logger;
  private readonly JwtTokenGenerator _jwtTokenGenerator;


  public RefreshTokenUseCase(IRefreshTokenRepository refreshTokenRepository, ILogger<RefreshTokenUseCase> logger, IUserRepository userRepository, JwtTokenGenerator jwtTokenGenerator)
  {
    _refreshTokenRepository = refreshTokenRepository;
    _logger = logger;
    _userRepository = userRepository;
    _jwtTokenGenerator = jwtTokenGenerator;
  }

  public async Task<string> ExecuteAsync(string refreshToken)
  {
    try
    {
      var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken) ?? throw new Exception("Token not found!");

      if (storedToken.ExpiresIn < DateTime.UtcNow)
      {
        throw new Exception("Refresh token has expired! Please login again.");
      }

      if (storedToken.IsRevoked)
      {
        throw new Exception("Refresh token has been revoked! Please login again.");
      }

      var user = await _userRepository.GetByIdAsync(storedToken.UserId) ?? throw new Exception("User not found!");

      var newAccessToken = _jwtTokenGenerator.GenerateAccessToken(user);
      return newAccessToken;

    }
    catch (Exception ex)
    {
      _logger.LogInformation(ex, "Failed to retrieve refresh token!");
      throw;
    }
  }
}