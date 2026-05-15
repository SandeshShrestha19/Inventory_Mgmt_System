using ECommerce.Domain.Entities;
using ECommerce.Domain.Ports;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Adapters;

public class ProductAdapter : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductAdapter(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<Product> GetAllAsync()
    {
       return _context.Products.AsQueryable(); 
    } 

    public async Task<Product?> GetByIdAsync(Guid id) =>
        await _context.Products.FindAsync(id);

    public async Task<Product> AddAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
        return true;
    }
}