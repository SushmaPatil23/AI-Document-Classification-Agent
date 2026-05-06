namespace Agent.Similarity;

/// <summary>
/// Defines methods to calculate similarity between two vectors.
/// </summary>
public interface ISimilarityMetric
{
    /// <summary>
    /// Computes similarity between two vectors.
    /// </summary>
    /// <param name="vectorA">First vector.</param>
    /// <param name="vectorB">Second vector.</param>
    /// <returns>Similarity score between the vectors.</returns>
    double Compute(float[] vectorA, float[] vectorB);
}