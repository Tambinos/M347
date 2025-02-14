using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorClient;
using MudBlazor.Services;
using MudBlazor;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Play.Frontend.MessageHandler;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

//Damit das Toaken im Header mitgegeben wird. 
builder.Services.AddTransient<CustomAuthorizationMessageHandler>();


builder.Services.AddHttpClient("frontend2", client =>
            {
                client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
            })
            .AddHttpMessageHandler<CustomAuthorizationMessageHandler>();

builder.Services.AddScoped(
                sp => sp.GetService<IHttpClientFactory>().CreateClient("frontend2"));


//Macht das Login 
builder.Services.AddOidcAuthentication(options =>
{
    builder.Configuration.Bind("Oidc", options.ProviderOptions);
    options.UserOptions.RoleClaim = "role";

});



builder.Services
.AddScoped(services => services.GetRequiredService<IHttpClientFactory>()
.CreateClient("frontend2"));

builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.NewestOnTop = true;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 10000;
    config.SnackbarConfiguration.HideTransitionDuration = 500;
    config.SnackbarConfiguration.ShowTransitionDuration = 500;
    config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;


});




await builder.Build().RunAsync();


