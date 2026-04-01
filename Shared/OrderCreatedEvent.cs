namespace Shared;

public class OrderCreatedEvent
{
    public Guid OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
