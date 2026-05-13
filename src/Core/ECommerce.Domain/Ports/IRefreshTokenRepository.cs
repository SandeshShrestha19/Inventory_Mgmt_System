using ECommerce.Domain.Entities;

namespace ECommerce.Domain.Ports;

public interface IRefreshTokenRepository
{
  IQueryable<RefreshToken> GetAll();
  Task<RefreshToken?> GetByTokenAsync(string refreshToken);
  Task<RefreshToken> AddAsync(RefreshToken refreshToken);
  Task UpdateASync(RefreshToken refreshToken);
  // Task<bool> DeleteAsync(Guid id);

}