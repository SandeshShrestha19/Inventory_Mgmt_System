using ECommerce.Domain.Entities;
using ECommerce.Domain.Models;
using ECommerce.Domain.Ports;

public class ProductFacade : IProductFacade
{
  private readonly IGetProductQueryUseCase _getProductUseCase;
  private readonly IProductResponseUseCase _getProductByIdUseCase;
  private readonly IAddProductUseCase _addProductUseCase;
  private readonly IUpdateProductUseCase _updateProductUseCase;
  private readonly IDeleteProductUseCase _deleteProductUseCase;
  public ProductFacade(IGetProductQueryUseCase getProductUseCase, IProductResponseUseCase getProductByIdUseCase, IAddProductUseCase addProductUseCase, IUpdateProductUseCase udpateProductUseCase, IDeleteProductUseCase deleteProductUseCase)
  {
    _getProductUseCase = getProductUseCase;
    _getProductByIdUseCase = getProductByIdUseCase;
    _addProductUseCase = addProductUseCase;
    _updateProductUseCase = udpateProductUseCase;
    _deleteProductUseCase = deleteProductUseCase;
  }

  public async Task<Product> AddAsync(AddProductModel addModel)
  {
    return await _addProductUseCase.ExecuteAsync(addModel);
  }

  public async Task<bool> DeleteAsync(Guid id)
  {
    return await _deleteProductUseCase.DeleteAsync(id);
  }

  public IQueryable<ProductResponseModel> GetAll(Guid? cursorId, int pageSize)
  {
    return _getProductUseCase.GetAll(cursorId, pageSize);
  }

  public async Task<ProductResponseModel> GetByIdAsync(Guid id)
  {
    return await _getProductByIdUseCase.GetByIdAsync(id);
  }

  public async Task UpdateAsync(Guid id, UpdateProductModel updateModel)
  {
    await _updateProductUseCase.ExecuteAsync(id, updateModel);
  }
}