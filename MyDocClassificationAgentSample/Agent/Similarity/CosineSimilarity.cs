namespace Agent.Similarity;

/// <summary>
/// Provides methods to calculate similarity between vectors.
/// </summary>
public static class CosineSimilarity
{
    /// <summary>
    /// Calculates cosine similarity between two vectors.
    /// </summary>
    /// <param name="a">First vector.</param>
    /// <param name="b">Second vector.</param>
    /// <returns>Similarity score between the two vectors.</returns>
    public static float Compute(float[] a, float[] b)
    {
        float dot = 0f;
        float magA = 0f;
        float magB = 0f;

        for (int i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            magA += a[i] * a[i];
            magB += b[i] * b[i];
        }

        return dot / ((float)Math.Sqrt(magA) * (float)Math.Sqrt(magB));
    }
}