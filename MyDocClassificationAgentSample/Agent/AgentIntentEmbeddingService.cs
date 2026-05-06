using Classifier.Embeddings;
using System;
using System.Threading.Tasks;

namespace Agent;

/// <summary>
/// Generates embeddings for user queries to compare with stored data.
/// </summary>
public sealed class AgentIntentEmbeddingService
{
    private readonly IEmbeddingGenerator _embeddingGenerator;

    /// <summary>
    /// Initializes the service with an embedding generator.
    /// </summary>
    /// <param name="embeddingGenerator">Embedding generator instance.</param>
    public AgentIntentEmbeddingService(IEmbeddingGenerator embeddingGenerator)
    {
        _embeddingGenerator = embeddingGenerator
                              ?? throw new ArgumentNullException(nameof(embeddingGenerator));

    }

    /// <summary>
    /// Generates an embedding for the given user query.
    /// </summary>
    /// <param name="intent">User input text.</param>
    /// <returns>Embedding of the user query.</returns>
    public async Task<DocumentEmbedding> GenerateIntentEmbeddingAsync(string intent)
    {
        if (string.IsNullOrWhiteSpace(intent))
        {
            throw new ArgumentException("Intent must not be null or empty.", nameof(intent));
        }

        return await _embeddingGenerator.GenerateAsync(
            intent,
            "agent-intent");
    }
}