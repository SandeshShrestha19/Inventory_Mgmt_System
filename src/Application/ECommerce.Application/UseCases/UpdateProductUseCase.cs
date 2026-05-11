using ECommerce.Domain.Models;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Ports;
using Microsoft.Extensions.Logging;

namespace ECommerce.Application.UseCases;  

public class UpdateProductUseCase : IUpdateProductUseCase
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<UpdateProductUseCase> _logger;

    public UpdateProductUseCase(IProductRepository productRepository, ILogger<UpdateProductUseCase> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<Product> ExecuteAsync(Guid id, UpdateProductModel updateModel)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(id) ?? throw new Exception("Product not found!");

            product.Name = updateModel.Name;
            product.Description = updateModel.Description;
            product.Price = updateModel.Price;
            product.Stock = updateModel.Stock;

            return await _productRepository.UpdateAsync(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update!");
            throw;  
        }
    }
}