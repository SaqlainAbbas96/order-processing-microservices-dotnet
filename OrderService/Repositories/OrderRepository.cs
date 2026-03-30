using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;

namespace OrderService.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderDbContext _dbContext;
        public OrderRepository(OrderDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Order> AddOrderAsync(Order order)
        {
            order.Id = Guid.NewGuid();
            order.CreatedAt = DateTime.UtcNow;

            await _dbContext.Orders.AddAsync(order);
            await _dbContext.SaveChangesAsync();

            return order;
        }

        public async Task<Order?> GetOrderByIdAsync(Guid id)
        {
            return await _dbContext.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id);
        }
    }
}
