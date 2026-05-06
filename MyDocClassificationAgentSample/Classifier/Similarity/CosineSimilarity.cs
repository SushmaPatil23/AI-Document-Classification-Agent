using System;

namespace Classifier.Similarity;

/// <summary>
/// Provides methods to calculate similarity between vectors.
/// </summary>
public static class CosineSimilarity
{
    /// <summary>
    /// Calculates cosine similarity between two vectors.
    /// </summary>
    /// <param name="vectorA">First vector.</param>
    /// <param name="vectorB">Second vector.</param>
    /// <returns>Similarity score between the two vectors.</returns>
    public static double Compute(float[] vectorA, float[] vectorB)
    {
        if (vectorA == null || vectorB == null)
        {
            throw new ArgumentException("Vectors must not be null.");
        }

        if (vectorA.Length != vectorB.Length)
        {
            throw new ArgumentException("Vectors must have the same length.");
        }

        double dotProduct = 0;
        double magnitudeA = 0;
        double magnitudeB = 0;

        for (int i = 0; i < vectorA.Length; i++)
        {
            dotProduct += vectorA[i] * vectorB[i];
            magnitudeA += vectorA[i] * vectorA[i];
            magnitudeB += vectorB[i] * vectorB[i];
        }

        if (magnitudeA == 0 || magnitudeB == 0)
        {
            return 0;
        }

        return dotProduct / (Math.Sqrt(magnitudeA) * Math.Sqrt(magnitudeB));
    }
}