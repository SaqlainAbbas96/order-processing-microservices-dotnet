using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Messaging;
using OrderService.Repositories;
using OrderService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IOrderService, OrderServiceImpl>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

builder.Services.AddSingleton<IEventPublisher, RabbitMqPublisher>();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

    // Retry DB connection
    var retries = 5;
    while (retries > 0)
    {
        try
        {
            db.Database.Migrate();
            break;
        }
        catch
        {
            retries--;
            Thread.Sleep(2000);
        }
    }
}

app.Run();