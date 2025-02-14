using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Reflection;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Play.Common.HealthChecks;
using Play.Common.MassTransit;
using Play.Common.Settings;
using Play.Common.Logging;
using Play.Common.OpenTelemetry;
using Play.Identity.Service.Entities;
using Play.Identity.Service.Exceptions;
using Play.Identity.Service.HostedServices;
using Play.Identity.Service.Settings;
using System.Security.Cryptography.X509Certificates;
using MassTransit;
using Microsoft.AspNetCore.HttpOverrides;














namespace Play.Identity.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .
                ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Program>();
                });


        public Program(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String)); //Guids are stored as strings in MongoDb
            var serviceSettings = Configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>(); 
            var mongoDbSettings = Configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>(); //MongoDbSettings are stored in appsettings.json
            var identityServerSettings = Configuration.GetSection(nameof(IdentityServerSettings)).Get<IdentityServerSettings>(); //IdentityServerSettings are stored in appsettings.json

            services.Configure<IdentitySettings>(Configuration.GetSection(nameof(IdentitySettings))) 
                .AddDefaultIdentity<ApplicationUser>() //AddDefaultIdentity<ApplicationUser>() is from Microsoft.AspNetCore.Identity.UI
                .AddRoles<ApplicationRole>() //AddRoles<ApplicationRole>() is from Microsoft.AspNetCore.Identity.UI
                .AddMongoDbStores<ApplicationUser, ApplicationRole, Guid> 
                (
                    mongoDbSettings.ConnectionString,
                    serviceSettings.ServiceName
                );




            services.AddMassTransitWithRabbitMq(retryConfigurator =>
            {
                retryConfigurator.Interval(3, TimeSpan.FromSeconds(5));
                retryConfigurator.Ignore(typeof(UnknownUserException));
                retryConfigurator.Ignore(typeof(InsufficientFundsException));
            });

            services.AddIdentityServer(options =>
            {
                options.Events.RaiseSuccessEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseErrorEvents = true;
                options.KeyManagement.KeyPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            })
                .AddAspNetIdentity<ApplicationUser>()
                .AddInMemoryApiScopes(identityServerSettings.ApiScopes)
                .AddInMemoryApiResources(identityServerSettings.ApiResources)
                .AddInMemoryClients(identityServerSettings.Clients)
                .AddInMemoryIdentityResources(identityServerSettings.IdentityResources);

            services.AddLocalApiAuthentication();

            services.AddControllers();
            services.AddHostedService<IdentitySeedHostedService>();

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
                    options.OAuth2RedirectUrl("http://host.docker.internal:5002/swagger/oauth2-redirect.html");
                    options.EnablePersistAuthorization();

                    //options.InjectStylesheet("/content/swagger-extras.css");
                });


                //
                app.UseCors(builder => //UseCors() is from Microsoft.AspNetCore.Identity.UI this is for the CORS policy 
                {
                    var allowedOrigins = Configuration.GetSection("AllowedOrigins").Get<string[]>();

                    builder.WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod();

                });
            }





            //app.UseHttpsRedirection();

            app.UseStaticFiles(); //UseStaticFiles() is from Microsoft.AspNetCore.Identity.UI this is for the wwwroot folder
            app.UseOpenTelemetryPrometheusScrapingEndpoint(); //UseOpenTelemetryPrometheusScrapingEndpoint() is from Microsoft.Extensions.DependencyInjection this is for the Prometheus endpoint

            app.UseRouting();

            //Reihenfolge hier sehr wichtig

            app.UseIdentityServer(); //UseIdentityServer() is from Microsoft.AspNetCore.Identity.UI
            app.UseAuthorization(); //UseAuthorization() is from Microsoft.AspNetCore.Identity.UI
            app.UseCookiePolicy(new CookiePolicyOptions //UseCookiePolicy() is from Microsoft.AspNetCore.Identity.UI
            {
                MinimumSameSitePolicy = SameSiteMode.Lax //SameSiteMode.Lax is from Microsoft.AspNetCore.Http this means that the cookie is sent with "safe" requests, like GET

            });

            app.UseEndpoints(endpoints => 
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
                endpoints.MapPlayEconomyHealthChecks();
            });
        }
    }
}

