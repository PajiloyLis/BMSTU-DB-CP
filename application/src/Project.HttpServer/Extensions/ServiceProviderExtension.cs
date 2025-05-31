using System.Text.Json;
using System.Text.Json.Serialization;
using Database.Context;
using Database.Repositories.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Npgsql;
using Project.Core.Models;
using Project.Services.CompanyService.Extensions;
using Project.Services.EducationService.Extensions;
using Project.Services.EmployeeService.Extensions;
using Project.Services.PositionHistoryService.Extensions;
using Project.Services.PositionService.Extensions;
using Project.Services.PostHistoryService.Extensions;
using Project.Services.PostService.Extensions;
using Project.Services.ScoreService.Extensions;
using StackExchange.Redis;

namespace Project.HttpServer.Extensions;

public static class ServiceProviderExtension
{
    public static IServiceCollection AddProjectControllers(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters
                .Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, false));
        });

        return serviceCollection;
    }

    public static IServiceCollection AddProjectCors(this IServiceCollection serviceCollection, string policyName)
    {
        serviceCollection.AddCors(options =>
        {
            options.AddPolicy(policyName,
                builder => { builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader(); });
        });
        return serviceCollection;
    }

    public static IServiceCollection AddProjectSwaggerGen(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "PPO Project.HttpServer", Version = "v1" });
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,

                BearerFormat = "JWT",
                Scheme = "Bearer"
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return serviceCollection;
    }

    public static IServiceCollection AddProjectServices(this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        serviceCollection.AddCompanyService();
        serviceCollection.AddEducationService();
        serviceCollection.AddEmployeeService();
        serviceCollection.AddPostHistoryService();
        serviceCollection.AddPostService();
        serviceCollection.AddPositionService();
        serviceCollection.AddPositionHistoryService();
        serviceCollection.AddScoreService();
        return serviceCollection;
    }

    public static IServiceCollection AddProjectDbContext(this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        var dataSource = new NpgsqlDataSourceBuilder(configuration.GetConnectionString("DefaultConnection"))
            .EnableDynamicJson().Build();

        serviceCollection.AddSingleton(dataSource);

        serviceCollection.AddDbContext<ProjectDbContext>((serviceProvider, options) =>
            options.UseNpgsql(serviceProvider.GetRequiredService<NpgsqlDataSource>()).AddInterceptors(new TempViewConnectionInterceptor()).EnableSensitiveDataLogging()  // Показывает значения параметров
                .LogTo(Console.WriteLine, LogLevel.Information));
        
                

        return serviceCollection;
    }

    public static IServiceCollection AddProjectAuthorization(this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        // TODO var jwtConfiguration = new JwtConfiguration();
        return serviceCollection;
    }

    public static IServiceCollection AddProjectDbRepositories(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddDbRepositories();
        return serviceCollection;
    }
    
    public static IServiceCollection AddProjectRedisCache(this IServiceCollection serviceCollection, 
        IConfiguration configuration)
    {
        var redisConfiguration = configuration.GetConnectionString("RedisConnection");
    
        if (!string.IsNullOrEmpty(redisConfiguration))
        {
            serviceCollection.AddSingleton<IConnectionMultiplexer>(sp => 
                ConnectionMultiplexer.Connect(redisConfiguration));
        }
        else
        {
            serviceCollection.AddDistributedMemoryCache();
        }
    
        return serviceCollection;
    }
}