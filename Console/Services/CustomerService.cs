using Application.Services;

namespace Presentation.KioskViewer.Services;

/// <summary>
/// Presentation layer wrapper for CustomerService.
/// Delegates to Application.Services.CustomerService.
/// </summary>
public class CustomerServiceWrapper
{
    private readonly CustomerService _customerService;

    public CustomerServiceWrapper(CustomerService customerService)
    {
        _customerService = customerService;
    }

    public async Task<IEnumerable<Domain.Entities.Customer>> GetAllCustomersAsync()
    {
        return await _customerService.GetAllCustomersAsync();
    }

    public async Task<Domain.Entities.Customer?> GetCustomerByIdAsync(Guid id)
    {
        return await _customerService.GetCustomerByIdAsync(id);
    }

    public async Task<Domain.Entities.Customer> CreateCustomerAsync(string firstName, string lastName, string email, string? phone = null)
    {
        return await _customerService.CreateCustomerAsync(firstName, lastName, email, phone);
    }

    public async Task UpdateCustomerAsync(Domain.Entities.Customer customer)
    {
        await _customerService.UpdateCustomerAsync(customer);
    }

    public async Task DeleteCustomerAsync(Guid id)
    {
        await _customerService.DeleteCustomerAsync(id);
    }
}
