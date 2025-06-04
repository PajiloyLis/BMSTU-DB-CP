using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Database.Context;
using Database.Repositories.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;
using Project.Core.Models;
using Project.Service.AuthorizationService.Configuration;
using Project.Services.CompanyService.Extensions;
using Project.Services.EducationService.Extensions;
using Project.Services.EmployeeService.Extensions;
using Project.Services.PositionHistoryService.Extensions;
using Project.Services.PositionService.Extensions;
using Project.Services.PostHistoryService.Extensions;
using Project.Services.PostService.Extensions;
using Project.Services.ScoreService.Extensions;
using Project.Services.AuthorizationService.Extensions;
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
        serviceCollection.Configure<JwtConfiguration>(configuration.GetSection("JwtConfiguration"));
        serviceCollection.Configure<PasswordHashingConfiguration>(configuration.GetSection("PasswordHashingConfiguration"));
        
        serviceCollection.AddCompanyService();
        serviceCollection.AddEducationService();
        serviceCollection.AddEmployeeService();
        serviceCollection.AddPostHistoryService();
        serviceCollection.AddPostService();
        serviceCollection.AddPositionService();
        serviceCollection.AddPositionHistoryService();
        serviceCollection.AddScoreService();
        serviceCollection.AddAuthorizationService();
        return serviceCollection;
    }

    public static IServiceCollection AddProjectDbContext(this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        var dataSource = new NpgsqlDataSourceBuilder(configuration.GetConnectionString("DefaultConnection"))
            .EnableDynamicJson().Build();

        serviceCollection.AddSingleton(dataSource);
        
        serviceCollection.AddDbContext<ProjectDbContext>((serviceProvider, options) =>
        {
            var httpContext = serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;
            var user = httpContext?.User;

            string dbUsername, dbPassword;
            
            if (user?.Identity?.IsAuthenticated != true)
            {
                dbUsername = "guest";
                dbPassword = "guest_password";
            }                
            else if (user.IsInRole("admin"))
            {
                // Администратор
                dbUsername = "admin_user";
                dbPassword = "admin_pass";
            }
            else
            {
                // Обычный сотрудник
                dbUsername = "employee";
                dbPassword = "employee_pass";
            }
            
            var connectionString = new NpgsqlConnectionStringBuilder(configuration.GetConnectionString("DefaultConnection"))
            {
                Username = dbUsername,
                Password = dbPassword
            }.ToString();

            options.UseNpgsql(connectionString)
                .LogTo(Console.WriteLine, LogLevel.Information)
                // .AddInterceptors(new TempViewConnectionInterceptor())
                .EnableSensitiveDataLogging();
            
            if (user?.Identity?.IsAuthenticated == true)
            {
                var userId = user.FindFirst(ClaimTypes.Email)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    options.UseNpgsql(connectionString, npgsqlOptions => 
                    {
                        npgsqlOptions.UseRelationalNulls();
                        npgsqlOptions.EnableRetryOnFailure();
                    });
                
                    // Добавляем перехватчик для установки user_id при открытии соединения
                    options.AddInterceptors(new RlsInterceptor(userId));
                }
            }
            
            // options.UseNpgsql(serviceProvider.GetRequiredService<NpgsqlDataSource>())
            //     .AddInterceptors(new TempViewConnectionInterceptor())
            //     .EnableSensitiveDataLogging() // Показывает значения параметров
            //     .LogTo(Console.WriteLine, LogLevel.Information)
        });
        
                

        return serviceCollection;
    }

    public static IServiceCollection AddProjectAuthorization(this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        // 1. Добавляем сервисы аутентификации (JWT Bearer)
        serviceCollection.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var jwtSection = configuration.GetSection("JwtConfiguration");
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSection["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSection["Audience"],
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSection["SecurityKey"])),
                    ClockSkew = TimeSpan.Zero,
                
                    // Указываем, что ClaimTypes.NameIdentifier будет содержать user_id
                    NameClaimType = ClaimTypes.Email
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        if (!string.IsNullOrEmpty(accessToken))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });
        
        // 2. Добавляем политики авторизации
        serviceCollection.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });
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