using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using InventoryService.Data;
using Shared;

namespace InventoryService.Consumers;

public class OrderCreatedConsumer : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _services;

    private IConnection? _connection;
    private IModel? _channel;

    public OrderCreatedConsumer(IConfiguration configuration, IServiceProvider services)
    {
        _configuration = configuration;
        _services = services;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMQ:Host"]
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Declare exchange (durable)
        _channel.ExchangeDeclare(
            exchange: "orders.exchange",
            type: ExchangeType.Fanout,
            durable: true
        );

        // Declare durable queue
        var queueName = _channel.QueueDeclare(
            queue: "inventory.order-created",
            durable: true,
            exclusive: false,
            autoDelete: false
        ).QueueName;

        // Bind queue to exchange
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

                using var scope = _services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

                var product = db.Products.FirstOrDefault(p => p.Id == evt.ProductId);

                if (product != null)
                {
                    product.Stock -= evt.Quantity;
                    db.SaveChanges();
                }

                // ACK only after success
                _channel.BasicAck(e.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");

                // Requeue for retry
                _channel?.BasicNack(e.DeliveryTag, multiple: false, requeue: true);
            }
        };

        _channel.BasicConsume(
            queue: queueName,
            autoAck: false,
            consumer: consumer
        );

        // Graceful shutdown
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