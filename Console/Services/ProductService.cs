using Application.Services;

namespace Presentation.KioskViewer.Services;

/// <summary>
/// Presentation layer wrapper for ProductService.
/// Delegates to Application.Services.ProductService.
/// </summary>
public class ProductServiceWrapper
{
    private readonly ProductService _productService;

    public ProductServiceWrapper(ProductService productService)
    {
        _productService = productService;
    }

    public async Task<IEnumerable<Domain.Entities.Product>> GetAllProductsAsync()
    {
        return await _productService.GetAllProductsAsync();
    }

    public async Task<Domain.Entities.Product?> GetProductByIdAsync(Guid id)
    {
        return await _productService.GetProductByIdAsync(id);
    }

    public async Task<Domain.Entities.Product> CreateProductAsync(string name, string? description, decimal price, int stockQuantity, string? sku = null)
    {
        return await _productService.CreateProductAsync(name, description, price, stockQuantity, sku);
    }

    public async Task UpdateProductAsync(Domain.Entities.Product product)
    {
        await _productService.UpdateProductAsync(product);
    }

    public async Task DeleteProductAsync(Guid id)
    {
        await _productService.DeleteProductAsync(id);
    }
}
