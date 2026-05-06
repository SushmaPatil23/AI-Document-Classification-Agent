using Classifier.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Linq;

namespace Classifier.Persistence;

/// <summary>
/// Database context for managing document classes, documents, embeddings, and centroids.
/// </summary>
public class ClassifierDbContext : DbContext
{
    /// <summary>
    /// Initializes the database context with the given options.
    /// </summary>
    /// <param name="options">Database configuration options.</param>
    public ClassifierDbContext(DbContextOptions<ClassifierDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Table for document classes.
    /// </summary>
    public DbSet<DocumentClassEntity> DocumentClasses => Set<DocumentClassEntity>();

    /// <summary>
    /// Table for documents.
    /// </summary>
    public DbSet<DocumentEntity> Documents => Set<DocumentEntity>();

    /// <summary>
    /// Table for document embeddings.
    /// </summary>
    public DbSet<DocumentEmbeddingEntity> DocumentEmbeddings => Set<DocumentEmbeddingEntity>();

    /// <summary>
    /// Table for class centroids.
    /// </summary>
    public DbSet<ClassCentroidEntity> ClassCentroids => Set<ClassCentroidEntity>();

    /// <summary>
    /// Configures relationships, indexes, and constraints for the database.
    /// </summary>
    /// <param name="modelBuilder">Used to build the database model.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        string prefix = "Team_404_";

        modelBuilder.Entity<DocumentClassEntity>()
            .ToTable(prefix + "DocumentClasses")
            .HasIndex(c => c.Name)
            .IsUnique();

        modelBuilder.Entity<DocumentEntity>()
            .ToTable(prefix + "Documents")
            .HasOne(d => d.Class)
            .WithMany(c => c.Documents)
            .HasForeignKey(d => d.ClassId);

        modelBuilder.Entity<DocumentEmbeddingEntity>()
            .ToTable(prefix + "DocumentEmbeddings")
            .HasOne(e => e.Document)
            .WithOne(d => d.Embedding)
            .HasForeignKey<DocumentEmbeddingEntity>(e => e.DocumentId);

        modelBuilder.Entity<ClassCentroidEntity>()
        .ToTable(prefix + "ClassCentroids");

        modelBuilder.Entity<ClassCentroidEntity>()
            .HasKey(c => c.Id);

        modelBuilder.Entity<ClassCentroidEntity>()
            .HasOne(c => c.Class)
            .WithMany()
            .HasForeignKey(c => c.ClassId);

        modelBuilder.Entity<DocumentEmbeddingEntity>()
    .Property(e => e.Embedding)
    .HasConversion(
        v => string.Join(',', v),
        v => v.Split(',', StringSplitOptions.None)
              .Select(float.Parse)
              .ToArray())
    .Metadata.SetValueComparer(new ValueComparer<float[]>(
        (a, b) => a != null && b != null && a.SequenceEqual(b),
        v => v.Aggregate(0, (hash, val) => HashCode.Combine(hash, val.GetHashCode())),
        v => v.ToArray()
    ));

        modelBuilder.Entity<ClassCentroidEntity>()
     .Property(c => c.Centroid)
     .HasConversion(
         v => string.Join(',', v),
         v => v.Split(',', StringSplitOptions.None)
               .Select(float.Parse)
               .ToArray())
     .Metadata.SetValueComparer(new ValueComparer<float[]>(
         (a, b) => a != null && b != null && a.SequenceEqual(b),
         v => v.Aggregate(0, (hash, val) => HashCode.Combine(hash, val.GetHashCode())),
         v => v.ToArray()
     ));

    }
}