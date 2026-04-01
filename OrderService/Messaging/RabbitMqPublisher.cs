using RabbitMQ.Client;
using Shared;
using System.Text;
using System.Text.Json;

namespace OrderService.Messaging;

public class RabbitMqPublisher : IEventPublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMqPublisher(IConfiguration configuration)
    {
        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:Host"]
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(
            exchange: "orders.exchange",
            type: ExchangeType.Fanout,
            durable: true
        );
    }

    public void PublishOrderCreated(OrderCreatedEvent evt)
    {
        var message = JsonSerializer.Serialize(evt);
        var body = Encoding.UTF8.GetBytes(message);

        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;

        _channel.BasicPublish(
            exchange: "orders.exchange",
            routingKey: "",
            basicProperties: properties,
            body: body
        );
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}