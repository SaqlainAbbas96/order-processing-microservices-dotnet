using OrderService.DTOs;
using OrderService.Messaging;
using OrderService.Models;
using OrderService.Repositories;
using Shared;

namespace OrderService.Services
{
    public class OrderServiceImpl : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IEventPublisher _publisher;

        public OrderServiceImpl(IOrderRepository orderRepository, IEventPublisher publisher)
        {
            _orderRepository = orderRepository;
            _publisher = publisher;
        }

        public async Task<OrderResponse> CreateOrder(OrderRequest orderRequest) 
        {
            Order order = new Order
            {
                ProductId = orderRequest.ProductId,
                Quantity = orderRequest.Quantity
            };

            var createdOrder = await _orderRepository.AddOrderAsync(order);

            var evt = new OrderCreatedEvent
            {
                OrderId = order.Id,
                ProductId = order.ProductId,
                Quantity = order.Quantity
            };

            _publisher.PublishOrderCreated(evt);

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
