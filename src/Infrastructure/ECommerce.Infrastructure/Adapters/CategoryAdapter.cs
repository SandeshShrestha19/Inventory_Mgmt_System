using ECommerce.Domain.Entities;
using ECommerce.Domain.Ports;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Adapters;

public class CategoryAdapter : ICategoryRepository
{
  private readonly AppDbContext _context;

  public CategoryAdapter(AppDbContext context)
  {
    _context = context;
  }

  public IQueryable<Category> GetAllAsync()
  {
    return _context.Categories.AsQueryable();
  }

  public async Task<Category?> GetByIdAsync(Guid id)
  {
    return await _context.Categories.FindAsync(id);
  }

  public async Task<Category> AddAsync(Category category)
  {
    _context.Categories.Add(category);
    await _context.SaveChangesAsync();
    return category;
  }

  public async Task UpdateAsync(Category category)
  {
    _context.Categories.Update(category);
    await _context.SaveChangesAsync();
  }


  public async Task<bool> DeleteAsync(Guid id)
  {
    var category = await _context.Categories.FindAsync(id);
    if (category != null)
    {
      _context.Categories.Remove(category);
      await _context.SaveChangesAsync();
    }
    return true;
  }
}