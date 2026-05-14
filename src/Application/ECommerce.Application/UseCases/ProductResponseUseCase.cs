using ECommerce.Domain.Ports;
using Microsoft.Extensions.Logging;

public class ProductResponseUseCase : IProductResponseUseCase
{
  private readonly IProductRepository _productRepository;
  private readonly ILogger<ProductResponseUseCase> _logger;

  public ProductResponseUseCase(IProductRepository productRepository, ILogger<ProductResponseUseCase> logger)
  {
    _productRepository = productRepository;
    _logger = logger;
  }

  public async Task<ProductResponseModel> ExecuteAsync(Guid id)
  {
    try
    {
      var product = await _productRepository.GetByIdAsync(id) ?? throw new Exception("User not found!");
      return ResponseMapper.ToProductResponse(product);
    }
    catch(Exception ex)
    {
      _logger.LogInformation(ex, "Failed to retrieve data!");
      throw;
    }
}
    
}