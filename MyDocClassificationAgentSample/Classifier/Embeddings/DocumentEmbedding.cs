namespace Classifier.Embeddings;

/// <summary>
/// Stores the embedding data for a single document.
/// Contains file name, class name, embedding vector, vector size, and creation time.
/// </summary>
public sealed class DocumentEmbedding
{
    public string FileName { get; set; } = string.Empty;

    public string ClassName { get; set; } = string.Empty;

    public int VectorSize { get; set; }

    public float[] Embedding { get; set; } = Array.Empty<float>();

    public DateTime CreatedUtc { get; set; }
}