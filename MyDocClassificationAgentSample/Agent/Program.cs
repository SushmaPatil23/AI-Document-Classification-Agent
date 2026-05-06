using Spectre.Console;
using Agent;
using Agent.Similarity;
using Classifier;
using Classifier.Embeddings;
using Classifier.PreProcessing;
using Classifier.Persistence;
using Classifier.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent;

/// <summary>
/// Entry point of the agent application that handles user interaction and classification flow.
/// </summary>
class Program
{
    /// <summary>
    /// Configures services, loads data, and starts the agent interface.
    /// </summary>
    static async Task Main()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
            builder.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
        });

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION")
                       ?? configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ClassifierDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
            options.LogTo(_ => { });
        });

        services.AddScoped<IEmbeddingRepository, SqlEmbeddingRepository>();
        services.AddScoped<IDocumentPreprocessor, ExternalDocumentPreprocessor>();

        services.AddScoped<IEmbeddingGenerator>(provider =>
        {
            var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            return new OpenAiEmbeddingGenerator(apiKey!);
        });

        services.AddScoped<DocumentClassificationRunner>();
        services.AddScoped<EmbeddingBuildService>();

        var provider = services.BuildServiceProvider();
        using (var dbScope = provider.CreateScope())
        {
            var context = dbScope.ServiceProvider.GetRequiredService<ClassifierDbContext>();
            context.Database.Migrate();
        }

        Console.OutputEncoding = Encoding.UTF8;

        Console.WriteLine("Agent Mode");
        Console.WriteLine("-----------");
        Console.WriteLine("Type your query and press Enter.");
        Console.WriteLine("Type 'menu' to return to main menu.");
        Console.WriteLine("Type 'exit' or 'E' to quit.");
        Console.WriteLine();

        var baseDir = AppContext.BaseDirectory;

        var solutionRoot = Directory.GetParent(baseDir)!
                                   .Parent!
                                   .Parent!
                                   .Parent!
                                   .Parent!
                                   .FullName;

        var documentsRoot = Path.Combine(
            solutionRoot,
            "Classifier",
            "Documents"
        );

        if (!Directory.Exists(documentsRoot))
        {
            Console.WriteLine($"Documents directory not found at:\n{documentsRoot}");
            return;
        }

        using var scope = provider.CreateScope();

        var repository = scope.ServiceProvider
            .GetRequiredService<IEmbeddingRepository>();

        var centroidEntities = await repository.GetAllCentroidsAsync();

        var centroids = centroidEntities
    .Select(c => new ClassCentroid
    {
        ClassName = c.Class.Name,
        Embedding = c.Centroid,
        VectorSize = c.Centroid.Length,
        CreatedUtc = c.CreatedAt
    })
    .ToList();

        if (!centroids.Any())
        {
            Console.WriteLine("No centroids found in database.");
            return;
        }

        IEmbeddingGenerator embedder =
            new OpenAiEmbeddingGenerator(
                Environment.GetEnvironmentVariable("OPENAI_API_KEY")!
            );

        var intentService = new AgentIntentEmbeddingService(embedder);
        var similarityMetric = new CosineSimilarityMetric();
        var matcher = new AgentSimilarityMatcher(similarityMetric);

        var agentRunner = new AgentRunner(
            intentService,
            matcher,
            centroids,
            repository
        );

        Console.WriteLine("[Agent] Reloading centroids and embeddings...");
        Console.WriteLine("[Agent] Ready for queries.");
        Console.WriteLine("--------------------------------");

        while (true)
        {
            int startupChoice = ShowStartupMenu();
            await HandleRebuildAsync(provider, startupChoice, documentsRoot);
            await RunQueryLoop(agentRunner);
        }
    }

    /// <summary>
    /// Runs the interactive loop for user queries.
    /// </summary>
    static async Task RunQueryLoop(AgentRunner agentRunner)
    {
        while (true)
        {
            Console.WriteLine();
            Console.Write(">> ");

            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("[Agent] Please enter a non-empty query.");
                continue;
            }

            if (input.Equals("menu", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Returning to main menu...");
                break;
            }

            if (input.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
                input.Equals("E", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Exiting Agent. Goodbye.");
                Environment.Exit(0);
            }

            await agentRunner.RunAsync(input);
        }
    }

    /// <summary>
    /// Displays startup options for embedding operations.
    /// </summary>
    static int ShowStartupMenu()
    {
        Console.WriteLine("===== Agent Startup Options =====");
        Console.WriteLine("1. Use existing embeddings");
        Console.WriteLine("2. Rebuild ALL embeddings");
        Console.WriteLine("3. Rebuild embeddings for a CLASS");
        Console.WriteLine("4. Rebuild embedding for a FILE");
        Console.WriteLine("5. Upload document and classify");
        Console.Write("Select option: ");

        var input = Console.ReadLine();
        return int.TryParse(input, out var choice) ? choice : 1;
    }

    /// <summary>
    /// Handles rebuild operations based on user choice.
    /// </summary>
    static async Task HandleRebuildAsync(
        IServiceProvider provider,
        int choice,
        string documentsRoot)
    {
        var rebuildService = provider.GetRequiredService<EmbeddingBuildService>();

        switch (choice)
        {
            case 1:
                Console.WriteLine("[Agent] Using existing embeddings.");
                break;

            case 2:
                Console.WriteLine("[Agent] Rebuilding ALL embeddings...");
                await rebuildService.BuildAllAsync(documentsRoot);
                Console.WriteLine("[Agent] Full embedding rebuild completed.");
                break;

            case 3:
                Console.Write("Enter class name: ");
                var className = Console.ReadLine()!;

                await rebuildService.BuildClassAsync(documentsRoot, className);
                break;

            case 4:
                Console.Write("Enter class name: ");
                var cls = Console.ReadLine()!;

                Console.Write("Enter file name: ");
                var file = Console.ReadLine()!;

                var classFolder = Path.Combine(documentsRoot, cls);

                if (!Directory.Exists(classFolder))
                {
                    Console.WriteLine($"[Agent] Class folder not found: {cls}");
                    return;
                }

                var matchedFile = Directory.GetFiles(classFolder)
                    .FirstOrDefault(f =>
                        Path.GetFileNameWithoutExtension(f)
                            .Equals(file, StringComparison.OrdinalIgnoreCase));

                if (matchedFile == null)
                {
                    Console.WriteLine($"[Agent] File '{file}' not found in class '{cls}'.");
                    return;
                }

                await rebuildService.BuildFileAsync(matchedFile, cls);
                break;

            case 5:
                Console.WriteLine("[Agent] Select a document to classify.");

                var selectedFile = BrowseForFile();

                await rebuildService.ClassifyUploadedDocumentAsync(selectedFile);
                break;

            default:
                Console.WriteLine("Invalid option. Using existing embeddings.");
                break;
        }
    }

    /// <summary>
    /// Allows user to browse and select a file from the system.
    /// </summary>
    static string BrowseForFile()
    {
        string[] startFolders =
        {
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads")
        };

        var currentFolder = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select starting folder")
                .AddChoices(startFolders!)
        );

        while (true)
        {
            var directories = Directory.GetDirectories(currentFolder);

            var files = Directory.GetFiles(currentFolder)
                .Where(f =>
                    f.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) ||
                    f.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ||
                    f.EndsWith(".docx", StringComparison.OrdinalIgnoreCase) ||
                    f.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            var options = directories
                .Select(d => "📁 " + Path.GetFileName(d)!)
                .Concat(files.Select(f => Path.GetFileName(f)!))
                .Concat(new[] { ".. (Go up)" })
                .ToArray();

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"Browsing: {currentFolder}")
                    .PageSize(10)
                    .AddChoices(options)
            );

            if (choice == ".. (Go up)")
            {
                var parent = Directory.GetParent(currentFolder);
                if (parent != null)
                    currentFolder = parent.FullName;

                continue;
            }

            if (choice.StartsWith("📁 "))
            {
                var folderName = choice.Replace("📁 ", "");
                currentFolder = Path.Combine(currentFolder, folderName);
                continue;
            }

            return Path.Combine(currentFolder, choice);
        }
    }
}
