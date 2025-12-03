using Domain.Entities.Interfaces;

namespace Domain.Entities;

public class Employee : IEntity
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateTime HireDate { get; set; }
    public decimal HourlyRate { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
    public ICollection<Paycheck> Paychecks { get; set; } = new List<Paycheck>();
}
