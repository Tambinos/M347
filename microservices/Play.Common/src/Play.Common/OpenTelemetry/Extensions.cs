using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Play.Common.MassTransit;
using Play.Common.Settings;



namespace Play.Common.OpenTelemetry
{
    public static class Extensions
    {
        public static IServiceCollection AddTracing(
            this IServiceCollection services,
            IConfiguration config)
        {
            services.AddOpenTelemetry()
                .WithTracing(builder => 
                       {
                var serviceSettings = config.GetSection(nameof(ServiceSettings))
                                                   .Get<ServiceSettings>();
                                                  
                builder.AddSource(serviceSettings.ServiceName)
                       .AddSource("MassTransit")
                       .SetResourceBuilder(
                           ResourceBuilder.CreateDefault()
                                .AddService(serviceName: serviceSettings.ServiceName))
                        .AddHttpClientInstrumentation()
                        .AddAspNetCoreInstrumentation()                        
                        .AddJaegerExporter(options =>
                        {
                            var jaegerSettings = config.GetSection(nameof(JaegerSettings))
                                                              .Get<JaegerSettings>();
                            options.AgentHost = jaegerSettings.Host;
                            options.AgentPort = jaegerSettings.Port;
                        });
            });
            services.AddConsumeObserver<ConsumeObserver>();         
            return services;
        }

        public static IServiceCollection AddMetrics(
            this IServiceCollection services,
            IConfiguration config)
        {
           services.AddOpenTelemetry() 
              .WithMetrics(builder =>            
            {
                var settings = config.GetSection(nameof(ServiceSettings))
                                            .Get<ServiceSettings>();
                builder.AddMeter(settings.ServiceName)
                        .AddMeter("MassTransit")
                        .AddHttpClientInstrumentation()
                        .AddAspNetCoreInstrumentation()
                        .AddRuntimeInstrumentation()                        
                        .AddPrometheusExporter();                        
            });            

            return services;
        }
    }
}