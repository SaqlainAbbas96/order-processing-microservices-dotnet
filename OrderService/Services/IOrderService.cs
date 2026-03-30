using OrderService.DTOs;

namespace OrderService.Services
{
    public interface IOrderService
    {
        Task<OrderResponse> CreateOrder(OrderRequest orderRequest);
        Task<OrderResponse?> GetOrderById(Guid id);
    }
}
