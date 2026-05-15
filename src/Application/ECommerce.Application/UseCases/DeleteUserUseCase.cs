using ECommerce.Domain.Ports;
using Microsoft.Extensions.Logging;

public class DeleteUserUseCase : IDeleteUserUseCase
{
  private readonly IUserRepository _userRepository;
  private readonly ILogger<DeleteUserUseCase> _logger;

  public DeleteUserUseCase(IUserRepository userRepository, ILogger<DeleteUserUseCase> logger)
  {
    _userRepository = userRepository;
    _logger = logger;
  }

  public async Task<bool> DeleteAsync(Guid id)
  {
    try
    {
      await _userRepository.DeleteAsync(id);
      return true;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to delete product");
      throw new Exception($"Failed to delete product: {ex.Message}");
    }

  }
}