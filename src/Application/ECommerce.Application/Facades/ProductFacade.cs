using ECommerce.Domain.Entities;
using ECommerce.Domain.Exceptions;
using ECommerce.Domain.Models;
using ECommerce.Domain.Ports;
using Microsoft.Extensions.Logging;

public class ProductFacade : IProductFacade
{
  private readonly IProductRepository _productRepository;
  private readonly ILogger<ProductFacade> _logger;
  private readonly IUnitOfWork _unitOfWork;

  public ProductFacade(IProductRepository productRepository, ILogger<ProductFacade> logger, IUnitOfWork unitOfWork)
  {
    _productRepository = productRepository;
    _logger = logger;
    _unitOfWork = unitOfWork;
  }

  public async Task<Product> AddAsync(AddProductModel model)
  {
    try
    {
      if (string.IsNullOrWhiteSpace(model.Name))
      {
        throw new ValidationException("Product name is required");
      }

      if (model.Price <= 0)
      {
        throw new ValidationException("Price must be greater than 0");
      }

      if (model.Stock < 0)
      {
        throw new ValidationException("Stock cannot be negative");
      }

      var product = new Product
      {
        Id = Guid.NewGuid(),
        Name = model.Name,
        Description = model.Description,
        Price = model.Price,
        Stock = model.Stock,
        CategoryId = model.CategoryId
      };

      return await _productRepository.AddAsync(product);

    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to add product");
      throw new Exception($"Failed to add product: {ex.Message}");
    }

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
      Stock = x.Stock,
      CategoryId = x.CategoryId
    });
  }

  public async Task<ProductResponseModel> GetByIdAsync(Guid id)
  {
    try
    {
      var product = await _productRepository.GetByIdAsync(id) ?? throw NotFoundException.Product();
      return ResponseMapper.ToProductResponse(product);
    }
    catch (Exception ex)
    {
      _logger.LogInformation(ex, "Failed to retrieve data!");
      throw;
    }
  }

  public async Task UpdateAsync(Guid id, UpdateProductModel updateModel)
  {
    try
    {
      var product = await _productRepository.GetByIdAsync(id) ?? throw NotFoundException.Product();

      product.Name = updateModel.Name ?? product.Name;
      product.Description = updateModel.Description ?? product.Description;
      if (updateModel.Price.HasValue)
      {
        product.Price = updateModel.Price.Value;
      }
      product.CategoryId = updateModel.CategoryId;
      product.Stock = updateModel.Stock;

      await _productRepository.UpdateAsync(product);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to update!");
      throw;
    }
  }

  public async Task IncreaseStockAsync(Guid productId, int increasingQuantity)
  {
    var product = await _productRepository.GetByIdAsync(productId);
    product!.IncreaseStock(increasingQuantity);
    await _productRepository.UpdateAsync(product);
    await _unitOfWork.SaveChangesAsync();
  }

  public async Task DecreaseStockAsync(Guid productId, int decreasingQuantity)
  {
    var product = await _productRepository.GetByIdAsync(productId);
    product!.DecreaseStock(decreasingQuantity);
    await _productRepository.UpdateAsync(product);
    await _unitOfWork.SaveChangesAsync();
  }

}