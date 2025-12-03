using Domain.Entities.Interfaces;

namespace Domain.Entities;

public class ReceiptItem : IEntity
{
    public Guid Id { get; set; }
    public Guid ReceiptId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    
    // Navigation properties
    public Receipt Receipt { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
