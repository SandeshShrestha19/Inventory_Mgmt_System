using ECommerce.Domain.Ports;
using Microsoft.Extensions.Logging;

public class BlacklistedTokenUseCase : IBlacklistedTokenUseCase
{
  private readonly IBlacklistedTokenRepository _blacklistedTokenRepository;
  private readonly ILogger<BlacklistedTokenUseCase> _logger;

  public BlacklistedTokenUseCase(IBlacklistedTokenRepository blacklistedTokenRepository, ILogger<BlacklistedTokenUseCase> logger){
    _blacklistedTokenRepository = blacklistedTokenRepository;
    _logger = logger;
  }

  public async Task ExecuteAsync(AddBlacklistedTokenModel addModel)
  {
    try
    {
      var jtiToken = new BlacklistedToken
      {
        Jti = addModel.Jti,
        ExpiresAt = addModel.ExpiresAt
      };
      await _blacklistedTokenRepository.AddAsync(jtiToken);
    }
    catch(Exception ex)
    {
      _logger.LogError(ex, "Failed to create jti token!");
      throw;
    }
  }
}