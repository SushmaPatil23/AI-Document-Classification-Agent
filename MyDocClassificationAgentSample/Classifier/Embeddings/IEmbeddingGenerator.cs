namespace Classifier.Embeddings;

/// <summary>
/// Defines methods for generating embeddings from text.
/// Allows different implementations to be used without changing the main logic.
/// </summary>
public interface IEmbeddingGenerator
{
    /// <summary>
    /// Generates an embedding vector from the given text.
    /// </summary>
    /// <param name="text">The input text to generate embedding from.</param>
    /// <param name="documentName">Name of the document.</param>
    /// <returns>A document embedding containing the generated vector.</returns>
    Task<DocumentEmbedding> GenerateAsync(string text, string documentName);
}