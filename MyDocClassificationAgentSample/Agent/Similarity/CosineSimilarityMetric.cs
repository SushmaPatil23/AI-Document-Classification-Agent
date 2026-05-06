using Classifier.Similarity;

namespace Agent.Similarity;

/// <summary>
/// Implementation of similarity metric using cosine similarity.
/// </summary>
public sealed class CosineSimilarityMetric : ISimilarityMetric
{
    /// <summary>
    /// Computes similarity between two vectors using cosine similarity.
    /// </summary>
    /// <param name="vectorA">First vector.</param>
    /// <param name="vectorB">Second vector.</param>
    /// <returns>Similarity score between the vectors.</returns>
    public double Compute(float[] vectorA, float[] vectorB)
    {
        return CosineSimilarity.Compute(vectorA, vectorB);
    }
}