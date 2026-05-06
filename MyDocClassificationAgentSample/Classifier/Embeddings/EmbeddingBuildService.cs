using Classifier;
using Classifier.Similarity;
using Classifier.PreProcessing;
using Classifier.Persistence.Repositories;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
 
namespace Classifier.Embeddings;
 
/// <summary>
/// Provides methods to build embeddings (all documents, a class, or a single file)
/// and to classify an uploaded document using stored centroid data.
/// </summary>
public class EmbeddingBuildService
{
    /// <summary>
    /// Runner used to process documents and generate embeddings.
    /// </summary>
    private readonly DocumentClassificationRunner _runner;
 
    /// <summary>
    /// Used to extract text from documents.
    /// </summary>
    private readonly IDocumentPreprocessor _preprocessor;
 
    /// <summary>
    /// Used to generate embedding vectors from text.
    /// </summary>
    private readonly IEmbeddingGenerator _embeddingGenerator;
 
    /// <summary>
    /// Used to store and retrieve embeddings and centroids.
    /// </summary>
    private readonly IEmbeddingRepository _repository;
 
    /// <summary>
    /// Used to log information, warnings, and errors.
    /// </summary>
    private readonly ILogger<EmbeddingBuildService> _logger;
 
    /// <summary>
    /// Initializes the service with required dependencies.
    /// </summary>
    public EmbeddingBuildService(
        DocumentClassificationRunner runner,
        IDocumentPreprocessor preprocessor,
        IEmbeddingGenerator embeddingGenerator,
        IEmbeddingRepository repository,
        ILogger<EmbeddingBuildService> logger)
    {
        _runner = runner;
        _preprocessor = preprocessor;
        _embeddingGenerator = embeddingGenerator;
        _repository = repository;
        _logger = logger;
    }
 
    /// <summary>
    /// Builds embeddings for all documents and updates centroids.
    /// </summary>
    /// <param name="docsRoot">Root folder containing all documents.</param>
    public async Task BuildAllAsync(string docsRoot)
    {
        try
        {
            await _runner.RunAsync(docsRoot, true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to rebuild all embeddings");
        }
    }
 
    /// <summary>
    /// Builds embeddings for all documents in a specific class.
    /// </summary>
    /// <param name="docsRoot">Root folder containing all documents.</param>
    /// <param name="className">Name of the class to rebuild.</param>
    public async Task BuildClassAsync(string docsRoot, string className)
    {
        try
        {
            var classPath = Path.Combine(docsRoot, className);
 
            if (!Directory.Exists(classPath))
                throw new DirectoryNotFoundException($"Class directory not found: {classPath}");
 
            await _runner.RunAsync(classPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to rebuild embeddings for class {ClassName}", className);
        }
    }
 
    /// <summary>
    /// Builds embedding for a single file.
    /// </summary>
    /// <param name="filePath">Path of the file.</param>
    /// <param name="className">Class to which the file belongs.</param>
    public async Task BuildFileAsync(string filePath, string className)
    {
        try
        {
await _runner.GenerateSingleAsync(filePath, className, true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to rebuild embedding for file {FilePath}", filePath);
        }
    }
 
    /// <summary>
    /// Classifies an uploaded document by comparing its embedding with stored class centroids
    /// and then finding the most similar document in the predicted class.
    /// </summary>
    /// <param name="filePath">Path of the uploaded file.</param>
    public async Task ClassifyUploadedDocumentAsync(string filePath)
    {
        try
        {
            _logger.LogInformation("Extracting text from uploaded document");
 
            var text = await _preprocessor.ExtractTextAsync(filePath);
 
            if (string.IsNullOrWhiteSpace(text))
            {
                _logger.LogWarning("Could not extract text from document");
                return;
            }
 
            if (text.Length > 6000)
                text = text.Substring(0, 6000);
 
            var embedding = await _embeddingGenerator.GenerateAsync(text, "uploaded");
 
            var centroids = await _repository.GetAllCentroidsAsync();

            var centroidVectors = centroids.Select(c => new
            {
                Class = c.Class.Name,
                Vector = c.Centroid
            });

            var rankedClasses = centroidVectors
                .Select(c => new
                {
                    Class = c.Class,
                    Score = CosineSimilarity.Compute(
                        embedding.Embedding,
                        c.Vector)
                })
                .OrderByDescending(x => x.Score)
                .Take(3)
                .ToList();
 
            Console.WriteLine();
            Console.WriteLine("Top matching classes:");
 
            int rank = 1;
            foreach (var cls in rankedClasses)
            {
                Console.WriteLine($"{rank}. {cls.Class} (score: {cls.Score:F4})");
                rank++;
            }
 
            var bestClass = rankedClasses.First();
            var documentEntities =
                await _repository.GetDocumentEmbeddingsByClassAsync(bestClass.Class);
 
            if (!documentEntities.Any())
            {
                _logger.LogWarning("No document embeddings found for class {ClassName}", bestClass.Class);
                return;
            }
 
            var bestDocument = documentEntities
                .Select(entity =>
                {
                    var vector = entity.Embedding;

                    var score = CosineSimilarity.Compute(
                        embedding.Embedding,
                        vector);
 
                    return new
                    {
                        File = entity.Document.FileName,
                        Score = score
                    };
                })
                .OrderByDescending(x => x.Score)
                .First();
 
            Console.WriteLine();
            Console.WriteLine($"📁 Predicted class: {bestClass.Class}");
            Console.WriteLine($"📊 Class similarity score: {bestClass.Score:F4}");
            Console.WriteLine($"📄 Best matching document: {bestDocument.File}");
            Console.WriteLine($"📊 Document similarity score: {bestDocument.Score:F4}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to classify uploaded document {FilePath}", filePath);
        }
    }
}