namespace OrderService.DTOs
{
    public class OrderResponse
    {
        public Guid Id { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
