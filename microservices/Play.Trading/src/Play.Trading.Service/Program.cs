
using System;
using System.Reflection;
using System.Text.Json.Serialization;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Play.Common.HealthChecks;
using Play.Common.Identity;
using Play.Common.Logging;
using Play.Common.MassTransit;
using Play.Common.MongoDB;
using Play.Common.OpenTelemetry;

using Play.Common.Settings;
//using Play.Common.Configuration;
using Play.Identity.Contracts;
using Play.Inventory.Contracts;
using Play.Trading.Service.Entities;
using Play.Trading.Service.Exceptions;
using Play.Trading.Service.Settings;
using Play.Trading.Service.SignalR;
using Play.Trading.Service.StateMachines;
using System.Collections.Generic;
using OpenTelemetry.Metrics; //used for metrics on line 202
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Exporter.Prometheus;







namespace Play.Trading.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Program>();
                });

        private const string AllowedOriginSetting = "AllowedOrigin";

        public Program(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMongo()
                    .AddMongoRepository<CatalogItem>("catalogitems")
                    .AddMongoRepository<InventoryItem>("inventoryitems")
                    .AddMongoRepository<ApplicationUser>("users")
                    .AddJwtBearerAuthentication();
            AddMassTransit(services);

            services.AddControllers(options =>
            {
                options.SuppressAsyncSuffixInActionNames = false;
            })
            .AddJsonOptions(options => options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);

            services.AddSwaggerGen(options =>
            {
                var scheme = new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri(Configuration.GetSection("Auth:Swagger:AuthorizationUrl").Get<string>()),
                            TokenUrl = new Uri(Configuration.GetSection("Auth:Swagger:TokenUrl").Get<string>())
                        }
                    },
                    Type = SecuritySchemeType.OAuth2
                };

                options.AddSecurityDefinition("OAuth", scheme);

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference { Id = "OAuth", Type = ReferenceType.SecurityScheme }
                            },
                            new List<string> { }
                        }
                });
            });


            services.AddSingleton<IUserIdProvider, UserIdProvider>()
                    .AddSingleton<MessageHub>()
                    .AddSignalR();

            services.AddHealthChecks()
                    .AddMongoDb();

            services.AddSeqLogging(Configuration)
                    .AddTracing(Configuration)
                    .AddMetrics(Configuration);     
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //Remove || env.IsProduction() for real Production. Made vor Docker 

            if (env.IsDevelopment() || env.IsProduction())
            {
                app.UseDeveloperExceptionPage();

                if (env.IsDevelopment() || env.IsProduction())
                {
                    app.UseDeveloperExceptionPage();
                    app.UseSwagger()
                    .UseSwaggerUI(options =>
                    {
                        options.OAuthClientId("api-swagger");
                        options.OAuthScopes("profile", "openid", "catalog.fullaccess",
                            "catalog.readaccess",
                            "catalog.writeaccess",
                            "inventory.fullaccess",
                            "trading.fullaccess",
                            "IdentityServerApi",
                            "roles");
                        options.OAuthUsePkce();
                        options.OAuth2RedirectUrl("http://host.docker.internal:5006/swagger/oauth2-redirect.html");
                        options.EnablePersistAuthorization();

                        //options.InjectStylesheet("/content/swagger-extras.css");
                    });

                    app.UseCors(builder =>
                   {
                       var allowedOrigins = Configuration.GetSection("AllowedOrigins").Get<string[]>();

                       builder.WithOrigins(allowedOrigins)
                           .AllowAnyHeader()
                           .AllowAnyMethod();
                   });
                }
            }

                // app.UseHttpsRedirection();

              
              app.UseOpenTelemetryPrometheusScrapingEndpoint();


            app.UseRouting();

                app.UseAuthentication();
                app.UseAuthorization();

                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                    endpoints.MapHub<MessageHub>("/messagehub"); //MapHub() is from Microsoft.AspNetCore.Identity.UI this is for the SignalR hub
                    endpoints.MapPlayEconomyHealthChecks();
                });
            }

            private void AddMassTransit(IServiceCollection services)
        {
            services.AddMassTransit(configure =>
            {
                configure.UsingPlayEconomyMessageBroker(Configuration, retryConfigurator =>
                {
                    retryConfigurator.Interval(3, TimeSpan.FromSeconds(5));
                    retryConfigurator.Ignore(typeof(UnknownItemException));
                });

                configure.AddConsumers(Assembly.GetEntryAssembly());
                configure.AddSagaStateMachine<PurchaseStateMachine, PurchaseState>(sagaConfigurator =>
                {
                    sagaConfigurator.UseInMemoryOutbox();
                })
                    .MongoDbRepository(r =>
                    {
                        var serviceSettings = Configuration.GetSection(nameof(ServiceSettings))
                                                           .Get<ServiceSettings>();
                        var mongoSettings = Configuration.GetSection(nameof(MongoDbSettings))
                                                           .Get<MongoDbSettings>();

                        r.Connection = mongoSettings.ConnectionString;
                        r.DatabaseName = serviceSettings.ServiceName;
                    });
            });

            var queueSettings = Configuration.GetSection(nameof(QueueSettings))
                                                           .Get<QueueSettings>();
            EndpointConvention.Map<GrantItems>(new Uri(queueSettings.GrantItemsQueueAddress));
            EndpointConvention.Map<DebitGil>(new Uri(queueSettings.DebitGilQueueAddress));
            EndpointConvention.Map<SubtractItems>(new Uri(queueSettings.SubtractItemsQueueAddress));
        }
        }

    }
