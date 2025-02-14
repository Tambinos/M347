using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Play.Identity.Service.Entities;
using Play.Identity.Service.Settings;

namespace Play.Identity.Service.HostedServices
{
    public class IdentitySeedHostedService : IHostedService // this is the class that is called in the Play.Identity project
    {
        private readonly IServiceScopeFactory serviceScopeFactory; // this is the IServiceScopeFactory that is passed in from the Play.Identity project
        private readonly IdentitySettings settings; 

        public IdentitySeedHostedService(
            IServiceScopeFactory serviceScopeFactory,
            IOptions<IdentitySettings> identityOptions)
        {
            this.serviceScopeFactory = serviceScopeFactory; 
            settings = identityOptions.Value;
        }

        public async Task StartAsync(CancellationToken cancellationToken) //
        {
            using var scope = serviceScopeFactory.CreateScope(); // using is a C# keyword that ensures that the IDisposable object is disposed of when the using block is exited

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>(); 
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>(); 
            await CreateRoleIfNotExistsAsync(Roles.Admin, roleManager); // Roles.Admin is a constant that is defined in the Roles.cs file in the Play.Identity project
            await CreateRoleIfNotExistsAsync(Roles.Player, roleManager); // Roles.Player is a constant that is defined in the Roles.cs file in the Play.Identity project

            var adminUser = await userManager.FindByEmailAsync(settings.AdminUserEmail); // settings.AdminUserEmail is a string that is defined in the appsettings.json file in the Play.Identity project

            if (adminUser == null)
            {
                adminUser = new ApplicationUser // ApplicationUser is a class that is defined in the ApplicationUser.cs file in the Play.Identity project
                {
                    UserName = settings.AdminUserEmail,
                    Email = settings.AdminUserEmail
                };

                await userManager.CreateAsync(adminUser, settings.AdminUserPassword); // settings.AdminUserPassword is a string that is defined in the appsettings.json file in the Play.Identity project or in the secrets.json 
                await userManager.AddToRoleAsync(adminUser, Roles.Admin);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private static async Task CreateRoleIfNotExistsAsync(
            string role,
            RoleManager<ApplicationRole> roleManager
        )
        {
            var roleExists = await roleManager.RoleExistsAsync(role);

            if (!roleExists)
            {
                await roleManager.CreateAsync(new ApplicationRole { Name = role });
            }
        }
    }
}