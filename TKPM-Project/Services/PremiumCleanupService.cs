using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TKPM_Project.Models;
using TKPM_Project.Repositories;

public class PremiumCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public PremiumCleanupService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var premiumRepo = scope.ServiceProvider.GetRequiredService<IUserPremiumRepository>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var expiredUsers = await premiumRepo.GetAllExpiredAsync();

                foreach (var expired in expiredUsers)
                {
                    var user = await userManager.FindByIdAsync(expired.UserId);
                    if (user != null)
                    {
                        await userManager.RemoveFromRoleAsync(user, "Premium");
                        if (!await userManager.IsInRoleAsync(user, "User"))
                        {
                            await userManager.AddToRoleAsync(user, "User");
                        }
                    }
                }
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken); // Chạy mỗi giờ
        }
    }
}
