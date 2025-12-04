using Application.Services;
using Application.Services.Interfaces;
using Domain.Entities;
using NSubstitute;
using System.Linq.Expressions;

namespace Application.Services.Tests;

/// <summary>
/// Unit tests for CustomerService business logic.
/// These tests verify that business rules are properly enforced.
/// </summary>
public class CustomerServiceTests
{
    private readonly IRepository<Customer> _mockRepository;
    private readonly CustomerService _sut; // System Under Test

    public CustomerServiceTests()
    {
        _mockRepository = Substitute.For<IRepository<Customer>>();
        _sut = new CustomerService(_mockRepository);
    }

    #region GetAllCustomersAsync Tests

    [Fact]
    public async Task GetAllCustomersAsync_ReturnsAllCustomers()
    {
        // Arrange
        var expectedCustomers = new List<Customer>
        {
            new() { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe", Email = "john@test.com" },
            new() { Id = Guid.NewGuid(), FirstName = "Jane", LastName = "Smith", Email = "jane@test.com" }
        };
        _mockRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(expectedCustomers);

        // Act
        var result = await _sut.GetAllCustomersAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(result, c => c.Email == "john@test.com");
        Assert.Contains(result, c => c.Email == "jane@test.com");
        await _mockRepository.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAllCustomersAsync_WhenNoCustomers_ReturnsEmptyList()
    {
        // Arrange
        _mockRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<Customer>());

        // Act
        var result = await _sut.GetAllCustomersAsync();

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region GetCustomerByIdAsync Tests

    [Fact]
    public async Task GetCustomerByIdAsync_WithValidId_ReturnsCustomer()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var expectedCustomer = new Customer
        {
            Id = customerId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com"
        };
        _mockRepository.GetByIdAsync(customerId, Arg.Any<CancellationToken>()).Returns(expectedCustomer);

        // Act
        var result = await _sut.GetCustomerByIdAsync(customerId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(customerId, result.Id);
        Assert.Equal("john@test.com", result.Email);
    }

    [Fact]
    public async Task GetCustomerByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        _mockRepository.GetByIdAsync(customerId, Arg.Any<CancellationToken>()).Returns((Customer?)null);

        // Act
        var result = await _sut.GetCustomerByIdAsync(customerId);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region CreateCustomerAsync Tests

    [Fact]
    public async Task CreateCustomerAsync_WithUniqueEmail_CreatesCustomer()
    {
        // Arrange
        _mockRepository.FirstOrDefaultAsync(Arg.Any<Expression<Func<Customer, bool>>>(), Arg.Any<CancellationToken>())
            .Returns((Customer?)null);
        _mockRepository.AddAsync(Arg.Any<Customer>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Customer>());

        // Act
        var result = await _sut.CreateCustomerAsync("John", "Doe", "john@test.com", "555-1234");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
        Assert.Equal("john@test.com", result.Email);
        Assert.Equal("555-1234", result.Phone);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.True(result.CreatedAt <= DateTime.UtcNow);
        await _mockRepository.Received(1).AddAsync(Arg.Any<Customer>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateCustomerAsync_WithDuplicateEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var existingCustomer = new Customer { Email = "john@test.com" };
        _mockRepository.FirstOrDefaultAsync(Arg.Any<Expression<Func<Customer, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(existingCustomer);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.CreateCustomerAsync("John", "Doe", "john@test.com", null));

        Assert.Contains("already exists", exception.Message);
        Assert.Contains("john@test.com", exception.Message);
        await _mockRepository.DidNotReceive().AddAsync(Arg.Any<Customer>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateCustomerAsync_WithoutPhone_CreatesCustomerSuccessfully()
    {
        // Arrange
        _mockRepository.FirstOrDefaultAsync(Arg.Any<Expression<Func<Customer, bool>>>(), Arg.Any<CancellationToken>())
            .Returns((Customer?)null);
        _mockRepository.AddAsync(Arg.Any<Customer>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Customer>());

        // Act
        var result = await _sut.CreateCustomerAsync("John", "Doe", "john@test.com");

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Phone);
    }

    #endregion

    #region UpdateCustomerAsync Tests

    [Fact]
    public async Task UpdateCustomerAsync_WithValidCustomer_UpdatesSuccessfully()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = new Customer
        {
            Id = customerId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _mockRepository.ExistsAsync(customerId, Arg.Any<CancellationToken>()).Returns(true);
        _mockRepository.FirstOrDefaultAsync(Arg.Any<Expression<Func<Customer, bool>>>(), Arg.Any<CancellationToken>())
            .Returns((Customer?)null);

        // Act
        await _sut.UpdateCustomerAsync(customer);

        // Assert
        Assert.NotNull(customer.UpdatedAt);
        Assert.True(customer.UpdatedAt <= DateTime.UtcNow);
        await _mockRepository.Received(1).UpdateAsync(customer, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateCustomerAsync_WithNonExistentCustomer_ThrowsInvalidOperationException()
    {
        // Arrange
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com"
        };

        _mockRepository.ExistsAsync(customer.Id, Arg.Any<CancellationToken>()).Returns(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.UpdateCustomerAsync(customer));

        Assert.Contains("does not exist", exception.Message);
        await _mockRepository.DidNotReceive().UpdateAsync(Arg.Any<Customer>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateCustomerAsync_WithDuplicateEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = new Customer
        {
            Id = customerId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com"
        };

        var otherCustomer = new Customer
        {
            Id = Guid.NewGuid(),
            Email = "john@test.com"
        };

        _mockRepository.ExistsAsync(customerId, Arg.Any<CancellationToken>()).Returns(true);
        _mockRepository.FirstOrDefaultAsync(Arg.Any<Expression<Func<Customer, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(otherCustomer);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.UpdateCustomerAsync(customer));

        Assert.Contains("already exists", exception.Message);
        await _mockRepository.DidNotReceive().UpdateAsync(Arg.Any<Customer>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region DeleteCustomerAsync Tests

    [Fact]
    public async Task DeleteCustomerAsync_WithValidId_DeletesCustomer()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        // Act
        await _sut.DeleteCustomerAsync(customerId);

        // Assert
        await _mockRepository.Received(1).DeleteByIdAsync(customerId, Arg.Any<CancellationToken>());
    }

    #endregion

    #region CustomerExistsAsync Tests

    [Fact]
    public async Task CustomerExistsAsync_WhenCustomerExists_ReturnsTrue()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        _mockRepository.ExistsAsync(customerId, Arg.Any<CancellationToken>()).Returns(true);

        // Act
        var result = await _sut.CustomerExistsAsync(customerId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CustomerExistsAsync_WhenCustomerDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        _mockRepository.ExistsAsync(customerId, Arg.Any<CancellationToken>()).Returns(false);

        // Act
        var result = await _sut.CustomerExistsAsync(customerId);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region GetCustomerCountAsync Tests

    [Fact]
    public async Task GetCustomerCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        _mockRepository.CountAsync(Arg.Any<CancellationToken>()).Returns(5);

        // Act
        var result = await _sut.GetCustomerCountAsync();

        // Assert
        Assert.Equal(5, result);
    }

    #endregion

    #region SearchCustomersAsync Tests

    [Fact]
    public async Task SearchCustomersAsync_FindsCustomersByFirstName()
    {
        // Arrange
        var customers = new List<Customer>
        {
            new() { FirstName = "John", LastName = "Doe", Email = "john@test.com" },
            new() { FirstName = "Johnny", LastName = "Smith", Email = "johnny@test.com" }
        };

        _mockRepository.FindAsync(Arg.Any<Expression<Func<Customer, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(customers);

        // Act
        var result = await _sut.SearchCustomersAsync("john");

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task SearchCustomersAsync_IsCaseInsensitive()
    {
        // Arrange
        var customers = new List<Customer>
        {
            new() { FirstName = "John", LastName = "Doe", Email = "john@test.com" }
        };

        _mockRepository.FindAsync(Arg.Any<Expression<Func<Customer, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(customers);

        // Act
        var result = await _sut.SearchCustomersAsync("JOHN");

        // Assert
        Assert.Single(result);
    }

    #endregion
}
