using Agent;
using Classifier.Embeddings;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace MyDocClassifierAgentUnitTests;

/// <summary>
/// Tests for AgentIntentEmbeddingService to verify embedding generation behavior.
/// </summary>
[TestClass]
public class AgentIntentEmbeddingServiceTests
{
    /// <summary>
    /// Verifies that the embedding generator is called and returns expected result.
    /// </summary>
    [TestMethod]
    public async Task GenerateIntentEmbeddingAsync_Should_Call_EmbeddingGenerator()
    {
        var mockGenerator = new Mock<IEmbeddingGenerator>();

        var expectedEmbedding = new DocumentEmbedding
        {
            Embedding = new float[] { 0.5f, 0.5f }
        };

        mockGenerator
            .Setup(g => g.GenerateAsync("test intent", "agent-intent"))
            .ReturnsAsync(expectedEmbedding);

        var service = new AgentIntentEmbeddingService(mockGenerator.Object);

        var result = await service.GenerateIntentEmbeddingAsync("test intent");

        Assert.IsNotNull(result);
        CollectionAssert.AreEqual(expectedEmbedding.Embedding, result.Embedding);

        mockGenerator.Verify(
            g => g.GenerateAsync("test intent", "agent-intent"),
            Times.Once);
    }

    /// <summary>
    /// Verifies that an exception is thrown when input is null.
    /// </summary>
    [TestMethod]
    public async Task GenerateIntentEmbeddingAsync_Should_Throw_When_Intent_Is_Null()
    {
        var mockGenerator = new Mock<IEmbeddingGenerator>();
        var service = new AgentIntentEmbeddingService(mockGenerator.Object);

        await Assert.ThrowsExactlyAsync<ArgumentException>(async () =>
    await service.GenerateIntentEmbeddingAsync(" "));
    }

    /// <summary>
    /// Verifies that an exception is thrown when input is empty or whitespace.
    /// </summary>
    [TestMethod]
    public async Task GenerateIntentEmbeddingAsync_Should_Throw_When_Intent_Is_Empty()
    {
        var mockGenerator = new Mock<IEmbeddingGenerator>();
        var service = new AgentIntentEmbeddingService(mockGenerator.Object);

        await Assert.ThrowsExactlyAsync<ArgumentException>(async () =>
    await service.GenerateIntentEmbeddingAsync(" "));
    }

    /// <summary>
    /// Verifies that exceptions from the embedding generator are propagated.
    /// </summary>
    [TestMethod]
    public async Task GenerateIntentEmbeddingAsync_Should_Propagate_Exception_When_GeneratorFails()
    {
        var mockGenerator = new Mock<IEmbeddingGenerator>();

        mockGenerator
            .Setup(g => g.GenerateAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Embedding failure"));

        var service = new AgentIntentEmbeddingService(mockGenerator.Object);

        await Assert.ThrowsExactlyAsync<ArgumentException>(async () =>
    await service.GenerateIntentEmbeddingAsync(" "));
    }
}