using Shared;

namespace OrderService.Messaging
{
    public interface IEventPublisher
    {
        void PublishOrderCreated(OrderCreatedEvent evt);
    }
}
