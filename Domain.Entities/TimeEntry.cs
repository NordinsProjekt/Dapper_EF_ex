using Domain.Entities.Interfaces;

namespace Domain.Entities;

public class TimeEntry : IEntity
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public DateTime ClockIn { get; set; }
    public DateTime? ClockOut { get; set; }
    public decimal? HoursWorked { get; set; }
    public string? Notes { get; set; }
    
    // Navigation properties
    public Employee Employee { get; set; } = null!;
}
