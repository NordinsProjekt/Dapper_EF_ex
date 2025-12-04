using Application.Services;
using Application.Services.Interfaces;
using Domain.Entities;
using NSubstitute;
using System.Linq.Expressions;

namespace Application.Services.Tests;

/// <summary>
/// Unit tests for EmployeeService business logic.
/// These tests verify that business rules are properly enforced.
/// </summary>
public class EmployeeServiceTests
{
    private readonly IRepository<Employee> _mockRepository;
    private readonly EmployeeService _sut;

    public EmployeeServiceTests()
    {
        _mockRepository = Substitute.For<IRepository<Employee>>();
        _sut = new EmployeeService(_mockRepository);
    }

    #region GetAllEmployeesAsync Tests

    [Fact]
    public async Task GetAllEmployeesAsync_ReturnsAllEmployees()
    {
        // Arrange
        var expectedEmployees = new List<Employee>
        {
            new() { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe", Email = "john@company.com", HourlyRate = 25.00m, IsActive = true },
            new() { Id = Guid.NewGuid(), FirstName = "Jane", LastName = "Smith", Email = "jane@company.com", HourlyRate = 30.00m, IsActive = true }
        };
        _mockRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(expectedEmployees);

        // Act
        var result = await _sut.GetAllEmployeesAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(result, e => e.Email == "john@company.com");
        Assert.Contains(result, e => e.Email == "jane@company.com");
    }

    #endregion

    #region GetEmployeeByIdAsync Tests

    [Fact]
    public async Task GetEmployeeByIdAsync_WithValidId_ReturnsEmployee()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var expectedEmployee = new Employee
        {
            Id = employeeId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@company.com",
            HourlyRate = 25.00m,
            HireDate = DateTime.Today.AddYears(-1),
            IsActive = true
        };
        _mockRepository.GetByIdAsync(employeeId, Arg.Any<CancellationToken>()).Returns(expectedEmployee);

        // Act
        var result = await _sut.GetEmployeeByIdAsync(employeeId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(employeeId, result.Id);
        Assert.Equal("john@company.com", result.Email);
    }

    #endregion

    #region CreateEmployeeAsync Tests

    [Fact]
    public async Task CreateEmployeeAsync_WithValidData_CreatesEmployee()
    {
        // Arrange
        var hireDate = DateTime.Today.AddMonths(-6);
        _mockRepository.FirstOrDefaultAsync(Arg.Any<Expression<Func<Employee, bool>>>(), Arg.Any<CancellationToken>())
            .Returns((Employee?)null);
        _mockRepository.AddAsync(Arg.Any<Employee>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Employee>());

        // Act
        var result = await _sut.CreateEmployeeAsync("John", "Doe", "john@company.com", "555-1234", hireDate, 25.00m, true);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
        Assert.Equal("john@company.com", result.Email);
        Assert.Equal("555-1234", result.Phone);
        Assert.Equal(25.00m, result.HourlyRate);
        Assert.True(result.IsActive);
        Assert.NotEqual(Guid.Empty, result.Id);
        await _mockRepository.Received(1).AddAsync(Arg.Any<Employee>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateEmployeeAsync_WithDuplicateEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var existingEmployee = new Employee { Email = "john@company.com" };
        _mockRepository.FirstOrDefaultAsync(Arg.Any<Expression<Func<Employee, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(existingEmployee);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.CreateEmployeeAsync("John", "Doe", "john@company.com", null, DateTime.Today, 25.00m));

        Assert.Contains("already exists", exception.Message);
        Assert.Contains("john@company.com", exception.Message);
        await _mockRepository.DidNotReceive().AddAsync(Arg.Any<Employee>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateEmployeeAsync_WithZeroHourlyRate_ThrowsArgumentException()
    {
        // Arrange
        _mockRepository.FirstOrDefaultAsync(Arg.Any<Expression<Func<Employee, bool>>>(), Arg.Any<CancellationToken>())
            .Returns((Employee?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.CreateEmployeeAsync("John", "Doe", "john@company.com", null, DateTime.Today, 0m));

        Assert.Contains("Hourly rate must be greater than zero", exception.Message);
        Assert.Equal("hourlyRate", exception.ParamName);
    }

    [Fact]
    public async Task CreateEmployeeAsync_WithNegativeHourlyRate_ThrowsArgumentException()
    {
        // Arrange
        _mockRepository.FirstOrDefaultAsync(Arg.Any<Expression<Func<Employee, bool>>>(), Arg.Any<CancellationToken>())
            .Returns((Employee?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.CreateEmployeeAsync("John", "Doe", "john@company.com", null, DateTime.Today, -5m));

        Assert.Contains("Hourly rate must be greater than zero", exception.Message);
    }

    [Fact]
    public async Task CreateEmployeeAsync_WithFutureHireDate_ThrowsArgumentException()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddDays(10);
        _mockRepository.FirstOrDefaultAsync(Arg.Any<Expression<Func<Employee, bool>>>(), Arg.Any<CancellationToken>())
            .Returns((Employee?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.CreateEmployeeAsync("John", "Doe", "john@company.com", null, futureDate, 25.00m));

        Assert.Contains("Hire date cannot be in the future", exception.Message);
        Assert.Equal("hireDate", exception.ParamName);
    }

    [Fact]
    public async Task CreateEmployeeAsync_WithoutPhone_CreatesEmployeeSuccessfully()
    {
        // Arrange
        _mockRepository.FirstOrDefaultAsync(Arg.Any<Expression<Func<Employee, bool>>>(), Arg.Any<CancellationToken>())
            .Returns((Employee?)null);
        _mockRepository.AddAsync(Arg.Any<Employee>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Employee>());

        // Act
        var result = await _sut.CreateEmployeeAsync("John", "Doe", "john@company.com", null, DateTime.Today, 25.00m);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Phone);
    }

    [Fact]
    public async Task CreateEmployeeAsync_DefaultsToActive()
    {
        // Arrange
        _mockRepository.FirstOrDefaultAsync(Arg.Any<Expression<Func<Employee, bool>>>(), Arg.Any<CancellationToken>())
            .Returns((Employee?)null);
        _mockRepository.AddAsync(Arg.Any<Employee>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Employee>());

        // Act
        var result = await _sut.CreateEmployeeAsync("John", "Doe", "john@company.com", null, DateTime.Today, 25.00m);

        // Assert
        Assert.True(result.IsActive);
    }

    #endregion

    #region UpdateEmployeeAsync Tests

    [Fact]
    public async Task UpdateEmployeeAsync_WithValidEmployee_UpdatesSuccessfully()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var employee = new Employee
        {
            Id = employeeId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@company.com",
            HourlyRate = 30.00m,
            HireDate = DateTime.Today.AddYears(-1),
            IsActive = true
        };

        _mockRepository.ExistsAsync(employeeId, Arg.Any<CancellationToken>()).Returns(true);
        _mockRepository.FirstOrDefaultAsync(Arg.Any<Expression<Func<Employee, bool>>>(), Arg.Any<CancellationToken>())
            .Returns((Employee?)null);

        // Act
        await _sut.UpdateEmployeeAsync(employee);

        // Assert
        await _mockRepository.Received(1).UpdateAsync(employee, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateEmployeeAsync_WithNonExistentEmployee_ThrowsInvalidOperationException()
    {
        // Arrange
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            Email = "john@company.com",
            HourlyRate = 25.00m,
            HireDate = DateTime.Today
        };

        _mockRepository.ExistsAsync(employee.Id, Arg.Any<CancellationToken>()).Returns(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.UpdateEmployeeAsync(employee));

        Assert.Contains("does not exist", exception.Message);
    }

    [Fact]
    public async Task UpdateEmployeeAsync_WithDuplicateEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var employee = new Employee
        {
            Id = employeeId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@company.com",
            HourlyRate = 25.00m,
            HireDate = DateTime.Today
        };

        var otherEmployee = new Employee
        {
            Id = Guid.NewGuid(),
            Email = "john@company.com"
        };

        _mockRepository.ExistsAsync(employeeId, Arg.Any<CancellationToken>()).Returns(true);
        _mockRepository.FirstOrDefaultAsync(Arg.Any<Expression<Func<Employee, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(otherEmployee);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.UpdateEmployeeAsync(employee));

        Assert.Contains("already exists", exception.Message);
    }

    [Fact]
    public async Task UpdateEmployeeAsync_WithNegativeHourlyRate_ThrowsArgumentException()
    {
        // Arrange
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            Email = "john@company.com",
            HourlyRate = -5m,
            HireDate = DateTime.Today
        };

        _mockRepository.ExistsAsync(employee.Id, Arg.Any<CancellationToken>()).Returns(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.UpdateEmployeeAsync(employee));

        Assert.Contains("Hourly rate must be greater than zero", exception.Message);
    }

    [Fact]
    public async Task UpdateEmployeeAsync_WithFutureHireDate_ThrowsArgumentException()
    {
        // Arrange
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            Email = "john@company.com",
            HourlyRate = 25.00m,
            HireDate = DateTime.UtcNow.AddDays(10)
        };

        _mockRepository.ExistsAsync(employee.Id, Arg.Any<CancellationToken>()).Returns(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.UpdateEmployeeAsync(employee));

        Assert.Contains("Hire date cannot be in the future", exception.Message);
    }

    #endregion

    #region DeleteEmployeeAsync Tests

    [Fact]
    public async Task DeleteEmployeeAsync_WithValidId_DeletesEmployee()
    {
        // Arrange
        var employeeId = Guid.NewGuid();

        // Act
        await _sut.DeleteEmployeeAsync(employeeId);

        // Assert
        await _mockRepository.Received(1).DeleteByIdAsync(employeeId, Arg.Any<CancellationToken>());
    }

    #endregion

    #region EmployeeExistsAsync Tests

    [Fact]
    public async Task EmployeeExistsAsync_WhenEmployeeExists_ReturnsTrue()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        _mockRepository.ExistsAsync(employeeId, Arg.Any<CancellationToken>()).Returns(true);

        // Act
        var result = await _sut.EmployeeExistsAsync(employeeId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task EmployeeExistsAsync_WhenEmployeeDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        _mockRepository.ExistsAsync(employeeId, Arg.Any<CancellationToken>()).Returns(false);

        // Act
        var result = await _sut.EmployeeExistsAsync(employeeId);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region GetEmployeeCountAsync Tests

    [Fact]
    public async Task GetEmployeeCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        _mockRepository.CountAsync(Arg.Any<CancellationToken>()).Returns(15);

        // Act
        var result = await _sut.GetEmployeeCountAsync();

        // Assert
        Assert.Equal(15, result);
    }

    #endregion

    #region GetActiveEmployeeCountAsync Tests

    [Fact]
    public async Task GetActiveEmployeeCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        _mockRepository.CountAsync(Arg.Any<Expression<Func<Employee, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(10);

        // Act
        var result = await _sut.GetActiveEmployeeCountAsync();

        // Assert
        Assert.Equal(10, result);
    }

    #endregion

    #region GetActiveEmployeesAsync Tests

    [Fact]
    public async Task GetActiveEmployeesAsync_ReturnsOnlyActiveEmployees()
    {
        // Arrange
        var activeEmployees = new List<Employee>
        {
            new() { FirstName = "John", LastName = "Doe", Email = "john@company.com", IsActive = true },
            new() { FirstName = "Jane", LastName = "Smith", Email = "jane@company.com", IsActive = true }
        };

        _mockRepository.FindAsync(Arg.Any<Expression<Func<Employee, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(activeEmployees);

        // Act
        var result = await _sut.GetActiveEmployeesAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, e => Assert.True(e.IsActive));
    }

    #endregion

    #region GetInactiveEmployeesAsync Tests

    [Fact]
    public async Task GetInactiveEmployeesAsync_ReturnsOnlyInactiveEmployees()
    {
        // Arrange
        var inactiveEmployees = new List<Employee>
        {
            new() { FirstName = "Bob", LastName = "Johnson", Email = "bob@company.com", IsActive = false }
        };

        _mockRepository.FindAsync(Arg.Any<Expression<Func<Employee, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(inactiveEmployees);

        // Act
        var result = await _sut.GetInactiveEmployeesAsync();

        // Assert
        Assert.Single(result);
        Assert.All(result, e => Assert.False(e.IsActive));
    }

    #endregion

    #region SearchEmployeesAsync Tests

    [Fact]
    public async Task SearchEmployeesAsync_FindsEmployeesByName()
    {
        // Arrange
        var employees = new List<Employee>
        {
            new() { FirstName = "John", LastName = "Doe", Email = "john@company.com" },
            new() { FirstName = "Johnny", LastName = "Smith", Email = "johnny@company.com" }
        };

        _mockRepository.FindAsync(Arg.Any<Expression<Func<Employee, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(employees);

        // Act
        var result = await _sut.SearchEmployeesAsync("john");

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task SearchEmployeesAsync_IsCaseInsensitive()
    {
        // Arrange
        var employees = new List<Employee>
        {
            new() { FirstName = "John", LastName = "Doe", Email = "john@company.com" }
        };

        _mockRepository.FindAsync(Arg.Any<Expression<Func<Employee, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(employees);

        // Act
        var result = await _sut.SearchEmployeesAsync("JOHN");

        // Assert
        Assert.Single(result);
    }

    #endregion

    #region ActivateEmployeeAsync Tests

    [Fact]
    public async Task ActivateEmployeeAsync_WithInactiveEmployee_ActivatesEmployee()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var employee = new Employee
        {
            Id = employeeId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@company.com",
            HourlyRate = 25.00m,
            HireDate = DateTime.Today,
            IsActive = false
        };

        _mockRepository.GetByIdAsync(employeeId, Arg.Any<CancellationToken>()).Returns(employee);

        // Act
        await _sut.ActivateEmployeeAsync(employeeId);

        // Assert
        Assert.True(employee.IsActive);
        await _mockRepository.Received(1).UpdateAsync(employee, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ActivateEmployeeAsync_WithAlreadyActiveEmployee_DoesNotUpdate()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var employee = new Employee
        {
            Id = employeeId,
            IsActive = true
        };

        _mockRepository.GetByIdAsync(employeeId, Arg.Any<CancellationToken>()).Returns(employee);

        // Act
        await _sut.ActivateEmployeeAsync(employeeId);

        // Assert
        await _mockRepository.DidNotReceive().UpdateAsync(Arg.Any<Employee>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ActivateEmployeeAsync_WithNonExistentEmployee_ThrowsInvalidOperationException()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        _mockRepository.GetByIdAsync(employeeId, Arg.Any<CancellationToken>()).Returns((Employee?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.ActivateEmployeeAsync(employeeId));

        Assert.Contains("does not exist", exception.Message);
    }

    #endregion

    #region DeactivateEmployeeAsync Tests

    [Fact]
    public async Task DeactivateEmployeeAsync_WithActiveEmployee_DeactivatesEmployee()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var employee = new Employee
        {
            Id = employeeId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@company.com",
            HourlyRate = 25.00m,
            HireDate = DateTime.Today,
            IsActive = true
        };

        _mockRepository.GetByIdAsync(employeeId, Arg.Any<CancellationToken>()).Returns(employee);

        // Act
        await _sut.DeactivateEmployeeAsync(employeeId);

        // Assert
        Assert.False(employee.IsActive);
        await _mockRepository.Received(1).UpdateAsync(employee, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeactivateEmployeeAsync_WithAlreadyInactiveEmployee_DoesNotUpdate()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var employee = new Employee
        {
            Id = employeeId,
            IsActive = false
        };

        _mockRepository.GetByIdAsync(employeeId, Arg.Any<CancellationToken>()).Returns(employee);

        // Act
        await _sut.DeactivateEmployeeAsync(employeeId);

        // Assert
        await _mockRepository.DidNotReceive().UpdateAsync(Arg.Any<Employee>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeactivateEmployeeAsync_WithNonExistentEmployee_ThrowsInvalidOperationException()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        _mockRepository.GetByIdAsync(employeeId, Arg.Any<CancellationToken>()).Returns((Employee?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.DeactivateEmployeeAsync(employeeId));

        Assert.Contains("does not exist", exception.Message);
    }

    #endregion
}
