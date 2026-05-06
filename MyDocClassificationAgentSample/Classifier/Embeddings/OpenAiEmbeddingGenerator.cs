using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Classifier.Embeddings;

/// <summary>
/// Generates embeddings using OpenAI API.
/// Sends text to OpenAI and converts the response into a DocumentEmbedding.
/// </summary>
public sealed class OpenAiEmbeddingGenerator : IEmbeddingGenerator
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes the generator with the provided API key.
    /// </summary>
    /// <param name="apiKey">OpenAI API key.</param>
    public OpenAiEmbeddingGenerator(string apiKey)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://api.openai.com/")
        };

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);
    }
    
    /// <summary>
    /// Sends text to OpenAI and generates an embedding vector.
    /// </summary>
    /// <param name="text">The input text.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <returns>A DocumentEmbedding with the generated vector.</returns>
    public async Task<DocumentEmbedding> GenerateAsync(
        string text,
        string fileName
    )
    {
        var payload = new
        {
            model = "text-embedding-3-large",
            input = text
        };

        var json = JsonSerializer.Serialize(payload);

        using var content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.PostAsync(
            "v1/embeddings",
            content
        );

        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(responseJson);

        var embeddingArray = doc
            .RootElement
            .GetProperty("data")[0]
            .GetProperty("embedding")
            .EnumerateArray()
            .Select(x => x.GetSingle())
            .ToArray();

        return new DocumentEmbedding
        {
            FileName = fileName,
            VectorSize = embeddingArray.Length,
            Embedding = embeddingArray,
            CreatedUtc = DateTime.UtcNow
        };
    }
}