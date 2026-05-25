using ECommerce.Domain.Entities;
using ECommerce.Domain.Ports;

public class DecreaseProductStockUseCase : IDecreaseProductStockUseCase
{
  private readonly IProductRepository _productRepository;
  private readonly IUnitOfWork _unitOfWork;

  public DecreaseProductStockUseCase(IProductRepository productRepository, IUnitOfWork unitOfWork)
  {
    _productRepository = productRepository;
    _unitOfWork = unitOfWork;
  }

  public async Task<Product> DecreaseProductStockAsync(Guid productId, int decreasingQuantity)
  {
    var product = await _productRepository.GetByIdAsync(productId);
    product!.DecreaseStock(decreasingQuantity);
    await _productRepository.UpdateAsync(product);
    await _unitOfWork.SaveChangesAsync();

    return product;
  }
}
