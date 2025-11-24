using AppServices.Contracts.Messaging;
using AppServices.Contracts.Repositories;
using AppServices.Contracts.Storage;
using AppServices.UseCases;
using Microsoft.EntityFrameworkCore;
using ServiceBus.Services;
using System.Threading.Channels;
using TransferaShipments.BlobStorage.Services;
using TransferaShipments.Persistence.Data;
using TransferaShipments.Persistence.Repositories;
using TransferaShipments.ServiceBus.HostedServices;
using TransferaShipments.ServiceBus.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                     .AddEnvironmentVariables();

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));

// DI - Repositories
builder.Services.AddScoped<IShipmentRepository, ShipmentRepository>();

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreateShipmentUseCase>());

// Blob Storage
builder.Services.AddSingleton<IBlobService, BlobService>();

// Service Bus - Local In-Memory Implementation
var channel = Channel.CreateUnbounded<string>(new UnboundedChannelOptions
{
    SingleReader = true,
    SingleWriter = false
});
builder.Services.AddSingleton(channel);
// For Azure Service Bus
//builder.Services.AddSingleton<AzureServiceBusService>();
builder.Services.AddSingleton<IServiceBusPublisher, LocalServiceBusPublisher>();
builder.Services.AddHostedService<LocalDocumentProcessorHostedService>();
Console.WriteLine("✓ Using Local In-Memory Service Bus");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

// Initialize database (with error handling)
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();
        app.Logger.LogInformation("Database initialized successfully.");
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Could not connect to database. The application will continue but database operations will fail.");
    }
}

app.Run();