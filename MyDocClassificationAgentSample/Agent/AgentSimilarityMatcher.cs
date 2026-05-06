using Classifier.Embeddings;
using System;
using System.Collections.Generic;
using System.Linq;
using Agent.Similarity;

namespace Agent;

/// <summary>
/// Compares query embeddings with class centroids and ranks them by similarity.
/// </summary>
public sealed class AgentSimilarityMatcher
{
    private readonly ISimilarityMetric _similarityMetric;

    /// <summary>
    /// Initializes the matcher with a similarity metric.
    /// </summary>
    /// <param name="similarityMetric">Similarity metric implementation.</param>
    public AgentSimilarityMatcher(ISimilarityMetric similarityMetric)
    {
        _similarityMetric = similarityMetric
                            ?? throw new ArgumentNullException(nameof(similarityMetric));
    }

    /// <summary>
    /// Ranks all classes based on similarity to the query embedding.
    /// </summary>
    /// <param name="intentEmbedding">Query embedding.</param>
    /// <param name="classCentroids">List of class centroids.</param>
    /// <returns>List of class names with similarity scores.</returns>
    public IReadOnlyList<(string ClassName, double Score)> RankClasses(
        DocumentEmbedding intentEmbedding,
        IEnumerable<ClassCentroid> classCentroids)
    {
        if (intentEmbedding == null)
        {
            throw new ArgumentNullException(nameof(intentEmbedding));
        }

        if (classCentroids == null)
        {
            throw new ArgumentNullException(nameof(classCentroids));
        }

        return classCentroids
            .Select(centroid => (
                centroid.ClassName,
                Score: _similarityMetric.Compute(
                    intentEmbedding.Embedding,
                    centroid.Embedding)))
            .OrderByDescending(result => result.Score)
            .ToList();
    }

    /// <summary>
    /// Returns the top N most similar classes.
    /// </summary>
    /// <param name="intentEmbedding">Query embedding.</param>
    /// <param name="classCentroids">List of class centroids.</param>
    /// <param name="topN">Number of top results to return.</param>
    /// <returns>Top N class results with scores.</returns>
    public IReadOnlyList<(string ClassName, double Score)> RankTopClasses(
        DocumentEmbedding intentEmbedding,
        IEnumerable<ClassCentroid> classCentroids,
        int topN)
    {
        if (topN <= 0)
        {
            throw new ArgumentException("topN must be greater than zero.", nameof(topN));
        }

        var ranked = RankClasses(intentEmbedding, classCentroids);

        return ranked
            .Take(topN)
            .ToList();
    }
}