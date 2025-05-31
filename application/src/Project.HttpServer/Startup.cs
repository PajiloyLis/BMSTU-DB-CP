using Project.HttpServer.Extensions;

namespace Project.HttpServer;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; set; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddProjectControllers()
            .AddProjectSwaggerGen()
            .AddProjectDbRepositories()
            .AddProjectRedisCache(Configuration)
            .AddProjectAuthorization(Configuration)
            .AddProjectCors(Configuration.GetValue("AllowedHeaders", "AllowAllHeaders"))
            .AddProjectDbContext(Configuration)
            .AddProjectServices(Configuration);
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

        app.UseSwagger();
        app.UseSwaggerUI(options => { options.SwaggerEndpoint("/swagger/v1/swagger.json", "Project.HttpServer v1"); });

        app.UseRouting();
        app.UseCors("AllowAllHeaders");
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(builder => builder.MapControllers());
    }
}