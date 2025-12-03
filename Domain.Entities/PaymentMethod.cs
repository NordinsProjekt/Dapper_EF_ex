using Domain.Entities.Interfaces;

namespace Domain.Entities;

public class PaymentMethod : IEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty; // e.g., "Cash", "Credit Card", "Debit Card"
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public ICollection<Receipt> Receipts { get; set; } = new List<Receipt>();
}
