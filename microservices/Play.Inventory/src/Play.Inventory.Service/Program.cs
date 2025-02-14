using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Play.Common.HealthChecks;
using Play.Common.Identity;
using Play.Common.MassTransit;
using Play.Common.MongoDB;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Entities;
using Play.Inventory.Service.Exceptions;
using Polly;
using Polly.Timeout;
using System.Collections.Generic;
using MassTransit;
using Play.Common.Logging;
using Play.Common.OpenTelemetry;






namespace Play.Inventory.Service
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


        // private const string AllowedOriginSetting = "AllowedOrigin";

        public Program(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMongo()
                    .AddMongoRepository<InventoryItem>("inventoryitems")
                    .AddMongoRepository<CatalogItem>("catalogitems")  
                    .AddMassTransitWithMessageBroker(Configuration)                 
                    /*.AddMassTransitWithMessageBroker(Configuration, retryConfigurator =>
                    {
                        retryConfigurator.Interval(3, TimeSpan.FromSeconds(5));
                        retryConfigurator.Ignore(typeof(UnknownItemException));
                    }) */
                    .AddJwtBearerAuthentication();

            //AddCatalogClient(services);  //This is the old way of adding the CatalogClient. We are now using the MassTransit client instead

            
            
            
            services.AddControllers();


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
                     options.OAuth2RedirectUrl("http://host.docker.internal:5004/swagger/oauth2-redirect.html");
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

            //app.UseHttpsRedirection();
            app.UseOpenTelemetryPrometheusScrapingEndpoint();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapPlayEconomyHealthChecks();
            });
        }

         //This is the old way of adding the CatalogClient. We are now using the MassTransit client instead it is only here for Learning purposes
        private static void AddCatalogClient(IServiceCollection services)
        {
            Random jitterer = new Random(); //This is a random number generator that we will use to add some randomness to the retry policy

            services.AddHttpClient<CatalogClient>(client => //We are using the HttpClient to make a GET request to the /items endpoint and get the items from the catalog service
            {
                client.BaseAddress = new Uri("https://localhost:5001");
            })
            .AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().WaitAndRetryAsync( //We are adding a retry policy to the HttpClient that will retry the request 5 times with an exponential backoff strategy
                5,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) //We are using an exponential backoff strategy to wait between retries. The first retry will wait 2 seconds, the second retry will wait 4 seconds, the third retry will wait 8 seconds, and so on
                                + TimeSpan.FromMilliseconds(jitterer.Next(0, 1000)), //We are adding some randomness to the retry policy to avoid all the clients retrying at the same time
                onRetry: (outcome, timespan, retryAttempt) => //We are logging the retries
                {
                    var serviceProvider = services.BuildServiceProvider(); //We are getting the ILogger<T> from the service provider
                    serviceProvider.GetService<ILogger<CatalogClient>>()? 
                        .LogWarning($"Delaying for {timespan.TotalSeconds} seconds, then making retry {retryAttempt}"); //We are logging the retry attempt
                }
            ))
            .AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().CircuitBreakerAsync(  //We are adding a circuit breaker policy to the HttpClient that will open the circuit if there are 3 consecutive failures and will close the circuit after 15 seconds
                3,
                TimeSpan.FromSeconds(15),
                onBreak: (outcome, timespan) => 
                {
                    var serviceProvider = services.BuildServiceProvider();
                    serviceProvider.GetService<ILogger<CatalogClient>>()?
                        .LogWarning($"Opening the circuit for {timespan.TotalSeconds} seconds..."); 
                },
                onReset: () =>  
                {
                    var serviceProvider = services.BuildServiceProvider();
                    serviceProvider.GetService<ILogger<CatalogClient>>()?
                        .LogWarning($"Closing the circuit...");
                }
            ))
            .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(1)); //We are adding a timeout policy to the HttpClient that will timeout the request after 1 second
        }
    }
}
