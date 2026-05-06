/// <summary>
/// Stores the centroid data for a document class.
/// Contains the class name, embedding vector, vector size, and creation time.
/// </summary>
public sealed class ClassCentroid
{
    public string ClassName { get; set; } = string.Empty;

    public int VectorSize { get; set; }

    public float[] Embedding { get; set; } = Array.Empty<float>();

    public DateTime CreatedUtc { get; set; }
}