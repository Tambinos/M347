using System;
using System.Reflection;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Play.Common.Settings;

namespace Play.Common.MassTransit
{
    public static class Extensions
    {
        private const string RabbitMq = "RABBITMQ";
        private const string ServiceBus = "SERVICEBUS";

        public static IServiceCollection AddMassTransitWithMessageBroker( // this is the method that is called in the Play.Common project
            this IServiceCollection services,  // this is the IServiceCollection that is passed in from the Play.Common project
            IConfiguration config, // this is the IConfiguration that is passed in from the Play.Common project
            Action<IRetryConfigurator> configureRetries = null)  // this is the Action<IRetryConfigurator> that is passed in from the Play.Common project
        {
            var serviceSettings = config.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();  //

            switch (serviceSettings.MessageBroker?.ToUpper()) // this is the message broker that is set in the appsettings.json file
            {
                case ServiceBus:
                    services.AddMassTransitWithServiceBus(configureRetries); 
                    break;
                case RabbitMq:
                default:
                    services.AddMassTransitWithRabbitMq(configureRetries);
                    break;                
            }

            return services;
        }

        public static IServiceCollection AddMassTransitWithRabbitMq(  
            this IServiceCollection services,
            Action<IRetryConfigurator> configureRetries = null)  
        {
            services.AddMassTransit(configure =>
            {
                configure.AddConsumers(Assembly.GetEntryAssembly());  
                configure.UsingPlayEconomyRabbitMq(configureRetries); 
            });

            return services;
        }

        public static IServiceCollection AddMassTransitWithServiceBus(
            this IServiceCollection services,
            Action<IRetryConfigurator> configureRetries = null)
        {
            services.AddMassTransit(configure =>
            {
                configure.AddConsumers(Assembly.GetEntryAssembly());
                configure.UsingPlayEconomyAzureServiceBus(configureRetries);
            });

            return services;
        }        

        public static void UsingPlayEconomyMessageBroker(  
            this IBusRegistrationConfigurator configure,
            IConfiguration config,
            Action<IRetryConfigurator> configureRetries = null)
        {
            var serviceSettings = config.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>(); 

            switch (serviceSettings.MessageBroker?.ToUpper())
            {
                case ServiceBus:
                    configure.UsingPlayEconomyAzureServiceBus(configureRetries);
                    break;
                case RabbitMq:
                default:
                    configure.UsingPlayEconomyRabbitMq(configureRetries);
                    break;                
            }
        }

        public static void UsingPlayEconomyRabbitMq(
            this IBusRegistrationConfigurator configure,
            Action<IRetryConfigurator> configureRetries = null)
        {
            configure.UsingRabbitMq((context, configurator) =>
            {
                var configuration = context.GetService<IConfiguration>();
                var serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
                var rabbitMQSettings = configuration.GetSection(nameof(RabbitMQSettings)).Get<RabbitMQSettings>();
                configurator.Host(rabbitMQSettings.Host);
                configurator.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter(serviceSettings.ServiceName, false)); // this is the endpoint name formatter that is used in the Play.Common project

                if (configureRetries == null)
                {
                    configureRetries = (retryConfigurator) => retryConfigurator.Interval(3, TimeSpan.FromSeconds(5));
                }

                configurator.UseMessageRetry(configureRetries);
                configurator.UseInstrumentation(serviceName: serviceSettings.ServiceName);
            });
        }

        public static void UsingPlayEconomyAzureServiceBus(
            this IBusRegistrationConfigurator configure,
            Action<IRetryConfigurator> configureRetries = null)
        {
            configure.UsingAzureServiceBus((context, configurator) =>
            {
                var configuration = context.GetService<IConfiguration>();
                var serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
                var serviceBusSettings = configuration.GetSection(nameof(ServiceBusSettings)).Get<ServiceBusSettings>();
                configurator.Host(serviceBusSettings.ConnectionString);
                configurator.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter(serviceSettings.ServiceName, false));

                if (configureRetries == null)
                {
                    configureRetries = (retryConfigurator) => retryConfigurator.Interval(3, TimeSpan.FromSeconds(5));
                }

                configurator.UseMessageRetry(configureRetries);
                configurator.UseInstrumentation(serviceName: serviceSettings.ServiceName);
            });
        }        
    }
}