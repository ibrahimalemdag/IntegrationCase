
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using EventBus.Library.Abstractions;
using EventBus.EventBusRabbitMQ;
using RabbitMQ.Client;
using Autofac;
using EventBus.Library;
using Autofac.Extensions.DependencyInjection;
using IntegrationEvents.Events;
using IntegrationEvents.EventHandlers;

public class Program
{
    public static void Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        var eventBus = host.Services.GetRequiredService<IEventBus>();

        eventBus.Subscribe<SaveRequestAcceptedIntegrationEvent,
            IIntegrationEventHandler<SaveRequestAcceptedIntegrationEvent>>("testQueue");

        host.Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
        .UseServiceProviderFactory(new AutofacServiceProviderFactory())
            .ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();

                    var factory = new ConnectionFactory()
                    {
                        HostName = "localhost",// "EventBusConnection",
                        DispatchConsumersAsync = false
                    };

                    return new DefaultRabbitMQPersistentConnection(factory, logger);
                });

                services.AddSingleton<IEventBus, EventBusRabbitMQ>(sp =>
                {
                    var rabbitMQPersistentConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
                    var iLifetimeScope = sp.GetRequiredService<ILifetimeScope>();
                    var logger = sp.GetRequiredService<ILogger<EventBusRabbitMQ>>();
                    var eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();

                    return new EventBusRabbitMQ(rabbitMQPersistentConnection, logger, iLifetimeScope, eventBusSubcriptionsManager);
                });

                services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();

                services.AddSingleton<IIntegrationEventHandler<SaveRequestAcceptedIntegrationEvent>,
                                                                SaveRequestAcceptedIntegrationEventHandler>();



            });
}