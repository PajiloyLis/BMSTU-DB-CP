using Microsoft.EntityFrameworkCore;

namespace Project.HttpServer.Extensions;

public static class HostExtension
{
    public static async Task<IHost> MigrateDatabaseAsync<TContext>(this IHost host) where TContext : DbContext
    {
        await using var serviceScope = host.Services.CreateAsyncScope();
        await using var context = serviceScope.ServiceProvider.GetRequiredService<TContext>();
        // await context.Database.MigrateAsync();

        return host;
    }
}