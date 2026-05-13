using ECommerce.Domain.Entities;
using ECommerce.Domain.Ports;
using Microsoft.EntityFrameworkCore;
using ECommerce.Infrastructure.Persistence;

public class RefreshTokenAdapter : IRefreshTokenRepository
{
  private readonly AppDbContext _context;

  public RefreshTokenAdapter(AppDbContext context)
  {
    _context = context;
  }

  public async Task<RefreshToken> AddAsync(RefreshToken refreshToken)
  {
    _context.RefreshTokens.Add(refreshToken);
    await _context.SaveChangesAsync();
    return refreshToken;

  }

  // public async Task<bool> DeleteAsync(Guid id)
  // {
  //   var refreshToken = await _context.RefreshTokens.FindAsync(id);
  //   if (refreshToken != null)
  //   {
  //     _context.Remove(refreshToken);
  //     await _context.SaveChangesAsync();
  //   }
  //   return true;
  // }

  public IQueryable<RefreshToken> GetAll()
  {
    return _context.RefreshTokens.AsQueryable();
  }

  public async Task<RefreshToken?> GetByTokenAsync(string rToken)
  {
    return await _context.RefreshTokens
        .FirstOrDefaultAsync(x => x.Token == rToken);
  }

  public async Task UpdateASync(RefreshToken refreshToken)
  {
    _context.RefreshTokens.Update(refreshToken);
    await _context.SaveChangesAsync();
  }
}