using Application.Services.Interfaces;
using Domain.Entities;

namespace Application.Services;

/// <summary>
/// Service for managing Customer entities.
/// Provides business logic layer between presentation and data access.
/// </summary>
public class CustomerService
{
    private readonly IRepository<Customer> _repository;

    public CustomerService(IRepository<Customer> repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Retrieves all customers from the repository.
    /// </summary>
    public async Task<IEnumerable<Customer>> GetAllCustomersAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.GetAllAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves a customer by their unique identifier.
    /// </summary>
    public async Task<Customer?> GetCustomerByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _repository.GetByIdAsync(id, cancellationToken);
    }

    /// <summary>
    /// Creates a new customer.
    /// </summary>
    public async Task<Customer> CreateCustomerAsync(
        string firstName, 
        string lastName, 
        string email, 
        string? phone = null,
        CancellationToken cancellationToken = default)
    {
        // Business logic: Validate email is unique
        var existingCustomer = await _repository.FirstOrDefaultAsync(c => c.Email == email, cancellationToken);
        if (existingCustomer != null)
        {
            throw new InvalidOperationException($"A customer with email '{email}' already exists.");
        }

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Phone = phone,
            CreatedAt = DateTime.UtcNow
        };

        return await _repository.AddAsync(customer, cancellationToken);
    }

    /// <summary>
    /// Updates an existing customer.
    /// </summary>
    public async Task UpdateCustomerAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        // Business logic: Ensure customer exists
        var exists = await _repository.ExistsAsync(customer.Id, cancellationToken);
        if (!exists)
        {
            throw new InvalidOperationException($"Customer with ID '{customer.Id}' does not exist.");
        }

        // Business logic: Validate email is unique (excluding current customer)
        var existingCustomer = await _repository.FirstOrDefaultAsync(
            c => c.Email == customer.Email && c.Id != customer.Id, 
            cancellationToken);
        
        if (existingCustomer != null)
        {
            throw new InvalidOperationException($"A customer with email '{customer.Email}' already exists.");
        }

        customer.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(customer, cancellationToken);
    }

    /// <summary>
    /// Deletes a customer by their unique identifier.
    /// </summary>
    public async Task DeleteCustomerAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _repository.DeleteByIdAsync(id, cancellationToken);
    }

    /// <summary>
    /// Checks if a customer exists by their unique identifier.
    /// </summary>
    public async Task<bool> CustomerExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _repository.ExistsAsync(id, cancellationToken);
    }

    /// <summary>
    /// Gets the total count of customers.
    /// </summary>
    public async Task<int> GetCustomerCountAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.CountAsync(cancellationToken);
    }

    /// <summary>
    /// Searches for customers by name or email.
    /// </summary>
    public async Task<IEnumerable<Customer>> SearchCustomersAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var normalizedSearch = searchTerm.ToLower();
        return await _repository.FindAsync(
            c => c.FirstName.ToLower().Contains(normalizedSearch) ||
                 c.LastName.ToLower().Contains(normalizedSearch) ||
                 c.Email.ToLower().Contains(normalizedSearch),
            cancellationToken);
    }
}
