namespace OrderService.DTOs
{
    public record OrderRequest(
        int ProductId,
        int Quantity
    );
}
