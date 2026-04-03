using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared;
using System.Text;
using System.Text.Json;

namespace NotificationService.Consumers;

public class OrderCreatedConsumer : BackgroundService
{
    private readonly IConfiguration _config;

    private IConnection? _connection;
    private IModel? _channel;

    public OrderCreatedConsumer(IConfiguration config)
    {
        _config = config;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _config["RabbitMQ:Host"]
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Durable exchange
        _channel.ExchangeDeclare(
            exchange: "orders.exchange",
            type: ExchangeType.Fanout,
            durable: true
        );

        // Durable queue
        var queueName = _channel.QueueDeclare(
            queue: "notification.order-created",
            durable: true,
            exclusive: false,
            autoDelete: false
        ).QueueName;

        _channel.QueueBind(
            queue: queueName,
            exchange: "orders.exchange",
            routingKey: ""
        );

        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += (sender, e) =>
        {
            try
            {
                var body = Encoding.UTF8.GetString(e.Body.ToArray());
                var evt = JsonSerializer.Deserialize<OrderCreatedEvent>(body);

                if (evt == null)
                {
                    _channel.BasicNack(e.DeliveryTag, false, false);
                    return;
                }

                // Simulate notification
                Console.WriteLine(
                    $"Notification: Order {evt.OrderId} created for Product {evt.ProductId}"
                );

                _channel.BasicAck(e.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");

                _channel?.BasicNack(e.DeliveryTag, false, true);
            }
        };

        _channel.BasicConsume(
            queue: queueName,
            autoAck: false,
            consumer: consumer
        );

        stoppingToken.Register(() =>
        {
            _channel?.Close();
            _connection?.Close();
        });

        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _channel?.Close();
        _connection?.Close();

        return base.StopAsync(cancellationToken);
    }
}