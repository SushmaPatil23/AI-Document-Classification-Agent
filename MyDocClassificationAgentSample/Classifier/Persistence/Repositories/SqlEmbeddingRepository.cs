using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Classifier.Persistence.Entities;

namespace Classifier.Persistence.Repositories;

/// <summary>
/// Handles database operations for embeddings and centroids using Entity Framework.
/// </summary>
public sealed class SqlEmbeddingRepository : IEmbeddingRepository
{
    /// <summary>
    /// Database context used for all operations.
    /// </summary>
    private readonly ClassifierDbContext _context;

    /// <summary>
    /// Initializes the repository with the database context.
    /// </summary>
    /// <param name="context">Database context.</param>
    public SqlEmbeddingRepository(ClassifierDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Checks if an embedding exists for the given file.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <returns>True if exists, otherwise false.</returns>
    public async Task<bool> EmbeddingExistsAsync(string fileName)
    {
        return await _context.DocumentEmbeddings
            .Include(e => e.Document)
            .AnyAsync(e => e.Document.FileName == fileName);
    }

    /// <summary>
    /// Recomputes centroids for all classes using stored embeddings.
    /// </summary>
    public async Task RecomputeCentroidsAsync()
    {
        var classes = await _context.DocumentClasses
            .Include(c => c.Documents)
            .ThenInclude(d => d.Embedding)
            .ToListAsync();

        foreach (var docClass in classes)
        {
            var embeddings = docClass.Documents
                .Where(d => d.Embedding != null)
                .Select(d => d.Embedding!.Embedding)
                .ToList();

            if (!embeddings.Any())
                continue;

            var dimension = embeddings.First().Length;
            var centroid = new float[dimension];

            foreach (var vector in embeddings)
            {
                for (int i = 0; i < dimension; i++)
                    centroid[i] += vector[i];
            }

            for (int i = 0; i < dimension; i++)
                centroid[i] /= embeddings.Count;

            await SaveClassCentroidAsync(docClass.Name, centroid);
        }
    }

    /// <summary>
    /// Gets all class centroids from the database.
    /// </summary>
    /// <returns>List of centroid entities.</returns>
    public async Task<List<ClassCentroidEntity>> GetAllCentroidsAsync()
    {
        return await _context.ClassCentroids
            .Include(c => c.Class)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all document embeddings for a specific class.
    /// </summary>
    /// <param name="className">Class name.</param>
    /// <returns>List of document embeddings.</returns>
    public async Task<List<DocumentEmbeddingEntity>>
        GetDocumentEmbeddingsByClassAsync(string className)
    {
        return await _context.DocumentEmbeddings
            .Include(e => e.Document)
            .ThenInclude(d => d.Class)
            .Where(e => e.Document.Class.Name == className)
            .ToListAsync();
    }

    /// <summary>
    /// Saves a document embedding and links it to its class.
    /// </summary>
    /// <param name="className">Class name.</param>
    /// <param name="fileName">File name.</param>
    /// <param name="embedding">Embedding data in float Vector.</param>
    public async Task SaveDocumentEmbeddingAsync(
        string className,
        string fileName,
        float[] embedding)
    {
        var docClass = await _context.DocumentClasses
            .FirstOrDefaultAsync(c => c.Name == className);

        if (docClass == null)
        {
            docClass = new DocumentClassEntity
            {
                Name = className
            };

            _context.DocumentClasses.Add(docClass);
            await _context.SaveChangesAsync();
        }

        var document = new DocumentEntity
        {
            FileName = fileName,
            ClassId = docClass.Id
        };

        _context.Documents.Add(document);
        await _context.SaveChangesAsync();

        var embeddingEntity = new DocumentEmbeddingEntity
        {
            DocumentId = document.Id,
            Embedding = embedding,
            CreatedAt = DateTime.UtcNow
        };

        _context.DocumentEmbeddings.Add(embeddingEntity);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Saves or updates the centroid for a class.
    /// </summary>
    /// <param name="className">Class name.</param>
    /// <param name="centroid">Centroid data in Float Vector.</param>
    public async Task SaveClassCentroidAsync(
        string className,
        float[] centroid)
    {
        var docClass = await _context.DocumentClasses
            .FirstAsync(c => c.Name == className);

        var existing = await _context.ClassCentroids
            .FirstOrDefaultAsync(c => c.ClassId == docClass.Id);

        if (existing == null)
        {
            var centroidEntity = new ClassCentroidEntity
            {
                ClassId = docClass.Id,
                Centroid = centroid,
                CreatedAt = DateTime.UtcNow
            };

            _context.ClassCentroids.Add(centroidEntity);
        }
        else
        {
            existing.Centroid = centroid;
            existing.CreatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes embedding and document for a specific file.
    /// </summary>
    /// <param name="fileName">File name.</param>
    public async Task DeleteEmbeddingForFileAsync(string fileName)
    {
        var embedding = await _context.DocumentEmbeddings
            .Include(e => e.Document)
            .FirstOrDefaultAsync(e => e.Document.FileName == fileName);

        if (embedding != null)
        {
            var document = embedding.Document;

            _context.DocumentEmbeddings.Remove(embedding);
            _context.Documents.Remove(document);

            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Deletes all document embeddings and documents.
    /// </summary>
    public async Task DeleteAllEmbeddingsAsync()
    {
        var embeddings = await _context.DocumentEmbeddings.ToListAsync();
        _context.DocumentEmbeddings.RemoveRange(embeddings);

        var documents = await _context.Documents.ToListAsync();
        _context.Documents.RemoveRange(documents);

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes all class centroids.
    /// </summary>
    public async Task DeleteAllCentroidsAsync()
    {
        var centroids = await _context.ClassCentroids.ToListAsync();
        _context.ClassCentroids.RemoveRange(centroids);

        await _context.SaveChangesAsync();
    }
}