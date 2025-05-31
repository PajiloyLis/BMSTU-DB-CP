using Database.Context;
using Project.HttpServer.Extensions;
using Serilog;

namespace Project.HttpServer;

public static class Program
{
    public static async Task Main(string[] args)
    {
        try
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            var logPath = configuration["Serilog:WriteTo:1:Args:path"];
            var fullLogPath = Path.GetFullPath(logPath);
            var logDir = Path.GetDirectoryName(fullLogPath);

            if (!Directory.Exists(logDir))
                Directory.CreateDirectory(logDir);

            // Проверка записи
            File.WriteAllText(Path.Combine(logDir, "access-test.tmp"), "test");
            File.Delete(Path.Combine(logDir, "access-test.tmp"));
            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateBootstrapLogger();

            var webHost = CreateHostBuilder(args).Build();

            await webHost.MigrateDatabaseAsync<ProjectDbContext>();

            await webHost.RunAsync();
        }
        catch (UnauthorizedAccessException e)
        {
            Console.Error.WriteLine("Ошибка: Нет прав на запись в папку логов!", e);
            Environment.Exit(1);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        var webHost = Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
                webBuilder.ConfigureKestrel(serverOptions =>
                {
                    serverOptions.Limits.MaxRequestBodySize = null;
                    serverOptions.Limits.MaxResponseBufferSize = null;
                });
            });

        return webHost;
    }
}