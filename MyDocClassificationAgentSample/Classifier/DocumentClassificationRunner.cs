using Classifier.Embeddings;
using Classifier.PreProcessing;
using Classifier.Persistence.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
 
namespace Classifier;
 
/// <summary>
/// Runs the document classification process including preprocessing, embedding generation, and storage.
/// </summary>
public sealed class DocumentClassificationRunner
{
    private readonly IDocumentPreprocessor _preprocessor;
    private readonly IEmbeddingGenerator _embeddingGenerator;
    private readonly IEmbeddingRepository _repository;
 
    private static readonly HashSet<string> SupportedExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".txt",
            ".pdf",
            ".docx",
            ".docs",
            ".png",
            ".jpg",
            ".jpeg"
        };
 
    /// <summary>
    /// Initializes the runner with required services.
    /// </summary>
    public DocumentClassificationRunner(
        IDocumentPreprocessor preprocessor,
        IEmbeddingGenerator embeddingGenerator,
        IEmbeddingRepository repository)
    {
        _preprocessor = preprocessor;
        _embeddingGenerator = embeddingGenerator;
        _repository = repository;
    }
 
    /// <summary>
    /// Processes all documents, generates embeddings, stores them, and recomputes centroids.
    /// </summary>
    /// <param name="documentsRoot">Root folder containing documents.</param>
    /// <param name="forceRebuild">If true, rebuilds all embeddings.</param>
    public async Task RunAsync(string documentsRoot, bool forceRebuild = false)
    {
        if (!Directory.Exists(documentsRoot))
            throw new DirectoryNotFoundException(documentsRoot);
 
        if (forceRebuild)
        {
            Log("Force rebuild requested. Clearing database embeddings...");
 
            await _repository.DeleteAllEmbeddingsAsync();
            await _repository.DeleteAllCentroidsAsync();
        }
 
        Log("Starting SQL-based classification process.");
 
        foreach (var classDir in Directory.GetDirectories(documentsRoot))
        {
            var className = Path.GetFileName(classDir);
 
            foreach (var file in Directory.GetFiles(classDir))
            {
                var extension = Path.GetExtension(file);
 
                if (!SupportedExtensions.Contains(extension))
                    continue;
 
                var fileName = Path.GetFileName(file);
 
                if (!forceRebuild && await _repository.EmbeddingExistsAsync(fileName))
                {
                    Log($"Skipping existing embedding: {fileName}");
                    continue;
                }
 
                if (forceRebuild)
                {
                    await _repository.DeleteEmbeddingForFileAsync(fileName);
                }
 
                try
                {
                    var text = await _preprocessor.ExtractTextAsync(file);
 
                    var embedding = await _embeddingGenerator.GenerateAsync(text, fileName);
 
                    embedding.ClassName = className;
 
                  
                    await _repository.SaveDocumentEmbeddingAsync(
                       className,
                       fileName,
                       embedding.Embedding);

                    Log($"Saved embedding to DB: {fileName}");
                }
                catch (Exception ex)
                {
                    Log($"Failed processing {fileName}: {ex.Message}");
                }
            }
        }
 
        Log("Recomputing centroids in DB...");
        await _repository.RecomputeCentroidsAsync();
    }
 
    /// <summary>
    /// Generates embedding for a single file and updates centroids.
    /// </summary>
    /// <param name="filePath">Path of the file.</param>
    /// <param name="className">Class name.</param>
    public async Task GenerateSingleAsync(string filePath, string className, bool forceRebuild = false)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException(filePath);
 
        var fileName = Path.GetFileName(filePath);
 
        if (await _repository.EmbeddingExistsAsync(fileName))
        {
            if (forceRebuild)
            {
                Log($"Deleting existing embedding for: {fileName}");
                await _repository.DeleteEmbeddingForFileAsync(fileName);
            }
            else
            {
                Log($"Embedding already exists: {fileName}");
                return;
            }
            Log($"Generating new embedding for: {fileName}");
        }
        var text = await _preprocessor.ExtractTextAsync(filePath);
 
        if (string.IsNullOrWhiteSpace(text))
        {
            Log($"Skipping {fileName} because extracted text is empty.");
            return;
        }
 
        var embedding = await _embeddingGenerator.GenerateAsync(text, fileName);
 
        embedding.ClassName = className;

        await _repository.SaveDocumentEmbeddingAsync(
    className,
    fileName,
    embedding.Embedding);

               await _repository.RecomputeCentroidsAsync();
 
        Log($"Rebuilt embedding for {fileName}");
    }
 
    /// <summary>
    /// Writes messages to the console for tracking progress.
    /// </summary>
    /// <param name="message">Message to display.</param>
    private static void Log(string message)
    {
        Console.WriteLine($"[Runner] {message}");
    }
}