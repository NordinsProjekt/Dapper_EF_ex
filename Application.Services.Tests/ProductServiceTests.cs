using Application.Services;
using Application.Services.Interfaces;
using Domain.Entities;
using NSubstitute;
using System.Linq.Expressions;

namespace Application.Services.Tests;

/// <summary>
/// Unit tests for ProductService business logic.
/// These tests verify that business rules are properly enforced.
/// </summary>
public class ProductServiceTests
{
    private readonly IRepository<Product> _mockRepository;
    private readonly ProductService _sut;

    public ProductServiceTests()
    {
        _mockRepository = Substitute.For<IRepository<Product>>();
        _sut = new ProductService(_mockRepository);
    }

    #region GetAllProductsAsync Tests

    [Fact]
    public async Task GetAllProductsAsync_ReturnsAllProducts()
    {
        // Arrange
        var expectedProducts = new List<Product>
        {
            new() { Id = Guid.NewGuid(), Name = "Product 1", Price = 10.99m, StockQuantity = 100 },
            new() { Id = Guid.NewGuid(), Name = "Product 2", Price = 20.99m, StockQuantity = 50 }
        };
        _mockRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(expectedProducts);

        // Act
        var result = await _sut.GetAllProductsAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(result, p => p.Name == "Product 1");
        Assert.Contains(result, p => p.Name == "Product 2");
    }

    #endregion

    #region GetProductByIdAsync Tests

    [Fact]
    public async Task GetProductByIdAsync_WithValidId_ReturnsProduct()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var expectedProduct = new Product
        {
            Id = productId,
            Name = "Test Product",
            Price = 29.99m,
            StockQuantity = 10
        };
        _mockRepository.GetByIdAsync(productId, Arg.Any<CancellationToken>()).Returns(expectedProduct);

        // Act
        var result = await _sut.GetProductByIdAsync(productId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(productId, result.Id);
        Assert.Equal("Test Product", result.Name);
    }

    #endregion

    #region CreateProductAsync Tests

    [Fact]
    public async Task CreateProductAsync_WithValidData_CreatesProduct()
    {
        // Arrange
        _mockRepository.FirstOrDefaultAsync(Arg.Any<Expression<Func<Product, bool>>>(), Arg.Any<CancellationToken>())
            .Returns((Product?)null);
        _mockRepository.AddAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Product>());

        // Act
        var result = await _sut.CreateProductAsync("Widget", "A useful widget", 19.99m, 50, "WDG-001");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Widget", result.Name);
        Assert.Equal(19.99m, result.Price);
        Assert.Equal(50, result.StockQuantity);
        Assert.Equal("WDG-001", result.SKU);
        Assert.NotEqual(Guid.Empty, result.Id);
        await _mockRepository.Received(1).AddAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateProductAsync_WithDuplicateSKU_ThrowsInvalidOperationException()
    {
        // Arrange
        var existingProduct = new Product { SKU = "WDG-001" };
        _mockRepository.FirstOrDefaultAsync(Arg.Any<Expression<Func<Product, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(existingProduct);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.CreateProductAsync("Widget", "Description", 19.99m, 50, "WDG-001"));

        Assert.Contains("already exists", exception.Message);
        Assert.Contains("WDG-001", exception.Message);
        await _mockRepository.DidNotReceive().AddAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateProductAsync_WithNegativePrice_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.CreateProductAsync("Widget", "Description", -10m, 50, "WDG-001"));

