using Application.Services.Interfaces;
using Domain.Entities;

namespace Application.Services;

/// <summary>
/// Service for managing Product entities.
/// Provides business logic layer between presentation and data access.
/// </summary>
public class ProductService
{
    private readonly IRepository<Product> _repository;

    public ProductService(IRepository<Product> repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Retrieves all products from the repository.
    /// </summary>
    public async Task<IEnumerable<Product>> GetAllProductsAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.GetAllAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves a product by its unique identifier.
    /// </summary>
    public async Task<Product?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _repository.GetByIdAsync(id, cancellationToken);
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    public async Task<Product> CreateProductAsync(
        string name, 
        string? description, 
        decimal price, 
        int stockQuantity, 
        string? sku = null,
        CancellationToken cancellationToken = default)
    {
        // Business logic: Validate SKU is unique if provided
        if (!string.IsNullOrEmpty(sku))
        {
            var existingProduct = await _repository.FirstOrDefaultAsync(p => p.SKU == sku, cancellationToken);
            if (existingProduct != null)
            {
                throw new InvalidOperationException($"A product with SKU '{sku}' already exists.");
            }
        }

        // Business logic: Validate price is positive
        if (price < 0)
        {
            throw new ArgumentException("Price cannot be negative.", nameof(price));
        }

        // Business logic: Validate stock quantity is non-negative
        if (stockQuantity < 0)
        {
            throw new ArgumentException("Stock quantity cannot be negative.", nameof(stockQuantity));
        }

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

        return await _repository.AddAsync(product, cancellationToken);
    }

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    public async Task UpdateProductAsync(Product product, CancellationToken cancellationToken = default)
    {
        // Business logic: Ensure product exists
        var exists = await _repository.ExistsAsync(product.Id, cancellationToken);
        if (!exists)
        {
            throw new InvalidOperationException($"Product with ID '{product.Id}' does not exist.");
        }

        // Business logic: Validate SKU is unique if provided (excluding current product)
        if (!string.IsNullOrEmpty(product.SKU))
        {
            var existingProduct = await _repository.FirstOrDefaultAsync(
                p => p.SKU == product.SKU && p.Id != product.Id, 
                cancellationToken);
            
            if (existingProduct != null)
            {
                throw new InvalidOperationException($"A product with SKU '{product.SKU}' already exists.");
            }
        }

        // Business logic: Validate price is positive
        if (product.Price < 0)
        {
            throw new ArgumentException("Price cannot be negative.", nameof(product.Price));
        }

        // Business logic: Validate stock quantity is non-negative
        if (product.StockQuantity < 0)
        {
            throw new ArgumentException("Stock quantity cannot be negative.", nameof(product.StockQuantity));
        }

        product.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(product, cancellationToken);
    }

    /// <summary>
    /// Deletes a product by its unique identifier.
    /// </summary>
    public async Task DeleteProductAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _repository.DeleteByIdAsync(id, cancellationToken);
    }

    /// <summary>
    /// Checks if a product exists by its unique identifier.
    /// </summary>
    public async Task<bool> ProductExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _repository.ExistsAsync(id, cancellationToken);
    }

    /// <summary>
    /// Gets the total count of products.
    /// </summary>
    public async Task<int> GetProductCountAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.CountAsync(cancellationToken);
    }

    /// <summary>
    /// Searches for products by name, description, or SKU.
    /// </summary>
    public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var normalizedSearch = searchTerm.ToLower();
        return await _repository.FindAsync(
            p => p.Name.ToLower().Contains(normalizedSearch) ||
                 (p.Description != null && p.Description.ToLower().Contains(normalizedSearch)) ||
                 (p.SKU != null && p.SKU.ToLower().Contains(normalizedSearch)),
            cancellationToken);
    }

    /// <summary>
    /// Gets products with low stock (less than specified threshold).
    /// </summary>
    public async Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold = 10, CancellationToken cancellationToken = default)
    {
        return await _repository.FindAsync(p => p.StockQuantity < threshold, cancellationToken);
    }

    /// <summary>
    /// Gets products within a price range.
    /// </summary>
    public async Task<IEnumerable<Product>> GetProductsByPriceRangeAsync(
        decimal minPrice, 
        decimal maxPrice, 
        CancellationToken cancellationToken = default)
    {
        return await _repository.FindAsync(
            p => p.Price >= minPrice && p.Price <= maxPrice, 
            cancellationToken);
    }
}
