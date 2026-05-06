using System;

namespace Classifier.Persistence.Entities;

/// <summary>
/// Stores embedding data of a document in the database.
/// </summary>
public class DocumentEmbeddingEntity
{
    public int Id { get; set; }

    public int DocumentId { get; set; }

    public float[] Embedding { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DocumentEntity Document { get; set; } = null!;
}