using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using ECommerce.Applcation.Helpers;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Exceptions;
using ECommerce.Domain.Models;
using ECommerce.Domain.Ports;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ECommerce.Application.UseCases;

public class LoginUseCase : ILoginUseCase
{
  private readonly IUserRepository _userRepository;
  private readonly JwtTokenGenerator _jwtTokenGenerator;
  private readonly IRefreshTokenRepository _refreshTokenRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly ILogger<LoginUseCase> _logger;

  public LoginUseCase(IUserRepository userRepository, IUnitOfWork unitOfWork, ILogger<LoginUseCase> logger, IRefreshTokenRepository refreshTokenRepository, JwtTokenGenerator jwtTokenGenerator)
  {
    _userRepository = userRepository;
    _logger = logger;
    _unitOfWork = unitOfWork;
    _refreshTokenRepository = refreshTokenRepository;
    _jwtTokenGenerator = jwtTokenGenerator;
  }

  public async Task<LoginResponseModel> ExecuteAsync(LoginModel model)
  {
    try
    {
      if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
      {
        throw new ValidationException("Email or Password field is empty!");
      }

      //verify user by email
      var user = _userRepository.GetAllAsync().FirstOrDefault(u => u.Email == model.Email) ?? throw new UnauthorizedException();

      var isValid = PasswordHashHandler.VerifyPassword(model.Password, user.Password);
      if (!isValid)
      {
        throw new UnauthorizedException();
      }
      if (!user.IsActive)
      {
        throw BusinessException.AccountDisabled();
      }

      var tempToken = _jwtTokenGenerator.GenerateTempToken(user);
      if (user.TwoFactorEnabled)
      {
        return new LoginResponseModel
        {
          RequiresTwoFactor = true,
          Email = model.Email,
          TempToken = tempToken
        };
      }

      var token = _jwtTokenGenerator.GenerateAccessToken(user);

      user.IsLoggedIn = true;

      var refreshToken = new RefreshToken
      {
        Id = Guid.NewGuid(),
        Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
        UserId = user.Id,
        ExpiresIn = DateTime.UtcNow.AddDays(7),
        IsRevoked = false
      };

      await _userRepository.UpdateAsync(user);
      await _refreshTokenRepository.AddAsync(refreshToken);
      await _unitOfWork.SaveChangesAsync();

      return new LoginResponseModel
      {
        RequiresTwoFactor = false,
        Email = model.Email,
        ExpiresIn = 7,
        AccessToken = token,
        Message = "Login Successful!",
        RefreshToken = refreshToken.Token
      };

    }
    catch (Exception ex)
    {
      _logger.LogInformation(ex, "Failed to login!");
      throw;
    }

  }
}