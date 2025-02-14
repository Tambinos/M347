using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using Microsoft.OpenApi.Models;
using Play.Catalog.Service.Entities;
using Play.Common.HealthChecks;
using Play.Common.Identity;
using Play.Common.MassTransit;
using Play.Common.MongoDB;
using Play.Common.Settings;
using Play.Common.Logging;
using Play.Common.OpenTelemetry;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;


namespace Play.Catalog.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run(); //We are using the CreateHostBuilder method to create the host and run the app
        }

        public static IHostBuilder CreateHostBuilder(string[] args) => //We are using the CreateHostBuilder method to create the host and run the app
            Host.CreateDefaultBuilder(args) //
                .ConfigureWebHostDefaults(webBuilder => //We are using the ConfigureWebHostDefaults method to configure the web host 
                {
                    webBuilder.UseStartup<Program>(); //We are using the UseStartup method to specify the startup class
                });


        private ServiceSettings serviceSettings; //We are using the ServiceSettings class to store the service settings see Play.Common/src/Play.Common/Settings/ServiceSettings.cs

        // Configuration of the services defined in the ConfigureServices method, allowing us to pass values as parameters
        public Program(IConfiguration configuration) //We are using dependency injection to get the configuration see Play.Common/src/Play.Common/Settings/ServiceSettings.cs
        {
            Configuration = configuration; //We are using the Configuration property to store the configuration
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) 
        {
            //Remove for real Production
            IdentityModelEventSource.ShowPII = true;
            //retrieves the values from appsettings.json
            serviceSettings = Configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>(); //We are using dependency injection to get the ServiceSettings see Play.Common/src/Play.Common/Settings/ServiceSettings.cs
            
            services.AddMongo() //Brings the MongoDB into the app
                    .AddMongoRepository<Item>("items") //Brings the MongoDB Repository into the app see Play.Common/src/Play.Common/MongoDB/Extensions.cs
                    .AddMassTransitWithMessageBroker(Configuration) //Brings the MassTransit into the app see Play.Common/src/Play.Common/MassTransit/Extensions.cs
                    .AddJwtBearerAuthentication(); //Brings the JwtBearerAuthentication into the app see Play.Common/src/Play.Common/Identity/Extensions.cs

            services.AddAuthorization(options => //Brings the Authorization into the app see Play.Common/src/Play.Common/Identity/Extensions.cs
            {
                options.AddPolicy(Policies.Read, policy => //We are using the AddPolicy method to add the Read policy
                {
                    policy.RequireRole("Admin"); //We are using the RequireRole method to specify that only users with the Admin role can access this policy
                    policy.RequireClaim("scope", "catalog.readaccess", "catalog.fullaccess"); //We are using the RequireClaim method to specify that the user must have the catalog.readaccess or catalog.fullaccess scope
                });

                options.AddPolicy(Policies.Write, policy =>   //We are using the AddPolicy method to add the Write policy
                {
                    policy.RequireRole("Admin"); //We are using the RequireRole method to specify that only users with the Admin role can access this policy
                    policy.RequireClaim("scope", "catalog.writeaccess", "catalog.fullaccess"); //We are using the RequireClaim method to specify that the user must have the catalog.writeaccess or catalog.fullaccess scope
                });
               

            });


            services.AddControllers(options => //Brings the Controllers into the app 
            {
                options.SuppressAsyncSuffixInActionNames = false; //We are using the SuppressAsyncSuffixInActionNames property to disable the async suffix in the controller actions
            });


            //Swagger for the API
            services.AddSwaggerGen(options =>
            {
                var scheme = new OpenApiSecurityScheme //We are using the OpenApiSecurityScheme to configure the OAuth2 authentication
                {
                    In = ParameterLocation.Header, //We are using the In property to specify that the token will be passed in the header
                    Name = "Authorization", //We are using the Name property to specify that the header name will be Authorization
                    Flows = new OpenApiOAuthFlows //We are using the Flows property to specify the OAuth2 flows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow //We are using the AuthorizationCode property to specify the Authorization Code flow
                        {
                            AuthorizationUrl = new Uri(Configuration.GetSection("Auth:Swagger:AuthorizationUrl").Get<string>()), //We are using the AuthorizationUrl property to specify the authorization URL
                            TokenUrl = new Uri(Configuration.GetSection("Auth:Swagger:TokenUrl").Get<string>()) //We are using the TokenUrl property to specify the token URL
                        }
                    },
                    Type = SecuritySchemeType.OAuth2 //We are using the Type property to specify that the type of the security scheme is OAuth2
                };

                options.AddSecurityDefinition("OAuth", scheme); //We are using the AddSecurityDefinition method to add the OAuth2 security scheme

                options.AddSecurityRequirement(new OpenApiSecurityRequirement //We are using the AddSecurityRequirement method to add the security requirements
                {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference { Id = "OAuth", Type = ReferenceType.SecurityScheme } //We are using the Reference property to specify the reference to the OAuth2 security scheme
                            },
                            new List<string> { } //We are using the List<string> to specify the scopes
                        }
                });
            });
         
            //Brings the HealthChecks into the app
            services.AddHealthChecks()
                    .AddMongoDb();
            //Brings the Metric into the app
            //Brings the OpenTelemetry into the app
            services.AddSeqLogging(Configuration)
                    .AddTracing(Configuration)
                    .AddMetrics(Configuration);     
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //Remove || env.IsProduction() for real Production. Made vor Docker otherwise troubleshooting in Docker for Learning is difficult
            if (env.IsDevelopment() || env.IsProduction())
            {
              
                app.UseDeveloperExceptionPage(); //We are using the UseDeveloperExceptionPage method to show the developer exception page that contains the exception details
                app.UseSwagger() //We are using the UseSwagger method to enable the Swagger middleware to show the Swagger UI
                .UseSwaggerUI(options => 
                {
                    options.OAuthClientId("api-swagger"); //We are using the OAuthClientId method to specify the client ID
                    options.OAuthScopes("profile", "openid", "catalog.fullaccess",
                        "catalog.readaccess",
                        "catalog.writeaccess",
                        "inventory.fullaccess",
                        "trading.fullaccess",
                        "IdentityServerApi",
                        "roles"); //We are using the OAuthScopes method to specify the scopes
                    options.OAuthUsePkce(); //We are using the OAuthUsePkce method to enable the PKCE flow that is required by IdentityServer because we are using the Authorization Code flow
                    options.OAuth2RedirectUrl("http://host.docker.internal:5000/swagger/oauth2-redirect.html"); //We are using the OAuth2RedirectUrl method to specify the redirect URL that will be used by the Swagger UI
                    options.EnablePersistAuthorization();

                    //options.InjectStylesheet("/content/swagger-extras.css");
                });

           // allows access to the API from other domains
            app.UseCors(builder => //We are using the UseCors method to enable the CORS middleware to allow cross-origin requests to the API 
             {
                 var allowedOrigins = Configuration.GetSection("AllowedOrigins").Get<string[]>();
                 builder.WithOrigins(allowedOrigins) //allows access to the API from other domains
                     .AllowAnyHeader()  //allows any header AS EXAMPLE: Authorization header
                     .AllowAnyMethod(); //allows Put, Post, Delete, Get etc.
             });
        

        }

        //app.UseHttpsRedirection();  // Removed because in microservices we are in a private network and we don't need https redirection it makes the app slower
        app.UseOpenTelemetryPrometheusScrapingEndpoint();
        // Be aware of the order of the middleware
        app.UseRouting(); //Routing is used to route the request to the correct controller
        app.UseAuthentication(); //Authentication is used to authenticate the user
        app.UseAuthorization(); //Authorization is used to authorize the user


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers(); //MapControllers is used to map the controllers
                endpoints.MapPlayEconomyHealthChecks(); //MapPlayEconomyHealthChecks is used to map the health checks
            });
        }

    }
}
