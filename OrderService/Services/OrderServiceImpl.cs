using OrderService.DTOs;
using OrderService.Models;
using OrderService.Repositories;

namespace OrderService.Services
{
    public class OrderServiceImpl : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        public OrderServiceImpl(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<OrderResponse> CreateOrder(OrderRequest orderRequest) 
        {
            Order order = new Order
            {
                ProductId = orderRequest.ProductId,
                Quantity = orderRequest.Quantity
            };

            var createdOrder = await _orderRepository.AddOrderAsync(order);

            return new OrderResponse
            {
                Id = createdOrder.Id,
                ProductId = createdOrder.ProductId,
                Quantity = createdOrder.Quantity,
                CreatedAt = createdOrder.CreatedAt
            };           
        }

        public async Task<OrderResponse?> GetOrderById(Guid id)
        {
            var order = await _orderRepository.GetOrderByIdAsync(id);

            if (order == null)
                return null;

            return new OrderResponse
            {
                Id = order.Id,
                ProductId = order.ProductId,
                Quantity = order.Quantity,
                CreatedAt = order.CreatedAt
            };
        }
    }
}
