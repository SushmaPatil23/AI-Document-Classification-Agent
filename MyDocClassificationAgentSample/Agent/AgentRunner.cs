using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Agent.Similarity;
using Classifier.Embeddings;
using Classifier.Persistence.Repositories;

namespace Agent;

/// <summary>
/// Handles the full agent workflow from user input to final result.
/// </summary>
public sealed class AgentRunner
{
    private readonly AgentIntentEmbeddingService _intentService;
    private readonly AgentSimilarityMatcher _matcher;
    private readonly List<ClassCentroid> _centroids;
    private readonly IEmbeddingRepository _repository;

    /// <summary>
    /// Initializes the agent runner with required services.
    /// </summary>
    public AgentRunner(
        AgentIntentEmbeddingService intentService,
        AgentSimilarityMatcher matcher,
        List<ClassCentroid> centroids,
        IEmbeddingRepository repository)
    {
        _intentService = intentService
            ?? throw new ArgumentNullException(nameof(intentService));

        _matcher = matcher
            ?? throw new ArgumentNullException(nameof(matcher));

        _centroids = centroids
            ?? throw new ArgumentNullException(nameof(centroids));

        _repository = repository
            ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>
    /// Processes user input, ranks classes, finds the best document, and displays results.
    /// </summary>
    /// <param name="inputText">User input text.</param>
    public async Task RunAsync(string inputText)
    {
        if (!ContainsMeaningfulText(inputText))
        {
            Console.WriteLine("Input does not appear to contain meaningful text.");
            return;
        }

        var intentEmbedding = await _intentService
            .GenerateIntentEmbeddingAsync(inputText);

        var rankedClasses = _matcher
            .RankTopClasses(intentEmbedding, _centroids, 3);

        if (!rankedClasses.Any())
        {
            Console.WriteLine("No matching document classes found.");
            return;
        }

        Console.WriteLine();
        Console.WriteLine("Top matching classes:");

        int rank = 1;
        foreach (var cls in rankedClasses)
        {
            Console.WriteLine($"{rank}. {cls.ClassName} (score: {cls.Score:F4})");
            rank++;
        }

        Console.WriteLine();

        var bestClass = rankedClasses.First();

        var documentEntities =
            await _repository.GetDocumentEmbeddingsByClassAsync(bestClass.ClassName);

        if (!documentEntities.Any())
        {
            Console.WriteLine("No document embeddings found.");
            return;
        }

        double threshold = 0.15;

        var matches = documentEntities
            .Select(entity =>
            {
                var vector = entity.Embedding;

                var score = CosineSimilarity.Compute(
                    intentEmbedding.Embedding,
                    vector);

                return new
                {
                    File = entity.Document.FileName,
                    Score = score
                };
            })
            .OrderByDescending(x => x.Score)
            .ToList();

        var bestDocument = matches.First();
        
       // Console.WriteLine($"[DEBUG] Best document score: {bestDocument.Score:F4}");

        if (bestDocument.Score < threshold)
        {
            Console.WriteLine("❌ No relevant document found (below threshold).");
            return;
        }

        Console.WriteLine($"📁 Predicted class: {bestClass.ClassName}");
        Console.WriteLine($"📊 Class similarity score: {bestClass.Score:F4}");
        Console.WriteLine($"📄 Best matching document: {bestDocument.File}");
        Console.WriteLine($"📊 Document similarity score: {bestDocument.Score:F4}");
    }

    /// <summary>
    /// Checks if the input text is valid for processing.
    /// </summary>
    /// <param name="text">Input text.</param>
    /// <returns>True if valid, otherwise false.</returns>
    private bool ContainsMeaningfulText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        if (text.All(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || c == '.' || c == '-'))
            return true;

        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        int validWords = 0;

        foreach (var word in words)
        {
            if (word.All(c => char.IsLetterOrDigit(c) || c == '.' || c == '-'))
            {
                validWords++;
            }
        }

        return validWords >= 1;
    }
}