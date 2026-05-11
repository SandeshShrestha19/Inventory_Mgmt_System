using ECommerce.Domain.Models;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Ports;
using Microsoft.Extensions.Logging;

public class AddProductUseCase : IAddProductUseCase
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<AddProductUseCase> _logger;

    public AddProductUseCase(IProductRepository productRepository, ILogger<AddProductUseCase> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<Product> ExecuteAsync(AddProductModel model)
    {
      try
      {
        if (string.IsNullOrWhiteSpace(model.Name))
              throw new Exception("Product name is required");

          if (model.Price <= 0)
              throw new Exception("Price must be greater than 0");

          if (model.Stock < 0)
              throw new Exception("Stock cannot be negative");
          var product = new Product
          {
              Id = Guid.NewGuid(),
              Name = model.Name,
              Description = model.Description,
              Price = model.Price,
              Stock = model.Stock
          };

          return await _productRepository.AddAsync(product);
          
      }
      catch (Exception ex)
      {
          _logger.LogError(ex, "Failed to add product");  
          throw new Exception($"Failed to add product: {ex.Message}");
      }
        
    }
}