using ECommerce.Domain.Entities;
using ECommerce.Domain.Ports;

public class IncreaseProductStockUseCase : IIncreaseProductStockUseCase
{
  private readonly IProductRepository _productRepository;
  private readonly IUnitOfWork _unitOfWork;

  public IncreaseProductStockUseCase(IProductRepository productRepository, IUnitOfWork unitOfWork)
  {
    _productRepository = productRepository;
    _unitOfWork = unitOfWork;
  }

  public async Task<Product> IncreaseProductStockAsync(Guid productId, int increasingQuantity)
  {
    var product = await _productRepository.GetByIdAsync(productId);
    product!.IncreaseStock(increasingQuantity);
    await _productRepository.UpdateAsync(product);
    await _unitOfWork.SaveChangesAsync();

    return product;
  }
}
