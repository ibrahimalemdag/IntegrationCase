using Autofac;
using Autofac.Extensions.DependencyInjection;
using EventBus.EventBusRabbitMQ;
using EventBus.Library;
using EventBus.Library.Abstractions;
using Integration.Service;
using IntegrationEvents.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Integration;

public abstract class Program
{
    public static void Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        var eventBus = host.Services.GetRequiredService<IEventBus>();
        eventBus.Subscribe<ResourceTakenIntegrationEvent,
            IIntegrationEventHandler<ResourceTakenIntegrationEvent>>("testQueue");

        var service = host.Services.GetRequiredService<IIntegrationService>();

        ThreadPool.QueueUserWorkItem(_ => service.TrySaveItem("a"));
        ThreadPool.QueueUserWorkItem(_ => service.TrySaveItem("b"));
        ThreadPool.QueueUserWorkItem(_ => service.TrySaveItem("c"));

        Thread.Sleep(500);

        ThreadPool.QueueUserWorkItem(_ => service.TrySaveItem("a"));
        ThreadPool.QueueUserWorkItem(_ => service.TrySaveItem("b"));
        ThreadPool.QueueUserWorkItem(_ => service.TrySaveItem("c"));

        Thread.Sleep(6000);

        Console.WriteLine("Everything recorded:");

        service.GetAllItems().ForEach(Console.WriteLine);

        Console.ReadLine();
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
                    HostName = "localhost",
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

            services.AddSingleton<IIntegrationEventHandler<ResourceTakenIntegrationEvent>,
                                                                ResourceTakenIntegrationEventHandler>();

            services.AddSingleton<IIntegrationService, ItemIntegrationService>();


        });
}