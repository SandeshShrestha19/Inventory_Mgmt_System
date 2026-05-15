using ECommerce.Domain.Entities;
using ECommerce.Domain.Ports;
using Microsoft.Extensions.Logging;

public class GetProductQueryUseCase : IGetProductQueryUseCase
{
  private readonly IProductRepository _productRepository;

  public GetProductQueryUseCase(IProductRepository productRepository)
  {
    _productRepository = productRepository;
  }
  public IQueryable<ProductResponseModel> GetAll(Guid? cursorId, int pageSize)
  {
    var products = _productRepository.GetAllAsync();
    if (cursorId.HasValue)
    {
      products = products.Where(x => x.Id > cursorId.Value);
    }
    return products.OrderBy(x => x.Id)
    .Take(pageSize)
    .Select(x => new ProductResponseModel
    {
      Id = x.Id,
      Name = x.Name,
      Price = x.Price,
      Description = x.Description,
      Stock = x.Stock
    });
  }
}