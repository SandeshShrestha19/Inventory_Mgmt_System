using ECommerce.Domain.Ports;

namespace ECommerce.Application.UseCase;

public class Verify2FAUseCase : IVerify2FAUseCase
{
  private readonly IUserRepository _userRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly TwoFactorService _twoFactorService;

  public Verify2FAUseCase(IUserRepository userRepository, IUnitOfWork unitOfWork, TwoFactorService twoFactorService)
  {
    _userRepository = userRepository;
    _unitOfWork = unitOfWork;
    _twoFactorService = twoFactorService;
  }

  public async Task<bool> ExecuteAsync(Guid userId, string code)
  {
    var user = await _userRepository.GetByIdAsync(userId) ?? throw new Exception("User not found!");

    if (string.IsNullOrEmpty(user.TwoFactorSecret))
    {
      throw new Exception("2FA not set up!");
    }
    var isValid = _twoFactorService.VerifyCode(user.TwoFactorSecret, code);
    if (!isValid)
    {
      throw new Exception("Invalid 2FA code!");
    }
    user.TwoFactorEnabled = true;
    await _userRepository.UpdateAsync(user);
    await _unitOfWork.SaveChangesAsync();
    return true;
  }
}