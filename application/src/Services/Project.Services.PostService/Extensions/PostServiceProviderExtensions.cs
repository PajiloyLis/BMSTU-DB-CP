using Microsoft.Extensions.DependencyInjection;
using Project.Core.Services;

namespace Project.Services.PostService.Extensions;

public static class PostServiceProviderExtensions
{
    public static IServiceCollection AddPostService(this IServiceCollection services)
    {
        services.AddScoped<IPostService, PostService>();
        return services;
    }
}