        Assert.Contains("Price cannot be negative", exception.Message);
        Assert.Equal("price", exception.ParamName);
    }

    [Fact]
    public async Task CreateProductAsync_WithNegativeStock_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.CreateProductAsync("Widget", "Description", 19.99m, -5, "WDG-001"));

        Assert.Contains("Stock quantity cannot be negative", exception.Message);
        Assert.Equal("stockQuantity", exception.ParamName);
    }

    [Fact]
    public async Task CreateProductAsync_WithoutSKU_CreatesProductSuccessfully()
    {
        // Arrange
        _mockRepository.AddAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Product>());

        // Act
        var result = await _sut.CreateProductAsync("Widget", "Description", 19.99m, 50);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.SKU);
    }

    [Fact]
    public async Task CreateProductAsync_WithoutDescription_CreatesProductSuccessfully()
    {
        // Arrange
        _mockRepository.AddAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Product>());

        // Act
        var result = await _sut.CreateProductAsync("Widget", null, 19.99m, 50);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Description);
    }

    #endregion

    #region UpdateProductAsync Tests

    [Fact]
    public async Task UpdateProductAsync_WithValidProduct_UpdatesSuccessfully()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            Name = "Updated Widget",
            Price = 29.99m,
            StockQuantity = 100,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _mockRepository.ExistsAsync(productId, Arg.Any<CancellationToken>()).Returns(true);
        _mockRepository.FirstOrDefaultAsync(Arg.Any<Expression<Func<Product, bool>>>(), Arg.Any<CancellationToken>())
            .Returns((Product?)null);

        // Act
        await _sut.UpdateProductAsync(product);

        // Assert
        Assert.NotNull(product.UpdatedAt);
        Assert.True(product.UpdatedAt <= DateTime.UtcNow);
        await _mockRepository.Received(1).UpdateAsync(product, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateProductAsync_WithNonExistentProduct_ThrowsInvalidOperationException()
    {
        // Arrange
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Widget",
            Price = 19.99m,
            StockQuantity = 50
        };

        _mockRepository.ExistsAsync(product.Id, Arg.Any<CancellationToken>()).Returns(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.UpdateProductAsync(product));

        Assert.Contains("does not exist", exception.Message);
    }

    [Fact]
    public async Task UpdateProductAsync_WithNegativePrice_ThrowsArgumentException()
    {
        // Arrange
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Widget",
            Price = -10m,
            StockQuantity = 50
        };

        _mockRepository.ExistsAsync(product.Id, Arg.Any<CancellationToken>()).Returns(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.UpdateProductAsync(product));

        Assert.Contains("Price cannot be negative", exception.Message);
    }

    [Fact]
    public async Task UpdateProductAsync_WithNegativeStock_ThrowsArgumentException()
    {
        // Arrange
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Widget",
            Price = 19.99m,
            StockQuantity = -5
        };

        _mockRepository.ExistsAsync(product.Id, Arg.Any<CancellationToken>()).Returns(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.UpdateProductAsync(product));

        Assert.Contains("Stock quantity cannot be negative", exception.Message);
    }

    [Fact]
    public async Task UpdateProductAsync_WithDuplicateSKU_ThrowsInvalidOperationException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            Name = "Widget",
            Price = 19.99m,
            StockQuantity = 50,
            SKU = "WDG-001"
        };

        var otherProduct = new Product
        {
            Id = Guid.NewGuid(),
            SKU = "WDG-001"
        };

        _mockRepository.ExistsAsync(productId, Arg.Any<CancellationToken>()).Returns(true);
        _mockRepository.FirstOrDefaultAsync(Arg.Any<Expression<Func<Product, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(otherProduct);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.UpdateProductAsync(product));

        Assert.Contains("already exists", exception.Message);
    }

    #endregion

    #region DeleteProductAsync Tests

    [Fact]
    public async Task DeleteProductAsync_WithValidId_DeletesProduct()
    {
        // Arrange
        var productId = Guid.NewGuid();

        // Act
        await _sut.DeleteProductAsync(productId);

        // Assert
        await _mockRepository.Received(1).DeleteByIdAsync(productId, Arg.Any<CancellationToken>());
    }

    #endregion

    #region ProductExistsAsync Tests

    [Fact]
    public async Task ProductExistsAsync_WhenProductExists_ReturnsTrue()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _mockRepository.ExistsAsync(productId, Arg.Any<CancellationToken>()).Returns(true);

        // Act
        var result = await _sut.ProductExistsAsync(productId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ProductExistsAsync_WhenProductDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _mockRepository.ExistsAsync(productId, Arg.Any<CancellationToken>()).Returns(false);

        // Act
        var result = await _sut.ProductExistsAsync(productId);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region GetProductCountAsync Tests

    [Fact]
    public async Task GetProductCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        _mockRepository.CountAsync(Arg.Any<CancellationToken>()).Returns(10);

        // Act
        var result = await _sut.GetProductCountAsync();

        // Assert
        Assert.Equal(10, result);
    }

    #endregion

    #region SearchProductsAsync Tests

    [Fact]
    public async Task SearchProductsAsync_FindsProductsByName()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Name = "Widget Pro", Description = "Advanced widget", Price = 29.99m, StockQuantity = 50 },
            new() { Name = "Widget Basic", Description = "Simple widget", Price = 19.99m, StockQuantity = 100 }
        };

        _mockRepository.FindAsync(Arg.Any<Expression<Func<Product, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(products);

        // Act
        var result = await _sut.SearchProductsAsync("widget");

        // Assert
        Assert.Equal(2, result.Count());
    }

    #endregion

    #region GetLowStockProductsAsync Tests

    [Fact]
    public async Task GetLowStockProductsAsync_WithDefaultThreshold_ReturnsProductsBelowTen()
    {
        // Arrange
        var lowStockProducts = new List<Product>
        {
            new() { Name = "Low Stock Item", StockQuantity = 5 },
            new() { Name = "Almost Out", StockQuantity = 2 }
        };

        _mockRepository.FindAsync(Arg.Any<Expression<Func<Product, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(lowStockProducts);

        // Act
        var result = await _sut.GetLowStockProductsAsync();

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetLowStockProductsAsync_WithCustomThreshold_ReturnsProductsBelowThreshold()
    {
        // Arrange
        var lowStockProducts = new List<Product>
        {
            new() { Name = "Low Stock Item", StockQuantity = 15 }
        };

        _mockRepository.FindAsync(Arg.Any<Expression<Func<Product, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(lowStockProducts);

        // Act
        var result = await _sut.GetLowStockProductsAsync(20);

        // Assert
        Assert.Single(result);
    }

    #endregion

    #region GetProductsByPriceRangeAsync Tests

    [Fact]
    public async Task GetProductsByPriceRangeAsync_ReturnsProductsInRange()
    {
        // Arrange
        var productsInRange = new List<Product>
        {
            new() { Name = "Mid-range Product 1", Price = 25m },
            new() { Name = "Mid-range Product 2", Price = 30m }
        };

        _mockRepository.FindAsync(Arg.Any<Expression<Func<Product, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(productsInRange);

        // Act
        var result = await _sut.GetProductsByPriceRangeAsync(20m, 35m);

        // Assert
        Assert.Equal(2, result.Count());
    }

    #endregion
}
