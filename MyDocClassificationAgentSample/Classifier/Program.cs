using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Classifier;
using Classifier.Persistence;
using Classifier.PreProcessing;
using Classifier.Embeddings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Classifier.Persistence.Repositories;

/// <summary>
/// Entry point of the application.
/// Sets up dependency injection and starts the document classification process.
/// </summary>
class Program
{
    /// <summary>
    /// Main method that configures services and runs the classification pipeline.
    /// </summary>
    static async Task Main()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Warning);
        });

        services.AddSingleton<IDocumentPreprocessor, ExternalDocumentPreprocessor>();

        var key = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        Console.WriteLine($"OPENAI_API_KEY loaded? {(string.IsNullOrEmpty(key) ? "NO" : "YES")}");

        services.AddSingleton<IEmbeddingGenerator>(
            _ => new OpenAiEmbeddingGenerator(
                Environment.GetEnvironmentVariable("OPENAI_API_KEY")!));

        services.AddSingleton<DocumentClassificationRunner>();

        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION")
                      ?? configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ClassifierDbContext>(options =>
            options.UseSqlServer(connectionString,
                sqlOptions => { }));

        services.AddScoped<IEmbeddingRepository, SqlEmbeddingRepository>();

        var provider = services.BuildServiceProvider();
        using (var scope = provider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ClassifierDbContext>();
           
                context.Database.Migrate();
           
        }

        var runner = provider.GetRequiredService<DocumentClassificationRunner>();

        var basePath = Directory.GetCurrentDirectory();

        await runner.RunAsync(
            Path.Combine(basePath, "Documents"));
    }
}