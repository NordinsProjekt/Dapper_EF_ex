using Application.Services;
using Domain.Entities;
using Infrastructure.EFCore;

namespace Presentation.KioskViewer.Services;

/// <summary>
/// Presentation layer wrapper for EmployeeService.
/// Delegates to Application.Services.EmployeeService.
/// </summary>
public class EmployeeServiceWrapper
{
    private readonly EmployeeService _employeeService;

    public EmployeeServiceWrapper(EmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
    {
        return await _employeeService.GetAllEmployeesAsync();
    }

    public async Task<Employee?> GetEmployeeByIdAsync(Guid id)
    {
        return await _employeeService.GetEmployeeByIdAsync(id);
    }

    public async Task<Employee> CreateEmployeeAsync(string firstName, string lastName, string email, string? phone, DateTime hireDate, decimal hourlyRate, bool isActive = true)
    {
        return await _employeeService.CreateEmployeeAsync(firstName, lastName, email, phone, hireDate, hourlyRate, isActive);
    }

    public async Task UpdateEmployeeAsync(Employee employee)
    {
        await _employeeService.UpdateEmployeeAsync(employee);
    }

    public async Task DeleteEmployeeAsync(Guid id)
    {
        await _employeeService.DeleteEmployeeAsync(id);
    }
}
