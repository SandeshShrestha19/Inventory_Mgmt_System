using ECommerce.Domain.Entities;
using ECommerce.Domain.Ports;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class UserAdapter : IUserRepository
{
  private readonly AppDbContext _dbContext;
  public UserAdapter(AppDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<User> AddAsync(User user)
  {
    _dbContext.Users.Add(user);
    await _dbContext.SaveChangesAsync();
    return user;
  }

  public async Task<bool> DeleteAsync(Guid id)
  {
    var user = await _dbContext.Users.FindAsync(id);
    if (user != null)
    {
      _dbContext.Users.Remove(user);
      await _dbContext.SaveChangesAsync();
    }
    return true;
  }

  public IQueryable<User> GetAllAsync()
  {
    return _dbContext.Users
          .Include(u => u.Orders)
          .ThenInclude(o => o.OrderItems)
          .AsQueryable();
  }

  public async Task<User?> GetByIdAsync(Guid id)
  {
    return await _dbContext.Users
        .Include(u => u.Orders)
        .ThenInclude(o => o.OrderItems)
        .FirstOrDefaultAsync(u => u.Id == id);
  }

  public async Task UpdateAsync(User user)
  {
    _dbContext.Users.Update(user);
    await _dbContext.SaveChangesAsync();
  }

}