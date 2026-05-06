using System;

namespace Classifier.Persistence.Entities;

/// <summary>
/// Represents a document stored in the database with its class and embedding details.
/// </summary>
public class DocumentEntity
{
    public int Id { get; set; }

    public string FileName { get; set; } = null!;

    public int ClassId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DocumentClassEntity Class { get; set; } = null!;

    public DocumentEmbeddingEntity? Embedding { get; set; }
}