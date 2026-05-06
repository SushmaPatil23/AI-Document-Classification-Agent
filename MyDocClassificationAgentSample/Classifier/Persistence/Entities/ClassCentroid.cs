using System;

namespace Classifier.Persistence.Entities;

/// <summary>
/// Stores centroid data for a document class in the database.
/// </summary>
public class ClassCentroidEntity
{
    public int Id { get; set; }

    public int ClassId { get; set; }

    public float[] Centroid { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DocumentClassEntity Class { get; set; } = null!;
}