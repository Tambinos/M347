using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace Play.Frontend.MessageHandler
{
    public class CustomAuthorizationMessageHandler : AuthorizationMessageHandler
    {
        public CustomAuthorizationMessageHandler(IAccessTokenProvider provider, NavigationManager navigation)
            : base(provider, navigation)
        {
            ConfigureHandler(
                authorizedUrls: new[] { "http://host.docker.internal:5000", "http://host.docker.internal:5002", "http://host.docker.internal:5004", "http://host.docker.internal:5006" },
                scopes: new[] { "openid",
          "profile",
          "catalog.fullaccess",
          "catalog.readaccess",
          "catalog.writeaccess",
          "inventory.fullaccess",
          "trading.fullaccess",
          "IdentityServerApi",
          "roles" });
        }
    }
}