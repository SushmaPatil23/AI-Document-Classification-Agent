using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Classifier.Persistence;

/// <summary>
/// Factory class used at design time to create the database context.
/// </summary>
public class ClassifierDbContextFactory
    : IDesignTimeDbContextFactory<ClassifierDbContext>
{
    /// <summary>
    /// Creates a new instance of ClassifierDbContext using configuration settings.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    /// <returns>A configured ClassifierDbContext instance.</returns>
    public ClassifierDbContext CreateDbContext(string[] args)
    {
        var basePath = Path.Combine(
            Directory.GetCurrentDirectory());

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        var optionsBuilder = new DbContextOptionsBuilder<ClassifierDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new ClassifierDbContext(optionsBuilder.Options);
    }
}