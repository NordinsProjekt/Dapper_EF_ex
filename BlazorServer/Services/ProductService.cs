using Application.Services.Interfaces;
using Domain.Entities;

namespace BlazorServer.Services;

public class ProductService
{
    private readonly IRepository<Product> _repository;

    public ProductService(IRepository<Product> repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Product?> GetProductByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<Product> CreateProductAsync(string name, string? description, decimal price, int stockQuantity, string? sku = null)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Price = price,
            StockQuantity = stockQuantity,
            SKU = sku,
            CreatedAt = DateTime.UtcNow
        };

        return await _repository.AddAsync(product);
    }

    public async Task UpdateProductAsync(Product product)
    {
        product.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(product);
    }

    public async Task DeleteProductAsync(Guid id)
    {
        await _repository.DeleteByIdAsync(id);
    }
}
