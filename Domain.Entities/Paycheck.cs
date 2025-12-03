using Domain.Entities.Interfaces;

namespace Domain.Entities;

public class Paycheck : IEntity
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public DateTime PayPeriodStart { get; set; }
    public DateTime PayPeriodEnd { get; set; }
    public DateTime PayDate { get; set; }
    public decimal GrossPay { get; set; }
    public decimal NetPay { get; set; }
    public decimal? TaxDeduction { get; set; }
    public decimal? OtherDeductions { get; set; }
    public string? Notes { get; set; }
    
    // Navigation properties
    public Employee Employee { get; set; } = null!;
}
