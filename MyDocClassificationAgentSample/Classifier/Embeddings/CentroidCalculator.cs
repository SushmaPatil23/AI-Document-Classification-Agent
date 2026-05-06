using System.Xml.Linq;
namespace Classifier.Embeddings;

/// <summary>
/// Calculates the average vector (centroid) from a list of vectors.
/// This is used to represent a class by combining all its document embeddings.
/// </summary>
public static class CentroidCalculator
{
    /// <summary>
    /// Computes the centroid by taking the average of all vectors.
    /// </summary>
    /// <param name="vectors">List of vectors to average.</param>
    /// <returns>A new vector representing the average (centroid).</returns>
    public static float[] Compute(IReadOnlyList<float[]> vectors)
    {
        if (vectors == null || vectors.Count == 0)
            throw new ArgumentException("No vectors provided");

        int dimension = vectors[0].Length;

        // Check that all vectors have the same size
        foreach (var vector in vectors)
        {
            if (vector.Length != dimension)
                throw new ArgumentException("All vectors must have the same dimension");
        }

        var centroid = new float[dimension];

        // Add all vector values
        foreach (var vector in vectors)
        {
            for (int i = 0; i < dimension; i++)
            {
                centroid[i] += vector[i];
            }
        }

        // Divide by number of vectors to get average
        for (int i = 0; i < dimension; i++)
        {
            centroid[i] /= vectors.Count;
        }

        return centroid;
    }
}