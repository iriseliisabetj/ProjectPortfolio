using ConsoleApp;
using DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var serviceCollection = new ServiceCollection();
ConfigureServices(serviceCollection);
var serviceProvider = serviceCollection.BuildServiceProvider();

using (var scope = serviceProvider.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();

    var configRepositoryDb = scope.ServiceProvider.GetRequiredService<ConfigRepositoryDb>();
    var gameRepositoryDb = scope.ServiceProvider.GetRequiredService<GameRepositoryDb>();

    var configRepositoryJson = scope.ServiceProvider.GetRequiredService<ConfigRepositoryJson>();
    var gameRepositoryJson = scope.ServiceProvider.GetRequiredService<GameRepositoryJson>();

    GameController.SetRepositories(configRepositoryDb, gameRepositoryDb, configRepositoryJson, gameRepositoryJson);
    
    Menus.MainMenu.Run();
}

void ConfigureServices(IServiceCollection services)
{
    services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite($"Data Source={Path.Combine(FileHelper.BasePath, "app.db")}")
            .EnableDetailedErrors()
            .EnableSensitiveDataLogging());

    services.AddScoped<ConfigRepositoryDb>();
    services.AddScoped<GameRepositoryDb>();

    services.AddScoped<ConfigRepositoryJson>();
    services.AddScoped<GameRepositoryJson>();

    services.AddLogging(configure =>
    {
        configure.AddConsole();
        configure.SetMinimumLevel(LogLevel.Debug);
    });
}