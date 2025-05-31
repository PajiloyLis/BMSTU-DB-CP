using Microsoft.Extensions.DependencyInjection;
using Project.Core.Repositories;
using Project.Database.Repositories;

namespace Database.Repositories.Extensions;

public static class DbRepositoriesProvider
{
    public static IServiceCollection AddDbRepositories(this IServiceCollection services)
    {
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IEducationRepository, EducationRepository>();
        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<IPositionRepository, PositionRepository>();
        services.AddScoped<IScoreRepository, ScoreRepository>();
        services.AddScoped<IPostHistoryRepository, PostHistoryRepository>();
        services.AddScoped<IPositionHistoryRepository, PositionHistoryRepository>();
        return services;
    }
}