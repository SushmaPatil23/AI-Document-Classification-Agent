using System.Collections.Generic;
using System.Threading.Tasks;
using Classifier.Persistence.Entities;

namespace Classifier.Persistence.Repositories;

/// <summary>
/// Defines methods to store, retrieve, and manage document embeddings and class centroids.
/// </summary>
public interface IEmbeddingRepository
{
    /// <summary>
    /// Checks if an embedding already exists for the given file.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <returns>True if embedding exists, otherwise false.</returns>
    Task<bool> EmbeddingExistsAsync(string fileName);

    /// <summary>
    /// Saves a document embedding to the database.
    /// </summary>
    /// <param name="className">Class name of the document.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="embedding">Embedding vector as float array.</param>
    Task SaveDocumentEmbeddingAsync(
        string className,
        string fileName,
        float[] embedding);

    /// <summary>
    /// Retrieves all class centroids.
    /// </summary>
    /// <returns>List of class centroid entities.</returns>
    Task<List<ClassCentroidEntity>> GetAllCentroidsAsync();

    /// <summary>
    /// Retrieves all document embeddings for a given class.
    /// </summary>
    /// <param name="className">Class name.</param>
    /// <returns>List of document embedding entities.</returns>
    Task<List<DocumentEmbeddingEntity>>
        GetDocumentEmbeddingsByClassAsync(string className);

    /// <summary>
    /// Recomputes centroids based on current document embeddings.
    /// </summary>
    Task RecomputeCentroidsAsync();

    /// <summary>
    /// Deletes embedding for a specific file.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    Task DeleteEmbeddingForFileAsync(string fileName);

    /// <summary>
    /// Deletes all document embeddings.
    /// </summary>
    Task DeleteAllEmbeddingsAsync();

    /// <summary>
    /// Deletes all class centroids.
    /// </summary>
    Task DeleteAllCentroidsAsync();
}