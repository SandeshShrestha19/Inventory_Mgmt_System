using ECommerce.Domain.Ports;
using Microsoft.Extensions.Logging;

public class DeleteProductUseCase : IDeleteProductUseCase
{
  private readonly IProductRepository _productRepository;
  private readonly ILogger<AddProductUseCase> _logger;

  public DeleteProductUseCase(IProductRepository productRepository, ILogger<AddProductUseCase> logger)
  {
    _productRepository = productRepository;
    _logger = logger;
  }

  public async Task<bool> DeleteAsync(Guid id)
  {
    try
    {
      await _productRepository.DeleteAsync(id);
      return true;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to delete product");
      throw new Exception($"Failed to delete product: {ex.Message}");
    }

  }
}