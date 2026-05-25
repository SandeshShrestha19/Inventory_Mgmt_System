using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens.Experimental;

namespace ECommerce.Applcation.Helpers;

public class JwtTokenGenerator
{
  private readonly IConfiguration _configuration;

  public JwtTokenGenerator(IConfiguration configuration)
  {
    _configuration = configuration;
  }
  public string GenerateAccessToken(User user)
  {
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
      Subject = new ClaimsIdentity(new[]
      {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), // needed for logout
          new Claim(JwtRegisteredClaimNames.Email, user.Email), //helps to know who is logged in
          new Claim(JwtRegisteredClaimNames.Name, user.Name),
          new Claim(ClaimTypes.Role, user.Role) ,  // this makes the role based authorization work     
          new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
          new Claim("IsActive", user.IsActive.ToString().ToLower()),
          new Claim("IsLoggedIn", user.IsLoggedIn.ToString().ToLower()),
      }),
      Issuer = issuer,
      Audience = audience,
      Expires = tokenExpiryTimeStamp,
      SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)), SecurityAlgorithms.HmacSha512Signature)

    };
    var tokenHandler = new JwtSecurityTokenHandler();
    var securityToken = tokenHandler.CreateToken(tokenDescriptor);
    var token = tokenHandler.WriteToken(securityToken);
    return token;
  }

  public string GenerateTempToken(User user)
  {
    var issuer = _configuration["JwtConfig:Issuer"];
    var audience = _configuration["JwtConfig:Audience"];
    var key = _configuration["JwtConfig:Key"];
    var tokenDescriptor = new SecurityTokenDescriptor
    {
      Subject = new ClaimsIdentity(new[]
      {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim("2fa_pending", "true"),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
      }),
      Issuer = issuer,
      Audience = audience,
      Expires = DateTime.UtcNow.AddMinutes(5),
      SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key!)), SecurityAlgorithms.HmacSha512Signature)
    };
    var tokenHandler = new JwtSecurityTokenHandler();
    var securityToken = tokenHandler.CreateToken(tokenDescriptor);
    var token = tokenHandler.WriteToken(securityToken);
    return token;
  }

  public Guid ValidateTempToken(string tempToken)
  {
    var issuer = _configuration["JwtConfig:Issuer"];
    var audience = _configuration["JwtConfig:Audience"];
    var key = _configuration["JwtConfig:Key"];

    var tokenHandler = new JwtSecurityTokenHandler();
    try
    {
      var principal = tokenHandler.ValidateToken(tempToken,
        new TokenValidationParameters
        {
          ValidateIssuer = true,
          ValidateAudience = true,
          ValidateLifetime = true,
          ValidateIssuerSigningKey = true,
          ValidIssuer = issuer,
          ValidAudience = audience,
          IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key!))
        }, out _);
      var is2FAPending = principal.FindFirst("2fa_pending")?.Value;

      if (is2FAPending != "true")
      {
        throw new UnauthorizedException("Invalid temporary token!");
      }

      var userId = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      return Guid.Parse(userId!);
    }
    catch
    {
      throw new UnauthorizedException("Invalid or expired temporary token!");
    }
  }
}

