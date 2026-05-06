using Classifier;
using Classifier.Embeddings;
using Classifier.PreProcessing;
using Classifier.Persistence.Repositories;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MyDocClassifierAgentUnitTests.ClassifierTests;

/// <summary>
/// Tests for DocumentClassificationRunner to verify document processing and embedding workflow.
/// </summary>
[TestClass]
public class DocumentClassificationRunnerTests
{
    /// <summary>
    /// Creates a runner instance with mocked dependencies.
    /// </summary>
    private DocumentClassificationRunner CreateRunner(
        Mock<IDocumentPreprocessor> preprocessor,
        Mock<IEmbeddingGenerator> generator,
        Mock<IEmbeddingRepository> repository)
    {
        return new DocumentClassificationRunner(
            preprocessor.Object,
            generator.Object,
            repository.Object);
    }

    /// <summary>
    /// Verifies that unsupported files are skipped.
    /// </summary>
    [TestMethod]
    public async Task RunAsync_SkipsUnsupportedFiles()
    {
        var docs = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(docs);

        var classDir = Path.Combine(docs, "TestClass");
        Directory.CreateDirectory(classDir);

        await File.WriteAllTextAsync(Path.Combine(classDir, "file.xyz"), "dummy");

        var preprocessor = new Mock<IDocumentPreprocessor>();
        var generator = new Mock<IEmbeddingGenerator>();
        var repo = new Mock<IEmbeddingRepository>();

        var runner = CreateRunner(preprocessor, generator, repo);

        await runner.RunAsync(docs);

        generator.Verify(g => g.GenerateAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    /// <summary>
    /// Verifies that existing embeddings are skipped.
    /// </summary>
    [TestMethod]
    public async Task RunAsync_SkipsExistingEmbedding()
    {
        var docs = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(docs);

        var classDir = Path.Combine(docs, "TestClass");
        Directory.CreateDirectory(classDir);

        var file = Path.Combine(classDir, "file.txt");
        await File.WriteAllTextAsync(file, "content");

        var preprocessor = new Mock<IDocumentPreprocessor>();
        preprocessor.Setup(p => p.ExtractTextAsync(It.IsAny<string>()))
            .ReturnsAsync("text");

        var generator = new Mock<IEmbeddingGenerator>();

        var repo = new Mock<IEmbeddingRepository>();
        repo.Setup(r => r.EmbeddingExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        var runner = CreateRunner(preprocessor, generator, repo);

        await runner.RunAsync(docs);

        generator.Verify(g => g.GenerateAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    /// <summary>
    /// Verifies that embeddings are generated for valid files.
    /// </summary>
    [TestMethod]
    public async Task RunAsync_GeneratesEmbeddingForValidFile()
    {
        var docs = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(docs);

        var classDir = Path.Combine(docs, "TestClass");
        Directory.CreateDirectory(classDir);

        var file = Path.Combine(classDir, "file.txt");
        await File.WriteAllTextAsync(file, "content");

        var preprocessor = new Mock<IDocumentPreprocessor>();
        preprocessor.Setup(p => p.ExtractTextAsync(It.IsAny<string>()))
            .ReturnsAsync("processed text");

        var generator = new Mock<IEmbeddingGenerator>();
        generator.Setup(g => g.GenerateAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new DocumentEmbedding
            {
                FileName = "file.txt",
                Embedding = new float[] { 1, 2 },
                VectorSize = 2,
                CreatedUtc = DateTime.UtcNow
            });

        var repo = new Mock<IEmbeddingRepository>();
        repo.Setup(r => r.EmbeddingExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        var runner = CreateRunner(preprocessor, generator, repo);

        await runner.RunAsync(docs);

        generator.Verify(g => g.GenerateAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    /// <summary>
    /// Verifies that embeddings are saved to the repository.
    /// </summary>
    [TestMethod]
    public async Task RunAsync_SavesEmbeddingToRepository()
    {
        var docs = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(docs);

        var classDir = Path.Combine(docs, "TestClass");
        Directory.CreateDirectory(classDir);

        var file = Path.Combine(classDir, "file.txt");
        await File.WriteAllTextAsync(file, "content");

        var preprocessor = new Mock<IDocumentPreprocessor>();
        preprocessor.Setup(p => p.ExtractTextAsync(It.IsAny<string>()))
            .ReturnsAsync("processed text");

        var generator = new Mock<IEmbeddingGenerator>();
        generator.Setup(g => g.GenerateAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new DocumentEmbedding());

        var repo = new Mock<IEmbeddingRepository>();
        repo.Setup(r => r.EmbeddingExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        var runner = CreateRunner(preprocessor, generator, repo);

        await runner.RunAsync(docs);

        repo.Verify(r => r.SaveDocumentEmbeddingAsync(
    "TestClass",
    "file.txt",
    It.IsAny<float[]>()),
    Times.Once);
    }

    /// <summary>
    /// Verifies that embedding is generated for a single file.
    /// </summary>
    [TestMethod]
    public async Task GenerateSingleAsync_GeneratesEmbedding()
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dir);

        var file = Path.Combine(dir, "test.txt");
        await File.WriteAllTextAsync(file, "content");

        var preprocessor = new Mock<IDocumentPreprocessor>();
        preprocessor.Setup(p => p.ExtractTextAsync(It.IsAny<string>()))
            .ReturnsAsync("processed");

        var generator = new Mock<IEmbeddingGenerator>();
        generator.Setup(g => g.GenerateAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new DocumentEmbedding());

        var repo = new Mock<IEmbeddingRepository>();
        repo.Setup(r => r.EmbeddingExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        var runner = CreateRunner(preprocessor, generator, repo);

        await runner.GenerateSingleAsync(file, "TestClass");

        generator.Verify(g => g.GenerateAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    /// <summary>
    /// Verifies that duplicate embeddings are skipped.
    /// </summary>
    [TestMethod]
    public async Task GenerateSingleAsync_SkipsDuplicateEmbedding()
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dir);

        var file = Path.Combine(dir, "test.txt");
        await File.WriteAllTextAsync(file, "content");

        var preprocessor = new Mock<IDocumentPreprocessor>();
        var generator = new Mock<IEmbeddingGenerator>();

        var repo = new Mock<IEmbeddingRepository>();
        repo.Setup(r => r.EmbeddingExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        var runner = CreateRunner(preprocessor, generator, repo);

        await runner.GenerateSingleAsync(file, "TestClass");

        generator.Verify(g => g.GenerateAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    /// <summary>
    /// Verifies that empty text is skipped.
    /// </summary>
    [TestMethod]
    public async Task GenerateSingleAsync_SkipsEmptyText()
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dir);

        var file = Path.Combine(dir, "test.txt");
        await File.WriteAllTextAsync(file, "content");

        var preprocessor = new Mock<IDocumentPreprocessor>();
        preprocessor.Setup(p => p.ExtractTextAsync(It.IsAny<string>()))
            .ReturnsAsync("");

        var generator = new Mock<IEmbeddingGenerator>();
        var repo = new Mock<IEmbeddingRepository>();

        var runner = CreateRunner(preprocessor, generator, repo);

        await runner.GenerateSingleAsync(file, "TestClass");

        generator.Verify(g => g.GenerateAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    /// <summary>
    /// Verifies that centroids are recomputed after processing.
    /// </summary>
    [TestMethod]
    public async Task RunAsync_RecomputesCentroids()
    {
        var docs = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(docs);

        var repo = new Mock<IEmbeddingRepository>();
        var preprocessor = new Mock<IDocumentPreprocessor>();
        var generator = new Mock<IEmbeddingGenerator>();

        var runner = CreateRunner(preprocessor, generator, repo);

        await runner.RunAsync(docs);

        repo.Verify(r => r.RecomputeCentroidsAsync(), Times.Once);
    }

    /// <summary>
    /// Verifies that the preprocessor is called for valid files.
    /// </summary>
    [TestMethod]
    public async Task RunAsync_CallsPreprocessorForValidFile()
    {
        var docs = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(docs);

        var classDir = Path.Combine(docs, "TestClass");
        Directory.CreateDirectory(classDir);

        var file = Path.Combine(classDir, "file.txt");
        await File.WriteAllTextAsync(file, "content");

        var preprocessor = new Mock<IDocumentPreprocessor>();
        preprocessor.Setup(p => p.ExtractTextAsync(It.IsAny<string>()))
            .ReturnsAsync("processed text");

        var generator = new Mock<IEmbeddingGenerator>();
        generator.Setup(g => g.GenerateAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new DocumentEmbedding());

        var repo = new Mock<IEmbeddingRepository>();
        repo.Setup(r => r.EmbeddingExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        var runner = CreateRunner(preprocessor, generator, repo);

        await runner.RunAsync(docs);

        preprocessor.Verify(p =>
            p.ExtractTextAsync(It.IsAny<string>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that multiple files are processed correctly.
    /// </summary>
    [TestMethod]
    public async Task RunAsync_ProcessesMultipleFiles()
    {
        var docs = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(docs);

        var classDir = Path.Combine(docs, "TestClass");
        Directory.CreateDirectory(classDir);

        await File.WriteAllTextAsync(Path.Combine(classDir, "file1.txt"), "content1");
        await File.WriteAllTextAsync(Path.Combine(classDir, "file2.txt"), "content2");

        var preprocessor = new Mock<IDocumentPreprocessor>();
        preprocessor.Setup(p => p.ExtractTextAsync(It.IsAny<string>()))
            .ReturnsAsync("processed text");

        var generator = new Mock<IEmbeddingGenerator>();
        generator.Setup(g => g.GenerateAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new DocumentEmbedding());

        var repo = new Mock<IEmbeddingRepository>();
        repo.Setup(r => r.EmbeddingExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        var runner = CreateRunner(preprocessor, generator, repo);

        await runner.RunAsync(docs);

        generator.Verify(g =>
            g.GenerateAsync(It.IsAny<string>(), It.IsAny<string>()),
            Times.Exactly(2));
    }

    /// <summary>
    /// Verifies that no embedding is saved when text is empty.
    /// </summary>
    [TestMethod]
    public async Task GenerateSingleAsync_DoesNotSave_WhenTextIsEmpty()
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dir);

        var file = Path.Combine(dir, "test.txt");
        await File.WriteAllTextAsync(file, "content");

        var preprocessor = new Mock<IDocumentPreprocessor>();
        preprocessor.Setup(p => p.ExtractTextAsync(It.IsAny<string>()))
            .ReturnsAsync("");

        var generator = new Mock<IEmbeddingGenerator>();

        var repo = new Mock<IEmbeddingRepository>();
        repo.Setup(r => r.EmbeddingExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        var runner = CreateRunner(preprocessor, generator, repo);

        await runner.GenerateSingleAsync(file, "TestClass");

        repo.Verify(r =>
    r.SaveDocumentEmbeddingAsync(
        It.IsAny<string>(),
        It.IsAny<string>(),
        It.IsAny<float[]>()),
    Times.Never);
    }

    /// <summary>
    /// Verifies that exceptions in preprocessing are handled safely.
    /// </summary>
    [TestMethod]
    public async Task RunAsync_HandlesPreprocessorException()
    {
        var docs = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(docs);

        var classDir = Path.Combine(docs, "TestClass");
        Directory.CreateDirectory(classDir);

        var file = Path.Combine(classDir, "file.txt");
        await File.WriteAllTextAsync(file, "content");

        var preprocessor = new Mock<IDocumentPreprocessor>();
        preprocessor.Setup(p => p.ExtractTextAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Parser failed"));

        var generator = new Mock<IEmbeddingGenerator>();
        var repo = new Mock<IEmbeddingRepository>();

        var runner = CreateRunner(preprocessor, generator, repo);

        await runner.RunAsync(docs);

        generator.Verify(g =>
            g.GenerateAsync(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }
}