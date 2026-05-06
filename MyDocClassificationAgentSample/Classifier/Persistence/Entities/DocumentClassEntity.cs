using System;
using System.Collections.Generic;

namespace Classifier.Persistence.Entities;

/// <summary>
/// Represents a document class used to group related documents.
/// </summary>
public class DocumentClassEntity
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public ICollection<DocumentEntity> Documents { get; set; } = new List<DocumentEntity>();
}