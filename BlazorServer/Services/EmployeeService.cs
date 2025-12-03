using Application.Services.Interfaces;
using Domain.Entities;

namespace BlazorServer.Services;

public class EmployeeService
{
    private readonly IRepository<Employee> _repository;

    public EmployeeService(IRepository<Employee> repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Employee?> GetEmployeeByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<Employee> CreateEmployeeAsync(string firstName, string lastName, string email, string? phone, DateTime hireDate, decimal hourlyRate, bool isActive = true)
    {
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Phone = phone,
            HireDate = hireDate,
            HourlyRate = hourlyRate,
            IsActive = isActive
        };

        return await _repository.AddAsync(employee);
    }

    public async Task UpdateEmployeeAsync(Employee employee)
    {
        await _repository.UpdateAsync(employee);
    }

    public async Task DeleteEmployeeAsync(Guid id)
    {
        await _repository.DeleteByIdAsync(id);
    }
}
