using Application.Services.Interfaces;
using Domain.Entities;

namespace BlazorServer.Services;

public class CustomerService
{
    private readonly IRepository<Customer> _repository;

    public CustomerService(IRepository<Customer> repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Customer?> GetCustomerByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<Customer> CreateCustomerAsync(string firstName, string lastName, string email, string? phone = null)
    {
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Phone = phone,
            CreatedAt = DateTime.UtcNow
        };

        return await _repository.AddAsync(customer);
    }

    public async Task UpdateCustomerAsync(Customer customer)
    {
        customer.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(customer);
    }

    public async Task DeleteCustomerAsync(Guid id)
    {
        await _repository.DeleteByIdAsync(id);
    }
}
