using Domain.Entities.Interfaces;

namespace Domain.Entities;

public class Receipt : IEntity
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid PaymentMethodId { get; set; }
    public DateTime PurchaseDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal? TaxAmount { get; set; }
    public string? Notes { get; set; }
    
    // Navigation properties
    public Customer Customer { get; set; } = null!;
    public PaymentMethod PaymentMethod { get; set; } = null!;
    public ICollection<ReceiptItem> Items { get; set; } = new List<ReceiptItem>();
}
