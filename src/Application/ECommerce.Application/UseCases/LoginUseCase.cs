using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ECommerce.Domain.Models;
using ECommerce.Domain.Ports;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace ECommerce.Application.UseCases;

public class LoginUseCase : ILoginUseCase
{
  private readonly IUserRepository _userRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IConfiguration _configuration;
  private readonly ILogger<LoginUseCase> _logger;

  public LoginUseCase(IUserRepository userRepository, IUnitOfWork unitOfWork, IConfiguration configuration, ILogger<LoginUseCase> logger)
  {
    _userRepository = userRepository;
    _configuration = configuration;
    _logger = logger;
    _unitOfWork = unitOfWork;
  }

  public async Task<string> ExecuteAsync(LoginModel model)
  {
    try
    {
      if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
      {
        throw new Exception("Email or Password field is emplty!");
      }

      //verify user by email
      var user = _userRepository.GetAllAsync().FirstOrDefault(u => u.Email == model.Email) ?? throw new Exception("Invalid email or Password!");

      var isValid = PasswordHashHandler.VerifyPassword(model.Password, user.Password);
      if (!isValid)
      {
        throw new Exception("Invalid email or Password!");
      }

      var issuer = _configuration["JwtConfig:Issuer"]
          ?? throw new Exception("JWT Issuer not configured!");

      var audience = _configuration["JwtConfig:Audience"]
          ?? throw new Exception("JWT Audience not configured!");

      var key = _configuration["JwtConfig:Key"]
          ?? throw new Exception("JWT Key not configured!");

      var tokenValidityMins = int.Parse(_configuration["JwtConfig:TokenValidityMins"] ?? "60");

      var tokenExpiryTimeStamp = DateTime.UtcNow.AddMinutes(tokenValidityMins);

      var tokenDescriptor = new SecurityTokenDescriptor
      {
        Subject = new ClaimsIdentity(new[] //payload
        {
          new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), // needed for logout
          new Claim(JwtRegisteredClaimNames.Email, model.Email), //helps to know who is logged in
          new Claim(JwtRegisteredClaimNames.Name, user.Name),  
          new Claim(ClaimTypes.Role, user.Role) ,  // this makes the role based authorization work     
          new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        }),
        Expires = tokenExpiryTimeStamp,
        Issuer = issuer,
        Audience = audience,
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)), SecurityAlgorithms.HmacSha512Signature) //header
      };

      user.IsLoggedIn = true;
      await _userRepository.UpdateAsync(user);
      await _unitOfWork.SaveChangesAsync();

      var tokenHandler = new JwtSecurityTokenHandler();
      var securityToken = tokenHandler.CreateToken(tokenDescriptor); 
      return tokenHandler.WriteToken(securityToken);

    }
    catch (Exception ex)
    {
      _logger.LogInformation(ex, "Failed to login!");
      throw;
    }

  }
}