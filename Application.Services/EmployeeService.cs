using Application.Services.Interfaces;
using Domain.Entities;

namespace Application.Services;

/// <summary>
/// Service for managing Employee entities.
/// Provides business logic layer between presentation and data access.
/// </summary>
public class EmployeeService
{
    private readonly IRepository<Employee> _repository;

    public EmployeeService(IRepository<Employee> repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Retrieves all employees from the repository.
    /// </summary>
    public async Task<IEnumerable<Employee>> GetAllEmployeesAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.GetAllAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves an employee by their unique identifier.
    /// </summary>
    public async Task<Employee?> GetEmployeeByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _repository.GetByIdAsync(id, cancellationToken);
    }

    /// <summary>
    /// Creates a new employee.
    /// </summary>
    public async Task<Employee> CreateEmployeeAsync(
        string firstName, 
        string lastName, 
        string email, 
        string? phone, 
        DateTime hireDate, 
        decimal hourlyRate, 
        bool isActive = true,
        CancellationToken cancellationToken = default)
    {
        // Business logic: Validate email is unique
        var existingEmployee = await _repository.FirstOrDefaultAsync(e => e.Email == email, cancellationToken);
        if (existingEmployee != null)
        {
            throw new InvalidOperationException($"An employee with email '{email}' already exists.");
        }

        // Business logic: Validate hourly rate is positive
        if (hourlyRate <= 0)
        {
            throw new ArgumentException("Hourly rate must be greater than zero.", nameof(hourlyRate));
        }

        // Business logic: Validate hire date is not in the future
        if (hireDate > DateTime.UtcNow)
        {
            throw new ArgumentException("Hire date cannot be in the future.", nameof(hireDate));
        }

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

        return await _repository.AddAsync(employee, cancellationToken);
    }

    /// <summary>
    /// Updates an existing employee.
    /// </summary>
    public async Task UpdateEmployeeAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        // Business logic: Ensure employee exists
        var exists = await _repository.ExistsAsync(employee.Id, cancellationToken);
        if (!exists)
        {
            throw new InvalidOperationException($"Employee with ID '{employee.Id}' does not exist.");
        }

        // Business logic: Validate email is unique (excluding current employee)
        var existingEmployee = await _repository.FirstOrDefaultAsync(
            e => e.Email == employee.Email && e.Id != employee.Id, 
            cancellationToken);
        
        if (existingEmployee != null)
        {
            throw new InvalidOperationException($"An employee with email '{employee.Email}' already exists.");
        }

        // Business logic: Validate hourly rate is positive
        if (employee.HourlyRate <= 0)
        {
            throw new ArgumentException("Hourly rate must be greater than zero.", nameof(employee.HourlyRate));
        }

        // Business logic: Validate hire date is not in the future
        if (employee.HireDate > DateTime.UtcNow)
        {
            throw new ArgumentException("Hire date cannot be in the future.", nameof(employee.HireDate));
        }

        await _repository.UpdateAsync(employee, cancellationToken);
    }

    /// <summary>
    /// Deletes an employee by their unique identifier.
    /// </summary>
    public async Task DeleteEmployeeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _repository.DeleteByIdAsync(id, cancellationToken);
    }

    /// <summary>
    /// Checks if an employee exists by their unique identifier.
    /// </summary>
    public async Task<bool> EmployeeExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _repository.ExistsAsync(id, cancellationToken);
    }

    /// <summary>
    /// Gets the total count of employees.
    /// </summary>
    public async Task<int> GetEmployeeCountAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.CountAsync(cancellationToken);
    }

    /// <summary>
    /// Gets the count of active employees.
    /// </summary>
    public async Task<int> GetActiveEmployeeCountAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.CountAsync(e => e.IsActive, cancellationToken);
    }

    /// <summary>
    /// Gets all active employees.
    /// </summary>
    public async Task<IEnumerable<Employee>> GetActiveEmployeesAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.FindAsync(e => e.IsActive, cancellationToken);
    }

    /// <summary>
    /// Gets all inactive employees.
    /// </summary>
    public async Task<IEnumerable<Employee>> GetInactiveEmployeesAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.FindAsync(e => !e.IsActive, cancellationToken);
    }

    /// <summary>
    /// Searches for employees by name or email.
    /// </summary>
    public async Task<IEnumerable<Employee>> SearchEmployeesAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var normalizedSearch = searchTerm.ToLower();
        return await _repository.FindAsync(
            e => e.FirstName.ToLower().Contains(normalizedSearch) ||
                 e.LastName.ToLower().Contains(normalizedSearch) ||
                 e.Email.ToLower().Contains(normalizedSearch),
            cancellationToken);
    }

    /// <summary>
    /// Activates an employee (sets IsActive to true).
    /// </summary>
    public async Task ActivateEmployeeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var employee = await _repository.GetByIdAsync(id, cancellationToken);
        if (employee == null)
        {
            throw new InvalidOperationException($"Employee with ID '{id}' does not exist.");
        }

        if (employee.IsActive)
        {
            return; // Already active, no change needed
        }

        employee.IsActive = true;
        await _repository.UpdateAsync(employee, cancellationToken);
    }

    /// <summary>
    /// Deactivates an employee (sets IsActive to false).
    /// </summary>
    public async Task DeactivateEmployeeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var employee = await _repository.GetByIdAsync(id, cancellationToken);
        if (employee == null)
        {
            throw new InvalidOperationException($"Employee with ID '{id}' does not exist.");
        }

        if (!employee.IsActive)
        {
            return; // Already inactive, no change needed
        }

        employee.IsActive = false;
        await _repository.UpdateAsync(employee, cancellationToken);
    }
}
